using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace GalleryServer.Web.Handler
{
	/// <summary>
	/// Same implementation as above but implements the Async request processsing.
	/// Quite useful in these scenarios since typical operations from file upload
	/// are IO operations - streaming to file or disk which tend to be relatively
	/// </summary>
	public abstract class UploadHandlerBaseAsync : UploadHandlerBase, IHttpAsyncHandler
	{
		#region Async Handler Implementation

		/// <summary>
		/// Delegate to ProcessRequest method
		/// </summary>
		//Action<HttpContext> processRequest;

		TaskCompletionSource<object> tcs;

    /// <summary>
    /// Initiates an asynchronous call to the HTTP handler.
    /// </summary>
    /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
    /// <param name="cb">The <see cref="T:System.AsyncCallback" /> to call when the asynchronous method call is complete. If <paramref name="cb" /> is null, the delegate is not called.</param>
    /// <param name="extraData">Any extra data needed to process the request.</param>
    /// <returns>An <see cref="T:System.IAsyncResult" /> that contains information about the status of the process.</returns>
    public System.IAsyncResult BeginProcessRequest(HttpContext context, System.AsyncCallback cb, object extraData)
		{
			tcs = new TaskCompletionSource<object>(context);

			// call ProcessRequest method asynchronously            
			var task = Task<object>.Factory.StartNew(
					(ctx) =>
					{
						ProcessRequest(ctx as HttpContext);

						if (cb != null)
							cb(tcs.Task);

						return null;
					}, context)
			.ContinueWith(tsk =>
			{
				if (tsk.IsFaulted)
					tcs.SetException(tsk.Exception);
				else
					// Not returning a value, but TCS needs one so just use null
					tcs.SetResult(null);

			}, TaskContinuationOptions.ExecuteSynchronously);


			return tcs.Task;
		}

		public void EndProcessRequest(System.IAsyncResult result)
		{
		}
		# endregion
	}
}
