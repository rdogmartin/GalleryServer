using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Text;
using System.Web;
using GalleryServer.Business;
using GalleryServer.Business.Interfaces;
using GalleryServer.Events.CustomExceptions;
using GalleryServer.Events.Properties;

namespace GalleryServer.Events
{
  /// <summary>
  /// Represents an application event or error that occurrs during the execution of Gallery Server code.
  /// </summary>
  public class Event : IEvent, IComparable
  {
    #region Private Fields

    private readonly List<KeyValuePair<string, string>> _cookies = new List<KeyValuePair<string, string>>(5);
    private readonly List<KeyValuePair<string, string>> _eventData = new List<KeyValuePair<string, string>>(1);
    private readonly string _exceptionType;
    private readonly List<KeyValuePair<string, string>> _formVariables = new List<KeyValuePair<string, string>>();
    private readonly int _galleryId;
    private readonly List<KeyValuePair<string, string>> _innerExData = new List<KeyValuePair<string, string>>(1);
    private readonly string _innerExMessage;
    private readonly string _innerExSource;
    private readonly string _innerExStackTrace;
    private readonly string _innerExTargetSite;
    private readonly string _innerExType;
    private readonly string _message;
    private readonly List<KeyValuePair<string, string>> _serverVariables = new List<KeyValuePair<string, string>>(60);
    private readonly List<KeyValuePair<string, string>> _sessionVariables = new List<KeyValuePair<string, string>>(5);
    private readonly string _source;
    private readonly string _stackTrace;
    private readonly string _targetSite;
    private readonly DateTime _timeStampUtc;
    private string _url;

    private const string gsp_event_col1_style = "vertical-align:top;padding:4px;background-color:#dcd8cf;white-space:nowrap;width:150px;border-bottom:1px solid #fff;";
    private const string gsp_event_col2_style = "vertical-align:top;padding:4px;border-bottom:1px solid #dcd8cf;";

    #endregion

    #region Public Properties

    /// <summary>
    ///   Gets or sets a value that uniquely identifies an application event.
    /// </summary>
    /// <value>A value that uniquely identifies an application event.</value>
    public int EventId { get; set; }

    /// <summary>
    ///   Gets or sets the type of the event.
    /// </summary>
    /// <value>The type of the event.</value>
    public EventType EventType { get; set; }

    /// <summary>
    ///   Gets the ID of the gallery that is the source of this event.
    /// </summary>
    /// <value>The ID of the gallery that is the source of this event</value>
    public int GalleryId
    {
      get { return _galleryId; }
    }

    /// <summary>
    ///   Gets the UTC date and time the event occurred. Guaranteed to not be null.
    /// </summary>
    /// <value>The date and time the event occurred.</value>
    public DateTime TimestampUtc
    {
      get { return _timeStampUtc; }
    }

    /// <summary>
    ///   Gets the message associated with the event. Guaranteed to not be null.
    /// </summary>
    /// <value>The message associated with the event.</value>
    public string Message
    {
      get { return _message; }
    }

    /// <summary>
    /// Gets the data associated with the event. When <see cref="EventType" /> is <see cref="Business.EventType.Error" />,
    /// any items in the exception data are added. Guaranteed to not be null.
    /// </summary>
    /// <value>The data associate with the exception.</value>
    public List<KeyValuePair<string, string>> EventData
    {
      get { return _eventData; }
    }

    /// <summary>
    ///   Gets the type of the exception. Contains a value only when <see cref="EventType" />
    ///   is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The type of the exception.</value>
    public string ExType
    {
      get { return _exceptionType; }
    }

    /// <summary>
    /// Gets the source of the exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The source of the exception.</value>
    public string ExSource
    {
      get { return _source; }
    }

    /// <summary>
    /// Gets the target site of the exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The target site of the exception.</value>
    public string ExTargetSite
    {
      get { return _targetSite; }
    }

    /// <summary>
    /// Gets the stack trace of the exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The stack trace of the exception.</value>
    public string ExStackTrace
    {
      get { return _stackTrace; }
    }

    /// <summary>
    /// Gets the type of the inner exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The type of the inner exception.</value>
    public string InnerExType
    {
      get { return _innerExType; }
    }

    /// <summary>
    /// Gets the message of the inner exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The message of the inner exception.</value>
    public string InnerExMessage
    {
      get { return _innerExMessage; }
    }

    /// <summary>
    /// Gets the source of the inner exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The source of the inner exception.</value>
    public string InnerExSource
    {
      get { return _innerExSource; }
    }

    /// <summary>
    ///   Gets the target site of the inner exception. Guaranteed to not be null.
    /// </summary>
    /// <value>The target site of the inner exception.</value>
    public string InnerExTargetSite
    {
      get { return _innerExTargetSite; }
    }

    /// <summary>
    /// Gets the stack trace of the inner exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. Guaranteed to not be null.
    /// </summary>
    /// <value>The stack trace of the inner exception.</value>
    public string InnerExStackTrace
    {
      get { return _innerExStackTrace; }
    }

    /// <summary>
    /// Gets the URL of the page where the event occurred. Guaranteed to not be null.
    /// </summary>
    /// <value>The URL of the page where the event occurred.</value>
    public string Url
    {
      get { return (!String.IsNullOrEmpty(_url) ? _url : Resources.Err_Missing_Data_Txt); }
    }

    /// <summary>
    /// Gets the HTTP user agent where the event occurred. Guaranteed to not be null.
    /// </summary>
    /// <value>The HTTP user agent where the event occurred.</value>
    public string HttpUserAgent
    {
      get
      {
        KeyValuePair<string, string> httpUserAgent = _serverVariables.Find(delegate (KeyValuePair<string, string> kvp) { return (String.Compare(kvp.Key, "HTTP_USER_AGENT", StringComparison.OrdinalIgnoreCase) == 0); });

        return httpUserAgent.Value ?? Resources.Err_Missing_Data_Txt;
      }
    }

    /// <summary>
    /// Gets the data associated with the inner exception. Contains a value only when <see cref="EventType" />
    /// is <see cref="Business.EventType.Error" />. This is extracted from <see cref="System.Exception.Data" />.
    /// Guaranteed to not be null.
    /// </summary>
    /// <value>The data associate with the inner exception.</value>
    public ReadOnlyCollection<KeyValuePair<string, string>> InnerExData
    {
      get { return _innerExData.AsReadOnly(); }
    }

    /// <summary>
    /// Gets the form variables from the web page where the event occurred. Guaranteed to not be null.
    /// </summary>
    /// <value>The form variables from the web page where the event occurred.</value>
    public ReadOnlyCollection<KeyValuePair<string, string>> FormVariables
    {
      get { return _formVariables.AsReadOnly(); }
    }

    /// <summary>
    /// Gets the cookies from the web page where the event occurred. Guaranteed to not be null.
    /// </summary>
    /// <value>The cookies from the web page where the event occurred.</value>
    public ReadOnlyCollection<KeyValuePair<string, string>> Cookies
    {
      get { return _cookies.AsReadOnly(); }
    }

    /// <summary>
    /// Gets the session variables from the web page where the event occurred. Guaranteed to not be null.
    /// </summary>
    /// <value>The session variables from the web page where the event occurred.</value>
    public ReadOnlyCollection<KeyValuePair<string, string>> SessionVariables
    {
      get { return _sessionVariables.AsReadOnly(); }
    }

    /// <summary>
    /// Gets the server variables from the web page where the event occurred. Guaranteed to not be null.
    /// </summary>
    /// <value>The server variables from the web page where the event occurred.</value>
    public ReadOnlyCollection<KeyValuePair<string, string>> ServerVariables
    {
      get { return _serverVariables.AsReadOnly(); }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Event" /> class.
    /// </summary>
    /// <param name="msg">The event message to record.</param>
    /// <param name="data">Additional optional data to record. May be null.</param>
    /// <param name="galleryId">The ID of the gallery the <paramref name="msg" /> is associated with. If it is not specific to a gallery
    ///   or the gallery ID is unknown, specify the ID for the template gallery.</param>
    /// <param name="eventType">Type of the event. Defaults to <see cref="Business.EventType.Info" /> when not specified.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId" /> is <see cref="Int32.MinValue" />.</exception>
    internal Event(string msg, int galleryId, EventType eventType = EventType.Info, Dictionary<string, string> data = null)
    {
      if (galleryId == int.MinValue)
        throw new ArgumentOutOfRangeException("galleryId", string.Format("The galleryId parameter in the Event contructor must represent an existing gallery. Instead, it was {0}", galleryId));

      EventId = int.MinValue;
      EventType = eventType;
      _message = msg;
      _galleryId = galleryId;
      _timeStampUtc = DateTime.UtcNow;

      _exceptionType = String.Empty;
      _source = String.Empty;
      _targetSite = String.Empty;
      _stackTrace = String.Empty;

      _innerExType = String.Empty;
      _innerExMessage = String.Empty;
      _innerExSource = String.Empty;
      _innerExTargetSite = String.Empty;
      _innerExStackTrace = String.Empty;

      if (data != null)
      {
        foreach (var dataItem in data)
        {
          _eventData.Add(dataItem);
        }
      }

      ExtractVersion();

      ExtractHttpContextInfo();
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="Event" /> class.
    /// </summary>
    /// <param name="ex">The exception to use as the source for a new instance of this object.</param>
    /// <param name="galleryId">
    ///   The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
    ///   If the exception is not specific to a gallery or the gallery ID is unknown, specify the ID for the
    ///   template gallery.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ex" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="galleryId" /> is <see cref="Int32.MinValue" />.</exception>
    internal Event(Exception ex, int galleryId)
    {
      if (ex == null)
        throw new ArgumentNullException("ex");

      if (galleryId == int.MinValue)
        throw new ArgumentOutOfRangeException("galleryId", string.Format("The galleryId parameter in the Event contructor must represent an existing gallery. Instead, it was {0}", galleryId));

      EventId = int.MinValue;
      EventType = EventType.Error;
      _galleryId = galleryId;
      _timeStampUtc = DateTime.UtcNow;

      var missingDataText = Resources.Err_Missing_Data_Txt;

      _exceptionType = ex.GetType().ToString();
      _message = ex.Message;
      _source = ex.Source ?? missingDataText;
      _targetSite = (ex.TargetSite == null) ? missingDataText : ex.TargetSite.ToString();
      _stackTrace = ex.StackTrace ?? missingDataText;

      foreach (DictionaryEntry entry in ex.Data)
      {
        string value = (entry.Value != null ? entry.Value.ToString() : String.Empty);
        _eventData.Add(new KeyValuePair<string, string>(entry.Key.ToString(), value));
      }

      var valEx = ex as DbEntityValidationException;
      if (valEx != null)
      {
        int i = 1, j = 1;
        foreach (DbEntityValidationResult valErr in valEx.EntityValidationErrors)
        {
          var msg1 = String.Format(CultureInfo.InvariantCulture, "Entity {0} ({1}) IsValid={2}", valErr.Entry.Entity.GetType(), valErr.Entry.State, valErr.IsValid);
          _eventData.Add(new KeyValuePair<string, string>(String.Concat("Entity Validation Error #", i++), msg1));

          foreach (DbValidationError dbValErr in valErr.ValidationErrors)
          {
            string msg2 = String.Format(CultureInfo.InvariantCulture, "{0} - {1}", dbValErr.PropertyName, dbValErr.ErrorMessage);
            _eventData.Add(new KeyValuePair<string, string>(String.Concat("Validation Error #", j++), msg2));
          }
        }
      }

      var innerEx = ex.InnerException;
      if (innerEx == null)
      {
        _innerExType = String.Empty;
        _innerExMessage = String.Empty;
        _innerExSource = String.Empty;
        _innerExTargetSite = String.Empty;
        _innerExStackTrace = String.Empty;
      }
      else
      {
        var innerExCounter = 0;
        while (innerEx != null)
        {
          innerExCounter++;

          if (innerExCounter == 1)
          {
            // This is the first inner exception.
            _innerExType = innerEx.GetType().ToString();
            _innerExMessage = innerEx.Message ?? missingDataText;
            _innerExSource = innerEx.Source ?? missingDataText;
            _innerExTargetSite = (innerEx.TargetSite == null) ? missingDataText : innerEx.TargetSite.ToString();
            _innerExStackTrace = innerEx.StackTrace ?? missingDataText;

            foreach (DictionaryEntry entry in innerEx.Data)
            {
              _innerExData.Add(new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString()));
            }
          }
          else
          {
            // The inner exception has one or more of its own inner exceptions. Add this data to the existing inner exception fields.
            _innerExType = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", _innerExType, Environment.NewLine, innerExCounter, innerEx.GetType());
            _innerExMessage = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", _innerExMessage, Environment.NewLine, innerExCounter, innerEx.Message);
            _innerExSource = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", _innerExSource, Environment.NewLine, innerExCounter, innerEx.Source ?? missingDataText);
            _innerExTargetSite = String.Format(CultureInfo.InvariantCulture, "{0};{1} Inner ex #{2}: {3}", _innerExTargetSite, Environment.NewLine, innerExCounter, (innerEx.TargetSite == null) ? missingDataText : innerEx.TargetSite.ToString());
            _innerExStackTrace = String.Format(CultureInfo.InvariantCulture, "{0}{0};{1} Inner ex #{2}: {3}", _innerExStackTrace, Environment.NewLine, innerExCounter, innerEx.StackTrace ?? missingDataText);

            foreach (DictionaryEntry entry in innerEx.Data)
            {
              string key = String.Format(CultureInfo.InvariantCulture, "Inner ex #{0} data: {1}", innerExCounter, entry.Key);
              _innerExData.Add(new KeyValuePair<string, string>(key, entry.Value.ToString()));
            }
          }

          innerEx = innerEx.InnerException;
        }
      }

      ExtractVersion();

      ExtractHttpContextInfo();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Event" /> class.
    /// </summary>
    /// <param name="eventId">The app event ID.</param>
    /// <param name="eventType">Type of the event.</param>
    /// <param name="galleryId">The gallery ID.</param>
    /// <param name="timeStamp">The time stamp.</param>
    /// <param name="exType">Type of the exception.</param>
    /// <param name="message">The message.</param>
    /// <param name="eventData">The exception data.</param>
    /// <param name="source">The source.</param>
    /// <param name="targetSite">The target site.</param>
    /// <param name="stackTrace">The stack trace.</param>
    /// <param name="innerExType">Type of the inner exception.</param>
    /// <param name="innerExMessage">The inner exception message.</param>
    /// <param name="innerExSource">The inner exception source.</param>
    /// <param name="innerExTargetSite">The inner exception target site.</param>
    /// <param name="innerExStackTrace">The inner exception stack trace.</param>
    /// <param name="innerExData">The inner exception data.</param>
    /// <param name="url">The URL where the exception occurred.</param>
    /// <param name="formVariables">The form variables.</param>
    /// <param name="cookies">The cookies.</param>
    /// <param name="sessionVariables">The session variables.</param>
    /// <param name="serverVariables">The server variables.</param>
    internal Event(int eventId, EventType eventType, int galleryId, DateTime timeStamp, string exType, string message, List<KeyValuePair<string, string>> eventData, string source, string targetSite, string stackTrace, string innerExType, string innerExMessage, string innerExSource, string innerExTargetSite, string innerExStackTrace, List<KeyValuePair<string, string>> innerExData, string url, List<KeyValuePair<string, string>> formVariables, List<KeyValuePair<string, string>> cookies, List<KeyValuePair<string, string>> sessionVariables, List<KeyValuePair<string, string>> serverVariables)
    {
      EventId = eventId;
      EventType = eventType;
      _galleryId = galleryId;
      _timeStampUtc = timeStamp;
      _exceptionType = exType;
      _message = message;
      _source = source;
      _targetSite = targetSite;
      _stackTrace = stackTrace;
      _eventData = eventData;
      _innerExType = innerExType;
      _innerExMessage = innerExMessage;
      _innerExSource = innerExSource;
      _innerExTargetSite = innerExTargetSite;
      _innerExStackTrace = innerExStackTrace;
      _innerExData = innerExData;
      _url = url;
      _formVariables = formVariables;
      _cookies = cookies;
      _sessionVariables = sessionVariables;
      _serverVariables = serverVariables;
    }

    #endregion

    #region Public Methods

    /// <summary>
    ///   Formats the name of the specified <paramref name="item" /> into an HTML paragraph tag. Example: If
    ///   <paramref name="item" /> = ErrorItem.StackTrace, the text "Stack Trace" is returned as the content of the tag.
    /// </summary>
    /// <param name="item">The enum value to be used as the content of the paragraph element. It is HTML encoded.</param>
    /// <returns>Returns an HTML paragraph tag.</returns>
    public string ToHtmlName(EventItem item)
    {
      return ToHtmlParagraph(EventController.GetFriendlyEnum(item));
    }

    /// <summary>
    ///   Formats the value of the specified <paramref name="item" /> into an HTML paragraph tag. Example: If
    ///   <paramref name="item" /> = ErrorItem.StackTrace, the action stack trace data associated with the current error
    ///   is returned as the content of the tag. If present, line breaks (\r\n) are converted to &lt;br /&gt; tags.
    /// </summary>
    /// <param name="item">
    ///   The enum value indicating the error item to be used as the content of the paragraph element.
    ///   The text is HTML encoded.
    /// </param>
    /// <returns>Returns an HTML paragraph tag.</returns>
    public string ToHtmlValue(EventItem item)
    {
      switch (item)
      {
        case EventItem.EventId:
          return ToHtmlParagraph(EventId.ToString(CultureInfo.InvariantCulture));
        case EventItem.EventType:
          return ToHtmlParagraph(EventType.ToString());
        case EventItem.Url:
          return ToHtmlParagraph(Url);
        case EventItem.Timestamp:
          return ToHtmlParagraph(TimestampUtc.ToString(CultureInfo.CurrentCulture));
        case EventItem.ExType:
          return ToHtmlParagraph(ExType);
        case EventItem.Message:
          return ToHtmlParagraph(Message);
        case EventItem.ExSource:
          return ToHtmlParagraph(ExSource);
        case EventItem.ExTargetSite:
          return ToHtmlParagraph(ExTargetSite);
        case EventItem.ExStackTrace:
          return ToHtmlParagraph(ExStackTrace);
        case EventItem.ExData:
          return ToHtmlParagraphs(EventData);
        case EventItem.InnerExType:
          return ToHtmlParagraph(InnerExType);
        case EventItem.InnerExMessage:
          return ToHtmlParagraph(InnerExMessage);
        case EventItem.InnerExSource:
          return ToHtmlParagraph(InnerExSource);
        case EventItem.InnerExTargetSite:
          return ToHtmlParagraph(InnerExTargetSite);
        case EventItem.InnerExStackTrace:
          return ToHtmlParagraph(InnerExStackTrace);
        case EventItem.InnerExData:
          return ToHtmlParagraphs(InnerExData);
        case EventItem.GalleryId:
          return ToHtmlParagraph(GalleryId.ToString(CultureInfo.InvariantCulture));
        case EventItem.HttpUserAgent:
          return ToHtmlParagraph(HttpUserAgent);
        case EventItem.FormVariables:
          return ToHtmlParagraphs(FormVariables);
        case EventItem.Cookies:
          return ToHtmlParagraphs(Cookies);
        case EventItem.SessionVariables:
          return ToHtmlParagraphs(SessionVariables);
        case EventItem.ServerVariables:
          return ToHtmlParagraphs(ServerVariables);
        default:
          throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Encountered unexpected EventItem enum value {0}. Event.ToHtmlValue() is not designed to handle this enum value. The function must be updated.", item));
      }
    }

    /// <summary>
    ///   Generate HTML containing detailed information about the application event. Does not include the outer html
    ///   and body tag. The HTML may contain references to CSS classes for formatting purposes, so be sure to include
    ///   these CSS definitions in the containing web page.
    /// </summary>
    /// <returns>Returns an HTML formatted string containing detailed information about the event.</returns>
    public string ToHtml()
    {
      var sb = new StringBuilder(20000);

      AddHtmlErrorInfo(sb);

      return sb.ToString();
    }

    /// <summary>
    ///   Generate a complete HTML page containing detailed information about the application event. Includes the outer html
    ///   and body tag, including definitions for the CSS classes that are referenced within the body. Does not depend
    ///   on external style sheets or other resources. This method can be used to generate the body of an HTML e-mail.
    /// </summary>
    /// <returns>Returns an HTML formatted string containing detailed information about the event.</returns>
    public string ToHtmlPage()
    {
      var sb = new StringBuilder(20000);

      sb.AppendLine("<!DOCTYPE html>");
      sb.AppendLine("<html><head></head>");

      sb.AppendLine("<body>");

      sb.AppendLine("<div class=\"gsp_ns\">");

      sb.AppendLine(String.Concat("<p>", Resources.Err_Email_Body_Prefix, "</p>"));

      AddHtmlErrorInfo(sb);

      sb.AppendLine("</div>");

      sb.AppendLine("</body></html>");

      return sb.ToString();
    }

    #region IComparable Members

    /// <summary>
    ///   Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>
    ///   A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than
    ///   <paramref name="obj" />. Zero This instance is equal to <paramref name="obj" />. Greater than zero This instance is greater than
    ///   <paramref name="obj" />.
    /// </returns>
    /// <exception cref="T:System.ArgumentException">
    ///   <paramref name="obj" /> is not the same type as this instance.
    /// </exception>
    public int CompareTo(object obj)
    {
      if (obj == null)
        return -1;

      var other = obj as IEvent;
      if (other != null)
        return -TimestampUtc.CompareTo(other.TimestampUtc);

      return -1;
    }

    #endregion

    /// <summary>
    ///   Create a new instance of <see cref="IEvent" /> from the specified <paramref name="ex" /> and associate it with
    ///   the specified <paramref name="galleryId" />.
    /// </summary>
    /// <param name="ex">The exception to use as the source for a new instance of this object.</param>
    /// <param name="galleryId">The ID of the gallery the <paramref name="ex">exception</paramref> is associated with.
    ///   If the exception is not specific to a gallery or the gallery ID is unknown, specify the ID of the template gallery.
    /// </param>
    /// <returns>Returns an <see cref="IEvent" /> containing information about <paramref name="ex" />.</returns>
    public static IEvent Create(Exception ex, int galleryId)
    {
      return new Event(ex, galleryId);
    }

    #endregion

    #region Private Methods

    /// <summary>
    ///   Extract information from the current HTTP context and assign to member variables.
    /// </summary>
    private void ExtractHttpContextInfo()
    {
      if (HttpContext.Current == null)
      {
        _url = "Not Available";
        return;
      }

      try
      {
        _url = HttpContext.Current.Request.Url.AbsoluteUri;
      }
      catch (HttpException)
      {
        _url = "Not Available";
        return;
      }

      var form = HttpContext.Current.Request.Form;
      for (int i = 0; i < form.Count; i++)
      {
          if (form.Keys[i].IndexOf("password", StringComparison.OrdinalIgnoreCase) < 0) // Don't include any form variables containing the string password. This prevents sensitive data from being recorded.
          {
              _formVariables.Add(new KeyValuePair<string, string>(form.Keys[i], form[i]));
          }
      }

      var cookies = HttpContext.Current.Request.Cookies;
      foreach (string item in cookies)
      {
        var cookie = cookies[item];
        if (cookie != null)
          _cookies.Add(new KeyValuePair<string, string>(cookie.Name, cookie.Value));
      }

      var session = HttpContext.Current.Session;
      if (session != null)
      {
        foreach (string item in session)
        {
          _sessionVariables.Add(new KeyValuePair<string, string>(item, session[item].ToString()));
        }
      }

      var serverVariables = HttpContext.Current.Request.ServerVariables;
      for (var i = 0; i < serverVariables.Count; i++)
      {
        var key = serverVariables.Keys[i];
        var value = serverVariables[i];
        var serverVarsToSkip = new[] { "ALL_HTTP", "ALL_RAW" };

        // Skip empty values & "ALL_HTTP" & "ALL_RAW" because their values are itemized in the rest of the collection
        if (String.IsNullOrWhiteSpace(value) || Array.IndexOf(serverVarsToSkip, key) >= 0)
          continue;

        _serverVariables.Add(new KeyValuePair<string, string>(serverVariables.Keys[i], value));
      }
    }

    /// <summary>
    /// Formats the specified <paramref name="item" /> into an HTML paragraph tag with a class attribute of
    /// <paramref name="cssClassName" />. The string representation of <paramref name="item" />
    /// is extracted from a resource file and will closely resemble the enum value. Example: If <paramref name="item" /> = ErrorItem.StackTrace,
    /// the text "Stack Trace" is used.
    /// </summary>
    /// <param name="item">The enum value to be used as the content of the paragraph element. It is HTML encoded.</param>
    /// <param name="cssClassName">The name of the CSS class to assign to the paragraph element.</param>
    /// <param name="cssStyle">The inline CSS style to assign to the paragraph element.</param>
    /// <returns>Returns an HTML paragraph tag.</returns>
    private static string ToHtmlParagraph(EventItem item, string cssClassName, string cssStyle)
    {
      return ToHtmlParagraph(EventController.GetFriendlyEnum(item), cssClassName, cssStyle);
    }

    /// <summary>
    ///   Formats the specified string into an HTML paragraph tag with a class attribute of <paramref name="cssClassName" />.
    /// </summary>
    /// <param name="str">The string to be assigned as the content of the paragraph element. It is HTML encoded.</param>
    /// <param name="cssClassName">The name of the CSS class to assign to the paragraph element. Defaults to "gsp_event_item"
    /// when not specified.</param>
    /// <param name="cssStyle">The inline CSS style to assign to the paragraph element.</param>
    /// <returns>Returns an HTML paragraph tag.</returns>
    private static string ToHtmlParagraph(string str, string cssClassName = "gsp_event_item", string cssStyle = "margin:0 0 0.4em 0; padding:0.4em 0 0 0;")
    {
      return $"<p class='{cssClassName}' style='{cssStyle}'>{HtmlEncode(str)}</p>";
    }

    private static string HtmlEncode(string str)
    {
      return (str == null ? null : HttpUtility.HtmlEncode(str).Replace("\r\n", "<br />"));
    }

    /// <summary>
    ///   Formats the <paramref name="list" /> into one or more HTML paragraph tags where the key and value of each item are
    ///   concatenated with a colon between them (e.g. &lt;p class='gsp_event_item'&gt;HTTP_HOST: localhost.&lt;/p&gt;)
    ///   A CSS class named "gsp_event_item" is automatically assigned to each paragraph element. The value property of
    ///   each collection item is processed so that it contains a space character every 70 characters or so. This is
    ///   required to allow HTML rendering engines to wrap the text. Guaranteed to return at least one paragraph
    ///   element. If <paramref name="list" /> is null or does not contain any items, a single paragraph element is
    ///   returned containing a string indicating there are not any items (e.g. "&lt;none&gt;")
    /// </summary>
    /// <param name="list">
    ///   The list containing the items to convert to HTML paragraph tags. The key and value of
    ///   each collection item is HTML encoded.
    /// </param>
    /// <returns>Returns one or more HTML paragraph tags.</returns>
    private static string ToHtmlParagraphs(ICollection<KeyValuePair<string, string>> list)
    {
      if ((list == null) || (list.Count == 0))
        return ToHtmlParagraph(Resources.Err_No_Data_Txt);

      if (list.Count > 6)
      {
        var sb = new StringBuilder();
        foreach (var pair in list)
        {
          sb.AppendLine("<p class='gsp_event_item'>");
          sb.AppendLine(HtmlEncode(String.Concat(pair.Key, ": ", MakeHtmlLineWrapFriendly(pair.Value))));
          sb.AppendLine("</p>");
        }

        return sb.ToString();
      }

      var listString = String.Empty;
      foreach (var pair in list)
      {
        listString += String.Concat("<p class='gsp_event_item'>", HtmlEncode(String.Concat(pair.Key, ": ", MakeHtmlLineWrapFriendly(pair.Value))), "</p>");
      }

      return listString;
    }

    /// <overloads>Formats the data into an HTML table.</overloads>
    /// <summary>
    ///   Formats the <paramref name="item" /> into an HTML table. Valid only for <see cref="EventItem" /> values that are collections.
    /// </summary>
    /// <param name="item">
    ///   The item to format into an HTML table. Must be one of the following enum values: FormVariables, Cookies,
    ///   SessionVariables, ServerVariables
    /// </param>
    /// <returns>Returns an HTML table.</returns>
    private string ToHtmlTable(EventItem item)
    {
      string htmlValue;

      switch (item)
      {
        case EventItem.FormVariables:
          htmlValue = ToHtmlTable(FormVariables);
          break;
        case EventItem.Cookies:
          htmlValue = ToHtmlTable(Cookies);
          break;
        case EventItem.SessionVariables:
          htmlValue = ToHtmlTable(SessionVariables);
          break;
        case EventItem.ServerVariables:
          htmlValue = ToHtmlTable(ServerVariables);
          break;
        default:
          throw new BusinessException(String.Format(CultureInfo.CurrentCulture, "Encountered unexpected EventItem enum value {0}. Event.ToHtmlTable() is not designed to handle this enum value. The function must be updated.", item));
      }

      return htmlValue;
    }

    /// <summary>
    ///   Formats the <paramref name="list" /> into a two-column HTML table where the first column contains the key and the second
    ///   contains the value. The table is assigned the CSS class "gsp_event_table"; each table cell in the first column has a CSS
    ///   class "gsp_event_col1", the second column has a CSS class "gsp_event_col2". Each cell contains a paragraph tag with a CSS
    ///   class "gsp_event_item" and the paragraphs content is either the key or value of the list item. If <paramref name="list" />
    ///   is null or doesn't contain any items, return a one-cell table with a message indicating there isn't any data (e.g. "&lt;none&gt;").
    /// </summary>
    /// <param name="list">The list to format into an HTML table. Keys and values are HTML encoded.</param>
    /// <returns>Returns an HTML table.</returns>
    private static string ToHtmlTable(ICollection<KeyValuePair<string, string>> list)
    {
      if ((list == null) || (list.Count == 0))
      {
        // No items. Just build simple table with message indicating there isn't any data.
        return String.Format(CultureInfo.InvariantCulture, @"
<table cellpadding='0' cellspacing='0' class='gsp_event_table' style='width:100%;border:1px solid #cdc9c2;'>
 <tr><td style='padding:4px;'>{0}</td></tr>
</table>", ToHtmlParagraph(Resources.Err_No_Data_Txt));
      }

      if (list.Count > 6)
      {
        // More than 6 items. Use StringBuilder when dealing with lots of items.
        var sb = new StringBuilder();
        sb.AppendLine("<table cellpadding='0' cellspacing='0' class='gsp_event_table' style='width:100%;border:1px solid #cdc9c2;'>");
        foreach (var pair in list)
        {
          sb.Append($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlParagraph(pair.Key)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlParagraph(MakeHtmlLineWrapFriendly(pair.Value))}</td></tr>\n");
        }
        sb.AppendLine("</table>");

        return sb.ToString();
      }

      // list contains between 1 and 6 items. Use standard string concatenation to build table
      var listString = "<table cellpadding='0' cellspacing='0' class='gsp_event_table' style='width:100%;border:1px solid #cdc9c2;'>";
      foreach (var pair in list)
      {
        listString += $"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlParagraph(pair.Key)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlParagraph(MakeHtmlLineWrapFriendly(pair.Value))}</td></tr>\n";
      }
      listString += "</table>";

      return listString;
    }

    /// <summary>
    ///   Add HTML formatted text to <paramref name="sb" /> that contains information about the current error.
    /// </summary>
    /// <param name="sb">The StringBuilder to add HTML data to.</param>
    private void AddHtmlErrorInfo(StringBuilder sb)
    {
      sb.AppendLine(ToHtmlParagraph(String.Concat((EventType == EventType.Error ? Resources.Err_Msg_Label : String.Empty), " ", Message), "gsp_event_h1", "margin:.5em 0 .5em 0;color:#800;font-size: 1.4em;"));

      AddHtmlEventSummary(sb);

      AddEventSection(sb, EventItem.FormVariables);

      AddEventSection(sb, EventItem.Cookies);

      AddEventSection(sb, EventItem.SessionVariables);

      AddEventSection(sb, EventItem.ServerVariables);
    }

    /// <summary>
    ///   Add HTML formatted text to <paramref name="sb" /> that contains summary information about the current error.
    /// </summary>
    /// <param name="sb">The StringBuilder to add HTML data to.</param>
    private void AddHtmlEventSummary(StringBuilder sb)
    {
      sb.AppendLine(ToHtmlParagraph(Resources.Err_Summary, "gsp_event_h2", "background-color:#cdc9c2;font-size: 1.2em; font-weight: bold;margin:1em 0 0 0;padding:.4em 0 .4em 4px;"));
      sb.AppendLine("<table cellpadding='0' cellspacing='0' class='gsp_event_table' style='width:100%;border:1px solid #cdc9c2;'>");

      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.Url)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.Url)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.Timestamp)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.Timestamp)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.ExType)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.ExType)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.Message)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.Message)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.ExSource)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.ExSource)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.ExTargetSite)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.ExTargetSite)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.ExStackTrace)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.ExStackTrace)}</td></tr>");

      if (EventData.Count > 0)
        sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.ExData)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.ExData)}</td></tr>");

      if (!String.IsNullOrEmpty(InnerExType))
        sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.InnerExType)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.InnerExType)}</td></tr>");

      if (!String.IsNullOrEmpty(InnerExMessage))
        sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.InnerExMessage)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.InnerExMessage)}</td></tr>");

      if (!String.IsNullOrEmpty(InnerExSource))
        sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.InnerExSource)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.InnerExSource)}</td></tr>");

      if (!String.IsNullOrEmpty(InnerExTargetSite))
        sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.InnerExTargetSite)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.InnerExTargetSite)}</td></tr>");

      if (!String.IsNullOrEmpty(InnerExStackTrace))
        sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.InnerExStackTrace)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.InnerExStackTrace)}</td></tr>");

      if (InnerExData.Count > 0)
        sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.InnerExData)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.InnerExData)}</td></tr>");

      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.EventId)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.EventId)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.GalleryId)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.GalleryId)}</td></tr>");
      sb.AppendLine($"<tr><td class='gsp_event_col1' style='{gsp_event_col1_style}'>{ToHtmlName(EventItem.HttpUserAgent)}</td><td class='gsp_event_col2' style='{gsp_event_col2_style}'>{ToHtmlValue(EventItem.HttpUserAgent)}</td></tr>");

      sb.AppendLine("</table>");
    }

    /// <summary>
    ///   Guarantee that <paramref name="value" /> contains a space character at least every 70 characters, inserting one if necessary.
    ///   Use this function to prepare text that will be sent to an HTML rendering engine. Without a space character to assist the
    ///   engine with line breaks, the text may be rendered in a single line, forcing the user to scroll to the right.
    /// </summary>
    /// <param name="value">The string to process.</param>
    /// <returns>
    ///   Returns <paramref name="value" /> with a space character inserted as needed.
    /// </returns>
    private static string MakeHtmlLineWrapFriendly(string value)
    {
      const int maxLineLength = 70;
      int numCharsSinceSpace = 0;

      if (String.IsNullOrEmpty(value))
        return String.Empty;

      if (value.Length < maxLineLength)
        return value;

      var sb = new StringBuilder(value.Length + 20);

      foreach (var ch in value)
      {
        sb.Append(ch);

        if (numCharsSinceSpace > maxLineLength)
        {
          sb.Append(" ");
          numCharsSinceSpace = 0;
        }

        numCharsSinceSpace++;

        if (char.IsWhiteSpace(ch))
          numCharsSinceSpace = 0;
      }

      return sb.ToString();
    }

    /// <summary>
    ///   Add an HTML formatted section to <paramref name="sb" /> with data related to <paramref name="item" />.
    /// </summary>
    /// <param name="sb">The StringBuilder to add HTML data to.</param>
    /// <param name="item">The <see cref="EventItem" /> value specifying the error section to build.</param>
    private void AddEventSection(StringBuilder sb, EventItem item)
    {
      sb.AppendLine(ToHtmlParagraph(item, "gsp_event_h2", "background-color:#cdc9c2;font-size: 1.2em; font-weight: bold;margin:1em 0 0 0;padding:.4em 0 .4em 4px;"));
      sb.AppendLine(ToHtmlTable(item));
    }

    private void ExtractVersion()
    {
      _eventData.Add(new KeyValuePair<string, string>("Version", GalleryDataSchemaVersionEnumHelper.ConvertGalleryDataSchemaVersionToString(Data.GalleryDb.DataSchemaVersion)));
    }

    #endregion
  }
}