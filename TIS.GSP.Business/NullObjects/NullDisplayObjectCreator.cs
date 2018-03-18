using System;
using GalleryServer.Business.Interfaces;

namespace GalleryServer.Business.NullObjects
{
	/// <summary>
	/// Represents a <see cref="IDisplayObjectCreator" /> that is equivalent to null. This class is used instead of null to prevent 
	/// <see cref="NullReferenceException" /> errors if the calling code accesses a property or executes a method.
	/// </summary>
	class NullDisplayObjectCreator : DisplayObjectCreator
	{
		public override void GenerateAndSaveFile()
		{
		}
	}
}
