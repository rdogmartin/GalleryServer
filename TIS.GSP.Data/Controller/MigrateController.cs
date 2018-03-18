using System;
using System.Globalization;
using System.Linq;
using GalleryServer.Business;
using GalleryServer.Business.Metadata;

namespace GalleryServer.Data
{
  /// <summary>
  /// Handle the migration of data changes during updates to newer versions. 
  /// </summary>
  public static class MigrateController
  {
    #region Methods

    /// <summary>
    /// Update database values as required for the current version. Typically this is used to apply bug fixes
    /// that require updates to database settings (such as media and UI templates).
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    /// <param name="galleryDataStore">The type of database used for the gallery data.</param>
    /// <remarks>This function detects the current app schema version as defined in the AppSetting table and applies
    /// all relevant updates to bring it up to the current version. By the time this method exits, the app schema version
    /// in the AppSetting table will match the current schema version as defined in <see cref="GalleryDb.DataSchemaVersion" />.
    /// </remarks>
    /// <exception cref="System.Exception"></exception>
    public static void ApplyDbUpdates(GalleryDb ctx, ProviderDataStore galleryDataStore)
    {
      if (!ctx.AppSettings.Any())
      {
        SeedController.InsertSeedData(ctx);
      }

      var curSchema = GetCurrentSchema(ctx);

      while (curSchema < GalleryDb.DataSchemaVersion)
      {
        var oldSchema = curSchema;

        switch (curSchema)
        {
          case GalleryDataSchemaVersion.V3_0_0: UpgradeTo301(ctx); break;
          case GalleryDataSchemaVersion.V3_0_1: UpgradeTo302(ctx); break;
          case GalleryDataSchemaVersion.V3_0_2: UpgradeTo303(ctx); break;
          case GalleryDataSchemaVersion.V3_0_3: UpgradeTo310(ctx); break;
          case GalleryDataSchemaVersion.V3_1_0: UpgradeTo320(ctx); break;
          case GalleryDataSchemaVersion.V3_2_0: UpgradeTo321(ctx); break;
          case GalleryDataSchemaVersion.V3_2_1: UpgradeTo400(ctx, galleryDataStore); break;
          case GalleryDataSchemaVersion.V4_0_0: UpgradeTo401(ctx); break;
          case GalleryDataSchemaVersion.V4_0_1: UpgradeTo410(ctx); break;
          case GalleryDataSchemaVersion.V4_1_0: UpgradeTo420(ctx); break;
          case GalleryDataSchemaVersion.V4_2_0: UpgradeTo421(ctx); break;
          case GalleryDataSchemaVersion.V4_2_1: UpgradeTo430(ctx); break;
          case GalleryDataSchemaVersion.V4_3_0: UpgradeTo440(ctx); break;
          case GalleryDataSchemaVersion.V4_4_0: UpgradeTo441(ctx); break;
          case GalleryDataSchemaVersion.V4_4_1: UpgradeTo442(ctx); break;
        }

        curSchema = GetCurrentSchema(ctx);

        if (curSchema == oldSchema)
        {
          throw new Exception(String.Format("The migration function for schema {0} should have incremented the data schema version in the AppSetting table, but it did not.", curSchema));
        }
      }
    }

    /// <summary>
    /// Upgrades the <paramref name="ds" /> to match the current schema of the application. This involves dropping any columns that are no
    /// longer used and making any data changes that can't be handled by the normal migrations algorithm.
    /// </summary>
    /// <param name="ds">The ds.</param>
    public static void UpgradeToCurrentSchema(System.Data.DataSet ds)
    {
      // Drop MediaObject HashKey column (removed in 3.1.0)
      if (ds.Tables.Contains("MediaObject") && ds.Tables["MediaObject"].Columns.Contains("HashKey"))
      {
        ds.Tables["MediaObject"].Columns.Remove("HashKey");
      }

      // Drop Album DateStart and DateEnd columns (removed in 4.0.0)
      if (ds.Tables.Contains("Album"))
      {
        if (ds.Tables["Album"].Columns.Contains("DateStart"))
        {
          ds.Tables["Album"].Columns.Remove("DateStart");
        }

        if (ds.Tables["Album"].Columns.Contains("DateEnd"))
        {
          ds.Tables["Album"].Columns.Remove("DateEnd");
        }
      }

      // Change Applications.ApplicationName to "Gallery Server" (changed in 4.0.0)
      if (ds.Tables.Contains("Applications"))
      {
        foreach (var r in ds.Tables["Applications"].Select("ApplicationName = 'Gallery Server Pro '"))
        {
          r["ApplicationName"] = "Gallery Server";
        }
      }
    }

    #endregion

    #region Functions

    /// <summary>
    /// Returns the current data schema version as defined in the AppSetting table.
    /// </summary>
    /// <returns>An instance of <see cref="GalleryDataSchemaVersion" /> indicating the current data schema version
    /// as defined in the AppSetting table.</returns>
    private static GalleryDataSchemaVersion GetCurrentSchema(GalleryDb ctx)
    {
      return GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToEnum(ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion").SettingValue);
    }

    /// <summary>
    /// Upgrades the 3.0.0 data to the 3.0.1 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    private static void UpgradeTo301(GalleryDb ctx)
    {
      // Bug 547: Change jQuery 1.10.0 to 1.10.1 (the migration code for 2.6 => 3.0 mistakenly specified 1.10.0 instead of 1.10.1)
      var appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//ajax.googleapis.com/ajax/libs/jquery/1.10.0/jquery.min.js")
      {
        appSetting.SettingValue = "//ajax.googleapis.com/ajax/libs/jquery/1.10.1/jquery.min.js";
      }

      // Bug 570: Change "DateAdded" to "Date Added"
      foreach (var metaDef in ctx.GallerySettings.Where(gs => gs.SettingName == "MetadataDisplaySettings"))
      {
        // Serialized values are separated by apostrophes when first inserted; they are replaced by quotes by the JSON serializer when subsequently
        // saved, so we check for both.
        metaDef.SettingValue = metaDef.SettingValue.Replace(@"""MetadataItem"":111,""Name"":""DateAdded"",""DisplayName"":""DateAdded""", @"""MetadataItem"":111,""Name"":""DateAdded"",""DisplayName"":""Date Added""");
        metaDef.SettingValue = metaDef.SettingValue.Replace(@"'MetadataItem':111,'Name':'DateAdded','DisplayName':'DateAdded'", @"'MetadataItem':111,'Name':'DateAdded','DisplayName':'Date Added'");
      }

      // Bug 578: Change MP4 encoder setting
      foreach (var metaDef in ctx.GallerySettings.Where(gs => gs.SettingName == "MediaEncoderSettings"))
      {
        metaDef.SettingValue = metaDef.SettingValue.Replace(@"-s {Width}x{Height} -b:v 384k", @"-vf ""scale=min(iw*min(640/iw\,480/ih)\,iw):min(ih*min(640/iw\,480/ih)\,ih)"" -b:v 384k");
      }

      // Bug 554: (a) Fix M4V templates
      var mimeType = ctx.MimeTypes.FirstOrDefault(mt => mt.FileExtension == ".m4v" && mt.MimeTypeValue == "video/x-m4v");
      if (mimeType != null)
      {
        mimeType.MimeTypeValue = "video/m4v";
      }

      // Bug 554: (b) Delete x-m4v / safari template
      var mediaTmpl = ctx.MediaTemplates.FirstOrDefault(mt => mt.MimeType == "video/x-m4v" && mt.BrowserId == "safari");
      if (mediaTmpl != null)
      {
        ctx.MediaTemplates.Remove(mediaTmpl);
      }

      // Bug 554: (c) Delete existing m4v templates
      foreach (var tmpl in ctx.MediaTemplates.Where(mt => mt.MimeType == "video/m4v"))
      {
        ctx.MediaTemplates.Remove(tmpl);
      }

      // Bug 554: (d) Add m4v templates based on the mp4 ones
      foreach (var tmpl in ctx.MediaTemplates.Where(mt => mt.MimeType == "video/mp4"))
      {
        ctx.MediaTemplates.Add(new MediaTemplateDto()
        {
          MimeType = "video/m4v",
          BrowserId = tmpl.BrowserId,
          HtmlTemplate = tmpl.HtmlTemplate,
          ScriptTemplate = tmpl.ScriptTemplate
        });
      }

      // Bug 555: (a) Add MP3 template for IE1TO8 (copy it from Firefox, which uses Silverlight)
      var mp3MediaTmpl = ctx.MediaTemplates.FirstOrDefault(mt => mt.MimeType == "audio/x-mp3" && mt.BrowserId == "firefox");
      if (mp3MediaTmpl != null && (!ctx.MediaTemplates.Any(mt => mt.MimeType == mp3MediaTmpl.MimeType && mt.BrowserId == "ie1to8")))
      {
        ctx.MediaTemplates.Add(new MediaTemplateDto()
        {
          MimeType = mp3MediaTmpl.MimeType,
          BrowserId = "ie1to8",
          HtmlTemplate = mp3MediaTmpl.HtmlTemplate,
          ScriptTemplate = mp3MediaTmpl.ScriptTemplate
        });

      }

      // Bug 555: (b) Delete MP3 template for Safari
      mp3MediaTmpl = ctx.MediaTemplates.FirstOrDefault(mt => mt.MimeType == "audio/x-mp3" && mt.BrowserId == "safari");
      if (mp3MediaTmpl != null)
      {
        ctx.MediaTemplates.Remove(mp3MediaTmpl);
      }

      // Bug 564: (a) Change MIME type of .qt and .moov files from video/quicktime to video/mp4
      foreach (var qtMimeType in ctx.MimeTypes.Where(mt => new[] { ".qt", ".moov" }.Contains(mt.FileExtension) && mt.MimeTypeValue == "video/quicktime"))
      {
        qtMimeType.MimeTypeValue = "video/mp4";
      }

      // Bug 564: (b) Delete video/quicktime safari template
      foreach (var qtMediaTmpl in ctx.MediaTemplates.Where(mt => mt.MimeType == "video/quicktime" && mt.BrowserId == "safari"))
      {
        ctx.MediaTemplates.Remove(qtMediaTmpl);
      }

      // Bug 562: Add PDF template for Safari. It looks mostly like the IE one except we have to clear the iframe src before we can hide it.

      const string pdfScriptTmplSafari = @"// IE and Safari render Adobe Reader iframes on top of jQuery UI dialogs, so add event handler to hide frame while dialog is visible
// Safari requires that we clear the iframe src before we can hide it
$('.gsp_mo_share_dlg').on('dialogopen', function() {
 $('#{UniqueId}_frame').attr('src', '').css('visibility', 'hidden');
}).on('dialogclose', function() {
$('#{UniqueId}_frame').attr('src', '{MediaObjectUrl}').css('visibility', 'visible');
});";

      if (!ctx.MediaTemplates.Any(mt => mt.MimeType == "application/pdf" && mt.BrowserId == "safari"))
      {
        ctx.MediaTemplates.Add(new MediaTemplateDto()
        {
          MimeType = "application/pdf",
          BrowserId = "safari",
          HtmlTemplate = "<p><a href='{MediaObjectUrl}'>Enlarge PDF to fit browser window</a></p><iframe id='{UniqueId}_frame' src='{MediaObjectUrl}' frameborder='0' style='width:680px;height:600px;border:1px solid #000;'></iframe>",
          ScriptTemplate = pdfScriptTmplSafari
        });
      }

      // Bug 580: Need to use AddMediaObject permission instead of EditAlbum permission when deciding whether to render the Add link in an empty album.
      // Task 579: Change lock icon tooltip when anonymous access is disabled
      // Task 575: Update signature of jsRender helper functions getAlbumUrl, getGalleryItemUrl, getDownloadUrl, currentUrl
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.Album))
      {
        const string srcText580 = "{{if Album.Permissions.EditAlbum}}<a href='{{: ~getAddUrl(#data) }}'>{{:Resource.AbmAddObj}}</a>{{/if}}";
        const string replText580 = "{{if Album.Permissions.AddMediaObject}}<a href='{{: ~getAddUrl(#data) }}'>{{:Resource.AbmAddObj}}</a>{{/if}}";

        const string srcText579 = "<img src='{{:App.SkinPath}}/images/lock-{{if Album.IsPrivate}}active-{{/if}}s.png' title='{{if Album.IsPrivate}}{{:Resource.AbmIsPvtTt}}{{else}}{{:Resource.AbmNotPvtTt}}{{/if}}' alt=''>";
        const string replText579 = "<img src='{{:App.SkinPath}}/images/lock-{{if Album.IsPrivate || !Settings.AllowAnonBrowsing}}active-{{/if}}s.png' title='{{if !Settings.AllowAnonBrowsing}}{{:Resource.AbmAnonDisabledTt}}{{else}}{{if Album.IsPrivate}}{{:Resource.AbmIsPvtTt}}{{else}}{{:Resource.AbmNotPvtTt}}{{/if}}{{/if}}' alt=''>";

        const string srcText575a = "~getAlbumUrl(#data)";
        const string replText575a = "~getAlbumUrl(Album.Id, true)";

        const string srcText575b = "~getGalleryItemUrl(#data)";
        const string replText575b = "~getGalleryItemUrl(#data, !IsAlbum)";

        const string srcText575c = "~getDownloadUrl(#data)";
        const string replText575c = "~getDownloadUrl(Album.Id)";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText580, replText580);
        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText579, replText579);
        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText575a, replText575a);
        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText575b, replText575b);
        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText575c, replText575c);
      }

      // Task 575: Update signature of jsRender helper function getDownloadUrl
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.MediaObject))
      {
        const string srcText575c = "~getDownloadUrl()";
        const string replText575c = "~getDownloadUrl(Album.Id)";

        const string srcText575d = "~currentUrl()";
        const string replText575d = "~getMediaUrl(MediaItem.Id, true)";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText575c, replText575c);
        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText575d, replText575d);
      }

      // Update data schema version to 3.0.1
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "3.0.1";

      ctx.SaveChanges();
    }

    /// <summary>
    /// Upgrades the 3.0.1 data to the 3.0.2 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    private static void UpgradeTo302(GalleryDb ctx)
    {
      // Bug 625: Search results do not allow downloading original file
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.MediaObject))
      {
        const string srcText = "{{if Settings.AllowOriginalDownload}}";
        const string replText = "{{if Album.Permissions.ViewOriginalMediaObject}}";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
      }

      // Update data schema version to 3.0.2
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "3.0.2";

      ctx.SaveChanges();
    }

    /// <summary>
    /// Upgrades the 3.0.2 data to the 3.0.3 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    private static void UpgradeTo303(GalleryDb ctx)
    {
      // Fix bug# 632: Error "Cannot find skin path"
      // Change skin name from "Dark" to "dark".
      var asSkin = ctx.AppSettings.First(a => a.SettingName == "Skin");
      if (asSkin.SettingValue == "Dark")
      {
        asSkin.SettingValue = "dark";
      }

      // Fix bug# 633: Upgrading from 2.6 may result in duplicate sets of tags
      // Delete any duplicate "tag" metadata rows
      // FYI: We need the ToList() to avoid this error in SQL CE: The ntext and image data types cannot be used in WHERE, HAVING, GROUP BY, ON, or IN clauses, except when these data types are used with the LIKE or IS NULL predicates.
      var dupMetaTags = ctx.Metadatas
        .Where(m => m.MetaName == MetadataItemName.Tags)
        .GroupBy(m => m.FKMediaObjectId).Where(m => m.Count() > 1)
        .ToList()
        .Select(m => m.Where(t => t.Value == String.Empty))
        .Select(m => m.FirstOrDefault());

      foreach (var dupMetaTag in dupMetaTags)
      {
        ctx.Metadatas.Remove(dupMetaTag);
      }

      // Update data schema version to 3.0.3
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "3.0.3";

      ctx.SaveChanges();
    }

    /// <summary>
    /// Upgrades the 3.0.3 data to the 3.1.0 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    private static void UpgradeTo310(GalleryDb ctx)
    {
      // Insert Orientation meta item into metadata definitions just before the ExposureProgram item.
      foreach (var metaDef in ctx.GallerySettings.Where(gs => gs.SettingName == "MetadataDisplaySettings"))
      {
        // First grab the sequence of ExposureProgram, then subtract one.
        // Matches: 'MetadataItem':14{ANY_TEXT}'Sequence':{ANY_DIGITS} => {ANY_DIGITS} is assigned to seq group name
        var match = System.Text.RegularExpressions.Regex.Match(metaDef.SettingValue, @"['""]MetadataItem['""]:14.+?['""]Sequence['""]:(?<seq>\d+)");

        var sequence = 12; // Default to 12 if we don't find one, which is correct if the user hasn't modified the original order
        if (match.Success)
        {
          sequence = Convert.ToInt32(match.Groups["seq"].Value, CultureInfo.InvariantCulture) - 1;
        }

        // Serialized values are separated by apostrophes when first inserted; they are replaced by quotes by the JSON serializer when subsequently
        // saved, so we check for both. Look for the beginning of the ExposureProgram item and insert the orientation item just before it.
        if (!metaDef.SettingValue.Contains(@"""MetadataItem"":43") && !metaDef.SettingValue.Contains(@"'MetadataItem':43"))
        {
          metaDef.SettingValue = metaDef.SettingValue.Replace(@"{""MetadataItem"":14", String.Concat(@"{""MetadataItem"":43,""Name"":""Orientation"",""DisplayName"":""Orientation"",""IsVisibleForAlbum"":false,""IsVisibleForGalleryObject"":true,""IsEditable"":false,""DefaultValue"":""{Orientation}"",""Sequence"":", sequence, @"},{""MetadataItem"":14"));
          metaDef.SettingValue = metaDef.SettingValue.Replace(@"{'MetadataItem':14", String.Concat(@"{'MetadataItem':43,'Name':'Orientation','DisplayName':'Orientation','IsVisibleForAlbum':false,'IsVisibleForGalleryObject':true,'IsEditable':false,'DefaultValue':'{Orientation}','Sequence':", sequence, @"},{'MetadataItem':14"));
        }
      }

      // Task 611 & 645: Update MP4 encoder setting to (1) perform higher quality transcoding (2) auto-rotate videos (3) remove orientation flag
      const string mp4Setting303 = @"-y -i ""{SourceFilePath}"" -vf ""scale=min(iw*min(640/iw\,480/ih)\,iw):min(ih*min(640/iw\,480/ih)\,ih)"" -b:v 384k -vcodec libx264 -flags +loop+mv4 -cmp 256 -partitions +parti4x4+parti8x8+partp4x4+partp8x8 -subq 6 -trellis 0 -refs 5 -bf 0 -coder 0 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 40 -i_qfactor 0.71 -qmin 10 -qmax 51 -qdiff 4 -ac 1 -ar 16000 -r 13 -ab 32000 -movflags +faststart ""{DestinationFilePath}""";
      const string mp4Setting310 = @"-y -i ""{SourceFilePath}"" -vf ""scale=min(iw*min(640/iw\,480/ih)\,iw):min(ih*min(640/iw\,480/ih)\,ih){AutoRotateFilter}"" -vcodec libx264 -movflags +faststart -metadata:s:v:0 rotate=0 ""{DestinationFilePath}""";
      foreach (var metaDef in ctx.GallerySettings.Where(gs => gs.SettingName == "MediaEncoderSettings"))
      {
        metaDef.SettingValue = metaDef.SettingValue.Replace(mp4Setting303, mp4Setting310);
      }

      // Task 664: Change jQuery 1.10.1 to 1.10.2
      var appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//ajax.googleapis.com/ajax/libs/jquery/1.10.1/jquery.min.js")
      {
        appSetting.SettingValue = "//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js";
      }

      // Task 649: Add .3GP file support
      if (!ctx.MimeTypes.Any(mt => mt.FileExtension == ".3gp"))
      {
        ctx.MimeTypes.Add(new MimeTypeDto
        {
          FileExtension = ".3gp",
          MimeTypeValue = "video/mp4",
          BrowserMimeTypeValue = ""
        });
      }

      // Update data schema version to 3.1.0
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "3.1.0";

      ctx.SaveChanges();
    }

    /// <summary>
    /// Upgrades the 3.1.0 data to the 3.2.0 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    private static void UpgradeTo320(GalleryDb ctx)
    {
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.Album))
      {
        // Task 669: Allow manual sorting of media objects
        // Step 1: Add the Custom option to the sort by dropdown
        var srcText = @"<li class='gsp_abm_sum_sbi_hdr'>{{:Resource.AbmSortbyTt}}</li>";

        var replText = @"<li class='gsp_abm_sum_sbi_hdr'>{{:Resource.AbmSortbyTt}}</li>
{{if Album.VirtualType == 1 && Album.Permissions.EditAlbum}}<li><a href='#' data-id='-2147483648'>{{:Resource.AbmSortbyCustom}}</a></li>{{/if}}";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);

        // Step 2: Convert the thumbnails into a list of <li> tags instead of <div>'s. Required for jQuery UI Sortable.
        srcText = @"<div class='gsp_floatcontainer'>
 {{for Album.GalleryItems}}
 <div class='thmb{{if IsAlbum}} album{{/if}}' data-id='{{:Id}}' data-it='{{:ItemType}}'>
  <a class='gsp_thmbLink' href='{{: ~getGalleryItemUrl(#data, !IsAlbum) }}'>
   <img class='gsp_thmb_img' style='width:{{:Views[ViewIndex].Width}}px;height:{{:Views[ViewIndex].Height}}px;' src='{{:Views[ViewIndex].Url}}'>
  </a>
  <p class='gsp_go_t' style='width:{{:Views[ViewIndex].Width + 40}}px;' title='{{stripHtml:Title}}'>{{stripHtmlAndTruncate:Title}}</p>
 </div>
 {{/for}}
</div>";

        replText = @"<ul class='gsp_floatcontainer gsp_abm_thmbs'>
 {{for Album.GalleryItems}}
 <li class='thmb{{if IsAlbum}} album{{/if}}' data-id='{{:Id}}' data-it='{{:ItemType}}' style='width:{{:Views[ViewIndex].Width + 40}}px;'>
  <a class='gsp_thmbLink' href='{{: ~getGalleryItemUrl(#data, !IsAlbum) }}'>
   <img class='gsp_thmb_img' style='width:{{:Views[ViewIndex].Width}}px;height:{{:Views[ViewIndex].Height}}px;' src='{{:Views[ViewIndex].Url}}'>
  </a>
  <p class='gsp_go_t' title='{{stripHtml:Title}}'>{{stripHtmlAndTruncate:Title}}</p>
 </li>
 {{/for}}
</ul>";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);

        // Step 3: Update the JavaScript so that left pane is rendered regardless of whether the client is on a touchscreen device
        srcText = @"var isTouch = window.Gsp.isTouchScreen();
var renderLeftPane = !isTouch  || (isTouch && ($('.gsp_tb_s_CenterPane:visible, .gsp_tb_s_RightPane:visible').length == 0));

if (renderLeftPane ) {
 $('#{{:Settings.LeftPaneClientId}}').html( $.render [ '{{:Settings.LeftPaneTmplName}}' ]( window.{{:Settings.ClientId}}.gspData ));
";

        replText = @"var $lp = $('#{{:Settings.LeftPaneClientId}}');

if ($lp.length > 0) {
 $lp.html( $.render [ '{{:Settings.LeftPaneTmplName}}' ]( window.{{:Settings.ClientId}}.gspData ));
";

        uiTmpl.ScriptTemplate = uiTmpl.ScriptTemplate.Replace(srcText, replText);

        // Bug 709: Link to add objects page should not appear in empty virtual albums
        // Add a condition to the if statement so that Add link appears only for non-virtual albums
        srcText = @"{{if Album.Permissions.AddMediaObject}}<a href='{{: ~getAddUrl(#data) }}'>{{:Resource.AbmAddObj}}</a>{{/if}}";

        replText = @"{{if Album.VirtualType == 1 && Album.Permissions.AddMediaObject}}<a href='{{: ~getAddUrl(#data) }}'>{{:Resource.AbmAddObj}}</a>{{/if}}";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);

        // Task 722: Add RSS link to album template
        srcText = @"
  <span class='gsp_abm_sum_col1_row1_hdr'>{{:Resource.AbmPfx}}</span>";

        replText = @"
{{if Album.RssUrl}}
  <a class='gsp_abm_sum_btn' href='{{:Album.RssUrl}}'>
    <img src='{{:App.SkinPath}}/images/rss-s.png' title='{{:Resource.AbmRssTt}}' alt=''>
  </a>
{{/if}}
  <span class='gsp_abm_sum_col1_row1_hdr'>{{:Resource.AbmPfx}}</span>";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);

        // Task 730: Assign owner button should not be visible for virtual albums
        srcText = @"
{{if Album.Permissions.AdministerGallery}}
  <a class='gsp_abm_sum_ownr_trigger gsp_abm_sum_btn' href='#'>";

        replText = @"
{{if Album.VirtualType == 1 && Album.Permissions.AdministerGallery}}
  <a class='gsp_abm_sum_ownr_trigger gsp_abm_sum_btn' href='#'>";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
      }

      // Task 681: Merge My account button into username in header
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.Header))
      {
        // Step 1: Remove the HTML for the 'My account' icon
        var srcText = @"
  {{if Settings.AllowManageOwnAccount}}
  <div class='gsp_useroption'>
   <a class='gsp_myaccountlink' href='{{:App.CurrentPageUrl}}?g=myaccount&aid={{:Album.Id}}'>
    <img class='gsp_myaccount-user-icon' title='{{:Resource.HdrMyAccountTt}}' alt='{{:Resource.HdrMyAccountTt}}' src='{{:App.SkinPath}}/images/user-l.png'>
   </a></div>
   {{/if}}";

        var replText = "";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);

        // Step 2: Replace the username with a hyperlinked username that takes user to 'My account' page,
        // except when AllowManageOwnAccount=false, in which case just display the username like before.
        srcText = @"	<span id='{{:Settings.ClientId}}_userwelcome' class='gsp_welcome'>{{:User.UserName}}</span></div>";

        replText = @"{{if Settings.AllowManageOwnAccount}}
    <a id='{{:Settings.ClientId}}_userwelcome' href='{{:App.CurrentPageUrl}}?g=myaccount&aid={{:Album.Id}}' class='gsp_welcome' title='{{:Resource.HdrMyAccountTt}}'>{{:User.UserName}}</a>
   {{else}}
    <span id='{{:Settings.ClientId}}_userwelcome' class='gsp_welcome'>{{:User.UserName}}</span>
   {{/if}}
   </div>";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
      }

      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.LeftPane))
      {
        // Task 675: Add RECENTLY ADDED and TOP RATED links to left pane
        const string srcText = @"<div id='{{:Settings.ClientId}}_lptv'></div>";

        const string replText = @"<div id='{{:Settings.ClientId}}_lptv' class='gsp_lpalbumtree'></div>
{{if App.LatestUrl}}<p class='gsp_lplatest'><a href='{{:App.LatestUrl}}' class='jstree-anchor'><i class='jstree-icon'></i>{{:Resource.LpRecent}}</a></p>{{/if}}
{{if App.TopRatedUrl}}<p class='gsp_lptoprated'><a href='{{:App.TopRatedUrl}}' class='jstree-anchor'><i class='jstree-icon'></i>{{:Resource.LpTopRated}}</a></p>{{/if}}";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
      }

      // Task 727: Update jQuery 1.10.2 to 1.11.1. Update jQuery UI 1.10.3 to 1.10.4.
      var appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js")
      {
        appSetting.SettingValue = "//ajax.googleapis.com/ajax/libs/jquery/1.11.1/jquery.min.js";
      }

      appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryUiScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//ajax.googleapis.com/ajax/libs/jqueryui/1.10.3/jquery-ui.min.js")
      {
        appSetting.SettingValue = "//ajax.googleapis.com/ajax/libs/jqueryui/1.10.4/jquery-ui.min.js";
      }

      // Task 722: Now that {MediaObjectUrl} is an absolute URL, remove any instances of {HostUrl} that precede it.
      // Under default settings this only affects the HTML portion of two video/divx records, but we're making it more 
      // generic to catch any customizations an admin may have done.
      foreach (var tmpl in ctx.MediaTemplates.Where(mt => mt.HtmlTemplate.Contains("{HostUrl}{MediaObjectUrl}") || mt.ScriptTemplate.Contains("{HostUrl}{MediaObjectUrl}")))
      {
        tmpl.HtmlTemplate = tmpl.HtmlTemplate.Replace("{HostUrl}{MediaObjectUrl}", "{MediaObjectUrl}");
        tmpl.ScriptTemplate = tmpl.ScriptTemplate.Replace("{HostUrl}{MediaObjectUrl}", "{MediaObjectUrl}");
      }

      // Update data schema version to 3.2.0
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "3.2.0";

      ctx.SaveChanges();

      // Task 674: Add/update Enterprise UI templates introduced in 3.2.0
      // [2016-02-17] The templates have been upgraded for 4.0, so this no longer applies. Skip it. The 4.0 migration code will insert them.
      //var pkRow = ctx.AppSettings.Single(a => a.SettingName == "ProductKey");

      //if (pkRow.SettingValue == Constants.LicenseKeyEnterprise)
      //{
      //  SeedController.InsertAdditionalUiTemplates(ctx);
      //}
    }

    /// <summary>
    /// Upgrades the 3.2.0 data to the 3.2.1 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    private static void UpgradeTo321(GalleryDb ctx)
    {
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.LeftPane))
      {
        // Task 671: Improve touchscreen support.
        // NOTE: This should have been in the 3.2.0 upgrade, but it was forgotten.
        const string srcText = @"// Render the left pane, but not for touchscreens UNLESS the left pane is the only visible pane
var isTouch = window.Gsp.isTouchScreen();
var renderLeftPane = !isTouch  || (isTouch && ($('.gsp_tb_s_CenterPane:visible, .gsp_tb_s_RightPane:visible').length == 0));

if (renderLeftPane ) {
 $('#{{:Settings.LeftPaneClientId}}').html( $.render [ '{{:Settings.LeftPaneTmplName}}' ]( window.{{:Settings.ClientId}}.gspData ));";

        const string replText = @"// Render the left pane if it exists
var $lp = $('#{{:Settings.LeftPaneClientId}}');

if ($lp.length > 0) {
 $lp.html( $.render [ '{{:Settings.LeftPaneTmplName}}' ]( window.{{:Settings.ClientId}}.gspData ));";

        uiTmpl.ScriptTemplate = uiTmpl.ScriptTemplate.Replace(srcText, replText);
      }

      // Update data schema version to 3.2.1
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "3.2.1";

      ctx.SaveChanges();
    }

    /// <summary>
    /// Upgrades the 3.2.1 data to the 4.0.0 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    /// <param name="galleryDataStore">The type of database used for the gallery data.</param>
    private static void UpgradeTo400(GalleryDb ctx, ProviderDataStore galleryDataStore)
    {
      Migrate40Controller.UpgradeTo400(ctx, galleryDataStore);
    }

    /// <summary>
    /// Upgrades the 4.0.0 data to the 4.0.1 data. Applies to data such as app settings, gallery settings, templates, etc.
    /// Does not contain data structure changes such as new columns.
    /// </summary>
    /// <param name="ctx">Context to be used for updating data.</param>
    private static void UpgradeTo401(GalleryDb ctx)
    {
      // Fix for bug 190: Allow user-entered HTML so that HTML extracted from metadata is preserved
      foreach (var gallerySetting in ctx.GallerySettings.Where(a => a.SettingName == "AllowUserEnteredHtml"))
      {
        gallerySetting.SettingValue = "True";
      }

      // Fix for bug 191 where encoder settings were reversed when the settings were saved on the Video & Audio page
      // Look for evidence of reversed default settings, then reverse back to original.
      const string reversedEncoderSettings = @"*audio||.m4a||-i ""{SourceFilePath}"" -y ""{DestinationFilePath}""~~*video||.flv||-i ""{SourceFilePath}"" -y ""{DestinationFilePath}""~~*video||.mp4||-y -i ""{SourceFilePath}"" -vf ""scale=min(iw*min(640/iw\,480/ih)\,iw):min(ih*min(640/iw\,480/ih)\,ih){AutoRotateFilter}"" -vcodec libx264 -movflags +faststart -metadata:s:v:0 rotate=0 ""{DestinationFilePath}""~~.m4a||.m4a||~~.flv||.flv||~~.mp3||.mp3||";
      const string correctEncoderSettings = @".mp3||.mp3||~~.flv||.flv||~~.m4a||.m4a||~~*video||.mp4||-y -i ""{SourceFilePath}"" -vf ""scale=trunc(min(iw*min(640/iw\,480/ih)\,iw)/2)*2:trunc(min(ih*min(640/iw\,480/ih)\,ih)/2)*2{AutoRotateFilter}"" -vcodec libx264 -movflags +faststart -metadata:s:v:0 rotate=0 ""{DestinationFilePath}""~~*video||.flv||-i ""{SourceFilePath}"" -y ""{DestinationFilePath}""~~*audio||.m4a||-i ""{SourceFilePath}"" -y ""{DestinationFilePath}""";

      foreach (var metaDef in ctx.GallerySettings.Where(gs => gs.SettingName == "MediaEncoderSettings"))
      {
        if (metaDef.SettingValue.Trim() == reversedEncoderSettings)
        {
          metaDef.SettingValue = correctEncoderSettings;
        }
      }

      // Fix for bug 137: "width not divisible by 2" or "height not divisible by 2" error when transcoding video
      // This was inadvertently omitted from 4.0.0.
      const string mp4Setting321 = @"scale=min(iw*min(640/iw\,480/ih)\,iw):min(ih*min(640/iw\,480/ih)\,ih)";
      const string mp4Setting401 = @"scale=trunc(min(iw*min(640/iw\,480/ih)\,iw)/2)*2:trunc(min(ih*min(640/iw\,480/ih)\,ih)/2)*2";
      foreach (var metaDef in ctx.GallerySettings.Where(gs => gs.SettingName == "MediaEncoderSettings"))
      {
        metaDef.SettingValue = metaDef.SettingValue.Replace(mp4Setting321, mp4Setting401);
      }

      // Update data schema version to 4.0.1
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.0.1";

      ctx.SaveChanges();
    }

    private static void UpgradeTo410(GalleryDb ctx)
    {
      // Bug 216: Tags containing apostrophes can cause invalid URLs
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.Album))
      {
        const string srcText = "{{: ~getGalleryItemUrl(#data, !IsAlbum) }}";
        const string replText = "{{> ~getGalleryItemUrl(#data, !IsAlbum) }}";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
      }

      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.MediaObject))
      {
        // NOTE: This should have been in the 3.2.0 upgrade, but it was forgotten.
        var srcText = "{{: ~prevUrl() }}";
        var replText = "{{> ~prevUrl() }}";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);

        srcText = "{{: ~nextUrl() }}";
        replText = "{{> ~nextUrl() }}";

        uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
      }

      // Update data schema version to 4.1.0
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.1.0";

      ctx.SaveChanges();
    }

    private static void UpgradeTo420(GalleryDb ctx)
    {
      // Feature 224: If a watermark image is specified, copy it to App_Data\Watermark_Images\{GalleryId} and change the setting
      // to include only the file name.
      var appPath = AppDomain.CurrentDomain.BaseDirectory.Substring(0, AppDomain.CurrentDomain.BaseDirectory.Length - 1).Replace("/", "\\");
      const string watermarkDirectory = "App_Data\\Watermark_Images";

      foreach (var gallerySetting in ctx.GallerySettings.Include("Gallery").Where(gs => gs.SettingName == "WatermarkImagePath"))
      {
        if (string.IsNullOrWhiteSpace(gallerySetting.SettingValue))
        {
          continue;
        }

        if (gallerySetting.Gallery.IsTemplate || gallerySetting.SettingValue == "gs/skins/dark/images/gs-logo.png" || gallerySetting.SettingValue == "gs/skins/dark/images/gsp-logo.png")
        {
          // When we encounter the default watermark path or it's the template gallery, set it to an empty string.
          gallerySetting.SettingValue = string.Empty;
          continue;
        }

        // It might be a full path, so first check for file existence. If not found, then assume it's relative to the web app.
        var sourceFilePath = gallerySetting.SettingValue;

        if (!System.IO.File.Exists(sourceFilePath))
        {
          sourceFilePath = System.IO.Path.Combine(appPath, gallerySetting.SettingValue.TrimStart('/', '\\').Replace("/", "\\"));
        }

        if (System.IO.File.Exists(sourceFilePath))
        {
          // We have a watermark image to move. Make sure destination directory exists and then copy it there.
          var watermarkDirContainer = System.IO.Path.Combine(appPath, watermarkDirectory); // e.g. "C:\Website\App_Data\Watermark_Images"
          var watermarkDir = System.IO.Path.Combine(watermarkDirContainer, gallerySetting.FKGalleryId.ToString()); // e.g. "C:\Website\App_Data\Watermark_Images\2"
          var fileName = System.IO.Path.GetFileName(sourceFilePath);

          if (!System.IO.Directory.Exists(watermarkDirContainer))
          {
            System.IO.Directory.CreateDirectory(watermarkDirContainer); // Create Watermark_Images directory in App_Data
          }

          if (!System.IO.Directory.Exists(watermarkDir))
          {
            System.IO.Directory.CreateDirectory(watermarkDir); // Create {GalleryId}} directory in App_Data\Watermark_Images
          }

          var destFilePath = System.IO.Path.Combine(watermarkDir, fileName); // e.g. "C:\Website\App_Data\Watermark_Images\2\gs-logo.png"
          System.IO.File.Copy(sourceFilePath, destFilePath);

          // Update the WatermarkImagePath setting. It now holds just the filename, not the path.
          gallerySetting.SettingValue = fileName;
        }
      }

      // Bug 222: Delete 3.X templates belonging to the template gallery when upgrading to 4.2.0.
      var tmplGalleryId = ctx.Galleries.Single(g => g.IsTemplate).GalleryId;

      foreach (var uiTemplateDto in ctx.UiTemplates.Where(ui => ui.FKGalleryId == tmplGalleryId))
      {
        if (uiTemplateDto.Name.EndsWith(" (3.2.1 version)", StringComparison.OrdinalIgnoreCase))
        {
          ctx.UiTemplates.Remove(uiTemplateDto);
        }
      }

      // Feature 225: Change jQuery 2.2.3 to 3.1.1
      var appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//code.jquery.com/jquery-2.2.3.min.js")
      {
        appSetting.SettingValue = "//code.jquery.com/jquery-3.1.1.min.js";
      }

      // Feature 225: Upgrade jQuery Migrate path
      appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryMigrateScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//code.jquery.com/jquery-migrate-1.3.0.min.js")
      {
        appSetting.SettingValue = "//code.jquery.com/jquery-migrate-3.0.0.min.js";
      }

      // Feature 225: Upgrade jQuery UI path
      appSetting = ctx.AppSettings.FirstOrDefault(a => a.SettingName == "JQueryUiScriptPath");

      if (appSetting != null && appSetting.SettingValue == "//ajax.googleapis.com/ajax/libs/jqueryui/1.10.4/jquery-ui.min.js")
      {
        appSetting.SettingValue = "//ajax.googleapis.com/ajax/libs/jqueryui/1.12.1/jquery-ui.min.js";
      }

      // Update data schema version to 4.2.0
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.2.0";

      ctx.SaveChanges();
    }

    private static void UpgradeTo421(GalleryDb ctx)
    {
      // Update data schema version to 4.2.1
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.2.1";

      ctx.SaveChanges();
    }

    private static void UpgradeTo430(GalleryDb ctx)
    {
      // Feature 245: Add replace button to ribbon tab
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.Header))
      {
        const string srcText = @"<li class='gs_rbn_mng_dlt'>";
        const string replText = @"<li class='gs_rbn_mng_rf'><a href='javascript:void(0);' class='gs_rbn_btn' title='{{:Resource.RbnRplTt}}'><span class='fa fa-fw fa-3x fa-upload'></span>
                <p>{{:Resource.RbnRpl}}<br /><span class='fa fa-caret-down'></span></p>
            </a>
                <div class='gsp_dlg gs_rbn_mr_dlg gs_rbn_mng_rf_mr_dlg'>
                    <div class='gs_rbn_hlp_ctr'><span></span></div>
                    <p class='gs_rbn_mr_dlg_rf_hdr'></p>
                    <div class='gs_rbn_mr_dlg_rf_uploader gsp_addbottommargin2'>
                        <p style='width: 100%; height: 150px; text-align: center; padding-top: 100px;'>Loading...&nbsp;<span class='fa fa-spinner fa-pulse'></span></p>
                    </div>
                    <p class='gs_rbn_btn_ctr'>
                        <span class='gs_rbn_mng_rf_btn_lbl'><span class='fa'></span></span>
                        <button class='gs_icon_btn gs_rbn_mng_rf_btn' title='{{:Resource.RbnRplTt}}'>{{:Resource.RbnRplBtn}}</button>
                    </p>
                </div>
            </li>
            <li class='gs_rbn_mng_dlt'>";

        if (!uiTmpl.HtmlTemplate.Contains("gs_rbn_mng_rf")) // Update only if gs_rbn_mng_rf isn't there, since we don't want to insert this multiple times (which would happen during 3.x upgrades)
        {
          uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
        }
      }

      // Update data schema version to 4.3.0
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.3.0";

      ctx.SaveChanges();
    }

    private static void UpgradeTo440(GalleryDb ctx)
    {
      // Feature 253: Add media asset link to share tab
      foreach (var uiTmpl in ctx.UiTemplates.Where(t => t.TemplateType == UiTemplateType.Header))
      {
        const string srcText = @"<p class='gs_rbn_mr_dlg_sh_hdr'></p>";
        const string replText = @"<p class='gs_rbn_mr_dlg_sh_hdr'></p>
                    <p class='gs_rbn_mr_dlg_sh_asset_pg_hdr'>{{:Resource.RbnShAssetUrlLbl}}</p>
                    <p class='gs_rbn_mr_dlg_sh_asset_pg_dtl'>
                        <input type='text' class='gs_rbn_mr_dlg_sh_ipt gs_rbn_mr_dlg_asset_sh_ipt_url' value='' /></p>";

        if (!uiTmpl.HtmlTemplate.Contains("gs_rbn_mr_dlg_sh_asset_pg_hdr")) // Update only if gs_rbn_mr_dlg_sh_asset_pg_hdr isn't there, since we don't want to insert this multiple times (which would happen during 3.x upgrades)
        {
          uiTmpl.HtmlTemplate = uiTmpl.HtmlTemplate.Replace(srcText, replText);
        }
      }

      // Update data schema version to 4.4.0
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.4.0";

      ctx.SaveChanges();
    }

    private static void UpgradeTo441(GalleryDb ctx)
    {
      // Update data schema version to 4.4.1
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.4.1";

      ctx.SaveChanges();
    }

    private static void UpgradeTo442(GalleryDb ctx)
    {
      // Update data schema version to 4.4.2
      var asDataSchema = ctx.AppSettings.First(a => a.SettingName == "DataSchemaVersion");
      asDataSchema.SettingValue = "4.4.2";

      ctx.SaveChanges();
    }

    #endregion
  }
}