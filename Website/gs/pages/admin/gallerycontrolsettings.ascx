<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="gallerycontrolsettings.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.gallerycontrolsettings" %>
<%@ Register Src="../../controls/albumtreeview.ascx" TagName="albumtreeview" TagPrefix="uc1" %>
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
          <asp:Label ID="lblViewMode" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ViewMode_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p>
          <asp:RadioButton ID="rbViewModeMultiple" runat="server" GroupName="grpViewMode" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ViewMode_Multiple_Label %>" />
        </p>
        <div>
          <p>
            <asp:RadioButton ID="rbViewModeSingle" runat="server" GroupName="grpViewMode" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ViewMode_Single_Label %>" />
          </p>
        </div>
      </div>
    </div>

    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Label ID="lblDefaultGalleryObject" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_Default_Gallery_Object_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p>
          <asp:RadioButton ID="rbDefaultGallery" runat="server" GroupName="grpDefaultObject" />
        </p>
        <div>
          <p>
            <asp:RadioButton ID="rbDefaultAlbum" runat="server" GroupName="grpDefaultObject"
              Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_Default_Album_Label %>" />
          </p>
          <div class="gsp_addleftpadding10" runat="server">
            <p id="gsp_defAlbum" class="gsp_gcs_da">
              <input id="gsp_iptDefAlbum" type="text" class="gsp_textbox" value="<%= DefaultAlbumTitle %>" /><img id="gsp_imgDefAlbum" alt="" src="<%= Utils.GetSkinnedUrl("/images/down-arrow-s-o.png") %>" />
            </p>
            <section id="gsp_defAlbumDropDown" class="gsp_gcs_da_dd ui-corner-bottom ui-corner-tr">
              <uc1:albumtreeview ID="tvUC" runat="server" AllowMultiCheck="false" />
            </section>
          </div>
        </div>
        <p>
          <asp:RadioButton ID="rbDefaultMediaObject" runat="server" GroupName="grpDefaultObject"
            Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_Default_MediaObject_Label %>" />
        </p>
        <div class="gsp_addleftpadding10">
          <asp:Literal ID="lb" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_Default_MediaObjectId_Label %>" />
          <asp:TextBox ID="txtDefaultMediaObjectId" runat="server" />
        </div>
        <p class="gsp_addtopmargin10">
          <asp:CheckBox ID="chkAllowUrlOverride" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_AllowUrlOverride_Label %>" />
        </p>
      </div>
    </div>

    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_Behavior_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p>
          <asp:CheckBox ID="chkOverride" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_AllowOverride_Label %>" />
        </p>
        <div class="gsp_addleftpadding6">
          <p class="gsp_addleftpadding10">
            <asp:Label ID="lblMediaViewSize" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_MediaViewSize_Label %>" />
            <asp:DropDownList ID="ddlMediaViewSize" runat="server" />
          </p>
          <p class="gsp_addleftpadding10">
            <asp:Label ID="lblSlideShowType" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_SlideShowType_Label %>" />
            <asp:DropDownList ID="ddlSlideShowType" runat="server" style="margin-right: 30px;" /><asp:CheckBox ID="chkSlideShowLoop" runat="server"  Text="<%$ Resources:GalleryServer, Admin_MediaObjects_SlideShowLoop_Label %>" ToolTip="<%$ Resources:GalleryServer, Admin_MediaObjects_SlideShowLoop_Tt %>"/>
          </p>
          <p class="gsp_addleftpadding10">
            <asp:Label ID="lblTreeviewNavigateUrl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_TreeviewNavUrl_Label %>" />
            <asp:TextBox ID="txtTreeviewNavigateUrl" runat="server" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowLeftPaneForAlbum" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowAlbumTreeViewForAlbum_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowLeftPaneForMO" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowAlbumTreeViewForMediaObject_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowCenterPane" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowCenterPane_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowRightPane" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowRightPane_Label %>" />
          </p>
          <p class="gsp_addtopmargin5">
            <asp:CheckBox ID="chkShowHeader" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowHeader_Label %>" />
          </p>
          <div class="gsp_addleftmargin5">
            <table class="gsp_standardTable">
              <tr>
                <td class="gsp_col1">
                  <asp:Label ID="lblGalleryTitle" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_WebsiteTitle_Label %>" />
                </td>
                <td class="gsp_col2">
                  <asp:TextBox ID="txtGalleryTitle" runat="server" CssClass="gsp_textbox" />
                </td>
              </tr>
              <tr>
                <td class="gsp_col1">
                  <asp:Label ID="lblGalleryTitleUrl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_WebsiteTitleUrl_Label %>" />
                </td>
                <td>
                  <asp:TextBox ID="txtGalleryTitleUrl" runat="server" CssClass="gsp_textbox" />
                </td>
              </tr>
            </table>
            <p>
              <asp:CheckBox ID="chkShowLogin" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowLogin_Label %>" />
            </p>
            <p>
              <asp:CheckBox ID="chkShowSearch" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowSearch_Label %>" />
            </p>
              <p>
                <asp:CheckBox ID="chkShowRibbonToolbar" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowRibbonToolbar_Label %>" />
              </p>
              <p>
                <asp:CheckBox ID="chkShowAlbumBreadcrumb" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowAlbumBreadcrumb_Label %>" />
              </p>
          </div>
          <p class="gsp_addtopmargin5">
            <asp:CheckBox ID="chkAllowAnonBrowsing" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_AllowAnonymousBrowsing_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowMediaObjectNavigation" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowMediaObjectNavigation_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowMediaObjectIndexPosition" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowMediaObjectIndexPosition_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkShowMediaObjectTitle" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_ShowMediaObjectTitle_Label %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkAutoPlaySlideshow" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_AutoPlaySlideShow_Label %>" />
          </p>
        </div>
      </div>
    </div>

    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
  </div>
</div>
<asp:PlaceHolder runat="server">
  <script>
    (function ($) {

      $(document).ready(function () {
        bindEvents();
        configAllControls();
        configTooltips();
      });

      var bindEvents = function () {
        $('#<%= rbDefaultGallery.ClientID %>').click(configDefaultGalleryObjectControls);
        $('#<%= rbDefaultAlbum.ClientID %>').click(configDefaultGalleryObjectControls);
        $('#<%= rbDefaultMediaObject.ClientID %>').click(configDefaultGalleryObjectControls);
        $('#<%= chkOverride.ClientID %>').click(configOverridableControls);
        $('#<%= chkShowLeftPaneForAlbum.ClientID %>').click(configShowLeftPaneControls);
        $('#<%= chkShowLeftPaneForMO.ClientID %>').click(configShowLeftPaneControls);
        $('#<%= chkShowHeader.ClientID %>').click(configShowHeaderControls);
      };

      var configAllControls = function () {
        configDefaultGalleryObjectControls();

        configOverridableControls();
      };

      var configShowLeftPaneControls = function () {
        var chkShowLeftPaneForAlbum = $('#<%= chkShowLeftPaneForAlbum.ClientID %>');
        var chkShowLeftPaneForMo = $('#<%= chkShowLeftPaneForMO.ClientID %>');

        $('#<%= txtTreeviewNavigateUrl.ClientID %>').prop('disabled', (!chkShowLeftPaneForAlbum.prop('checked') && !chkShowLeftPaneForMo.prop('checked')));
      };

      var configDefaultGalleryObjectControls = function () {

        var mouseIsInsideDefaultAlbumSection = false;
        var rbDefaultAlbum = $('#<%= rbDefaultAlbum.ClientID %>');
        var rbDefaultMediaObject = $('#<%= rbDefaultMediaObject.ClientID %>');

        if (!rbDefaultAlbum.prop('checked') || rbDefaultAlbum.prop('disabled')) {
          $("#gsp_defAlbum").off("click");
          $("#gsp_iptDefAlbum").prop('disabled', true);
        } else {
          // Default album is selected. Bind treeview dropdown
          $("#gsp_defAlbum").click(function () {
            $("#gsp_defAlbumDropDown").slideToggle();
          });

          $("#gsp_iptDefAlbum").prop('disabled', false);

          $("#<%= tvUC.TreeViewClientId %>").on("changed.jstree", function (e, data) {
            switch (data.action) {
              case 'select_node':
                $("#gsp_iptDefAlbum").val(data.node.text);
                $("#gsp_defAlbumDropDown").slideUp();
                break;
              case 'deselect_node':
                $("#gsp_defAlbumDropDown").slideUp();
                break;
            }
          });
        }

        $('#gsp_defAlbum, #gsp_defAlbumDropDown').hover(function () {
          mouseIsInsideDefaultAlbumSection = true;
        }, function () {
          mouseIsInsideDefaultAlbumSection = false;
        });

        $("body").mouseup(function () {
          if (!mouseIsInsideDefaultAlbumSection) $('#gsp_defAlbumDropDown').slideUp();
        });

        $('#<%= txtDefaultMediaObjectId.ClientID %>').prop('disabled', (!rbDefaultMediaObject.prop('checked') || rbDefaultMediaObject.prop('disabled')));
      };

      var configOverridableControls = function () {
        var chkOverride = $('#<%= chkOverride.ClientID %>');
        var chkShowHeader = $('#<%= chkShowHeader.ClientID %>');

        var isOverrideInactive = (!chkOverride.prop('checked') || chkOverride.prop('disabled'));

        $('#<%= chkShowLeftPaneForAlbum.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkShowLeftPaneForMO.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkShowCenterPane.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkShowRightPane.ClientID %>').prop('disabled', isOverrideInactive);

        chkShowHeader.prop('disabled', isOverrideInactive);

        configShowLeftPaneControls();
        configShowHeaderControls();

        $('#<%= chkAllowAnonBrowsing.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkShowMediaObjectNavigation.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkShowMediaObjectIndexPosition.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkShowMediaObjectTitle.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkAutoPlaySlideshow.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= ddlMediaViewSize.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= ddlSlideShowType.ClientID %>').prop('disabled', isOverrideInactive);
        $('#<%= chkSlideShowLoop.ClientID %>').prop('disabled', isOverrideInactive);
      };

      var configShowHeaderControls = function () {
        var chkShowHeader = $('#<%= chkShowHeader.ClientID %>');
        var isShowHeaderInactive = (!chkShowHeader.prop('checked') || chkShowHeader.prop('disabled'));

        $('#<%= txtGalleryTitle.ClientID %>').prop('disabled', isShowHeaderInactive);
        $('#<%= txtGalleryTitleUrl.ClientID %>').prop('disabled', isShowHeaderInactive);
        $('#<%= chkShowLogin.ClientID %>').prop('disabled', isShowHeaderInactive);
        $('#<%= chkShowSearch.ClientID %>').prop('disabled', isShowHeaderInactive);
        $('#<%= chkShowRibbonToolbar.ClientID %>').prop('disabled', isShowHeaderInactive);
        $('#<%= chkShowAlbumBreadcrumb.ClientID %>').prop('disabled', isShowHeaderInactive);
      };

      var configTooltips = function () {
        $('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_Overview_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_Overview_Bdy.JsEncode() %>'
        });

        $('#<%= lblViewMode.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ViewMode_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ViewMode_Bdy.JsEncode() %>'
        });

        $('#<%= lblDefaultGalleryObject.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_DefaultGalleryObject_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_DefaultGalleryObject_Bdy.JsEncode() %>'
        });

        $('#<%= chkAllowUrlOverride.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_AllowUrlOverride_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_AllowUrlOverride_Bdy.JsEncode() %>'
        });

        $('#<%= chkOverride.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_OverrideSettings_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_OverrideSettings_Bdy.JsEncode() %>'
        });

        $('#<%= lblMediaViewSize.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_MediaViewSize_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_MediaViewSize_Bdy.JsEncode() %>'
        });

        $('#<%= lblSlideShowType.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_SlideShowType_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_SlideShowType_Bdy.JsEncode() %>'
        });

        $('#<%= lblTreeviewNavigateUrl.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_TreeviewNavigateUrl_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_TreeviewNavigateUrl_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowLeftPaneForAlbum.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowAlbumTreeViewForAlbum_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowAlbumTreeViewForAlbum_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowLeftPaneForMO.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowAlbumTreeViewForMediaObject_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowAlbumTreeViewForMediaObject_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowCenterPane.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowCenterPane_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowCenterPane_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowRightPane.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowRightPane_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowRightPane_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowHeader.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowHeader_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowHeader_Bdy.JsEncode() %>'
        });

        $('#<%= lblGalleryTitle.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_GalleryTitle_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_GalleryTitle_Bdy.JsEncode() %>'
        });

        $('#<%= lblGalleryTitleUrl.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_GalleryTitleUrl_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_GalleryTitleUrl_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowLogin.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowLogin_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowLogin_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowSearch.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowSearch_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowSearch_Bdy.JsEncode() %>'
        });

        $('#<%= chkAllowAnonBrowsing.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_AllowAnonBrowsing_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_AllowAnonBrowsing_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowRibbonToolbar.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowRibbonToolbar_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowRibbonToolbar_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowAlbumBreadcrumb.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowAlbumBreadcrumb_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowAlbumBreadcrumb_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowMediaObjectNavigation.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowMediaObjectNavigation_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowMediaObjectNavigation_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowMediaObjectIndexPosition.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowMediaObjectIndexPosition_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowMediaObjectIndexPosition_Bdy.JsEncode() %>'
        });

        $('#<%= chkShowMediaObjectTitle.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_ShowMediaObjectTitle_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_ShowMediaObjectTitle_Bdy.JsEncode() %>'
        });

        $('#<%= chkAutoPlaySlideshow.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_GCS_AutoPlaySlideshow_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_GCS_AutoPlaySlideshow_Bdy.JsEncode() %>'
        });
      };
    })(jQuery);
  </script>
</asp:PlaceHolder>
