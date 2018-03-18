using System;
using System.Runtime.Serialization;

namespace GalleryServer.Data
{
	/// <summary>
	///   The exception that is thrown when the SQL CE database cannot be compacted.
	/// </summary>
	[Serializable]
	public class CannotCompactSqlCeException : Exception
	{
		/// <summary>
		///   Throws an exception to indicate the SQL CE database cannot be compacted.
		/// </summary>
		public CannotCompactSqlCeException()
			: base("Cannot compact the SQL CE database.")
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected query string value.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		public CannotCompactSqlCeException(string msg)
			: base(msg)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected query string value.
		/// </summary>
		/// <param name="msg">A message that describes the error.</param>
		/// <param name="innerException">
		///   The exception that is the cause of the current exception. If the
		///   innerException parameter is not a null reference, the current exception is raised in a catch
		///   block that handles the inner exception.
		/// </param>
		public CannotCompactSqlCeException(string msg, Exception innerException)
			: base(msg, innerException)
		{
		}

		/// <summary>
		///   Throws an exception to indicate an unexpected query string value.
		/// </summary>
		/// <param name="info">
		///   The System.Runtime.Serialization.SerializationInfo that holds the serialized object
		///   data about the exception being thrown.
		/// </param>
		/// <param name="context">
		///   The System.Runtime.Serialization.StreamingContext that contains contextual
		///   information about the source or destination.
		/// </param>
		protected CannotCompactSqlCeException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
