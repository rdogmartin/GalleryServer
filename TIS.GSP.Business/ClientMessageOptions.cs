namespace GalleryServer.Business
{
	/// <summary>
	/// Contains settings for controlling the content and behavior for displaying a message to a user.
	/// </summary>
	public class ClientMessageOptions
	{
		private int? _autoCloseDelay;

		/// <summary>
		/// Gets or sets the type of the message. When specified, the remaining properties can be
		/// automatically determined. Specify <see cref="MessageType.None" /> when manually setting the
		/// remaining properties.
		/// </summary>
		/// <value>An instance of <see cref="MessageType" />.</value>
		public MessageType MessageId { get; set; }

		/// <summary>
		/// Gets or sets the title. This value is displayed in the title bar of the control shown to the user.
		/// May be null.
		/// </summary>
		/// <value>A string.</value>
		public string Title { get; set; }

		/// <summary>
		/// Gets or sets the message. This value is displayed in the body of the control shown to the user.
		/// May be null.
		/// </summary>
		/// <value>A string.</value>
		public string Message { get; set; }

		/// <summary>
		/// The number of milliseconds to wait until a message auto-closes. Use 0 to never auto-close.
		/// Defaults to 4000 (4 seconds) when Style is <see cref="MessageStyle.Success" />; otherwise defaults
		/// to 0 (stay open).
		/// </summary>
		/// <value>An integer.</value>
		public int AutoCloseDelay
		{
			get
			{
				if (Style == MessageStyle.Success)
					return _autoCloseDelay.GetValueOrDefault(4000);
				else
					return _autoCloseDelay.GetValueOrDefault(0);					
			}
			set { _autoCloseDelay = value; }
		}

		/// <summary>
		/// Gets or sets the category of message. This value controls the formatting of the control shown
		/// to the user. For example, the value <see cref="MessageStyle.Error" /> results in a red-themed
		/// display message.
		/// </summary>
		/// <value>An instance of <see cref="MessageStyle" />.</value>
		public MessageStyle Style { get; set; }
	}
}