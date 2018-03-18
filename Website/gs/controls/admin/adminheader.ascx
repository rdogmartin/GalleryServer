<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="adminheader.ascx.cs" Inherits="GalleryServer.Web.Controls.Admin.adminheader" %>
<p class="gsp_admin_h1">
	<asp:Literal ID="litPageHeader" runat="server" Text="<%$ Resources:GalleryServer, Admin_Header %>" />
</p>
<div style="float: right">
	<p class="gsp_minimargin">
		<asp:Button ID="btnOkTop" runat="server" Text="<%$ Resources:GalleryServer, Default_Task_Ok_Button_Text %>"
			TabIndex="1" AccessKey="O" />&nbsp;</p>
</div>
<p class="gsp_admin_h2">
	<asp:Label ID="lblAdminPageHeader" runat="server" CssClass="gsp_admin_h2_txt" />
</p>
