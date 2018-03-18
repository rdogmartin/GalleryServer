<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="mediatemplates.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.mediatemplates" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content" runat="server">
  <p class="gsp_a_ap_to">
    <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
      EnableViewState="false" />&nbsp;<asp:Literal runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
  </p>
  <asp:PlaceHolder ID="phAdminHeader" runat="server" />
  <div class="gsp_addleftpadding5">
    <p>
      MIME type:&nbsp;
      <asp:DropDownList ID="ddlMimeType" runat="server" CssClass="gsp_mt_b_slt" AutoPostBack="True" OnSelectedIndexChanged="ddlMimeType_SelectedIndexChanged" />
    </p>
    <p>
      <span>Browser ID:</span>&nbsp;<asp:DropDownList ID="ddlBrowserId" runat="server" CssClass="gsp_mt_b_slt" AutoPostBack="True"
        OnSelectedIndexChanged="ddlBrowserId_SelectedIndexChanged" />
      &nbsp;
      <asp:LinkButton ID="lbCreate" runat="server" Text="Copy as new..." ToolTip="Create a new template from the current one"
        OnClick="lbCreate_Click" />
    </p>
    <hr style="max-width: 1000px;" />
    <table class="gsp_standardTable gsp_addtopmargin10">
      <tr>
        <td class="gsp_col1">
          <p><span class="gsp_mt_mt_h">MIME type:</span></p>
        </td>
        <td class="gsp_col2">
          <p>
            <asp:TextBox ID="txtMimeType" runat="server" CssClass="gsp_textbox" />&nbsp;<asp:Button
              ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" ToolTip="Save the template" />&nbsp;<asp:Button
                ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" ToolTip="Cancel changes" />&nbsp;<asp:Button
                  ID="btnDelete" runat="server" Text="Delete" OnClick="btnDelete_Click" ToolTip="Delete this template"
                  OnClientClick="return confirm('Are you sure you want to delete this template?');" />
          </p>
        </td>
      </tr>
      <tr>
        <td class="gsp_col1" style="margin-bottom: 10px;"><span class="gsp_mt_bid_h">Browser ID:</span></td>
        <td class="gsp_col2">
          <asp:TextBox ID="txtBrowserId" runat="server" CssClass="gsp_textbox" /></td>
      </tr>
    </table>

    <div id="<%= cid %>_tmplTabContainer" class="gsp_tabContainer">
      <ul>
        <li><a href="#<%= cid %>_tmplTabHtml">HTML</a></li>
        <li><a href="#<%= cid %>_tmplTabScript">JavaScript</a></li>
      </ul>
      <div id="<%= cid %>_tmplTabHtml" class="ui-corner-all">
        <p>
          <span class="gsp_mt_html_h">HTML:</span>
        </p>
        <p>
          <asp:TextBox ID="txtHtml" runat="server" TextMode="MultiLine" Rows="20" CssClass="gsp_a_tmpl_html_txtarea" />
        </p>
      </div>
      <div id="<%= cid %>_tmplTabScript" class="ui-corner-all">
        <p>
          <span class="gsp_mt_js_h">JavaScript:</span>
        </p>
        <p>
          <asp:TextBox ID="txtScript" runat="server" TextMode="MultiLine" Rows="20" CssClass="gsp_a_tmpl_script_txtarea" />
        </p>
      </div>
    </div>
    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
  </div>
</div>

<asp:PlaceHolder runat="server">
  <script>
    (function ($) {
      jQuery(document).ready(function () {
        configControls();
        configTooltips();
      });

      var configControls = function () {
        if (!Gs.Vars.IsPostBack) {
            Gs.Vars.Cookies.remove('<%= SelectedTabCookieName %>');
        }

        $("#<%= cid %>_tmplTabContainer").tabs({
          active: (Gs.Vars.Cookies.get('<%= SelectedTabCookieName %>') || 0),
          activate: function (e, ui) {
            Gs.Vars.Cookies.set('<%= SelectedTabCookieName %>', ui.newTab.index());
          }
        }).show();
      };

      var configTooltips = function () {
        $('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_MediaTmpl_Overview_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_MediaTmpl_Overview_Bdy.JsEncode() %>'
        });
        
        $('.gsp_mt_mt_h', '#<%= cid %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_MediaTmpl_MimeType_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_MediaTmpl_MimeType_Bdy.JsEncode() %>'
        });
        
        $('.gsp_mt_bid_h', '#<%= cid %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_MediaTmpl_BrowserId_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_MediaTmpl_BrowserId_Bdy.JsEncode() %>'
        });
         
        $('.gsp_mt_html_h', '#<%= cid %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_MediaTmpl_Tmpl_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_MediaTmpl_Tmpl_Bdy.JsEncode() %>'
        });
       
        $('.gsp_mt_js_h', '#<%= cid %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_MediaTmpl_Tmpl_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_MediaTmpl_Tmpl_Bdy.JsEncode() %>'
        });
      };

    })(jQuery);
  </script>
</asp:PlaceHolder>
