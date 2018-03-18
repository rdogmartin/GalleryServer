using System;
using System.IO;

namespace GalleryServer.Web.Handler
{

	/// <summary>
	/// Upload handler that uploads files into a specific folder.
	/// Depending on the unique_names flag on the plUpload component,
	/// the filename will either be unique or the original filename.
	/// </summary>
	public class UploadHandler : UploadHandlerBase
	{
		/// <summary>
		/// Physical folder location where the file will be uploaded.
		/// 
		/// Note that you can assign an IIS virtual path (~/path)
		/// to this property, which automatically translates to a 
		/// physical path.
		/// </summary>
		public string FileUploadPhysicalPath
		{
			get
			{
				if (_FileUploadPhysicalPath.StartsWith("~"))
					_FileUploadPhysicalPath = Context.Server.MapPath(_FileUploadPhysicalPath);
				return _FileUploadPhysicalPath;
			}
			set
			{
				_FileUploadPhysicalPath = value;
			}
		}
		private string _FileUploadPhysicalPath;


    /// <summary>
    /// Initializes a new instance of the <see cref="UploadHandler"/> class.
    /// </summary>
    public UploadHandler()
		{
			FileUploadPhysicalPath = "~/temp/";
		}

		/// <summary>
		/// Stream each chunk to a file and effectively append it. 
		/// </summary>
		/// <param name="chunkStream"></param>
		/// <param name="chunk"></param>
		/// <param name="chunks"></param>
		/// <param name="uploadedFilename"></param>
		/// <returns></returns>
		protected override bool OnUploadChunk(Stream chunkStream, int chunk, int chunks, string uploadedFilename)
		{
			var path = FileUploadPhysicalPath;

			// try to create the path
			if (!Directory.Exists(path))
			{
				try
				{
					Directory.CreateDirectory(path);
				}
				catch
				{
					WriteErrorResponse(Resources.GalleryServer.Task_Add_Objects_UploadDirectoryDoesntExistAndCouldntCreate);
					return false;
				}
			}

			// Normalize the filename to prevent against any path traversal hacks
			uploadedFilename = Path.GetFileName(uploadedFilename) ?? String.Empty;

			string uploadFilePath = Path.Combine(path, uploadedFilename);
			if (chunk == 0)
			{
				if (File.Exists(uploadFilePath))
					File.Delete(uploadFilePath);
			}

			Stream stream = null;
			try
			{
				stream = new FileStream(uploadFilePath, (chunk == 0) ? FileMode.CreateNew : FileMode.Append);
				chunkStream.CopyTo(stream, 16384);
			}
			catch
			{
				WriteErrorResponse(Resources.GalleryServer.Task_Add_Objects_UnableToWriteOutFile);
				return false;
			}
			finally
			{
				if (stream != null)
					stream.Dispose();
			}

			return true;
		}
	}
}
