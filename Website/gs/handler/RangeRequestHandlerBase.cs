using System;
using System.Globalization;
using System.IO;
using System.Web;

/// <summary>
/// An abstract HTTP Handler that provides range support and resumable file downloads in ASP.NET.
/// </summary>
/// <remarks>
/// Gallery Server uses a heavily modified version of the handler created by:
/// 
///			Scott Mitchell (mitchell@4guysfromrolla.com)
///     http://www.4guysfromrolla.com/ScottMitchell.shtml
///			http://dotnetslackers.com/articles/aspnet/Range-Specific-Requests-in-ASP-NET.aspx
/// 
/// His version supported only physical files, so the main modification was to add support for in-memory
/// streams. There is also quite a bit of refactoring to match my own coding standards and style.
/// Scott's version was a fairly close port of Alexander Schaaf's ZIPHandler HTTP Handler, which is at:
/// 
///     Tracking and Resuming Large File Downloads in ASP.NET
///     http://www.devx.com/dotnet/Article/22533/1954
/// 
/// Scott also found a similar version of this code in the download for the September 2006 issue of MSDN Magazine:
/// http://download.microsoft.com/download/f/2/7/f279e71e-efb0-4155-873d-5554a0608523/MSDNMag2006_09.exe
/// 
/// Alexander's code is in Visual Basic and was written for ASP.NET version 1.x. Scott ported the code to C#,
/// refactored portions of the code, and made use of functionality and features introduced in .NET 2.0 and 3.5.
/// </remarks>
public abstract class RangeRequestHandlerBase : IHttpHandler
{
	#region Constants

	private const string MultipartBoundary = "<q1w2e3r4t5y6u7i8o9p0>";
	private const string MultipartContenttype = "multipart/byteranges; boundary=" + MultipartBoundary;
	private const string DefaultContentType = "application/octet-stream";
	private const string HttpHeaderAcceptRanges = "Accept-Ranges";
	private const string HttpHeaderAcceptRangesBytes = "bytes";
	private const string HttpHeaderAcceptRangesNone = "none";
	private const string HttpHeaderContentType = "Content-Type";
	private const string HttpHeaderContentRange = "Content-Range";
	private const string HttpHeaderContentLength = "Content-Length";
	private const string HttpHeaderEntityTag = "ETag";
	private const string HttpHeaderContentDispositionTag = "Content-Disposition";
	private const string HttpHeaderLastModified = "Last-Modified";
	private const string HttpHeaderRange = "Range";
	private const string HttpHeaderIfRange = "If-Range";
	private const string HttpHeaderIfMatch = "If-Match";
	private const string HttpHeaderIfNoneMatch = "If-None-Match";
	private const string HttpHeaderIfModifiedSince = "If-Modified-Since";
	private const string HttpHeaderIfUnmodifiedSince = "If-Unmodified-Since";
	private const string HttpHeaderUnlessModifiedSince = "Unless-Modified-Since";
	private const string HttpMethodGet = "GET";
	private const string HttpMethodHead = "HEAD";

	//private const int DebuggingSleepTime = 0;

	#endregion

	#region Enumerations

	/// <summary>
	/// Specifies whether a resource is sent inline or as an attachment.
	/// </summary>
	protected enum ResponseHeaderContentDisposition
	{
		/// <summary>
		/// Specifies that a resource is to be sent inline to the browser.
		/// </summary>
		Inline,

		/// <summary>
		/// Specifies that a resource is to be sent as an attachment to the browser.
		/// </summary>
		Attachment
	}

	#endregion

	#region Constructor

	/// <summary>
	/// Initializes a new instance of the <see cref="RangeRequestHandlerBase"/> class.
	/// </summary>
	protected RangeRequestHandlerBase()
	{
		ProcessRequestFunctions =
				new Func<bool>[]
						{
								CheckAuthorizationRules,
								CheckHttpMethod,
								CheckRangesRequested,
								CheckIfModifiedSinceHeader,
								CheckIfUnmodifiedSinceHeader,
								CheckIfMatchHeader,
								CheckIfNoneMatchHeader,
								CheckIfRangeHeader
						};
	}

	#endregion

	#region Fields

	private HttpContext _context;

	#endregion

	#region Properties

	/// <summary>
	/// Indicates if the HTTP request is for multiple ranges.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if this HTTP request is for multiple ranges; otherwise, <c>false</c>.
	/// </value>
	public bool IsMultipartRequest { get; private set; }

	/// <summary>
	/// Indicates if the HTTP request is for one or more ranges.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if this HTTP request is for one or more ranges; otherwise, <c>false</c>.
	/// </value>
	public bool IsRangeRequest { get; private set; }

	/// <summary>
	/// The start byte(s) for the requested range(s).
	/// </summary>
	/// <value>The start byte(s) for the requested range(s).</value>
	public long[] StartRangeBytes { get; private set; }

	/// <summary>
	/// The end byte(s) for the requested range(s).
	/// </summary>
	/// <value>The end byte(s) for the requested range(s).</value>
	public long[] EndRangeBytes { get; private set; }

	/// <summary>
	/// The size of each chunk of data streamed back to the client. Defaults to 10240. Override to set
	/// a different value.
	/// </summary>
	/// <value>The size of each chunk of data streamed back to the client.</value>
	/// <remarks>
	/// When a client makes a range request the requested file's contents are
	/// read in <see cref="BufferSize" /> chunks, with each chunk flushed to the output stream
	/// until the requested byte range has been read.
	/// </remarks>
	public virtual int BufferSize { get { return 10240; } }

	/// <summary>
	/// Gets the value to use for the response's content disposition. Default value is
	/// <see cref="ResponseHeaderContentDisposition.Inline" />.
	/// </summary>
	/// <value>The value to use for the response's content disposition.</value>
	protected virtual ResponseHeaderContentDisposition ContentDisposition { get { return ResponseHeaderContentDisposition.Inline; } }

	/// <summary>
	/// Gets the name of the requested file. Used to set the Content-Disposition response header. Specify
	/// <see cref="String.Empty" /> or null if no file name is applicable.
	/// </summary>
	/// <value>
	/// 	A <see cref="System.String" /> instance, or null if no file name is applicable.
	/// </value>
	public virtual string FileName { get { return String.Empty; } }

	///// <summary>
	///// Indicates the path to the log file that records HTTP request and response headers.
	///// </summary>
	///// <remarks>
	///// The log is only enabled when the application is executing in Debug mode.
	///// </remarks>
	//public virtual string LogFileName { get { return "~/ResumableFileDownloadHandler.log"; } }

	/// <summary>
	/// Indicates whether Range requests are enabled. If false, the HTTP Handler
	/// ignores the Range HTTP Header and returns the entire contents.
	/// </summary>
	public virtual bool EnableRangeRequests { get { return true; } }

	/// <summary>
	/// Gets or sets the functions to execute against the request before the requested resource is sent.
	/// </summary>
	/// <value>An array of functions that each return a bool indicating whether processing should continue.</value>
	private Func<bool>[] ProcessRequestFunctions { get; set; }

	/// <summary>
	/// Gets or sets the length of the requested resource.
	/// </summary>
	/// <value>Returns a long.</value>
	private long ResourceLength { get; set; }

	/// <summary>
	/// Gets or sets the timestamp of the last write time of the requested resource. Specify 
	/// <see cref="DateTime.MinValue" /> for a dynamically created resource.
	/// </summary>
	/// <value>A <see cref="DateTime" /> instance.</value>
	private DateTime ResourceLastWriteTimeUtc { get; set; }

	/// <summary>
	/// Gets or sets the Entity Tag (ETag) for the requested content. Returns an empty string if an ETag value
	/// is not applicable.
	/// </summary>
	/// <value>A <see cref="System.String" /> instance.</value>
	private string ResourceFileEntityTag { get; set; }

	/// <summary>
	/// Gets or sets the MIME type for the requested content (e.g. image/jpeg, video/quicktime).
	/// </summary>
	/// <value>A <see cref="System.String" />.</value>
	private string ResourceMimeType { get; set; }

	//private readonly NameValueCollection _internalResponseHeaders = new NameValueCollection();

	#endregion

	#region Methods

	/// <summary>
	/// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
	/// </summary>
	/// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
	public void ProcessRequest(HttpContext context)
	{
		try
		{
			if (InitializeRequest(context) == false)
				return;

			if (CheckResourceRequested() == false)
				return;

			ResourceLength = GetResourceLength();
			ResourceLastWriteTimeUtc = GetResourceLastWriteTimeUtc();
			ResourceFileEntityTag = GetResourceFileEntityTag();
			ResourceMimeType = GetResourceMimeType();

#if DEBUG
			//LogRequestHttpHeaders(context.Server.MapPath(this.LogFileName), context.Request);
#endif

			// Parse the Range header (if it exists), populating the StartRangeBytes and EndRangeBytes arrays
			CalculateRequestRange();

			// Perform each check; exit if any check returns false
			foreach (var procRequestFunc in ProcessRequestFunctions)
				if (procRequestFunc() == false)
				{
#if DEBUG
					//LogResponseHttpHeaders(context.Server.MapPath(this.LogFileName), context.Response);
#endif
					return;
				}

			// Checks passed, process request!
			if (EnableRangeRequests && IsRangeRequest)
				ReturnPartialEntity();
			else
				ReturnEntireEntity();
		}
		finally
		{
			CleanUpResources();
		}
	}

	/// <summary>
	/// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
	/// </summary>
	/// <value></value>
	/// <returns>true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.</returns>
	public bool IsReusable { get { return false; } }

	/// <summary>
	/// Gets a <see cref="Stream" /> object representing the requested content.
	/// </summary>
	/// <returns>Returns a <see cref="Stream" /> instance.</returns>
	/// <remarks>The derived class should dispose of any Stream resources by overriding the 
	/// <see cref="CleanUpResources" /> method.</remarks>
	public abstract Stream GetResourceStream();

	/// <summary>
	/// Gets the length of the requested resource.
	/// </summary>
	/// <returns>Returns a long.</returns>
	public abstract long GetResourceLength();

	/// <summary>
	/// Gets the timestamp of the last write time of the requested resource. Returns <see cref="DateTime.MinValue" />
	/// for a dynamically created resource.
	/// </summary>
	/// <returns>A <see cref="DateTime" /> instance.</returns>
	/// <remarks>A derived class should override this method for optimum browser caching performance.</remarks>
	public virtual DateTime GetResourceLastWriteTimeUtc()
	{
		return DateTime.UtcNow;
	}

	/// <summary>
	/// Returns the Entity Tag (ETag) for the requested content. Returns an empty string if an ETag value
	/// is not applicable or if the derived class does not provide an implementation.
	/// </summary>
	/// <returns>A <see cref="System.String" /> instance.</returns>
	/// <remarks>A derived class should override this method for optimum browser caching performance.</remarks>
	public virtual string GetResourceFileEntityTag()
	{
		return String.Empty;
	}

	/// <summary>
	/// Cleans up resources. This is called in a finally block at the end of the <see cref="ProcessRequest" />
	/// method.
	/// </summary>
	/// <remarks>A derived class may override this method to dispose of resources such as the Stream object.</remarks>
	public virtual void CleanUpResources()
	{
	}

	/// <summary>
	/// Returns the MIME type for the requested content (e.g. image/jpeg, video/quicktime).
	/// </summary>
	/// <returns>A <see cref="System.String" />.</returns>
	/// <remarks>
	/// A dervied class SHOULD override this method and return the MIME type specific
	/// to the requested content.
	/// </remarks>
	public virtual string GetResourceMimeType()
	{
		return DefaultContentType;
	}

	/// <summary>
	/// Initializes the request. A derived class may override this method as needed.
	/// </summary>
	/// <param name="context">The HTTP context.</param>
	/// <returns>Returns <c>true</c> when the method succeeds; otherwise <c>false</c>.</returns>
	public virtual bool InitializeRequest(HttpContext context)
	{
		_context = context;
		return true;
	}

	/// <summary>
	/// Adds an HTTP Response Header
	/// </summary>
	/// <remarks>
	/// This method is used to store the Response Headers in a private, member variable,
	/// InternalResponseHeaders, so that the Response Headers may be accesed in the
	/// LogResponseHttpHeaders method, if needed. The Response.Headers property can only
	/// be accessed directly when using IIS 7's Integrated Pipeline mode. This workaround
	/// permits logging of Response Headers when using Classic mode or a web server other
	/// than IIS 7.
	/// </remarks>
	protected void AddHeader(HttpResponse response, string name, string value)
	{
		//_internalResponseHeaders.Add(name, value);

		response.AddHeader(name, value);
	}

	#endregion

	#region Private Functions

	/// <summary>
	/// Send the requested resource as a 200 OK response.
	/// </summary>
	private void ReturnEntireEntity()
	{
		HttpResponse response = _context.Response;

		response.StatusCode = 200;  // OK
		WriteCommonResponseHeaders(response, ResourceLength, ResourceMimeType);

#if DEBUG
		//LogResponseHttpHeaders(context.Server.MapPath(this.LogFileName), Response);
#endif

		ReturnChunkedResponse();
	}

	/// <summary>
	/// Send the requested resource to the HTTP response stream as a 206 Partial Content
	/// </summary>
	private void ReturnPartialEntity()
	{
		HttpResponse response = _context.Response;

		response.StatusCode = 206;  // Partial response

		// Specify the byte range being returned for non-multipart requests
		if (IsMultipartRequest == false)
		{
			string range = String.Format(CultureInfo.InvariantCulture, "bytes {0}-{1}/{2}", StartRangeBytes[0], EndRangeBytes[0], ResourceLength);
			AddHeader(response, HttpHeaderContentRange, range);
		}

		WriteCommonResponseHeaders(response, ComputeContentLength(), IsMultipartRequest ? MultipartContenttype : ResourceMimeType);

#if DEBUG
		//LogResponseHttpHeaders(context.Server.MapPath(this.LogFileName), Response);
#endif

		if (_context.Request.HttpMethod.Equals(HttpMethodHead) == false)
			ReturnChunkedResponse();
	}

	/// <summary>
	/// Send the requested resource to the HTTP response stream.
	/// </summary>
	private void ReturnChunkedResponse()
	{
		HttpResponse response = _context.Response;

		byte[] buffer = new byte[BufferSize];
		using (Stream fs = GetResourceStream())
		{
			for (int i = 0; i < StartRangeBytes.Length; i++)
			{
				// Position the stream at the starting byte
				fs.Seek(StartRangeBytes[i], SeekOrigin.Begin);

				long bytesToReadRemaining = EndRangeBytes[i] - StartRangeBytes[i] + 1;

				// Output multipart boundary, if needed
				if (IsMultipartRequest)
				{
					response.Output.Write("--" + MultipartBoundary);
					response.Output.Write(String.Format("{0}: {1}", HttpHeaderContentType, ResourceMimeType));
					response.Output.Write(String.Format("{0}: bytes {1}-{2}/{3}",
																									HttpHeaderContentRange,
																									StartRangeBytes[i].ToString(),
																									EndRangeBytes[i].ToString(),
																									ResourceLength.ToString()
																							)
															 );
					response.Output.WriteLine();
				}

				// Stream out the requested chunks for the current range request
				while (bytesToReadRemaining > 0)
				{
					if (response.IsClientConnected)
					{
						int chunkSize = fs.Read(buffer, 0, BufferSize < bytesToReadRemaining ? BufferSize : Convert.ToInt32(bytesToReadRemaining));
						response.OutputStream.Write(buffer, 0, chunkSize);

						bytesToReadRemaining -= chunkSize;

						response.Flush();

#if DEBUG
						//Thread.Sleep(DebuggingSleepTime);
#endif
					}
					else
					{
						// Client disconnected - quit
						return;
					}
				}
			}
		}
	}

	private int ComputeContentLength()
	{
		int contentLength = 0;

		for (int i = 0; i < StartRangeBytes.Length; i++)
		{
			contentLength += Convert.ToInt32(EndRangeBytes[i] - StartRangeBytes[i]) + 1;

			if (IsMultipartRequest)
				contentLength += MultipartBoundary.Length
												+ ResourceMimeType.Length
												+ StartRangeBytes[i].ToString().Length
												+ EndRangeBytes[i].ToString().Length
												+ ResourceLength.ToString().Length
												+ 49;       // Length needed for multipart header
		}

		if (IsMultipartRequest)
			contentLength += MultipartBoundary.Length
													+ 8;    // Length of dash and line break

		return contentLength;
	}

	private void WriteCommonResponseHeaders(HttpResponse response, long contentLength, string contentType)
	{
		AddHeader(response, HttpHeaderContentLength, contentLength.ToString());
		AddHeader(response, HttpHeaderContentType, contentType);
		AddHeader(response, HttpHeaderAcceptRanges, EnableRangeRequests ? HttpHeaderAcceptRangesBytes : HttpHeaderAcceptRangesNone);

		if (!String.IsNullOrEmpty(ResourceFileEntityTag))
			AddHeader(response, HttpHeaderEntityTag, String.Concat("\"", ResourceFileEntityTag, "\""));

		if (ResourceLastWriteTimeUtc > DateTime.MinValue)
			AddHeader(response, HttpHeaderLastModified, ResourceLastWriteTimeUtc.ToString("r"));

		if (!String.IsNullOrEmpty(FileName))
			AddHeader(response, HttpHeaderContentDispositionTag, String.Format("{0};filename=\"{1}\"", ContentDisposition.ToString().ToLowerInvariant(), MakeFileNameDownloadFriendly(FileName)));
	}

	/// <summary>
	/// Gets the requested header from the HTTP request, returning <paramref name="defaultValue" /> if it
	/// is not present.
	/// </summary>
	/// <param name="headerName">Name of the HTTP request header to retrieve.</param>
	/// <param name="defaultValue">Value to return if the requested header is not available.</param>
	/// <returns>Returns an instance of <see cref="System.String" />.</returns>
	private string RetrieveHeader(string headerName, string defaultValue)
	{
		return String.IsNullOrEmpty(_context.Request.Headers[headerName]) ? defaultValue : _context.Request.Headers[headerName].Replace("\"", String.Empty);
	}

	/// <summary>
	/// Converts the <paramref name="dateTime" /> to a value that can be used in HTTP-related date/time
	/// calculations. Specifically, it removes the millisecond portion from the value.
	/// </summary>
	/// <param name="dateTime">The date time.</param>
	/// <returns>Returns a <see cref="DateTime" /> instance.</returns>
	private static DateTime ConvertToHttpDateTime(DateTime dateTime)
	{
		return new DateTime(
			dateTime.Year,
			dateTime.Month,
			dateTime.Day,
			dateTime.Hour,
			dateTime.Minute,
			dateTime.Second
			);
	}

	/// <summary>
	/// Calculates the start and end range for the requested resource, taking into account any range request
	/// present in the HTTP header.
	/// </summary>
	private void CalculateRequestRange()
	{
		string rangeHeader = RetrieveHeader(HttpHeaderRange, String.Empty);

		if (String.IsNullOrEmpty(rangeHeader))
		{
			// No Range HTTP Header supplied; send back entire contents
			StartRangeBytes = new long[] { 0 };
			EndRangeBytes = new[] { ResourceLength - 1 };
			IsRangeRequest = false;
			IsMultipartRequest = false;
		}
		else
		{
			// rangeHeader contains the value of the Range HTTP Header and can have values like:
			//      Range: bytes=0-1            * Get bytes 0 and 1, inclusive
			//      Range: bytes=0-500          * Get bytes 0 to 500 (the first 501 bytes), inclusive
			//      Range: bytes=400-1000       * Get bytes 500 to 1000 (501 bytes in total), inclusive
			//      Range: bytes=-200           * Get the last 200 bytes
			//      Range: bytes=500-           * Get all bytes from byte 500 to the end
			//
			// Can also have multiple ranges delimited by commas, as in:
			//      Range: bytes=0-500,600-1000 * Get bytes 0-500 (the first 501 bytes), inclusive plus bytes 600-1000 (401 bytes) inclusive

			// Remove "Ranges" and break up the ranges
			string[] ranges = rangeHeader.Replace("bytes=", String.Empty).Split(",".ToCharArray());

			StartRangeBytes = new long[ranges.Length];
			EndRangeBytes = new long[ranges.Length];

			IsRangeRequest = true;
			IsMultipartRequest = (StartRangeBytes.Length > 1);

			for (int i = 0; i < ranges.Length; i++)
			{
				const int start = 0, end = 1;

				// Get the START and END values for the current range
				string[] currentRange = ranges[i].Split("-".ToCharArray());

				if (String.IsNullOrEmpty(currentRange[end]))
					// No end specified
					EndRangeBytes[i] = ResourceLength - 1;
				else
					// An end was specified
					EndRangeBytes[i] = Int64.Parse(currentRange[end]);

				if (String.IsNullOrEmpty(currentRange[start]))
				{
					// No beginning specified, get last n bytes of file
					StartRangeBytes[i] = ResourceLength - 1 - EndRangeBytes[i];
					EndRangeBytes[i] = ResourceLength - 1;
				}
				else
				{
					// A normal begin value
					StartRangeBytes[i] = Int64.Parse(currentRange[0]);
				}
			}
		}
	}

	/// <summary>
	/// Makes the file name download friendly. Since IE automatically replaces spaces with underscores, we can prevent this by encoding them.
	/// The fileName is unmodified for all other browsers. Example: If fileName="My cat.jpg" and the current browser is IE, this function 
	/// returns "My%20cat.jpg".
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>Returns the <paramref name="fileName" /> parameter, modified if necessary.</returns>
	private string MakeFileNameDownloadFriendly(string fileName)
	{
		if (_context.Request.Browser.Browser.Equals("IE", StringComparison.OrdinalIgnoreCase))
		{
			fileName = fileName.Replace(" ", "%20");
		}

		return fileName;
	}

	#region Process Request Step Checks

	/// <summary>
	/// Verifies that the current user can access the requested resource.
	/// </summary>
	/// <returns><c>true</c> if validation succeeds; otherwise <c>false</c>.</returns>
	/// <remarks>A derived class should override this function to perform authorization.</remarks>
	protected virtual bool CheckAuthorizationRules()
	{
		// Any authorization checks should be implemented in the derived class
		return true;
	}

	/// <summary>
	/// Verifies this is an HTTP GET or HEAD request.
	/// </summary>
	/// <returns><c>true</c> if validation succeeds; otherwise <c>false</c>.</returns>
	protected virtual bool CheckHttpMethod()
	{
		string httpMethod = _context.Request.HttpMethod;

		if (httpMethod.Equals(HttpMethodGet) || httpMethod.Equals(HttpMethodHead))
		{
			return true;
		}
		else
		{
			_context.Response.StatusCode = 501;  // Not Implemented
			return false;
		}
	}

	/// <summary>
	/// Verifies that the requested resource exists and can be sent to the user.
	/// </summary>
	/// <returns><c>true</c> if validation succeeds; otherwise <c>false</c>.</returns>
	/// <remarks>A derived class should override this method as needed, such as to verify that a requested
	/// file exists.</remarks>
	protected virtual bool CheckResourceRequested()
	{
		// Overridden in getmedia.ashx
		if (ResourceLength > Int32.MaxValue)
		{
			_context.Response.StatusCode = 413; // Request Entity Too Large
			return false;
		}

		return true;
	}

	/// <summary>
	/// Verifies the requested ranges are logical.
	/// </summary>
	/// <returns><c>true</c> if validation succeeds; otherwise <c>false</c>.</returns>
	protected virtual bool CheckRangesRequested()
	{
		for (int i = 0; i < StartRangeBytes.Length; i++)
		{
			if (EndRangeBytes[i] == ResourceLength)
			{
				// IE/Silverlight might send a range request that causes a 400 error in the next
				// test; prevent this by detecting it and subtracting one.
				EndRangeBytes[i]--;
			}

			if (StartRangeBytes[i] > ResourceLength - 1 ||
					EndRangeBytes[i] > ResourceLength - 1)
			{
				_context.Response.StatusCode = 400; // Bad Request
				return false;
			}

			if (StartRangeBytes[i] < 0 || EndRangeBytes[i] < 0)
			{
				_context.Response.StatusCode = 400; // Bad Request
				return false;
			}

			if (EndRangeBytes[i] < StartRangeBytes[i])
			{
				_context.Response.StatusCode = 400; // Bad Request
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Checks for existence of If-Modified-Since header. If present, determines whether to respond with
	/// a 304 Not Modified status code.
	/// </summary>
	/// <returns><c>true</c> if resource does not qualify for 304 status; <c>false</c> if it does.</returns>
	protected virtual bool CheckIfModifiedSinceHeader()
	{
		string ifModifiedSinceHeader = RetrieveHeader(HttpHeaderIfModifiedSince, String.Empty);

		if (!String.IsNullOrEmpty(ifModifiedSinceHeader))
		{
			// Determine the date
			DateTime ifModifiedSinceDate;
			if (DateTime.TryParse(ifModifiedSinceHeader, out ifModifiedSinceDate) && (ifModifiedSinceDate > DateTime.MinValue))
			{
				if ((ResourceLastWriteTimeUtc > DateTime.MinValue) && (ConvertToHttpDateTime(ResourceLastWriteTimeUtc) <= ifModifiedSinceDate.ToUniversalTime()))
				{
					// File was created before specified date
					_context.Response.StatusCode = 304;  // Not Modified
					return false;
				}
			}
		}

		return true;
	}

	/// <summary>
	/// Checks for existence of If-Unmodified-Since or Unless-Modified-Since headers. If present, validates
	/// the date, responding with a 412 Precondition Failed if necessary.
	/// </summary>
	/// <returns><c>true</c> if resource does not qualify for 412 status; <c>false</c> if it does.</returns>
	protected virtual bool CheckIfUnmodifiedSinceHeader()
	{
		string ifUnmodifiedSinceHeader = RetrieveHeader(HttpHeaderIfUnmodifiedSince, String.Empty);

		// If-Unmodified-Since not present, so look for Unless-Modified-Since
		if (String.IsNullOrEmpty(ifUnmodifiedSinceHeader))
			ifUnmodifiedSinceHeader = RetrieveHeader(HttpHeaderUnlessModifiedSince, String.Empty);

		if (String.IsNullOrEmpty(ifUnmodifiedSinceHeader))
			return true; // Neither are present; just return.

		// Convert to date and perform test
		DateTime ifUnmodifiedSinceDate;
		if (DateTime.TryParse(ifUnmodifiedSinceHeader, out ifUnmodifiedSinceDate))
		{
			ifUnmodifiedSinceDate = ifUnmodifiedSinceDate.ToUniversalTime();
		}

		if (ConvertToHttpDateTime(ResourceLastWriteTimeUtc) > ifUnmodifiedSinceDate)
		{
			// Could not convert value into date or file was created after specified date
			_context.Response.StatusCode = 412;  // Precondition failed
			return false;
		}

		return true;
	}

	/// <summary>
	/// Checks for If-Match header. Returns <c>true</c> if not present or the ETag of the requested 
	/// resource matches the value supplied by the client.
	/// </summary>
	/// <returns><c>true</c> if resource does not qualify for 412 status; <c>false</c> if it does.</returns>
	protected virtual bool CheckIfMatchHeader()
	{
		string ifMatchHeader = RetrieveHeader(HttpHeaderIfMatch, String.Empty);

		if (String.IsNullOrEmpty(ifMatchHeader) || ifMatchHeader == "*")
			return true; // Match found

		// Look for a matching ETag value in ifMatchHeader
		string[] entityIds = ifMatchHeader.Replace("bytes=", String.Empty).Split(new[] {','});

		foreach (string entityId in entityIds)
		{
			if (ResourceFileEntityTag == entityId)
				return true; // Match found
		}

		// If we reach here, no match found
		_context.Response.StatusCode = 412;  // Precondition failed
		return false;
	}

	/// <summary>
	/// Checks for If-None-Match header. Returns <c>true</c> if not present or no match exists; returns 
	/// <c>false</c> if the request is invalid or the ETag of the requested resource matches the value 
	/// supplied by the client.
	/// </summary>
	/// <returns><c>true</c> if resource does not qualify for 304 or 412 status; <c>false</c> if it does.</returns>
	protected virtual bool CheckIfNoneMatchHeader()
	{
		HttpResponse response = _context.Response;

		string ifNoneMatchHeader = RetrieveHeader(HttpHeaderIfNoneMatch, String.Empty);

		if (String.IsNullOrEmpty(ifNoneMatchHeader))
			return true;

		if (ifNoneMatchHeader == "*")
		{
			// Logically invalid request
			response.StatusCode = 412;  // Precondition failed
			return false;
		}

		// Look for a matching ETag value in ifNoneMatchHeader
		string[] entityIds = ifNoneMatchHeader.Replace("bytes=", String.Empty).Split(new[] { ',' });

		foreach (string entityId in entityIds)
		{
			if (ResourceFileEntityTag == entityId)
			{
				AddHeader(response, HttpHeaderEntityTag, String.Concat("\"", entityId, "\""));
				response.StatusCode = 304;  // Not modified
				return false;        // Match found
			}
		}

		return true; // No match found
	}

	/// <summary>
	/// Checks for If-Range header. Returns <c>false</c> if not present or the ETag of the requested 
	/// resource matches the value supplied by the client; otherwise returns <c>true</c>. When returning
	/// <c>false</c>, this function sends the requested resource to the HTTP response stream.
	/// </summary>
	/// <returns>Returns <c>false</c> if not present or the ETag of the requested 
	/// resource matches the value supplied by the client; otherwise <c>true</c></returns>
	protected virtual bool CheckIfRangeHeader()
	{
		string ifRangeHeader = RetrieveHeader(HttpHeaderIfRange, ResourceFileEntityTag);

		if (!IsRangeRequest || ifRangeHeader == ResourceFileEntityTag)
		{
			return true;
		}
		else
		{
			ReturnEntireEntity();
			return false;
		}
	}

	#endregion

	#endregion

	#region Logging Methods
	//private void LogRequestHttpHeaders(string logFile, HttpRequest Request)
	//{
	//    string output = string.Concat("REQUEST INFORMATION (", Request.HttpMethod, ")", Environment.NewLine);
	//    foreach (string name in Request.Headers.Keys)
	//    {
	//        output += string.Format("{0}: {1}{2}", name, Request.Headers[name], Environment.NewLine);
	//    }

	//    output += Environment.NewLine + Environment.NewLine;

	//    File.AppendAllText(logFile, output);
	//}

	//private void LogResponseHttpHeaders(string logFile, HttpResponse Response)
	//{
	//    string output = string.Concat("RESPONSE INFORMATION (", Response.StatusCode.ToString(), ")", Environment.NewLine);
	//    foreach (string name in InternalResponseHeaders.Keys)
	//    {
	//        output += string.Format("{0}: {1}{2}", name, InternalResponseHeaders[name], Environment.NewLine);
	//    }

	//    output += Environment.NewLine + Environment.NewLine;

	//    File.AppendAllText(logFile, output);
	//}
	#endregion
}