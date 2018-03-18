using System;
using System.Globalization;
using System.Web.UI.WebControls;

namespace GalleryServer.Web.Controls
{
	/// <summary>
	/// A user control that assists with paging of media objects.
	/// </summary>
	public partial class pager : GalleryUserControl
	{
		//#region Private Fields

		//private int? _pageSize;
		//private PagedDataSource _dataSource;
		//private string _cssClass = "gsp_pager";
		//private string _currentPageCssClass = "gsp_curpage";
		//private string _otherPageCssClass = "gsp_otherpage";
		//private string _prevLinkCssClass = "gsp_pagerprev";
		//private string _nextLinkCssClass = "gsp_pagernext";

		//#endregion

		//#region Public Properties

		///// <summary>
		///// Gets or sets the data source for which the pager is to be rendered.
		///// </summary>
		///// <value>The data source.</value>
		//public PagedDataSource DataSource
		//{
		//	get
		//	{
		//		return this._dataSource;
		//	}
		//	set
		//	{
		//		this._dataSource = value;
		//	}
		//}

		///// <summary>
		///// Gets or sets the index of the current page of data objects. A value of one means the first page.
		///// </summary>
		///// <value>The index of the current page.</value>
		//public int CurrentPage
		//{
		//	get { return this.DataSource.CurrentPageIndex + 1; }
		//	set { this.DataSource.CurrentPageIndex = value - 1; }
		//}

		///// <summary>
		///// Gets or sets the number of objects to display on each page. This value defaults to the pageSize setting
		///// in the configuration file.
		///// </summary>
		///// <value>The number of objects to display on each page.</value>
		//public int PageSize
		//{
		//	get
		//	{
		//		if (!this._pageSize.HasValue)
		//		{
		//			this._pageSize = this.GalleryPage.GallerySettings.PageSize;
		//		}

		//		return this._pageSize.Value;
		//	}
		//	set { this._pageSize = value; }
		//}

		///// <summary>
		///// Gets or sets the CSS class for the pager container DOM element. Default value "gsp_pager".
		///// </summary>
		///// <value>The CSS class for the pager container DOM element.</value>
		//public string CssClass
		//{
		//	get { return this._cssClass; }
		//	set { this._cssClass = value; }
		//}

		///// <summary>
		///// Gets or sets the CSS class for the DOM element containing the current page item. Default value "gsp_curpage".
		///// </summary>
		///// <value>The CSS class for the DOM element containing the current page item.</value>
		//public string CurrentPageCssClass
		//{
		//	get { return this._currentPageCssClass; }
		//	set { this._currentPageCssClass = value; }
		//}

		///// <summary>
		///// Gets or sets the CSS class for the DOM element containing the previous link. Default value "gsp_pagerprev".
		///// </summary>
		///// <value>The CSS class for the DOM element containing the previous link.</value>
		//public string PreviousLinkCssClass
		//{
		//	get { return this._prevLinkCssClass; }
		//	set { this._prevLinkCssClass = value; }
		//}

		///// <summary>
		///// Gets or sets the CSS class for the DOM element containing the next link. Default value "gsp_pagernext".
		///// </summary>
		///// <value>The CSS class for the DOM element containing the next link.</value>
		//public string NextLinkCssClass
		//{
		//	get { return this._nextLinkCssClass; }
		//	set { this._nextLinkCssClass = value; }
		//}

		///// <summary>
		///// Gets or sets the CSS class for the DOM element containing the page links other than the current page.
		///// Default value "gsp_otherpage".
		///// </summary>
		///// <value>The CSS class for the DOM element containing the page links other than the current page.</value>
		//public string OtherPageCssClass
		//{
		//	get { return this._otherPageCssClass; }
		//	set { this._otherPageCssClass = value; }
		//}

		//#endregion

		//#region Event Handlers

		///// <summary>
		///// Handles the Load event of the Page control.
		///// </summary>
		///// <param name="sender">The source of the event.</param>
		///// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		//protected void Page_Load(object sender, EventArgs e)
		//{
		//	CreatePagingControls();
		//}

		//#endregion

		//#region Private Methods

		//private void CreatePagingControls()
		//{
		//	if (DataSource == null || DataSource.DataSourceCount <= PageSize)
		//		return;

		//	Panel pnlPager = new Panel();
		//	pnlPager.CssClass = CssClass;
		//	pnlPager.EnableViewState = false;

		//	HyperLink hlPrevious = new HyperLink();
		//	hlPrevious.Text = Resources.GalleryServer.UC_Pager_Previous_Text;
		//	hlPrevious.ToolTip = Resources.GalleryServer.UC_Pager_Previous_Tooltip;
		//	hlPrevious.NavigateUrl = Utils.GetUrl(PageId.album, "aid={0}&page={1}", this.GalleryPage.GetAlbumId(), (CurrentPage - 1));
		//	hlPrevious.CssClass = PreviousLinkCssClass;
			
		//	if (DataSource.IsFirstPage)
		//	{
		//		hlPrevious.Enabled = false;
		//		hlPrevious.CssClass = hlPrevious.CssClass + " gsp_disabled";
		//	}

		//	pnlPager.Controls.Add(hlPrevious);

		//	// Loop through each page in our collection, creating hyperlinks as we go.
		//	int i;
		//	for (i = 1; i <= DataSource.DataSourceCount / PageSize; i++)
		//	{
		//		if (CurrentPage == i)
		//		{
		//			// We are rendering the item for the current page. Just use a label instead of a hyperlink.
		//			Label lblCurrentPage = new Label();
		//			lblCurrentPage.Text = i.ToString(CultureInfo.InvariantCulture);
		//			lblCurrentPage.CssClass = CurrentPageCssClass;

		//			pnlPager.Controls.Add(lblCurrentPage);
		//		}
		//		else
		//		{
		//			// Create hyperlink to link to this page.
		//			HyperLink hl = new HyperLink();
		//			hl.Text = i.ToString(CultureInfo.InvariantCulture);
		//			hl.NavigateUrl = Utils.GetUrl(PageId.album, "aid={0}&page={1}", this.GalleryPage.GetAlbumId(), i);
		//			hl.CssClass = OtherPageCssClass;

		//			pnlPager.Controls.Add(hl);
		//		}
		//	}

		//	// If there is a final page with a partial collection, create a link for it.
		//	if (DataSource.DataSourceCount % PageSize > 0)
		//	{
		//		if (CurrentPage == i)
		//		{
		//			// We are rendering the item for the current page. Just use a label instead of a hyperlink.
		//			Label lblCurrentPage = new Label();
		//			lblCurrentPage.Text = i.ToString(CultureInfo.InvariantCulture);
		//			lblCurrentPage.CssClass = CurrentPageCssClass;

		//			pnlPager.Controls.Add(lblCurrentPage);
		//		}
		//		else
		//		{
		//			// Create hyperlink to link to this page.
		//			HyperLink hl = new HyperLink();
		//			hl.Text = i.ToString(CultureInfo.InvariantCulture);
		//			hl.NavigateUrl = Utils.GetUrl(PageId.album, "aid={0}&page={1}", this.GalleryPage.GetAlbumId(), i);
		//			hl.CssClass = OtherPageCssClass;

		//			pnlPager.Controls.Add(hl);
		//		}
		//	}

		//	HyperLink hlNext = new HyperLink();
		//	hlNext.Text = Resources.GalleryServer.UC_Pager_Next_Text;
		//	hlNext.ToolTip = Resources.GalleryServer.UC_Pager_Next_Tooltip;
		//	hlNext.NavigateUrl = Utils.GetUrl(PageId.album, "aid={0}&page={1}", this.GalleryPage.GetAlbumId(), (CurrentPage + 1));
		//	hlNext.CssClass = NextLinkCssClass;
			
		//	if (DataSource.IsLastPage)
		//	{
		//		hlNext.Enabled = false;
		//		hlNext.CssClass = hlNext.CssClass + " gsp_disabled";
		//	}

		//	pnlPager.Controls.Add(hlNext);

		//	this.Controls.Add(pnlPager);
		//}

		//#endregion

	}
}