using System;
using System.Collections.Generic;
using System.Text;

namespace GalleryServer.WebControls
{
	/// <summary>
	/// Extender style interface that allows adding a wwDataBinder 
	/// object to a control and interact with a DataBinder object
	/// on a Page. 
	/// 
	/// Any control marked with this interface can be automatically
	/// pulled into the a wwDataBinder instance with 
	/// wwDataBinder.LoadFromControls().
	/// </summary>
	public interface IwwDataBinder
	{
		wwDataBindingItem BindingItem
		{
			get;
		}
	}
}
