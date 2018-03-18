using System;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Reflection;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Data
{
  /// <summary>
  /// Provides functionality for interacting with the GalleryControlSetting table.
  /// </summary>
  public class GalleryControlSettingRepository : Repository<GalleryDb, GalleryControlSettingDto>
  {

    /// <summary>
    /// Persist the current gallery control settings to the data store.
    /// </summary>
    /// <param name="galleryControlSettings">An instance of <see cref="IGalleryControlSettings"/> to persist to the data store.</param>
    public void Save(IGalleryControlSettings galleryControlSettings)
    {
      var propertiesToExclude = new[] { "GalleryControlSettingId", "ControlId" };

      Type gsType = galleryControlSettings.GetType();
      string viewModeType = typeof(ViewMode).ToString();
      string displayObjectTypeType = typeof(DisplayObjectType).ToString();
      string slideShowTypeType = typeof(SlideShowType).ToString();

      string boolType = typeof(bool).ToString();
      string boolNullableType = typeof(bool?).ToString();
      string intType = typeof(int).ToString();
      string intNullableType = typeof(int?).ToString();
      string stringType = typeof(string).ToString();

      //ctx.GalleryControlSettings.Load();
      this.Context.GalleryControlSettings.Load();

      foreach (PropertyInfo prop in gsType.GetProperties())
      {
        if (Array.IndexOf(propertiesToExclude, prop.Name) >= 0)
        {
          continue; // Skip this one.
        }

        // Get a reference to the database record (won't exist for new items).
        string propName = prop.Name;
        //GalleryControlSettingDto gcsDto = (from g in ctx.GalleryControlSettings.Local
        //                                   where g.ControlId == galleryControlSettings.ControlId && g.SettingName == propName
        //                                   select g).FirstOrDefault();
        GalleryControlSettingDto gcsDto = Context.GalleryControlSettings.Local.FirstOrDefault(g => g.ControlId == galleryControlSettings.ControlId && g.SettingName == propName);

        object objPropValue = prop.GetValue(galleryControlSettings, null);

        if (objPropValue != null)
        {
          string propValue;

          if (prop.PropertyType.FullName == null)
          {
            continue;
          }

          if (prop.PropertyType.FullName.Equals(boolType) || prop.PropertyType.FullName.Equals(boolNullableType))
          {
            propValue = Convert.ToBoolean(objPropValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
          }
          else if (prop.PropertyType.FullName.Equals(intType) || prop.PropertyType.FullName.Equals(intNullableType))
          {
            propValue = Convert.ToInt32(objPropValue, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
          }
          else if (prop.PropertyType.FullName.Equals(stringType))
          {
            propValue = Convert.ToString(objPropValue, CultureInfo.InvariantCulture);
          }
          else if (prop.PropertyType.FullName.Equals(viewModeType))
          {
            // Only save ViewMode if it has a non-default value. 
            var viewMode = (ViewMode)Enum.Parse(typeof(ViewMode), prop.GetValue(galleryControlSettings, null).ToString(), true);

            if (viewMode == ViewMode.NotSet)
            {
              // Property not assigned. Delete the record.
              if (gcsDto != null)
              {
                Delete(gcsDto);
              }

              continue; // We're done with this property, so let's move on to the next one.
            }

            propValue = viewMode.ToString();
          }
          else if (prop.PropertyType.FullName.Equals(displayObjectTypeType))
          {
            // Only save MediaViewSize if it has a non-default value. 
						var dotType = (DisplayObjectType)Enum.Parse(typeof(DisplayObjectType), prop.GetValue(galleryControlSettings, null).ToString(), true);

            if (dotType == DisplayObjectType.Unknown)
            {
              // Property not assigned. Delete the record.
              if (gcsDto != null)
              {
                Delete(gcsDto);
              }

              continue; // We're done with this property, so let's move on to the next one.
            }

            propValue = dotType.ToString();
          }
          else if (prop.PropertyType.FullName.Equals(slideShowTypeType))
          {
            // Only save SlideShowType if it has a non-default value. 
						var ssType = (SlideShowType)Enum.Parse(typeof(SlideShowType), prop.GetValue(galleryControlSettings, null).ToString(), true);

						if (ssType == SlideShowType.NotSet)
            {
              // Property not assigned. Delete the record.
              if (gcsDto != null)
              {
                Delete(gcsDto);
              }

              continue; // We're done with this property, so let's move on to the next one.
            }

            propValue = ssType.ToString();
          }
          else
          {
            propValue = prop.GetValue(galleryControlSettings, null).ToString();
          }

          // Insert or update the item.
          if (gcsDto == null)
          {
            gcsDto = new GalleryControlSettingDto { ControlId = galleryControlSettings.ControlId, SettingName = propName, SettingValue = propValue };
            Add(gcsDto);
          }
          else
          {
            gcsDto.SettingValue = propValue;
          }
        }
        else
        {
          // Property not assigned. Delete the record.
          if (gcsDto != null)
          {
            Delete(gcsDto);
          }

          // Include this only for debug purposes.
          //System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
          //string msg = String.Format(CultureInfo.CurrentCulture, "Deleted Gallery Control Setting \"{0}\". Stack trace: {1}", prop.Name, st);
          //errMessages.Add(msg);
        }
      }

      Save();
    }
  }
}