using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.NullObjects
{
	/// <summary>
	/// Represents a null version of a <see cref="IWpfMetadata" /> instance. This is used when a valid
	/// <see cref="BitmapMetadata" /> instance is not available for a media file. The main advantage
	/// to using this class is to reduce the dependency on calling code to check for null when accessing
	/// metadata properties.
	/// </summary>
	internal class NullWpfMetadata : IWpfMetadata
	{
		public string DateTaken { get; set; }
		public string Title { get; set; }
		public ReadOnlyCollection<string> Author { get; set; }
		public string CameraModel { get; set; }
		public string CameraManufacturer { get; set; }
		public ReadOnlyCollection<string> Keywords { get; set; }
		public int Rating { get; set; }
		public string Comment { get; set; }
		public string Copyright { get; set; }
		public string Subject { get; set; }
		public object GetQuery(string query)
		{
			return null;
		}
	}
}
