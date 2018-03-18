<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="css.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.css" %>
<%@ Import Namespace="GalleryServer.Web" %>

<div class="gsp_content" runat="server">
  <p class="gsp_a_ap_to">
    <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
      EnableViewState="false" />&nbsp;<asp:Literal runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
  </p>
  <asp:PlaceHolder ID="phAdminHeader" runat="server" />
  <div class="gsp_addleftpadding5">
    <div class="gsp_addtoppadding5">
      <p>
        <asp:TextBox ID="txtCss" runat="server" CssClass="gsp_a_css_txt" TextMode="MultiLine" Columns="20" Rows="20"></asp:TextBox>
      </p>
    </div>
    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
  </div>
</div>
<asp:PlaceHolder runat="server">
  <script>
  	(function ($) {
  		var configTooltips = function () {
  			$('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
  				title: '<%= Resources.GalleryServer.Admin_Css_Overview_Hdr.JsEncode() %>',
  				content: '<%= Resources.GalleryServer.Admin_Css_Overview_Bdy.JsEncode() %>'
  			});
  		};

      $(document).ready(function () {
        configTooltips();
      });
    })(jQuery);
  </script>
</asp:PlaceHolder>
