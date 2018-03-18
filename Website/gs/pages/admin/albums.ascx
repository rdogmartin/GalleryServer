<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="albums.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.albums" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
<%@ Import Namespace="GalleryServer.Web" %>
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
          <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_Thumbnail_Settings_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p>
          <asp:CheckBox ID="chkEnablePaging" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_EnablePaging_Lbl %>" />
        </p>
        <table class="gsp_standardTable gsp_addleftmargin10">
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblPageSize" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_PageSize_Label %>" />
            </td>
            <td>
              <asp:TextBox ID="txtPageSize" runat="server" />&nbsp;<asp:RangeValidator ID="rvPageSize"
                runat="server" Display="Dynamic" ControlToValidate="txtPageSize" Type="Integer"
                MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServer, Validation_Positive_Int_Text %>" />
            </td>
          </tr>
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblPagerLocation" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_PagerLocation_Label %>" />
            </td>
            <td>
              <asp:DropDownList ID="ddlPagerLocation" runat="server" />
            </td>
          </tr>
        </table>
        <table class="gsp_standardTable gsp_addtopmargin10">
          <tr>
            <td class="gsp_col1 gsp_aligntop">
              <asp:Label ID="lblSortField" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_SortField_Label %>" />
            </td>
            <td>
              <asp:DropDownList ID="ddlSortField" runat="server" />
              <asp:DropDownList ID="ddlSortDirection" runat="server">
                <Items>
                  <asp:ListItem Text="Ascending" Value="Ascending"></asp:ListItem>
                  <asp:ListItem Text="Descending" Value="Descending"></asp:ListItem>
                </Items>
              </asp:DropDownList>
              <p><asp:CheckBox ID="chkUpdateSort" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_UpdateSort_Label %>"/></p>
            </td>
          </tr>
        </table>
        <p>
          <asp:Label ID="lblTitleDisplayLength" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_TitleDisplayLength_Label %>" />&nbsp;<asp:TextBox
            ID="txtTitleDisplayLength" runat="server" />&nbsp;<asp:RangeValidator ID="rvTitleDisplayLength"
              runat="server" Display="Dynamic" ControlToValidate="txtTitleDisplayLength" Type="Integer"
              MinimumValue="1" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_1_To_100_Text %>" />
        </p>
      </div>
    </div>

    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_Sync_Settings_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p>
          <asp:CheckBox ID="chkEnableAutoSync" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_EnableAutoSync_Lbl %>" />
        </p>
        <div class="gsp_addleftmargin10">
          <p>
            <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_AutoSyncIntervalMinutes_Lbl1 %>" />
            &nbsp;<asp:TextBox ID="txtAutoSyncIntervalMinutes" runat="server" CssClass="gsp_textbox_narrow" />&nbsp;<asp:Label
              ID="lblAutoSyncIntervalMinutesLbl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_AutoSyncIntervalMinutes_Lbl2 %>" />
          </p>
          <p>
            <asp:RangeValidator ID="rv1" runat="server" Display="Dynamic" ControlToValidate="txtAutoSyncIntervalMinutes"
              Type="Integer" MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServer, Validation_Positive_Int_Text %>" />
          </p>
          <p>
            <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_LastAutoSync_Lbl %>" />&nbsp;<asp:Label
              ID="lblLastAutoSync" runat="server" CssClass="gsp_msgfriendly" />
          </p>
        </div>
        <p class="gsp_addtopmargin10">
          <asp:CheckBox ID="chkEnableRemoteSync" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_EnableRemoteSync_Lbl %>" />
        </p>
        <div class="gsp_addleftpadding6">
          <p>
            <asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_RemoteAccessPassword_Lbl %>" />
            &nbsp;<asp:TextBox ID="txtRemoteAccessPassword" runat="server" />
          </p>
          <p>
            <asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_RemoteAccess_Msg %>" />
          </p>
          <p runat="server" style="width: 100%; overflow: auto;">
            <a href="<%= SyncAlbumUrl %>">
              <%= Utils.HtmlEncode(SyncAlbumUrl) %></a>
          </p>
        </div>
      </div>
    </div>

    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Label ID="lblEmptyAlbumThmb" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_Empty_Album_Thumbnail_Settings_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <table class="gsp_standardTable">
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblThumbnailText" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_Text_Label %>" />
            </td>
            <td>
              <asp:TextBox ID="txtText" runat="server" />
            </td>
          </tr>
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblFontName" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_FontName_Label %>" />
            </td>
            <td>
              <asp:TextBox ID="txtFontName" runat="server" />
            </td>
          </tr>
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblFontSize" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_FontSize_Label %>" />
            </td>
            <td>
              <asp:TextBox ID="txtFontSize" runat="server" />&nbsp;<asp:RangeValidator ID="rvFontSize"
                runat="server" Display="Dynamic" ControlToValidate="txtFontSize" Type="Integer"
                MinimumValue="6" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Font_Size_Text %>" />
            </td>
          </tr>
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblFontColor" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_FontColor_Label %>" />
            </td>
            <td>
              <asp:TextBox ID="txtFontColor" runat="server" />
              <asp:RequiredFieldValidator ID="rfv2" runat="server" ControlToValidate="txtFontColor"
                Display="Static" ErrorMessage="<%$ Resources:GalleryServer, Admin_Albums_General_Invalid_Color_Text %>"
                ForeColor="" CssClass="gsp_msgfailure">
              </asp:RequiredFieldValidator>
              <asp:CustomValidator ID="cvFontColor" runat="server" ControlToValidate="txtFontColor"
                Text="<%$ Resources:GalleryServer, Admin_Albums_General_Invalid_Color_Text %>"
                ErrorMessage="<%$ Resources:GalleryServer, Admin_Albums_General_Invalid_Color_Text %>"
                CssClass="gsp_msgwarning" OnServerValidate="cvColor_ServerValidate"></asp:CustomValidator>
            </td>
          </tr>
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblBackgroundColor" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_BackgroundColor_Label %>" />
            </td>
            <td>
              <asp:TextBox ID="txtBackgroundColor" runat="server" />
              <asp:RequiredFieldValidator ID="rfv3" runat="server" ControlToValidate="txtBackgroundColor"
                Display="Static" ErrorMessage="<%$ Resources:GalleryServer, Admin_Albums_General_Invalid_Color_Text %>"
                ForeColor="" CssClass="gsp_msgfailure">
              </asp:RequiredFieldValidator>
              <asp:CustomValidator ID="cvBackgroundColor" runat="server" ControlToValidate="txtBackgroundColor"
                Text="<%$ Resources:GalleryServer, Admin_Albums_General_Invalid_Color_Text %>"
                ErrorMessage="<%$ Resources:GalleryServer, Admin_Albums_General_Invalid_Color_Text %>"
                CssClass="gsp_msgwarning" OnServerValidate="cvColor_ServerValidate"></asp:CustomValidator>
            </td>
          </tr>
          <tr>
            <td class="gsp_col1">
              <asp:Label ID="lblAspectRatio" runat="server" Text="<%$ Resources:GalleryServer, Admin_Albums_AspectRatio_Label %>" />
            </td>
            <td>
              <asp:TextBox ID="txtAspectRatio" runat="server" />
            </td>
          </tr>
        </table>
      </div>
    </div>
  </div>
  <tis:wwDataBinder ID="wwDataBinder" runat="server" OnAfterBindControl="wwDataBinder_AfterBindControl"
    OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl" OnValidateControl="wwDataBinder_ValidateControl">
    <DataBindingItems>
      <tis:wwDataBindingItem ID="bi1" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="PageSize" ControlId="txtPageSize" UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_PageSize_Label %>" />
      <tis:wwDataBindingItem ID="bi2" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="PagerLocation" ControlId="ddlPagerLocation" BindingProperty="SelectedValue"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_PagerLocation_Label %>" />
      <tis:wwDataBindingItem ID="bi2b" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="DefaultAlbumSortMetaName" ControlId="ddlSortField" BindingProperty="SelectedValue"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_SortField_Label %>" />
      <tis:wwDataBindingItem ID="bi3" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="MaxThumbnailTitleDisplayLength" ControlId="txtTitleDisplayLength"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_TitleDisplayLength_Label %>" />
      <tis:wwDataBindingItem ID="bi4" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EnableAutoSync" ControlId="chkEnableAutoSync" UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_EnableAutoSync_Lbl %>"
        BindingProperty="Checked" />
      <tis:wwDataBindingItem ID="bi5" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="AutoSyncIntervalMinutes" ControlId="txtAutoSyncIntervalMinutes"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_AutoSyncIntervalMinutes_Lbl %>" />
      <tis:wwDataBindingItem ID="bi6" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="LastAutoSync" ControlId="lblLastAutoSync" BindingMode="OneWay" DisplayFormat="{0:MMM dd, yyyy h:mm:ss tt (UTCzzz)}" />
      <tis:wwDataBindingItem ID="bi7" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EnableRemoteSync" ControlId="chkEnableRemoteSync" UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_EnableRemoteSync_Lbl %>"
        BindingProperty="Checked" />
      <tis:wwDataBindingItem ID="bi8" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="RemoteAccessPassword" ControlId="txtRemoteAccessPassword"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_RemoteAccessPassword_Lbl %>" />
      <tis:wwDataBindingItem ID="bi20" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EmptyAlbumThumbnailText" ControlId="txtText" UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_Text_Label %>" />
      <tis:wwDataBindingItem ID="bi21" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EmptyAlbumThumbnailFontName" ControlId="txtFontName" UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_FontName_Label %>" />
      <tis:wwDataBindingItem ID="bi22" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EmptyAlbumThumbnailFontSize" ControlId="txtFontSize" UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_FontSize_Label %>" />
      <tis:wwDataBindingItem ID="bi23" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EmptyAlbumThumbnailFontColor" ControlId="txtFontColor" UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_FontColor_Label %>" />
      <tis:wwDataBindingItem ID="bi24" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EmptyAlbumThumbnailBackgroundColor" ControlId="txtBackgroundColor"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_BackgroundColor_Label %>" />
      <tis:wwDataBindingItem ID="bi25" runat="server" BindingSource="GallerySettingsUpdateable"
        BindingSourceMember="EmptyAlbumThumbnailWidthToHeightRatio" ControlId="txtAspectRatio"
        UserFieldName="<%$ Resources:GalleryServer, Admin_Albums_AspectRatio_Label %>" />
      <tis:wwDataBindingItem runat="server" ControlId="chkEnablePaging">
      </tis:wwDataBindingItem>
    </DataBindingItems>
  </tis:wwDataBinder>
  <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
  <script>
    (function ($) {
      $(document).ready(function () {
        bindEventHandlers();
        updateUi();
        configTooltips();
      });

      var bindEventHandlers = function () {
        $('#<%= chkEnablePaging.ClientID %>, #<%= chkEnableAutoSync.ClientID %>, #<%= chkEnableRemoteSync.ClientID %>').click(updateUi);
      };

      var updateUi = function () {
        var txtPageSize = $('#<%= txtPageSize.ClientID %>');
        var chkEnablePaging = $('#<%= chkEnablePaging.ClientID %>');
        var ddlPagerLocation = $('#<%= ddlPagerLocation.ClientID %>');

        txtPageSize.prop('disabled', !chkEnablePaging.prop('checked') || chkEnablePaging.prop('disabled'));
        ddlPagerLocation.prop('disabled', txtPageSize.prop('disabled'));

        var chkEnableAutoSync = $('#<%= chkEnableAutoSync.ClientID %>');
        $('#<%= txtAutoSyncIntervalMinutes.ClientID %>').prop('disabled', !chkEnableAutoSync.prop('checked') || chkEnableAutoSync.prop('disabled'));

        var chkEnableRemoteSync = $('#<%= chkEnableRemoteSync.ClientID %>');
        $('#<%= txtRemoteAccessPassword.ClientID %>').prop('disabled', !chkEnableRemoteSync.prop('checked') || chkEnableRemoteSync.prop('disabled'));
      };

      var configTooltips = function () {
        $('#<%= chkEnablePaging.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_enabledPaging_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_enabledPaging_Bdy.JsEncode() %>'
        });

        $('#<%= lblPageSize.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_pageSize_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_pageSize_Bdy.JsEncode() %>'
        });

        $('#<%= lblPagerLocation.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_pagerLocation_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_pagerLocation_Bdy.JsEncode() %>'
        });

        $('#<%= lblSortField.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_sortByMetaName_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_sortByMetaName_Bdy.JsEncode() %>'
        });

        $('#<%= chkUpdateSort.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Albums_UpdateSort_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Albums_UpdateSort_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblTitleDisplayLength.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_maxAlbumThumbnailTitleDisplayLength_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_maxAlbumThumbnailTitleDisplayLength_Bdy.JsEncode() %>'
        });

        $('#<%= chkEnableAutoSync.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_enableAutoSync_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_enableAutoSync_Bdy.JsEncode() %>'
        });

        $('#<%= lblAutoSyncIntervalMinutesLbl.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_autoSyncIntervalMinutes_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_autoSyncIntervalMinutes_Bdy.JsEncode() %>'
        });

        $('#<%= chkEnableRemoteSync.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_enableRemoteSync_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_enableRemoteSync_Bdy.JsEncode() %>'
        });

        $('#<%= txtRemoteAccessPassword.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_remoteAccessPassword_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_remoteAccessPassword_Bdy.JsEncode() %>'
        });

        $('#<%= lblEmptyAlbumThmb.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Albums_Empty_Album_Thumbnail_Settings_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Albums_Empty_Album_Thumbnail_Settings_Dtl.JsEncode() %>'
        });

        $('#<%= lblThumbnailText.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailText_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailText_Bdy.JsEncode() %>'
        });

        $('#<%= lblFontName.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailFontName_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailFontName_Bdy.JsEncode() %>'
        });

        $('#<%= lblFontSize.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailFontSize_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailFontSize_Bdy.JsEncode() %>'
        });

        $('#<%= lblFontColor.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailFontColor_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailFontColor_Bdy.JsEncode() %>'
        });

        $('#<%= lblBackgroundColor.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailBackgroundColor_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailBackgroundColor_Bdy.JsEncode() %>'
        });

        $('#<%= lblAspectRatio.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailWidthToHeightRatio_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_defaultAlbumThumbnailWidthToHeightRatio_Bdy.JsEncode() %>'
        });
      };
    })(jQuery);
  </script>
</asp:PlaceHolder>

