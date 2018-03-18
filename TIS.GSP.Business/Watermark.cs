using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Drawing.Imaging;
using GalleryServer.Business.Interfaces;
using GalleryServer.Business.Properties;

namespace GalleryServer.Business
{
  /// <summary>
  /// The Watermark class contains functionality for applying a text and/or image watermark to an image.
  /// </summary>
  public class Watermark : IDisposable
  {
    #region Private Fields

    private ImageAttributes _imageAttributes;

    private System.Drawing.Image _watermarkImage;
    private int _watermarkImageWidth = int.MinValue;
    private int _watermarkImageHeight = int.MinValue;
    private string _imagePath;
    private ContentAlignment _imageLocation;
    private int _imageWidthPercent;
    private int _imageOpacityPercent;

    private string _watermarkText;
    private System.Drawing.Color _textColor;
    private int _textHeightPixels;
    private int _textWidthPercent;
    private string _textFontName;
    private ContentAlignment _textLocation;
    private int _textOpacityPercent;

    private const int MIN_FONT_HEIGHT_PIXELS = 4;
    private const int DEFAULT_TEXT_HEIGHT_PIXELS = 12; // Text height used if not specified.
    private const float BORDER_PERCENT = .01f; // The distance from the border to place the watermark text and image. Ex: .01
    // means border thickness should be 1% of the width of the recipient image.
    private static readonly object _sharedLock = new object();
    private bool _hasBeenDisposed; // Used by Dispose() methods

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the location for the watermark image on the recipient image.
    /// </summary>
    /// <value>The image location.</value>
    public ContentAlignment ImageLocation
    {
      get { return _imageLocation; }
      set { _imageLocation = value; }
    }

    /// <summary>
    /// Gets or sets the percent of the overall width of the recipient image that should be covered with the
    /// watermark image. The size of the image is automatically scaled to achieve the desired width. For example,
    /// a value of 50 means the watermark image is 50% as wide as the recipient image. Valid values are 0 - 100.
    /// A value of 0 turns off this feature and causes the image to be rendered its actual size.
    /// </summary>
    /// <value>The image width, in percent.</value>
    public int ImageWidthPercent
    {
      get { return _imageWidthPercent; }
      set { _imageWidthPercent = value; }
    }

    /// <summary>
    /// Gets or sets the watermark text to be applied to the recipient image.
    /// </summary>
    /// <value>The watermark text.</value>
    public string WatermarkText
    {
      get { return _watermarkText; }
      set { _watermarkText = value; }
    }

    /// <summary>
    /// Gets or sets the height, in pixels, of the watermark text. This value is ignored if the property
    /// TextWidthPercent is non-zero. Valid values are 0 - 10000.
    /// </summary>
    /// <value>The text height, in pixels.</value>
    public int TextHeightPixels
    {
      get { return _textHeightPixels; }
      set
      {
        if ((value < 0) || (value > 10000))
        {
          throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "TextHeightPixels must be an integer between 0 and 10000. The value {0} is invalid.", value));
        }
        _textHeightPixels = value;
      }
    }

    /// <summary>
    /// Gets or sets the percent of the overall width of the recipient image that should be covered with the
    /// watermark text. The size of the text is automatically scaled up or down to achieve the desired width. For example,
    /// a value of 50 means the text is 50% as wide as the recipient image. Valid values are 0 - 100. The text is never
    /// rendered in a font smaller than 6 pixels, so in cases of long text it may stretch wider than the percentage
    /// specified in this setting. A value of 0 turns off this feature and causes the text size to be determined by the
    /// TextSizePixels property.
    /// </summary>
    /// <value>The text width, in percent.</value>
    public int TextWidthPercent
    {
      get { return _textWidthPercent; }
      set
      {
        if ((value < 0) || (value > 100))
        {
          throw new ArgumentOutOfRangeException(String.Format(CultureInfo.CurrentCulture, "TextWidthPercent must be an integer between 0 and 100. The value {0} is invalid.", value));
        }

        _textWidthPercent = value;
      }
    }

    /// <summary>
    /// Gets or sets the font family name to use for the watermark text applied to the recipient image.
    /// If the name does not represent a font installed on the server, a generic sans serif font is used.
    /// </summary>
    /// <value>The name of the text font.</value>
    public string TextFontName
    {
      get { return _textFontName; }
      set { _textFontName = value; }
    }

    /// <summary>
    /// Gets or sets the location for the watermark text on the recipient image.
    /// </summary>
    /// <value>The text location.</value>
    public ContentAlignment TextLocation
    {
      get { return _textLocation; }
      set { _textLocation = value; }
    }

    /// <summary>
    /// Gets or sets the color of the watermark text.
    /// </summary>
    /// <value>The color of the text.</value>
    public System.Drawing.Color TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Gets or sets the opacity of the watermark text. Valid values are 0 - 100, with 0 being completely
    /// transparent and 100 completely opaque.
    /// </summary>
    /// <value>The text opacity, in percent.</value>
    public int TextOpacityPercent
    {
      get { return _textOpacityPercent; }
      set { _textOpacityPercent = value; }
    }

    /// <summary>
    /// Gets or sets the opacity of the watermark image. Valid values are 0 - 100, with 0 being completely
    /// transparent and 100 completely opaque.
    /// </summary>
    /// <value>The image opacity, in percent.</value>
    public int ImageOpacityPercent
    {
      get { return _imageOpacityPercent; }
      set { _imageOpacityPercent = value; }
    }

    /// <summary>
    /// Calculates the full path to the file name specified in <paramref name="watermarkImageFilename" /> and verifies the
    /// file can be instantiated in a <see cref="Bitmap" /> instance.
    /// </summary>
    /// <param name="watermarkImageFilename">The watermark image filename. Example: "logo.png"</param>
    /// <param name="galleryId">The gallery ID. This value is used to probe the correct directory in App_Data\Watermark_Images.</param>
    /// <exception cref="FileNotFoundException">Thrown when the <paramref name="watermarkImageFilename" /> has a value and no
    /// corresponding file is found.</exception>
    private void SetImagePath(string watermarkImageFilename, int galleryId)
    {
      if (string.IsNullOrWhiteSpace(watermarkImageFilename))
      {
        _imagePath = string.Empty;
        _watermarkImage = null;
        return;
      }

      var fullPath = Path.Combine(AppSetting.Instance.PhysicalApplicationPath, GlobalConstants.WatermarkDirectory, galleryId.ToString(), watermarkImageFilename);

      if (!File.Exists(fullPath))
      {
        throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, "No image file exists at {0}. Review the watermark settings on the Image Settings page.", fullPath));
      }

      // File exists! Assign.
      _imagePath = fullPath;

      using (var img = System.Drawing.Image.FromFile(_imagePath))
      {
        _watermarkImage = new Bitmap(img);
      }
    }

    /// <summary>
    /// Gets the full path to a watermark image to be applied to the recipient image. This value is calculated by
    /// combining the watermark image storage location (App_Data\Watermark_Images\{GalleryId}) with the filename
    /// specified by the user. Returns an empty string if no watermark image is specified.
    /// </summary>
    /// <value>The full path to a watermark image to be applied to the recipient image.</value>
    public string ImagePath
    {
      get { return _imagePath; }
    }

    /// <summary>
    /// Gets the watermark image to be applied to the recipient image. The image is created when the
    /// ImagePath property is assigned. Returns null if ImagePath is not specified (that is, the user did
    /// not enter a value in the watermarkImagePath property in the configuration file).
    /// </summary>
    /// <value>The watermark image to be applied to the recipient image.</value>
    public System.Drawing.Image WatermarkImage
    {
      get { return _watermarkImage; }
    }

    /// <summary>
    /// Gets the width, in pixels, of the watermark image. Returns int.MinValue if no watermark image is specified.
    /// </summary>
    /// <value>The width, in pixels, of the watermark image.</value>
    public int WatermarkImageWidth
    {
      get
      {
        if ((_watermarkImageWidth == int.MinValue) && (this.WatermarkImage != null))
        {
          _watermarkImageWidth = this.WatermarkImage.Width;
        }

        return _watermarkImageWidth;
      }
    }

    /// <summary>
    /// Gets the height, in pixels, of the watermark image. Returns int.MinValue if no watermark image is specified.
    /// </summary>
    /// <value>The height, in pixels, of the watermark image.</value>
    public int WatermarkImageHeight
    {
      get
      {
        if ((_watermarkImageHeight == int.MinValue) && (this.WatermarkImage != null))
        {
          _watermarkImageHeight = this.WatermarkImage.Height;
        }

        return _watermarkImageHeight;
      }
    }

    #endregion

    #region Constructors

    private Watermark()
    {
    }

    #endregion

    #region Destructor (finalizer)

    /// <summary>
    /// Releases unmanaged resources and performs other cleanup operations before the
    /// <see cref="Watermark"/> is reclaimed by garbage collection.
    /// </summary>
    ~Watermark()
    {
      Dispose(false);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Overlay the text and/or image watermark over the image specified in the <paramref name="filePath"/> parameter and return.
    /// </summary>
    /// <param name="filePath">A string representing the full path to the image file  
    /// (e.g. "C:\mypics\myprettypony.jpg", "myprettypony.jpg").</param>
    /// <returns>Returns a <see cref="System.Drawing.Image" /> instance containing the image with the watermark applied.</returns>
    public System.Drawing.Image ApplyWatermark(string filePath)
    {
      System.Drawing.Image img = System.Drawing.Image.FromFile(filePath);

      ApplyTextWatermark(img);

      if (this.WatermarkImage != null)
      {
        System.Drawing.Image watermarkedImage = ApplyImageWatermark(img);
        img.Dispose();
        return watermarkedImage;
      }
      else
      {
        return img;
      }
    }

    /// <summary>
    /// Gets the watermark that is configured for the specified <paramref name="galleryId" />.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>Returns a <see cref="Watermark" /> instance.</returns>
    public static Watermark GetUserSpecifiedWatermark(int galleryId)
    {
      IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

      Watermark tempWatermark = null;
      try
      {
        tempWatermark = new Watermark();
        tempWatermark.WatermarkText = gallerySetting.WatermarkText;
        tempWatermark.TextFontName = gallerySetting.WatermarkTextFontName;
        tempWatermark.TextColor = HelperFunctions.GetColor(gallerySetting.WatermarkTextColor);
        tempWatermark.TextHeightPixels = gallerySetting.WatermarkTextFontSize;
        tempWatermark.TextWidthPercent = gallerySetting.WatermarkTextWidthPercent;
        tempWatermark.TextOpacityPercent = gallerySetting.WatermarkTextOpacityPercent;
        tempWatermark.TextLocation = gallerySetting.WatermarkTextLocation;
        tempWatermark.SetImagePath(gallerySetting.WatermarkImagePath, galleryId);
        tempWatermark.ImageWidthPercent = gallerySetting.WatermarkImageWidthPercent;
        tempWatermark.ImageOpacityPercent = gallerySetting.WatermarkImageOpacityPercent;
        tempWatermark.ImageLocation = gallerySetting.WatermarkImageLocation;
      }
      catch
      {
        tempWatermark?.Dispose();

        throw;
      }

      return tempWatermark;
    }

    /// <summary>
    /// Gets the watermark to use when the application is in reduced functionality mode.
    /// </summary>
    /// <param name="galleryId">The gallery ID.</param>
    /// <returns>Returns a <see cref="Watermark" /> instance.</returns>
    public static Watermark GetReducedFunctionalityModeWatermark(int galleryId)
    {
      IGallerySettings gallerySetting = Factory.LoadGallerySetting(galleryId);

      Watermark tempWatermark = null;
      try
      {
        tempWatermark = new Watermark();
        tempWatermark.WatermarkText = Resources.Reduced_Functionality_Mode_Watermark_Text;
        tempWatermark.TextFontName = gallerySetting.WatermarkTextFontName;
        tempWatermark.TextColor = HelperFunctions.GetColor(gallerySetting.WatermarkTextColor);
        tempWatermark.TextHeightPixels = 0;
        tempWatermark.TextWidthPercent = 100;
        tempWatermark.TextOpacityPercent = 100;
        tempWatermark.TextLocation = ContentAlignment.MiddleCenter;
        tempWatermark._watermarkImage = Resources.GsLogo;
        tempWatermark.ImageWidthPercent = 85;
        tempWatermark.ImageOpacityPercent = 50;
        tempWatermark.ImageLocation = ContentAlignment.BottomCenter;
      }
      catch
      {
        tempWatermark?.Dispose();

        throw;
      }

      return tempWatermark;
    }

    #endregion

    #region Private Methods

    private System.Drawing.Image ApplyImageWatermark(System.Drawing.Image recipientImage)
    {
      if (recipientImage == null)
        throw new ArgumentNullException(nameof(recipientImage));

      if (this.WatermarkImage == null)
        return recipientImage;

      // Create a Bitmap from the image we are going to draw the watermark on.
      lock (_sharedLock)
      {
        Bitmap recipientBitmap = null;
        try
        {
          recipientBitmap = new Bitmap(recipientImage);
          recipientBitmap.SetResolution(recipientImage.HorizontalResolution, recipientImage.VerticalResolution);

          int recipientImageWidth = recipientImage.Width;
          int recipientImageHeight = recipientImage.Height;

          // Get the watermark image, scaling it up or down if needed.
          System.Drawing.Image watermarkImage = GetWatermarkImage(recipientImageWidth, recipientImageHeight);

          int watermarkWidth = watermarkImage.Width;
          int watermarkHeight = watermarkImage.Height;

          // Turn off the border if the watermark image is too big to allow for it.
          float borderPercent = BORDER_PERCENT;
          if ((watermarkHeight > (recipientImageHeight - (recipientImageHeight * borderPercent))) ||
              (watermarkWidth > (recipientImageWidth - (recipientImageWidth * borderPercent))))
          {
            borderPercent = 0;
          }

          // Get the X and Y position for where to start drawing the watermark image on the recipient image.
          Point watermarkStartingPoint = GetWatermarkStartingPoint((float)watermarkWidth, (float)watermarkHeight, (float)recipientImageWidth, (float)recipientImageHeight, this.ImageLocation, borderPercent);

          // Draw the watermark image on the recipient image.
          using (Graphics grWatermark = Graphics.FromImage(recipientBitmap))
          {
            grWatermark.DrawImage(watermarkImage,
                                  new Rectangle(watermarkStartingPoint.X, watermarkStartingPoint.Y, watermarkWidth, watermarkHeight), //Set the destination position
                                  0, // x-coordinate of recipient image to start drawing watermark 
                                  0, // y-coordinate of of recipient image to start drawing watermark
                                  watermarkWidth,
                                  watermarkHeight,
                                  GraphicsUnit.Pixel,
                                  GetWatermarkImageAttributes(this.ImageOpacityPercent));
          }

          //Replace the original image with the one with the newly drawn watermark image.
          recipientImage = recipientBitmap;

        }
        catch
        {
          recipientBitmap?.Dispose();

          throw;
        }
      }

      return recipientImage;
    }

    private System.Drawing.Image GetWatermarkImage(int recipientImageWidth, int recipientImageHeight)
    {
      int watermarkWidth = this.WatermarkImageWidth;
      int watermarkHeight = this.WatermarkImageHeight;

      if (this.ImageWidthPercent > 0)
      {
        // We need to resize the watermark image so that its width takes up the specified percentage of
        // the overall width of the recipient image.
        int resizedWatermarkWidth = (int)(recipientImageWidth * (((float)this.ImageWidthPercent) / 100));
        int resizedWatermarkHeight = (resizedWatermarkWidth * watermarkHeight) / watermarkWidth;

        // If the resized height is taller than the recipient image, then readjust the width and height
        // to make the watermark as tall as the recipient image.
        if (resizedWatermarkHeight > recipientImageHeight)
        {
          resizedWatermarkHeight = recipientImageHeight;
          resizedWatermarkWidth = (watermarkWidth * resizedWatermarkHeight) / watermarkHeight;
        }

        // Get the resized image and assign the width and height vars.
        return ImageHelper.CreateResizedBitmap(this.WatermarkImage, watermarkWidth, watermarkHeight, resizedWatermarkWidth, resizedWatermarkHeight);
      }
      else
      {
        return this.WatermarkImage;
      }
    }

    private ImageAttributes GetWatermarkImageAttributes(int imageOpacityPercent)
    {
      // Change the opacity of the watermark.  This is done by applying a 5x5 matrix that contains the 
      // coordinates for the RGBA space.  Set the 3rd row and 3rd column to the desired opacity. (0.0 - 1.0).

      if (_imageAttributes != null)
        return _imageAttributes;

      float opacity = imageOpacityPercent / 100.0f;
      float[][] colorMatrixElements = {
        new float[] {1.0f,  0.0f,  0.0f,  0.0f, 0.0f},
        new float[] {0.0f,  1.0f,  0.0f,  0.0f, 0.0f},
        new float[] {0.0f,  0.0f,  1.0f,  0.0f, 0.0f},
        new float[] {0.0f,  0.0f,  0.0f,  opacity, 0.0f},
        new float[] {0.0f,  0.0f,  0.0f,  0.0f, 1.0f}};
      ColorMatrix wmColorMatrix = new ColorMatrix(colorMatrixElements);

      _imageAttributes = new ImageAttributes();
      _imageAttributes.SetColorMatrix(wmColorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Default);
      _imageAttributes.SetColorKey(Color.Transparent, Color.Transparent);

      return _imageAttributes;
    }

    private void ApplyTextWatermark(System.Drawing.Image img)
    {
      if (String.IsNullOrEmpty(this.WatermarkText))
        return;

      int opacity = (int)((255.0f * this._textOpacityPercent) / 100.0f);
      int recipientImageWidth = img.Width;
      int recipientImageHeight = img.Height;

      Font font = null;
      Graphics gr = null;
      try
      {
        gr = Graphics.FromImage(img);

        #region Generate font

        if (TextWidthPercent == 0)
        {
          // We want to use the TextHeightPixels property to set the size.
          int fontSize = (this.TextHeightPixels == 0 ? DEFAULT_TEXT_HEIGHT_PIXELS : this.TextHeightPixels);
          font = new Font(this.TextFontName, fontSize);
        }
        else
        {
          // We have a value for TextWidthPercent, which means we want to create a font/size combination
          // whose width takes up the specified percentage across the recipient image.
          int fontSize = MIN_FONT_HEIGHT_PIXELS;
          float maxTextWidth = recipientImageWidth * (TextWidthPercent / 100.0f);
          font = new Font(this.TextFontName, fontSize);

          // Starting with the default minimum font size, keep increasing it until we reach the desired width. Note that
          // we may end up with a font height taller than the image. An early version of this routine limited the font height 
          // to no larger than the recipient image height, but that resulted in undesirable empty space above and below the 
          // text, since the measured height includes space for all characters in the character set. This created the 
          // impression that the character was not really as tall as the recipient image. 'tis better to let the watermark text be
          // taller than the image in certain circumstances - the user can always reduce the TextWidthPercent until the 
          // desired height is achieved.
          while (gr.MeasureString(this.WatermarkText, font).Width < maxTextWidth)
          {
            fontSize += 1;
            font.Dispose();
            font = new Font(this.TextFontName, fontSize);
          }

          // At this point the font size is one larger than it should be. Reduce it and create the final font object.
          fontSize -= 1;
          font.Dispose();
          font = new Font(this.TextFontName, fontSize);
        }

        #endregion

        SizeF watermarkSize = gr.MeasureString(this.WatermarkText, font);

        // Turn off the border if the watermark text is too big to allow for it.
        float borderPercent = BORDER_PERCENT;
        if ((watermarkSize.Height > (recipientImageHeight - (recipientImageHeight * borderPercent))) ||
            (watermarkSize.Width > (recipientImageWidth - (recipientImageWidth * borderPercent))))
        {
          borderPercent = 0;
        }

        Point textStartingPoint = GetWatermarkStartingPoint(watermarkSize.Width, watermarkSize.Height, (float)recipientImageWidth, (float)recipientImageHeight, this.TextLocation, borderPercent);

        using (Brush semitransparentBrush = new SolidBrush(Color.FromArgb(opacity, this.TextColor)))
        {
          gr.DrawString(this.WatermarkText, font, semitransparentBrush, textStartingPoint);
        }
      }
      finally
      {
        font?.Dispose();
        gr?.Dispose();
      }
    }

    private static Point GetWatermarkStartingPoint(float watermarkWidth, float watermarkHeight, float imageWidth, float imageHeight, ContentAlignment watermarkLocation, float borderPercent)
    {
      Point startingPoint = new Point();
      switch (watermarkLocation)
      {
        case ContentAlignment.TopLeft:
          startingPoint.X = (int)(imageWidth * borderPercent);
          startingPoint.Y = (int)(imageHeight * borderPercent);
          break;
        case ContentAlignment.TopCenter:
          startingPoint.X = (int)(imageWidth - watermarkWidth) / 2;
          startingPoint.Y = (int)(imageHeight * borderPercent);
          break;
        case ContentAlignment.TopRight:
          startingPoint.X = (int)(imageWidth - watermarkWidth - (imageWidth * borderPercent));
          startingPoint.Y = (int)(imageHeight * borderPercent);
          break;
        case ContentAlignment.MiddleLeft:
          startingPoint.X = (int)(imageWidth * borderPercent);
          startingPoint.Y = (int)(imageHeight - watermarkHeight) / 2;
          break;
        case ContentAlignment.MiddleCenter:
          startingPoint.X = (int)(imageWidth - watermarkWidth) / 2;
          startingPoint.Y = (int)(imageHeight - watermarkHeight) / 2;
          break;
        case ContentAlignment.MiddleRight:
          startingPoint.X = (int)(imageWidth - watermarkWidth - (imageWidth * borderPercent));
          startingPoint.Y = (int)(imageHeight - watermarkHeight) / 2;
          break;
        case ContentAlignment.BottomLeft:
          startingPoint.X = (int)(imageWidth * borderPercent);
          startingPoint.Y = (int)(imageHeight - watermarkHeight - (imageHeight * borderPercent));
          break;
        case ContentAlignment.BottomCenter:
          startingPoint.X = (int)(imageWidth - watermarkWidth) / 2;
          startingPoint.Y = (int)(imageHeight - watermarkHeight - (imageHeight * borderPercent));
          break;
        case ContentAlignment.BottomRight:
          startingPoint.X = (int)(imageWidth - watermarkWidth - (imageWidth * borderPercent));
          startingPoint.Y = (int)(imageHeight - watermarkHeight - (imageHeight * borderPercent));
          break;
        default:
          startingPoint.X = (int)(imageWidth * borderPercent);
          startingPoint.Y = (int)(imageHeight * borderPercent);
          break;
      }

      return startingPoint;
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!this._hasBeenDisposed)
      {
        // Dispose of resources held by this instance.
        lock (_sharedLock)
        {
          this._imageAttributes?.Dispose();
        }

        this._watermarkImage?.Dispose();

        // Set the sentinel.
        this._hasBeenDisposed = true;
      }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    #endregion
  }
}
