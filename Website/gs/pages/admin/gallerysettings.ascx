<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="gallerysettings.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.gallerysettings" %>
<%@ Import Namespace="GalleryServer.Web" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
<div class="gsp_content">
  <p class="gsp_a_ap_to">
    <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
      EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server" EnableViewState="false" />
  </p>
  <asp:PlaceHolder ID="phAdminHeader" runat="server" />
  <div class="gsp_addleftpadding5">
    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_SystemSettings_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <div id="gsp_g_tt_dlg" class="gsp_tt_dlg">
          <div class="gsp_tt_dlg_title">
            <asp:Literal runat="server" Text="<%$ Resources:GalleryServer, Cfg_showHeader_Hdr %>" /></div>
          <div class="gsp_tt_dlg_bdy">
            <asp:Literal runat="server" Text="<%$ Resources:GalleryServer, Cfg_showHeader_Bdy %>" /></div>
        </div>
        <p class="gsp_addtopmargin5">
          <asp:CheckBox ID="chkShowHeader" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowHeader_Label %>" />
        </p>
        <div class="gsp_collapse gsp_addleftmargin5">
          <table class="gsp_standardTable">
            <tr>
              <td class="gsp_col1">
                <asp:Label ID="lblWebsiteTitle" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_WebsiteTitle_Label %>" />
              </td>
              <td class="gsp_col2">
                <asp:TextBox ID="txtWebsiteTitle" runat="server" CssClass="gsp_textbox" />
              </td>
            </tr>
            <tr>
              <td class="gsp_col1">
                <asp:Label ID="lblWebsiteTitleUrl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_WebsiteTitleUrl_Label %>" />
              </td>
              <td>
                <asp:TextBox ID="txtWebsiteTitleUrl" runat="server" CssClass="gsp_textbox" />
              </td>
            </tr>
          </table>
          <p>
            <asp:CheckBox ID="chkShowLogin" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowLogin_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowSearch" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowSearch_Label %>" />
          </p>
        </div>
      </div>
    </div>

    <div class="gsp_single_tab ">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ErrorHandling_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p class="gsp_addtopmargin5">
          <asp:CheckBox ID="chkSendEmail" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_SendEmail_Label %>" />
        </p>
        <div class="gsp_addleftpadding6">
          <p>
            <asp:Label ID="lblUsersToNotify" runat="server" Text="<%$ Resources:GalleryServer, Admin_General_Users_To_Notify_When_Error_Occurs_Lbl %>" />
          </p>
          <p>
            <asp:ListBox ID="cblU" runat="server" EnableViewState="false" SelectionMode="Multiple" />
            <asp:HiddenField ID="hdnUsersToNotify" runat="server" />
          </p>
        </div>
        <p class="gsp_addtopmargin5">
          <asp:CheckBox ID="chkEnableExceptionHandler" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_EnableExceptionHandler_Label %>" />
        </p>
        <p class="gsp_addleftmargin5">
          <asp:CheckBox ID="chkShowErrorDetails" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowErrorDetails_Label %>" />
        </p>
        <p>
          <asp:Button ID="btnThrowError" runat="server" Text="Generate sample error" OnClick="btnThrowError_Click" />
        </p>
      </div>
    </div>

  </div>
  <tis:wwDataBinder ID="wwDataBinder" runat="server" OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl">
    <DataBindingItems>
      <tis:wwDataBindingItem ID="wbi1" runat="server" ControlId="chkShowHeader" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="ShowHeader" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowHeader_Label %>"
        BindingProperty="Checked" />
      <tis:wwDataBindingItem ID="wbi2" runat="server" ControlId="txtWebsiteTitle" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="GalleryTitle" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_WebsiteTitle_Label %>"
        IsRequired="False" />
      <tis:wwDataBindingItem ID="wbi3" runat="server" ControlId="txtWebsiteTitleUrl" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="GalleryTitleUrl" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_WebsiteTitleUrl_Label %>"
        IsRequired="False" />
      <tis:wwDataBindingItem ID="wbi6" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="ShowLogin" ControlId="chkShowLogin" BindingProperty="Checked"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowLogin_Label %>" />
      <tis:wwDataBindingItem ID="wbi7" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="ShowSearch" ControlId="chkShowSearch" BindingProperty="Checked"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowSearch_Label %>" />
      <tis:wwDataBindingItem ID="wbi14" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="SendEmailOnError" ControlId="chkSendEmail" BindingProperty="Checked"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_SendEmail_Label %>" />
      <tis:wwDataBindingItem ID="wbi16" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EnableExceptionHandler" ControlId="chkEnableExceptionHandler"
        BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_EnableExceptionHandler_Label %>" />
      <tis:wwDataBindingItem ID="wbi17" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="ShowErrorDetails" ControlId="chkShowErrorDetails" BindingProperty="Checked"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_ShowErrorDetails_Label %>" />
    </DataBindingItems>
  </tis:wwDataBinder>
  <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
  <script>
    (function ($) { // Can safely use the $ alias
      $(document).ready(function() {
        configControls();
        updateUi();
        configTooltips();
      });
      
      var updateUi = function () {
        // Enable/disable the 'users to notify when error occurs' combobox
        var chkEnableErrorEmailing = $('#<%= chkSendEmail.ClientID %>');

        if (!chkEnableErrorEmailing.prop('checked') || chkEnableErrorEmailing.prop('disabled'))
          $('#<%= cblU.ClientID %>').multiselect('disable');
        else
          $('#<%= cblU.ClientID %>').multiselect('enable');

        // Enable/disable the 'show error details' checkbox
        var chkEnableExceptionHandler = $('#<%= chkEnableExceptionHandler.ClientID %>');
        $('#<%= chkShowErrorDetails.ClientID %>').prop('disabled', (!chkEnableExceptionHandler.prop('checked') || chkEnableExceptionHandler.prop('disabled')));

        // Enable/disable the 'Show header' controls
        var chkShowHeader = $('#<%= chkShowHeader.ClientID %>');
        var showHeaderSelected = chkShowHeader.prop('checked') && !chkShowHeader.prop('disabled');

        $('#<%= txtWebsiteTitle.ClientID %>').prop('disabled', !showHeaderSelected);
        $('#<%= txtWebsiteTitleUrl.ClientID %>').prop('disabled', !showHeaderSelected);
        $('#<%= chkShowLogin.ClientID %>').prop('disabled', !showHeaderSelected);
        $('#<%= chkShowSearch.ClientID %>').prop('disabled', !showHeaderSelected);
      };

      var configControls = function () {
        $('#<%= chkShowHeader.ClientID %>, #<%= chkSendEmail.ClientID %>, #<%= chkEnableExceptionHandler.ClientID %>').click(updateUi);

        // Convert 'users to notify' list to a jQuery multi-select
        $('#<%= cblU.ClientID %>')
          .multiselect({
            minWidth: Gs.Utils.isWidthLessThan(500) ? 300 : 500,
            header: false,
            noneSelectedText: '&lt;No users selected&gt;',
            selectedList: 5,
            close: function() {
              // Assign selected users to hidden field
              $("#<%= hdnUsersToNotify.ClientID %>").val(JSON.stringify($('#<%= cblU.ClientID %>').val()));
            }
          })
          .multiselect('widget')
          .appendTo($('#<%= cid %>')); // Move to .gsp_ns namespace so it'll inherit the jQuery UI CSS classes
      };
   
      var configTooltips = function() {
        $('#<%= chkShowHeader.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_showHeader_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_showHeader_Bdy.JsEncode() %>'
        });

        $('#<%= lblWebsiteTitle.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_pageHeaderText_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_pageHeaderText_Bdy.JsEncode() %>'
        });

        $('#<%= lblWebsiteTitleUrl.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_pageHeaderTextUrl_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_pageHeaderTextUrl_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowLogin.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_showLogin_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_showLogin_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowSearch.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_showSearch_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_showSearch_Bdy.JsEncode() %>'
        });

        $('#<%= chkSendEmail.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_sendEmailOnError_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_sendEmailOnError_Bdy.JsEncode() %>'
        });

        $('#<%= chkEnableExceptionHandler.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_enableExceptionHandler_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_enableExceptionHandler_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowErrorDetails.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_showErrorDetails_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_showErrorDetails_Bdy.JsEncode() %>'
        });
      };
    })(jQuery);
  </script>
</asp:PlaceHolder>

