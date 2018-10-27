using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business
{
    /// <summary>
    /// Contains extensions methods and helper functions for use with enums.
    /// </summary>
    public static class EnumUtils
    {
        /// <summary>
        /// Gets the value of the <see cref="DescriptionAttribute" /> applied to an enumeration <paramref name="value" />.
        /// If not present, the textual representation of the value is returned (enumValue.ToString()).
        /// </summary>
        /// <param name="value">The enumeration value.</param>
        /// <returns>System.String.</returns>
        public static string GetDescription(this Enum value)
        {
            // Check for Enum that is marked with FlagAttribute
            const char enumSeperatorCharacter = ',';
            var entries = value.ToString().Split(enumSeperatorCharacter);
            var description = new string[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                var fieldInfo = value.GetType().GetField(entries[i].Trim());
                var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                description[i] = (attributes.Length > 0) ? attributes[0].Description : entries[i].Trim();
            }
            return String.Join(", ", description);
        }
    }

    /// <summary>
    /// Specifies the type of the display object.
    /// </summary>
    public enum DisplayObjectType
    {
        /// <summary>
        /// Gets the Unknown display object type.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Gets the Thumbnail display object type.
        /// </summary>
        Thumbnail = 1,
        /// <summary>
        /// Gets the Optimized display object type.
        /// </summary>
        Optimized = 2,
        /// <summary>
        /// Gets the Original display object type.
        /// </summary>
        Original = 3,
        /// <summary>
        /// Gets the display object type that represents a media object that is external to Gallery Server (e.g. YouTube, Silverlight).
        /// </summary>
        External = 4
    }

    /// <summary>
    /// Contains functionality to support the <see cref="DisplayObjectType" /> enumeration.
    /// </summary>
    public static class DisplayObjectTypeEnumHelper
    {
        /// <summary>
        /// Determines if the displayType parameter is one of the defined enumerations. This method is more efficient than using
        /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="displayType">A <see cref="DisplayObjectType" /> to test.</param>
        /// <returns>Returns true if displayType is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidDisplayObjectType(DisplayObjectType displayType)
        {
            switch (displayType)
            {
                case DisplayObjectType.External:
                case DisplayObjectType.Optimized:
                case DisplayObjectType.Original:
                case DisplayObjectType.Thumbnail:
                case DisplayObjectType.Unknown:
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Parses the string into an instance of <see cref="DisplayObjectType" />. If <paramref name="displayType"/> is null or white space, then 
        /// <see cref="DisplayObjectType.Unknown" /> is returned. If it has a value that cannot be converted into one of the enumeration values,
        /// <see cref="DisplayObjectType.Unknown" /> is returned.
        /// </summary>
        /// <param name="displayType">The display object type to parse into an instance of <see cref="DisplayObjectType" />.</param>
        /// <returns>Returns an instance of <see cref="DisplayObjectType" />.</returns>
        public static DisplayObjectType ParseDisplayObjectType(string displayType)
        {
            if (String.IsNullOrWhiteSpace(displayType))
            {
                return DisplayObjectType.Unknown;
            }

            DisplayObjectType mtc;
            return (Enum.TryParse<DisplayObjectType>(displayType.Trim(), true, out mtc) ? mtc : DisplayObjectType.Unknown);
        }
    }

    /// <summary>
    /// Contains functionality to support the <see cref="MediaObjectTransitionType" /> enumeration.
    /// </summary>
    public static class MediaObjectTransitionTypeEnumHelper
    {
        /// <summary>
        /// Determines if the transitionType parameter is one of the defined enumerations. This method is more efficient than using
        /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="transitionType">An instance of <see cref="MediaObjectTransitionType" /> to test.</param>
        /// <returns>Returns true if transitionType is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidMediaObjectTransitionType(MediaObjectTransitionType transitionType)
        {
            switch (transitionType)
            {
                case MediaObjectTransitionType.None:
                case MediaObjectTransitionType.Blind:
                case MediaObjectTransitionType.Bounce:
                case MediaObjectTransitionType.Clip:
                case MediaObjectTransitionType.Drop:
                case MediaObjectTransitionType.Explode:
                case MediaObjectTransitionType.Fade:
                case MediaObjectTransitionType.Fold:
                case MediaObjectTransitionType.Highlight:
                case MediaObjectTransitionType.Puff:
                case MediaObjectTransitionType.Pulsate:
                case MediaObjectTransitionType.Scale:
                case MediaObjectTransitionType.Shake:
                case MediaObjectTransitionType.Size:
                case MediaObjectTransitionType.Slide:
                case MediaObjectTransitionType.Transfer:
                    break;

                default:
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Contains functionality to support the <see cref="System.Drawing.ContentAlignment" /> enumeration.
    /// </summary>
    public static class ContentAlignmentEnumHelper
    {
        /// <summary>
        /// Determines if the <paramref name="contentAlignment" /> parameter is one of the defined enumerations. This method is 
        /// more efficient than using <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="contentAlignment">A of <see cref="System.Drawing.ContentAlignment" /> to test.</param>
        /// <returns>Returns true if contentAlignment is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidContentAlignment(System.Drawing.ContentAlignment contentAlignment)
        {
            switch (contentAlignment)
            {
                case System.Drawing.ContentAlignment.BottomCenter:
                case System.Drawing.ContentAlignment.BottomLeft:
                case System.Drawing.ContentAlignment.BottomRight:
                case System.Drawing.ContentAlignment.MiddleCenter:
                case System.Drawing.ContentAlignment.MiddleLeft:
                case System.Drawing.ContentAlignment.MiddleRight:
                case System.Drawing.ContentAlignment.TopCenter:
                case System.Drawing.ContentAlignment.TopLeft:
                case System.Drawing.ContentAlignment.TopRight:
                    break;

                default:
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Specifies the category to which this mime type belongs. This usually corresponds to the first portion of 
    /// the full mime type description. (e.g. "image" if the full mime type is "image/jpeg") The one exception to 
    /// this is the "Other" enumeration, which represents any category not represented by the others. If a value
    /// has not yet been assigned, it defaults to the NotSet value.
    /// </summary>
    public enum MimeTypeCategory
    {
        /// <summary>
        /// Gets the NotSet mime type name, which indicates that no assignment has been made.
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Gets the Other mime type name.
        /// </summary>
        Other = 1,
        /// <summary>
        /// Gets the Image mime type name.
        /// </summary>
        Image = 2,
        /// <summary>
        /// Gets the Video mime type name.
        /// </summary>
        Video = 3,
        /// <summary>
        /// Gets the Audio mime type name.
        /// </summary>
        Audio = 4
    }

    /// <summary>
    /// Contains functionality to support the <see cref="MimeTypeCategory" /> enumeration.
    /// </summary>
    public static class MimeTypeEnumHelper
    {
        /// <summary>
        /// Determines if the mimeTypeCategory parameter is one of the defined enumerations. This method is more efficient than using
        /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="mimeTypeCategory">An instance of <see cref="MimeTypeCategory" /> to test.</param>
        /// <returns>Returns true if mimeTypeCategory is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidMimeTypeCategory(MimeTypeCategory mimeTypeCategory)
        {
            switch (mimeTypeCategory)
            {
                case MimeTypeCategory.NotSet:
                case MimeTypeCategory.Audio:
                case MimeTypeCategory.Image:
                case MimeTypeCategory.Other:
                case MimeTypeCategory.Video:
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Parses the string into an instance of <see cref="MimeTypeCategory" />. If <paramref name="mimeTypeCategory"/> is null or white space, then 
        /// <see cref="MimeTypeCategory.NotSet" /> is returned. If it has a value that cannot be converted into one of the enumeration values,
        /// <see cref="MimeTypeCategory.Other" /> is returned.
        /// </summary>
        /// <param name="mimeTypeCategory">The MIME type category to parse into an instance of <see cref="MimeTypeCategory" />.</param>
        /// <returns>Returns an instance of <see cref="MimeTypeCategory" />.</returns>
        public static MimeTypeCategory ParseMimeTypeCategory(string mimeTypeCategory)
        {
            if (String.IsNullOrWhiteSpace(mimeTypeCategory))
            {
                return MimeTypeCategory.NotSet;
            }

            MimeTypeCategory mtc;
            return (Enum.TryParse<MimeTypeCategory>(mimeTypeCategory.Trim(), true, out mtc) ? mtc : MimeTypeCategory.Other);
        }
    }

    /// <summary>
    /// Specifies the position for a pager rendered to a UI. A pager is a control that allows a user to navigate
    /// large collections of objects. It typically has next and previous buttons, and my contain buttons for quickly
    /// accessing intermediate pages.
    /// </summary>
    public enum PagerPosition
    {
        /// <summary>
        /// A pager positioned at the top of the control.
        /// </summary>
        Top = 0,
        /// <summary>
        /// A pager positioned at the bottom of the control.
        /// </summary>
        Bottom,
        /// <summary>
        /// Pagers positioned at both the top and the bottom of the control.
        /// </summary>
        TopAndBottom
    }

    /// <summary>
    /// Contains functionality to support the <see cref="PagerPosition" /> enumeration.
    /// </summary>
    public static class PagerPositionEnumHelper
    {
        /// <summary>
        /// Determines if the <paramref name="pagerPosition"/> is one of the defined enumerations. This method is more efficient than using
        /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="pagerPosition">An instance of <see cref="PagerPosition" /> to test.</param>
        /// <returns>Returns true if <paramref name="pagerPosition"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidPagerPosition(PagerPosition pagerPosition)
        {
            switch (pagerPosition)
            {
                case PagerPosition.Top:
                case PagerPosition.Bottom:
                case PagerPosition.TopAndBottom:
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Parses the string into an instance of <see cref="PagerPosition" />. If <paramref name="pagerPosition"/> is null or empty, an 
        /// <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="pagerPosition">The pager position to parse into an instance of <see cref="PagerPosition" />.</param>
        /// <returns>Returns an instance of <see cref="PagerPosition" />.</returns>
        public static PagerPosition ParsePagerPosition(string pagerPosition)
        {
            if (String.IsNullOrEmpty(pagerPosition))
                throw new ArgumentException("Invalid PagerPosition value: " + pagerPosition, "pagerPosition");

            return (PagerPosition)Enum.Parse(typeof(PagerPosition), pagerPosition.Trim(), true);
        }
    }

    /// <summary>
    /// Specifies the trust level of the current application domain. For web applications, this maps to the
    /// AspNetHostingPermissionLevel.
    /// </summary>
    public enum ApplicationTrustLevel
    {
        /// <summary>Specifies that this enumeration has not been assigned a value.</summary>
        None = 0,
        /// <summary>Gets the Unknown trust level. This is used when the trust level cannot be determined.</summary>
        Unknown = 10,
        /// <summary>Gets the Minimal trust level.</summary>
        Minimal = 20,
        /// <summary>Gets the Low trust level.</summary>
        Low = 30,
        /// <summary>Gets the Medium trust level.</summary>
        Medium = 40,
        /// <summary>Gets the High trust level.</summary>
        High = 50,
        /// <summary>Gets the Full trust level.</summary>
        Full = 60
    }

    /// <summary>
    /// Specifies one or more security-related actions within Gallery Server. A user may or may not have authorization to
    /// perform each security action. A user's authorization is determined by the role or roles to which he or she
    /// belongs. This enumeration is defined with the Flags attribute, so one can combine multiple security actions by
    /// performing a bitwise OR.
    /// </summary>
    [Flags]
    public enum SecurityActions
    {
        /// <summary>
        /// No security action has been specified.
        /// </summary>
        NotSpecified = 0,
        /// <summary>
        /// Represents the ability to view an album or media object. Does not include the ability to view high resolution
        /// versions of images. Includes the ability to download the media object and view a slide show.
        /// </summary>
        ViewAlbumOrMediaObject = 1,
        /// <summary>
        /// Represents the ability to create a new album within the current album. This includes the ability to move or
        /// copy an album into the current album.
        /// </summary>
        AddChildAlbum = 2,
        /// <summary>
        /// Represents the ability to add a new media object to the current album. This includes the ability to move or
        /// copy a media object into the current album.
        /// </summary>
        AddMediaObject = 4,
        /// <summary>
        /// Represents the ability to edit an album's title, summary, and begin and end dates. Also includes rearranging the
        /// order of objects within the album and assigning the album's thumbnail image. Does not include the ability to
        /// add or delete child albums or media objects.
        /// </summary>
        EditAlbum = 8,
        /// <summary>
        /// Represents the ability to edit a media object's caption, rotate it, and delete the high resolution version of
        /// an image.
        /// </summary>
        EditMediaObject = 16,
        /// <summary>
        /// Represents the ability to delete the current album. This permission is required to move 
        /// albums to another album, since it is effectively deleting it from the current album's parent.
        /// </summary>
        DeleteAlbum = 32,
        /// <summary>
        /// Represents the ability to delete child albums within the current album.
        /// </summary>
        DeleteChildAlbum = 64,
        /// <summary>
        /// Represents the ability to delete media objects within the current album. This permission is required to move 
        /// media objects to another album, since it is effectively deleting it from the current album.
        /// </summary>
        DeleteMediaObject = 128,
        /// <summary>
        /// Represents the ability to synchronize media objects on the hard drive with records in the data store.
        /// </summary>
        Synchronize = 256,
        /// <summary>
        /// Represents the ability to administer a particular gallery. Automatically includes all other permissions except
        /// AdministerSite.
        /// </summary>
        AdministerGallery = 512,
        /// <summary>
        /// Represents the ability to administer all aspects of Gallery Server. Automatically includes all other permissions.
        /// </summary>
        AdministerSite = 1024,
        /// <summary>
        /// Represents the ability to not render a watermark over media objects.
        /// </summary>
        HideWatermark = 2048,
        /// <summary>
        /// Represents the ability to view the original version of media objects.
        /// </summary>
        ViewOriginalMediaObject = 4096,
        /// <summary>
        /// Represents all possible permissions. Note: This enum value is defined to contain ALL POSSIBLE enum values to ensure
        /// the <see cref="SecurityActionEnumHelper.IsValidSecurityAction(SecurityActions)" /> method properly works. If a developer adds or removes
        /// items from this enum, this item must be updated to reflect the ORed list of all possible values.
        /// </summary>
        All = (ViewAlbumOrMediaObject | AddChildAlbum | AddMediaObject | EditAlbum | EditMediaObject | DeleteAlbum | DeleteChildAlbum | DeleteMediaObject | Synchronize | AdministerGallery | AdministerSite | HideWatermark | ViewOriginalMediaObject)
    }

    /// <summary>
    /// Specifies whether multiple <see cref="SecurityActions" /> values passed in a parameter all must pass a test for it to succeed, or
    /// whether it passes if only a single item succeeds. Relevant only when the <see cref="SecurityActions" /> specified contain multiple
    /// values.
    /// </summary>
    public enum SecurityActionsOption
    {
        /// <summary>
        /// Specifies that every <see cref="SecurityActions" /> must pass the test for the method to succeed.
        /// </summary>
        RequireAll,
        /// <summary>
        /// Specifies that the method succeeds if only a single <see cref="SecurityActions" /> item passes.
        /// </summary>
        RequireOne
    }

    /// <summary>
    /// Contains functionality to support the <see cref="SecurityActions" /> enumeration.
    /// </summary>
    public static class SecurityActionEnumHelper
    {
        /// <summary>
        /// Determines if the securityActions parameter is one of the defined enumerations or a valid combination of valid enumeration
        /// values (since <see cref="SecurityActions" /> is defined with the Flags attribute). <see cref="Enum.IsDefined" /> cannot be used since it does not return
        /// true when the enumeration contains more than one enum value. This method requires the <see cref="SecurityActions" /> enum to have a member
        /// All that contains every enum value ORed together.
        /// </summary>
        /// <param name="securityActions">A <see cref="SecurityActions" />. It may be a single value or some
        /// combination of valid enumeration values.</param>
        /// <returns>Returns true if securityActions is one of the defined items in the enumeration or is a valid combination of
        /// enumeration values; otherwise returns false.</returns>
        public static bool IsValidSecurityAction(SecurityActions securityActions)
        {
            if ((securityActions != 0) && ((securityActions & SecurityActions.All) == securityActions))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the securityActions parameter is one of the defined enumerations or a valid combination of valid enumeration
        /// values (since <see cref="SecurityActions" /> is defined with the Flags attribute). <see cref="Enum.IsDefined" /> cannot be used since it does not return
        /// true when the enumeration contains more than one enum value. This method requires the <see cref="SecurityActions" /> enum to have a member
        /// All that contains every enum value ORed together.
        /// </summary>
        /// <param name="securityActions">An integer representing a <see cref="SecurityActions" />.</param>
        /// <returns>Returns true if securityAction is one of the defined items in the enumeration or is a valid combination of
        /// enumeration values; otherwise returns false.</returns>
        public static bool IsValidSecurityAction(int securityActions)
        {
            if ((securityActions != 0) && ((securityActions & (int)SecurityActions.All) == securityActions))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if the specified value is a single, valid enumeration value. Since the <see cref="SecurityActions" /> enum has the 
        /// Flags attribute and may contain a bitwise combination of more than one value, this function is useful in
        /// helping the developer decide if the enum value is just one value or it must be parsed into its constituent
        /// parts with the GalleryServer.Business.SecurityManager.ParseSecurityAction method.
        /// </summary>
        /// <param name="securityActions">A <see cref="SecurityActions" />. It may be a single value or some
        /// combination of valid enumeration values.</param>
        /// <returns>Returns true if securityAction is a valid, single bit flag; otherwise return false.</returns>
        public static bool IsSingleSecurityAction(SecurityActions securityActions)
        {
            if (IsValidSecurityAction(securityActions) && (securityActions == SecurityActions.NotSpecified)
                || (securityActions == SecurityActions.ViewAlbumOrMediaObject)
                || (securityActions == SecurityActions.ViewOriginalMediaObject) || (securityActions == SecurityActions.AddMediaObject)
                || (securityActions == SecurityActions.AdministerSite) || (securityActions == SecurityActions.DeleteAlbum)
                || (securityActions == SecurityActions.DeleteChildAlbum) || (securityActions == SecurityActions.DeleteMediaObject)
                || (securityActions == SecurityActions.EditAlbum) || (securityActions == SecurityActions.EditMediaObject)
                || (securityActions == SecurityActions.HideWatermark) || (securityActions == SecurityActions.Synchronize)
                || (securityActions == SecurityActions.AddChildAlbum) || (securityActions == SecurityActions.AdministerGallery))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Parses the security action into one or more <see cref="SecurityActions"/>. Since the <see cref="SecurityActions" /> 
        /// enum has the Flags attribute and may contain a bitwise combination of more than one value, this function is useful
        /// in creating a list of the values that can be enumerated.
        /// </summary>
        /// <param name="securityActionsToParse">A <see cref="SecurityActions" />. It may be a single value or some
        /// combination of valid enumeration values.</param>
        /// <returns>Returns a list of <see cref="SecurityActions"/> that can be enumerated.</returns>
        public static IEnumerable<SecurityActions> ParseSecurityAction(SecurityActions securityActionsToParse)
        {
            List<SecurityActions> securityActions = new List<SecurityActions>(2);

            foreach (SecurityActions securityActionIterator in Enum.GetValues(typeof(SecurityActions)))
            {
                if (securityActionIterator == SecurityActions.NotSpecified)
                    continue; // Skip NotSpecified, since it falsely matches the test below

                if ((securityActionsToParse & securityActionIterator) == securityActionIterator)
                {
                    securityActions.Add(securityActionIterator);
                }
            }

            return securityActions;
        }
    }

    /// <summary>
    /// Specifies the visual transition effect to use when moving from one media object to another.
    /// These values map to the jQuery UI effects: http://docs.jquery.com/UI/Effects
    /// </summary>
    public enum MediaObjectTransitionType
    {
        /// <summary>
        /// No visual transition effect.
        /// </summary>
        None = 0,
        /// <summary>The blind media asset transition type.</summary>
        Blind = 1,
        /// <summary>The bounce media asset transition type.</summary>
        Bounce = 2,
        /// <summary>The clip media asset transition type.</summary>
        Clip = 3,
        /// <summary>The drop media asset transition type.</summary>
        Drop = 4,
        /// <summary>The explode media asset transition type.</summary>
        Explode = 5,
        /// <summary>The fade media asset transition type.</summary>
        Fade = 6,
        /// <summary>The fold media asset transition type.</summary>
        Fold = 7,
        /// <summary>The highlight media asset transition type.</summary>
        Highlight = 8,
        /// <summary>The puff media asset transition type.</summary>
        Puff = 9,
        /// <summary>The pulsate media asset transition type.</summary>
        Pulsate = 10,
        /// <summary>The scale media asset transition type.</summary>
        Scale = 11,
        /// <summary>The shake media asset transition type.</summary>
        Shake = 12,
        /// <summary>The size media asset transition type.</summary>
        Size = 13,
        /// <summary>The slide media asset transition type.</summary>
        Slide = 14,
        /// <summary>The transfer media asset transition type.</summary>
        Transfer = 15
    }

    /// <summary>
    /// Specifies a particular item within an application event (<see cref="IEvent"/>).
    /// </summary>
    public enum EventItem
    {
        /// <summary>
        /// The value that uniquely identifies an application event.
        /// </summary>
        EventId,
        /// <summary>
        /// The event type.
        /// </summary>
        EventType,
        /// <summary>
        /// The URL where the event occurred.
        /// </summary>
        Url,
        /// <summary>
        /// The date and time of the event.
        /// </summary>
        Timestamp,
        /// <summary>
        /// The message associated with the exception. This is usually mapped to <see cref="Exception.Message"/>.
        /// </summary>
        Message,
        /// <summary>
        /// The type of the exception (e.g. System.InvalidOperationException). Applies only when the event represents an exception.
        /// </summary>
        ExType,
        /// <summary>
        /// The source of the exception. This is usually mapped to <see cref="Exception.Source"/>.
        /// Applies only when the event represents an exception.
        /// </summary>
        ExSource,
        /// <summary>
        /// The target site of the exception. This is usually mapped to <see cref="Exception.TargetSite"/>. 
        /// Applies only when the event represents an exception.
        /// </summary>
        ExTargetSite,
        /// <summary>
        /// The stack trace of the exception. This is usually mapped to <see cref="Exception.StackTrace"/>. 
        /// Applies only when the event represents an exception.
        /// </summary>
        ExStackTrace,
        /// <summary>
        /// The exception data, if any, associated with the exception. This is usually mapped to <see cref="Exception.Data"/>. 
        /// Applies only when the event represents an exception.
        /// </summary>
        ExData,
        /// <summary>
        /// The type of the inner exception (e.g. System.InvalidOperationException). Applies only when the event represents an exception.
        /// </summary>
        InnerExType,
        /// <summary>
        /// The message associated with the inner exception. This is usually mapped to <see cref="Exception.Message"/>.
        /// Applies only when the event represents an exception.
        /// </summary>
        InnerExMessage,
        /// <summary>
        /// The source of the inner exception. This is usually mapped to <see cref="Exception.Source"/>.
        /// Applies only when the event represents an exception.
        /// </summary>
        InnerExSource,
        /// <summary>
        /// The target site of the inner exception. This is usually mapped to <see cref="Exception.TargetSite"/>.
        /// Applies only when the event represents an exception.
        /// </summary>
        InnerExTargetSite,
        /// <summary>
        /// The stack trace of the inner exception. This is usually mapped to <see cref="Exception.StackTrace"/>.
        /// Applies only when the event represents an exception.
        /// </summary>
        InnerExStackTrace,
        /// <summary>
        /// The exception data, if any, associated with the exception. This is usually mapped to <see cref="Exception.Data"/>.
        /// Applies only when the event represents an exception.
        /// </summary>
        InnerExData,
        /// <summary>
        /// The ID of the gallery where the error occurred.
        /// </summary>
        GalleryId,
        /// <summary>
        /// The HTTP user agent (that is, the browser) the user was using when the error occurred.
        /// </summary>
        HttpUserAgent,
        /// <summary>
        /// Refers to the collection of form variables on the web page when the error occurred.
        /// </summary>
        FormVariables,
        /// <summary>
        /// Refers to the cookies associated with the user when the error occurried.
        /// </summary>
        Cookies,
        /// <summary>
        /// Refers to the collection of session variables on the web page when the error occurred.
        /// </summary>
        SessionVariables,
        /// <summary>
        /// Refers to the collection of server variables on the web page when the error occurred.
        /// </summary>
        ServerVariables
    }

    /// <summary>
    /// Specifies the status of the Gallery Server maintenance task.
    /// </summary>
    public enum MaintenanceStatus
    {
        /// <summary>
        /// Specifies that the maintenance task has not begun.
        /// </summary>
        NotStarted = 0,
        /// <summary>
        /// Specifies that the maintenance task has begun.
        /// </summary>
        InProgress,
        /// <summary>
        /// Specifies that the maintenance task is complete.
        /// </summary>
        Complete
    }

    /// <summary>
    /// Specifies how the Gallery user control should render media objects.
    /// </summary>
    public enum ViewMode
    {
        /// <summary>
        /// The default value to use when the view mode is unknown or it is not relevant to specify.
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Specifies that the entire contents of an album be displayed as a set of thumbnails.
        /// </summary>
        Multiple = 1,
        /// <summary>
        /// Specifies that the media objects be displayed one at a time.
        /// </summary>
        Single = 2,
        /// <summary>
        /// Specifies that the media objects be displayed one at a time in a random order.
        /// </summary>
        SingleRandom = 3,
    }

    /// <summary>
    /// Specifies the style of slide show used.
    /// </summary>
    public enum SlideShowType
    {
        /// <summary>
        /// The default value to use when the slide show type is unknown or it is not relevant to specify.
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Specifies that slide show images are displayed in their normal position within the page. Use this
        /// when it would be inappropriate for the <see cref="FullScreen" /> option to take over the 
        /// entire screen area.
        /// </summary>
        Inline = 1,
        /// <summary>
        /// Specifies that the slide show is shown using a full screen viewer.
        /// </summary>
        FullScreen = 2
    }

    /// <summary>
    /// Contains functionality to support the <see cref="DisplayObjectType" /> enumeration.
    /// </summary>
    public static class SlideShowTypeEnumHelper
    {
        /// <summary>
        /// Determines if the <paramref name="ssType" /> parameter is one of the defined enumerations. This method is more efficient than using
        /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="ssType">A <see cref="SlideShowType" /> to test.</param>
        /// <returns>Returns true if <paramref name="ssType" /> is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidSlideShowType(SlideShowType ssType)
        {
            switch (ssType)
            {
                case SlideShowType.FullScreen:
                case SlideShowType.Inline:
                case SlideShowType.NotSet:
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Parses the string into an instance of <see cref="SlideShowType" />. If <paramref name="ssType"/> is null or white space, then 
        /// <see cref="SlideShowType.NotSet" /> is returned. If it has a value that cannot be converted into one of the enumeration values,
        /// <see cref="SlideShowType.NotSet" /> is returned.
        /// </summary>
        /// <param name="ssType">The slide show type to parse into an instance of <see cref="SlideShowType" />.</param>
        /// <returns>Returns an instance of <see cref="SlideShowType" />.</returns>
        public static SlideShowType ParseSlideShowType(string ssType)
        {
            if (String.IsNullOrWhiteSpace(ssType))
            {
                return SlideShowType.NotSet;
            }

            SlideShowType mtc;
            return (Enum.TryParse<SlideShowType>(ssType.Trim(), true, out mtc) ? mtc : SlideShowType.NotSet);
        }
    }


    /// <summary>
    /// Specifies a reason why an album or media object cannot be deleted.
    /// </summary>
    public enum GalleryObjectDeleteValidationFailureReason
    {
        /// <summary>
        /// The default value to use when no validation failure exists or it has not yet been calculated.
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// The album cannot be deleted because it is configured as the user album container.
        /// </summary>
        AlbumSpecifiedAsUserAlbumContainer,
        /// <summary>
        /// The album cannot be deleted because it contains the user album container.
        /// </summary>
        AlbumContainsUserAlbumContainer,
        /// <summary>
        /// The album cannot be deleted because it is configured as the default gallery object.
        /// </summary>
        AlbumSpecifiedAsDefaultGalleryObject,
        /// <summary>
        /// The album cannot be deleted because it contains an album configured as the default gallery object.
        /// </summary>
        AlbumContainsDefaultGalleryObjectAlbum,
        /// <summary>
        /// The album cannot be deleted because it contains a media object configured as the default gallery object.
        /// </summary>
        AlbumContainsDefaultGalleryObjectMediaObject
    }

    /// <summary>
    /// Specifies the type of the gallery object.
    /// </summary>
    public enum GalleryObjectType
    {
        /// <summary>
        /// Specifies that no gallery object type has been assigned.
        /// </summary>
        NotSpecified = 0,
        /// <summary>
        /// Gets all possible gallery object types.
        /// </summary>
        All = 1,
        /// <summary>
        /// Gets all gallery object types except the Album type.
        /// </summary>
        MediaObject = 2,
        /// <summary>
        /// Gets the Album gallery object type.
        /// </summary>
        Album = 3,
        /// <summary>
        /// Gets the Image gallery object type.
        /// </summary>
        Image = 4,
        /// <summary>
        /// Gets the Audio gallery object type.
        /// </summary>
        Audio = 5,
        /// <summary>
        /// Gets the Video gallery object type.
        /// </summary>
        Video = 6,
        /// <summary>
        /// Gets the Generic gallery object type.
        /// </summary>
        Generic = 7,
        /// <summary>
        /// Gets the External gallery object type.
        /// </summary>
        External = 8,
        /// <summary>
        /// Gets the Unknown gallery object type.
        /// </summary>
        Unknown = 9,
        /// <summary>
        /// Specifies no gallery object type.
        /// </summary>
        None = 10
    }

    /// <summary>
    /// Contains functionality to support the <see cref="GalleryObjectType" /> enumeration.
    /// </summary>
    public static class GalleryObjectTypeEnumHelper
    {
        /// <summary>
        /// Determines if the galleryObjectType parameter is one of the defined enumerations. This method is more efficient than using
        /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="galleryObjectType">An instance of <see cref="GalleryObjectType" /> to test.</param>
        /// <returns>Returns true if galleryObjectType is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidGalleryObjectType(GalleryObjectType galleryObjectType)
        {
            switch (galleryObjectType)
            {
                case GalleryObjectType.NotSpecified:
                case GalleryObjectType.All:
                case GalleryObjectType.MediaObject:
                case GalleryObjectType.Album:
                case GalleryObjectType.Image:
                case GalleryObjectType.Audio:
                case GalleryObjectType.Video:
                case GalleryObjectType.Generic:
                case GalleryObjectType.External:
                case GalleryObjectType.Unknown:
                case GalleryObjectType.None:
                    break;

                default:
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Parses the string into an instance of <see cref="GalleryObjectType" />. If <paramref name="galleryObjectType"/>
        /// is null, empty, or an invalid value, then <paramref name="defaultFilter" /> is returned.
        /// </summary>
        /// <param name="galleryObjectType">The gallery object type to parse into an instance of <see cref="GalleryObjectType" />.</param>
        /// <param name="defaultFilter">The value to return if <paramref name="galleryObjectType" /> is invalid.</param>
        /// <returns>Returns an instance of <see cref="GalleryObjectType" />.</returns>
        public static GalleryObjectType Parse(string galleryObjectType, GalleryObjectType defaultFilter)
        {
            GalleryObjectType got;

            return Enum.TryParse(galleryObjectType, true, out got) ? got : defaultFilter;
        }
    }

    /// <summary>
    /// Specifies the category a UI template belongs to. For example, when a template is designed for rendering
    /// a media object, it will have the <see cref="UiTemplateType.MediaObject" /> enumeration value.
    /// </summary>
    public enum UiTemplateType
    {
        /// <summary>
        /// Specifies that no template type has been specified.
        /// </summary>
        NotSpecified = 0,
        /// <summary>
        /// Specifies the Album UI template type.
        /// </summary>
        Album = 1,
        /// <summary>
        /// Specifies the Media Object UI template type.
        /// </summary>
        MediaObject = 2,
        /// <summary>
        /// Specifies the Header UI template type.
        /// </summary>
        Header = 3,
        /// <summary>
        /// Specifies the UI template type for the left pane of a three-pane window.
        /// </summary>
        LeftPane = 4,
        /// <summary>
        /// Specifies the UI template type for the right pane of a three-pane window.
        /// </summary>
        RightPane = 5
    }

    /// <summary>
    /// Specifies the type of event that can occur in the gallery.
    /// </summary>
    public enum EventType
    {
        /// <summary>No event type is specified.</summary>
        NotSpecified = 0,
        /// <summary>The info event type.</summary>
        Info = 1,
        /// <summary>The warning event type.</summary>
        Warning = 2,
        /// <summary>The error event type.</summary>
        Error = 3
    }

    /// <summary>
    /// Specifies the type of database used to store data for the application
    /// </summary>
    public enum ProviderDataStore
    {
        /// <summary>
        /// Specifies the unknown data provider.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Specifies SQL Server CE
        /// </summary>
        SqlCe,
        /// <summary>
        /// Specifies SQL Server
        /// </summary>
        SqlServer
    }

    /// <summary>
    /// References a version of the database schema used by Gallery Server. A new schema version is added for any
    /// release that requires a database change. Data schemas earlier than 2.1.3162 are not supported.
    /// </summary>
    public enum GalleryDataSchemaVersion
    {
        // IMPORTANT: When modifying these values, be sure to update the functions ConvertGalleryDataSchemaVersionToString and
        // ConvertGalleryDataSchemaVersionToEnum as well!
        /// <summary>
        /// Gets the Unknown data schema version.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Gets the schema version for 2.1.3162.
        /// </summary>
        V2_1_3162,
        /// <summary>
        /// Gets the schema version for 2.3.3421.
        /// </summary>
        V2_3_3421,
        /// <summary>
        /// Gets the schema version for 2.4.1.
        /// </summary>
        V2_4_1,
        /// <summary>
        /// Gets the schema version for 2.4.3.
        /// </summary>
        V2_4_3,
        /// <summary>
        /// Gets the schema version for 2.4.4.
        /// </summary>
        V2_4_4,
        /// <summary>
        /// Gets the schema version for 2.4.5.
        /// </summary>
        V2_4_5,
        /// <summary>
        /// Gets the schema version for 2.4.6.
        /// </summary>
        V2_4_6,
        /// <summary>
        /// Gets the schema version for 2.5.0.
        /// </summary>
        V2_5_0,
        /// <summary>
        /// Gets the schema version for 2.6.0.
        /// </summary>
        V2_6_0,
        /// <summary>
        /// Gets the schema version for 3.0.0.
        /// </summary>
        V3_0_0,
        /// <summary>
        /// Gets the schema version for 3.0.1.
        /// </summary>
        V3_0_1,
        /// <summary>
        /// Gets the schema version for 3.0.2.
        /// </summary>
        V3_0_2,
        /// <summary>
        /// Gets the schema version for 3.0.3.
        /// </summary>
        V3_0_3,
        /// <summary>
        /// Gets the schema version for 3.1.0.
        /// </summary>
        V3_1_0,
        /// <summary>
        /// Gets the schema version for 3.2.0.
        /// </summary>
        V3_2_0,
        /// <summary>
        /// Gets the schema version for 3.2.1.
        /// </summary>
        V3_2_1,
        /// <summary>
        /// Gets the schema version for 4.0.0.
        /// </summary>
        V4_0_0,
        /// <summary>
        /// Gets the schema version for 4.0.1.
        /// </summary>
        V4_0_1,
        /// <summary>
        /// Gets the schema version for 4.1.0.
        /// </summary>
        V4_1_0,
        /// <summary>
        /// Gets the schema version for 4.2.0.
        /// </summary>
        V4_2_0,
        /// <summary>
        /// Gets the schema version for 4.2.1.
        /// </summary>
        V4_2_1,
        /// <summary>
        /// Gets the schema version for 4.3.0.
        /// </summary>
        V4_3_0,
        /// <summary>
        /// Gets the schema version for 4.4.0.
        /// </summary>
        V4_4_0,
        /// <summary>
        /// Gets the schema version for 4.4.1.
        /// </summary>
        V4_4_1,
        /// <summary>
        /// Gets the schema version for 4.4.2.
        /// </summary>
        V4_4_2,
        /// <summary>
        /// Gets the schema version for 4.4.3.
        /// </summary>
        V4_4_3,
        /// <summary>
        /// Gets the schema version for 4.5.0.
        /// </summary>
        V4_5_0
    }

    /// <summary>
    /// Contains functionality to support the <see cref="GalleryDataSchemaVersion" /> enumeration.
    /// </summary>
    public static class GalleryDataSchemaVersionEnumHelper
    {
        /// <summary>
        /// Convert <paramref name="version"/> to its string equivalent. Example: Return "2.4.1" when <paramref name="version"/> 
        /// is <see cref="GalleryDataSchemaVersion.V2_4_1"/>. This is a lookup function and does not return the current version 
        /// of the database or application schema requirements.
        /// </summary>
        /// <param name="version">The version of the gallery's data schema for which a string representation is to be returned.</param>
        /// <returns>Returns the string equivalent of the specified <see cref="GalleryDataSchemaVersion"/> value.</returns>
        public static string ConvertGalleryDataSchemaVersionToString(GalleryDataSchemaVersion version)
        {
            switch (version)
            {
                case GalleryDataSchemaVersion.V2_1_3162:
                    return "2.1.3162";
                case GalleryDataSchemaVersion.V2_3_3421:
                    return "2.3.3421";
                case GalleryDataSchemaVersion.V2_4_1:
                    return "2.4.1";
                case GalleryDataSchemaVersion.V2_4_3:
                    return "2.4.3";
                case GalleryDataSchemaVersion.V2_4_4:
                    return "2.4.4";
                case GalleryDataSchemaVersion.V2_4_5:
                    return "2.4.5";
                case GalleryDataSchemaVersion.V2_4_6:
                    return "2.4.6";
                case GalleryDataSchemaVersion.V2_5_0:
                    return "2.5.0";
                case GalleryDataSchemaVersion.V2_6_0:
                    return "2.6.0";
                case GalleryDataSchemaVersion.V3_0_0:
                    return "3.0.0";
                case GalleryDataSchemaVersion.V3_0_1:
                    return "3.0.1";
                case GalleryDataSchemaVersion.V3_0_2:
                    return "3.0.2";
                case GalleryDataSchemaVersion.V3_0_3:
                    return "3.0.3";
                case GalleryDataSchemaVersion.V3_1_0:
                    return "3.1.0";
                case GalleryDataSchemaVersion.V3_2_0:
                    return "3.2.0";
                case GalleryDataSchemaVersion.V3_2_1:
                    return "3.2.1";
                case GalleryDataSchemaVersion.V4_0_0:
                    return "4.0.0";
                case GalleryDataSchemaVersion.V4_0_1:
                    return "4.0.1";
                case GalleryDataSchemaVersion.V4_1_0:
                    return "4.1.0";
                case GalleryDataSchemaVersion.V4_2_0:
                    return "4.2.0";
                case GalleryDataSchemaVersion.V4_2_1:
                    return "4.2.1";
                case GalleryDataSchemaVersion.V4_3_0:
                    return "4.3.0";
                case GalleryDataSchemaVersion.V4_4_0:
                    return "4.4.0";
                case GalleryDataSchemaVersion.V4_4_1:
                    return "4.4.1";
                case GalleryDataSchemaVersion.V4_4_2:
                    return "4.4.2";
                case GalleryDataSchemaVersion.V4_4_3:
                    return "4.4.3";
                case GalleryDataSchemaVersion.V4_5_0:
                    return "4.5.0";
                case GalleryDataSchemaVersion.Unknown:
                    return "Unknown";
                default:
                    throw new InvalidEnumArgumentException(String.Format(CultureInfo.CurrentCulture, "The function GalleryServer.Business.ConvertGalleryDataSchemaVersionToString was not designed to handle the GalleryDataSchemaVersion enumeration value {0}. A developer must update this method to handle this value.", version));
            }
        }

        /// <summary>
        /// Convert <paramref name="version"/> to its <see cref="GalleryDataSchemaVersion"/> equivalent. Example: Return 
        /// <see cref="GalleryDataSchemaVersion.V2_4_1"/> when <paramref name="version"/> is "02.04.01" or "2.4.1". This is a 
        /// lookup function and does not return the current version of the database or application schema requirements.
        /// </summary>
        /// <param name="version">The version of the gallery's data schema.</param>
        /// <returns>Returns the <see cref="GalleryDataSchemaVersion"/> equivalent of the specified string.</returns>
        public static GalleryDataSchemaVersion ConvertGalleryDataSchemaVersionToEnum(string version)
        {
            if (version == null)
            {
                return GalleryDataSchemaVersion.Unknown;
            }

            switch (version)
            {
                case "2.1.3162":
                    return GalleryDataSchemaVersion.V2_1_3162;
                case "2.3.3421":
                    return GalleryDataSchemaVersion.V2_3_3421;
                case "02.04.01":
                    return GalleryDataSchemaVersion.V2_4_1;
                case "02.04.03":
                    return GalleryDataSchemaVersion.V2_4_3;
                case "02.04.04":
                    return GalleryDataSchemaVersion.V2_4_4;
                case "02.04.05":
                    return GalleryDataSchemaVersion.V2_4_5;
                case "02.04.06":
                    return GalleryDataSchemaVersion.V2_4_6;
                case "02.05.00":
                    return GalleryDataSchemaVersion.V2_5_0;
                case "02.06.00":
                    return GalleryDataSchemaVersion.V2_6_0;
                case "2.4.1":
                    return GalleryDataSchemaVersion.V2_4_1;
                case "2.4.3":
                    return GalleryDataSchemaVersion.V2_4_3;
                case "2.4.4":
                    return GalleryDataSchemaVersion.V2_4_4;
                case "2.4.5":
                    return GalleryDataSchemaVersion.V2_4_5;
                case "2.4.6":
                    return GalleryDataSchemaVersion.V2_4_6;
                case "2.5.0":
                    return GalleryDataSchemaVersion.V2_5_0;
                case "2.6.0":
                    return GalleryDataSchemaVersion.V2_6_0;
                case "3.0.0":
                    return GalleryDataSchemaVersion.V3_0_0;
                case "3.0.1":
                    return GalleryDataSchemaVersion.V3_0_1;
                case "3.0.2":
                    return GalleryDataSchemaVersion.V3_0_2;
                case "3.0.3":
                    return GalleryDataSchemaVersion.V3_0_3;
                case "3.1.0":
                    return GalleryDataSchemaVersion.V3_1_0;
                case "3.2.0":
                    return GalleryDataSchemaVersion.V3_2_0;
                case "3.2.1":
                    return GalleryDataSchemaVersion.V3_2_1;
                case "4.0.0":
                    return GalleryDataSchemaVersion.V4_0_0;
                case "4.0.1":
                    return GalleryDataSchemaVersion.V4_0_1;
                case "4.1.0":
                    return GalleryDataSchemaVersion.V4_1_0;
                case "4.2.0":
                    return GalleryDataSchemaVersion.V4_2_0;
                case "4.2.1":
                    return GalleryDataSchemaVersion.V4_2_1;
                case "4.3.0":
                    return GalleryDataSchemaVersion.V4_3_0;
                case "4.4.0":
                    return GalleryDataSchemaVersion.V4_4_0;
                case "4.4.1":
                    return GalleryDataSchemaVersion.V4_4_1;
                case "4.4.2":
                    return GalleryDataSchemaVersion.V4_4_2;
                case "4.4.3":
                    return GalleryDataSchemaVersion.V4_4_3;
                case "4.5.0":
                    return GalleryDataSchemaVersion.V4_5_0;
                default:
                    return GalleryDataSchemaVersion.Unknown;
            }
        }

        /// <summary>
        /// Determines if the <paramref name="version"/> is one of the defined enumerations. This method is more efficient than using
        /// <see cref="Enum.IsDefined" />, since <see cref="Enum.IsDefined" /> uses reflection.
        /// </summary>
        /// <param name="version">An instance of <see cref="GalleryDataSchemaVersion" /> to test.</param>
        /// <returns>Returns true if <paramref name="version"/> is one of the defined items in the enumeration; otherwise returns false.</returns>
        public static bool IsValidGalleryDataSchemaVersion(GalleryDataSchemaVersion version)
        {
            switch (version)
            {
                case GalleryDataSchemaVersion.V2_1_3162:
                case GalleryDataSchemaVersion.V2_3_3421:
                case GalleryDataSchemaVersion.V2_4_1:
                case GalleryDataSchemaVersion.V2_4_3:
                case GalleryDataSchemaVersion.V2_4_4:
                case GalleryDataSchemaVersion.V2_4_5:
                case GalleryDataSchemaVersion.V2_4_6:
                case GalleryDataSchemaVersion.V2_5_0:
                case GalleryDataSchemaVersion.V2_6_0:
                case GalleryDataSchemaVersion.V3_0_0:
                case GalleryDataSchemaVersion.V3_0_1:
                case GalleryDataSchemaVersion.V3_0_2:
                case GalleryDataSchemaVersion.V3_0_3:
                case GalleryDataSchemaVersion.V3_1_0:
                case GalleryDataSchemaVersion.V3_2_0:
                case GalleryDataSchemaVersion.V3_2_1:
                case GalleryDataSchemaVersion.V4_0_0:
                case GalleryDataSchemaVersion.V4_0_1:
                case GalleryDataSchemaVersion.V4_1_0:
                case GalleryDataSchemaVersion.V4_2_0:
                case GalleryDataSchemaVersion.V4_2_1:
                case GalleryDataSchemaVersion.V4_3_0:
                case GalleryDataSchemaVersion.V4_4_0:
                case GalleryDataSchemaVersion.V4_4_1:
                case GalleryDataSchemaVersion.V4_4_2:
                case GalleryDataSchemaVersion.V4_4_3:
                case GalleryDataSchemaVersion.V4_5_0:
                case GalleryDataSchemaVersion.Unknown:

                    return true;

                default:
                    return false;
            }
        }

    }

    /// <summary>
    /// Contains generics-based functionality for convenient interaction with enumerations.
    /// </summary>
    /// <typeparam name="T">An enumeration type.</typeparam>
    /// <example>
    /// GalleryObjectType goType = Enum&lt;GalleryObjectType&gt;.Parse("MediaObject");
    /// </example>
    public static class Enum<T>
    {
        /// <overloads>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent
        /// enumerated object.
        /// </overloads>
        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent 
        /// enumerated object.
        /// </summary>
        /// <param name="value">A string containing the name or value to convert. </param>
        /// <returns>An object of type T whose value is represented by <paramref name="value" />.</returns>
        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }

        /// <summary>
        /// Converts the string representation of the name or numeric value of one or more enumerated constants to an equivalent 
        /// enumerated object. A parameter specifies whether the operation is case-sensitive.
        /// </summary>
        /// <param name="value">A string containing the name or value to convert. </param>
        /// <param name="ignoreCase"><c>true</c> to ignore case; <c>false</c> to regard case. </param>
        /// <returns>An object of type T whose value is represented by <paramref name="value" />.</returns>
        public static T Parse(string value, bool ignoreCase)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }

        /// <summary>
        /// Retrieves a collection of the values of the constants in the enumeration.
        /// </summary>
        /// <returns>A collection that contains the values of the constants in the enumeration. The items are sorted by the 
        /// binary values of the enumeration constants.</returns>
        public static IList<T> GetValues()
        {
            IList<T> list = new List<T>();
            foreach (object value in Enum.GetValues(typeof(T)))
            {
                list.Add((T)value);
            }
            return list;
        }
    }

    /// <summary>
    /// Specifies the status of media object conversion queue.
    /// </summary>
    public enum MediaQueueStatus
    {
        /// <summary>
        /// Specifies the unknown media object conversion queue status.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Specifies that the media object conversion queue is not processing any items.
        /// </summary>
        Idle = 1,
        /// <summary>
        /// Specifies that an item in the media object conversion queue is currently being processed.
        /// </summary>
        Processing = 2,
        /// <summary>
        /// Specifies that the queue processor has been paused by an administrator.
        /// </summary>
        Paused = 3
    }

    /// <summary>
    /// Specifies the status of the media object in the media object conversion queue.
    /// </summary>
    public enum MediaQueueItemStatus
    {
        /// <summary>
        /// Specifies the unknown media queue status.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Specifies an error occurred while processing the media item.
        /// </summary>
        Error = 1,
        /// <summary>
        /// Specifies the item is waiting to be processed.
        /// </summary>
        Waiting = 2,
        /// <summary>
        /// Specifies the item is currently being processed.
        /// </summary>
        Processing = 3,
        /// <summary>
        /// Specifies that processing is canceled.
        /// </summary>
        Canceled = 4,
        /// <summary>
        /// Specifies that processing is complete.
        /// </summary>
        Complete = 5
    }

    /// <summary>
    /// Specifies the type of processing to be executed on a media object in the media object conversion queue.
    /// </summary>
    public enum MediaQueueItemConversionType
    {
        /// <summary>
        /// Specifies the unknown media queue conversion type.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Specifies that an optimized media file is to be created.
        /// </summary>
        CreateOptimized = 1,
        /// <summary>
        /// Specifies that a video is to be rotated.
        /// </summary>
        RotateVideo = 2
    }

    /// <summary>
    /// Specifies the category describing the result of an action.
    /// </summary>
    public enum ActionResultStatus
    {
        /// <summary>
        /// Gets the NotSet value, which indicates that no assignment has been made.
        /// </summary>
        NotSet = 0,
        /// <summary>
        /// Specifies that the result was successful.
        /// </summary>
        Success = 1,
        /// <summary>
        /// Specifies that an error occurred while processing the action.
        /// </summary>
        Error = 2,
        /// <summary>
        /// Specifies that a warning occurred while processing the action.
        /// </summary>
        Warning,
        /// <summary>
        /// Specifies a piece of information related to the action.
        /// </summary>
        Info,
        /// <summary>
        /// Specifies that an action is being executed asyncronously and its exact status has not yet been determined.
        /// </summary>
        Async
    }

    /// <summary>
    /// Specifies the style of message to be displayed to the user.
    /// </summary>
    public enum MessageStyle
    {
        /// <summary>No message style.</summary>
        None = 0,
        /// <summary>The success message style.</summary>
        Success = 1,
        /// <summary>The info message style.</summary>
        Info = 2,
        /// <summary>The warning message style.</summary>
        Warning = 3,
        /// <summary>The error message style.</summary>
        Error = 4
    }

    /// <summary>
    /// Specifies a particular message that is to be displayed to the user. The text of the message is extracted from the resource file.
    /// Make sure the enum numbers match their Gs.Enums.MessageType counterparts in gallery.ts.
    /// </summary>
    public enum MessageType
    {
        /// <summary>No message.</summary>
        None = 0,
        /// <summary>The media asset does not exist.</summary>
        MediaObjectDoesNotExist = 1,
        /// <summary>The album does not exist.</summary>
        AlbumDoesNotExist = 2,
        /// <summary>The user name or password is incorrect.</summary>
        UserNameOrPasswordIncorrect = 3,
        /// <summary>The album is not authorized for user.</summary>
        AlbumNotAuthorizedForUser = 4,
        /// <summary>There is no authorized album for user.</summary>
        NoAuthorizedAlbumForUser = 5,
        /// <summary>The assets were skipped during upload.</summary>
        ObjectsSkippedDuringUpload = 6,
        /// <summary>Error: Cannot edit gallery because it is read only.</summary>
        CannotEditGalleryIsReadOnly = 7,
        /// <summary>The gallery was successfully changed.</summary>
        GallerySuccessfullyChanged = 8,
        /// <summary>The settings were successfully changed.</summary>
        SettingsSuccessfullyChanged = 9,
        /// <summary>The assets are being processed asynchronously.</summary>
        ObjectsBeingProcessedAsyncronously = 10,
        /// <summary>The album was successfully deleted.</summary>
        AlbumSuccessfullyDeleted = 11
    }

    /// <summary>
    /// Identifies the type of search being performed.
    /// </summary>
    public enum GalleryObjectSearchType
    {
        /// <summary>
        /// Indicates that no search type has been specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Indicates that a search by title or caption is specified.
        /// </summary>
        SearchByTitleOrCaption,

        /// <summary>
        /// Indicates that a search by tag is specified.
        /// </summary>
        SearchByTag,

        /// <summary>
        /// Indicates that a search for people is specified.
        /// </summary>
        SearchByPeople,

        /// <summary>
        /// Indicates that a search by keyword is specified.
        /// </summary>
        SearchByKeyword,

        /// <summary>
        /// Indicates a request for the highest album the current user can view.
        /// </summary>
        HighestAlbumUserCanView,

        /// <summary>
        /// Indicates the most recently added gallery objects.
        /// </summary>
        MostRecentlyAdded,

        /// <summary>
        /// Indicates that a search by rating is specified.
        /// </summary>
        SearchByRating
    }

    /// <summary>
    /// Identifies the type of search being performed.
    /// </summary>
    public enum TagSearchType
    {
        /// <summary>
        /// Indicates that no search type has been specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Indicates a request for all tags in the current gallery.
        /// </summary>
        AllTagsInGallery,

        /// <summary>
        /// Indicates a request for all people in the current gallery.
        /// </summary>
        AllPeopleInGallery,

        /// <summary>
        /// Indicates a request for all tags visible to the current user in the current gallery.
        /// </summary>
        TagsUserCanView,

        /// <summary>
        /// Indicates a request for all people visible to the current user in the current gallery.
        /// </summary>
        PeopleUserCanView
    }

    /// <summary>
    /// Identifies the type of virtual album.
    /// </summary>
    public enum VirtualAlbumType
    {
        /// <summary>
        /// Indicates that no virtual album type has been specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Specifies that the album is not a virtual album.
        /// </summary>
        NotVirtual = 1,

        /// <summary>
        /// Specifies that the album is a virtual container whose purpose is to hold child objects the user
        /// has permission to access. This is used when a restricted permission user has access to two albums
        /// without a common parent. In this case, a virtual album is created to serve as the container.
        /// </summary>
        Root = 2,

        /// <summary>
        /// Indicates that a virtual album contains the results of a tag search.
        /// </summary>
        Tag = 3,

        /// <summary>
        /// Indicates that a virtual album contains the results of a people search.
        /// </summary>
        People = 4,

        /// <summary>
        /// Indicates that a virtual album contains the results of a keyword search.
        /// </summary>
        Search = 5,

        /// <summary>
        /// Indicates that a virtual album contains the results of a title/caption search.
        /// </summary>
        TitleOrCaption = 6,

        /// <summary>
        /// Indicates the most recently added gallery objects.
        /// </summary>
        MostRecentlyAdded = 7,

        /// <summary>
        /// Indicates gallery objects having a specific rating.
        /// </summary>
        Rated = 8
    }

    /// <summary>
    /// Identifies the amount to rotate or flip a media asset.
    /// </summary>
    public enum MediaAssetRotateFlip
    {
        /// <summary>
        /// Indicates that no rotation or flip has been specified.
        /// </summary>
        NotSpecified = 0,

        /// <summary>
        /// Specifies no clockwise rotation and no flipping.
        /// </summary>
        Rotate0FlipNone = 1,

        /// <summary>
        /// Specifies no clockwise rotation followed by a horizontal flip.
        /// </summary>
        Rotate0FlipX = 2,

        /// <summary>
        /// Specifies no clockwise rotation followed by a vertical flip.
        /// </summary>
        Rotate0FlipY = 3,

        /// <summary>
        /// Specifies a 90-degree clockwise rotation without flipping.
        /// </summary>
        Rotate90FlipNone = 4,

        /// <summary>
        /// Specifies a 90-degree clockwise rotation followed by a horizontal flip.
        /// </summary>
        Rotate90FlipX = 5,

        /// <summary>
        /// Specifies a 90-degree clockwise rotation followed by a vertical flip.
        /// </summary>
        Rotate90FlipY = 6,

        /// <summary>
        /// Specifies a 180-degree clockwise rotation without flipping.
        /// </summary>
        Rotate180FlipNone = 7,

        /// <summary>
        /// Specifies a 180-degree clockwise rotation followed by a horizontal flip.
        /// </summary>
        Rotate180FlipX = 8,

        /// <summary>
        /// Specifies a 180-degree clockwise rotation followed by a vertical flip.
        /// </summary>
        Rotate180FlipY = 9,

        /// <summary>
        /// Specifies a 270-degree clockwise rotation without flipping.
        /// </summary>
        Rotate270FlipNone = 10,

        /// <summary>
        /// Specifies a 270-degree clockwise rotation followed by a horizontal flip.
        /// </summary>
        Rotate270FlipX = 11,

        /// <summary>
        /// Specifies a 270-degree clockwise rotation followed by a vertical flip.
        /// </summary>
        Rotate270FlipY = 12
    }

    /// <summary>
    /// Identifies the type of transfer a gallery asset can undertake. That is, specifies whether it is being moved or copied.
    /// </summary>
    public enum GalleryAssetTransferType
    {
        /// <summary>
        /// Indicates an album or media asset is being moved.
        /// </summary>
        Move = 1,

        /// <summary>
        /// Indicates an album or media asset is being copied.
        /// </summary>
        Copy = 2
    }

    /// <summary>
    /// Specifies whether an item is editable and, if so, the type of editor to use.
    /// </summary>
    public enum PropertyEditorMode
    {
        /// <summary>
        /// Indicates no property editor mode has been specified
        /// </summary>
        NotSet = 0,

        /// <summary>
        /// Indicates that a property is not editable by users.
        /// </summary>
        NotEditable = 1,

        /// <summary>
        /// Indicates that a plain text editor is to be used for property editing.
        /// </summary>
        PlainTextEditor = 2,

        /// <summary>
        /// Indicates that the tinyMCE HTML editor is to be used for property editing.
        /// </summary>
        TinyMCEHtmlEditor = 3

    }

}