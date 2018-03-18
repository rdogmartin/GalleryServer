namespace GalleryServer.Business
{
	/// <summary>
	/// A data object containing information about the result of an action. The object may be serialized into
	/// JSON and used by the browser.
	/// </summary>
	public class ActionResult
	{
		/// <summary>
		/// Gets or sets the category describing the result of this action. The value must
		/// map to the string representation of the <see cref="ActionResultStatus" /> enumeration.
		/// </summary>
		public string Status;

		/// <summary>
		/// Gets or sets a title describing the action result.
		/// </summary>
		public string Title;

		/// <summary>
		/// Gets or sets an explanatory message describing the action result.
		/// </summary>
		public string Message;

		/// <summary>
		/// Gets or sets the object that is the target of the action. The object must be serializable.
		/// </summary>
		public object ActionTarget;
	}
}