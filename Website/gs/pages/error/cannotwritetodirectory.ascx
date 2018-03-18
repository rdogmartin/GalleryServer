<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="cannotwritetodirectory.ascx.cs" Inherits="GalleryServer.Web.Pages.Err.cannotwritetodirectory" EnableViewState="false" %>
<style type="text/css">
 html, body { margin: 0; padding: 0; background-color: #f2f2f2;font: 12px Verdana, Arial, Helvetica, sans-serif; }  
.gsp_ns { color: #000; }
.gsp_ns a { text-decoration: underline; }
.gsp_ns a:link { color: #4a4a4a; }
.gsp_ns a:visited { color: #4a4a4a; }
.gsp_ns a:hover { color: #000; }
.gsp_ns a:active { color: #000; }
.gsp_ns .gsp_errHeader { height: 75px; background-color: #a7a7a7; padding: 10px; }
.gsp_ns .gsp_errContent { background-color: #f2f2f2;padding: 1em; }
.gsp_ns p.gsp_h2 { color:#800;margin: 0 0 1em 0; padding: 0; font-size: 1.2em; font-weight: bold;  }
.gsp_ns .gsp_tip {font-size: 1.1em; font-weight: bold; margin: 0; color: #060;}
.gsp_ns .gsp_tip_detail {color: #060;}
.gsp_ns .gsp_tip_detail li { padding-bottom: 20px;}
</style>
<div class="gsp_ns gsp_errContainer">
	<div class="gsp_errHeader">
		<asp:Image ID="imgGspLogo" runat="server" AlternateText="Gallery Server logo" />
	</div>
	<div class="gsp_errContent">
		<p>
			<asp:HyperLink ID="hlHome" runat="server" ToolTip="<%$ Resources:GalleryServer, Error_Home_Page_Link_Text %>"
				Text="<%$ Resources:GalleryServer, Error_Home_Page_Link_Text %>" /></p>
		<p class="gsp_h2">
			<asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, Error_Cannot_Write_To_Directory_Hdr %>" /></p>
		<p>
			<asp:Literal ID="litErrorInfo" runat="server" /></p>
		<p class="gsp_tip">
			<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Error_Cannot_Write_To_Directory_Hint_Hdr %>" />
		</p>
		<div class="gsp_tip_detail">
			<asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Error_Cannot_Write_To_Directory_Hint_Dtl %>" />
		</div>
	</div>
</div>
