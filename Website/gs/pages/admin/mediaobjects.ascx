<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="mediaobjects.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.mediaobjects" %>
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
                    <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_Storage_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p class="gsp_bold">
                    <asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_Original_Storage_Label %>" />
                </p>
                <div class="gsp_addleftpadding6">
                    <p class="gsp_addtopmargin5">
                        <asp:Label ID="lblMediaPath" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_MoPath_Label %>" />
                    </p>
                    <p class="gsp_collapse">
                        <asp:TextBox ID="txtMoPath" runat="server" CssClass="gsp_textbox" />
                    </p>
                    <p class="gsp_addtopmargin5">
                        <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_CurrentMoPath_Label %>" />
                    </p>
                    <p class="gsp_addleftpadding6">
                        <asp:Label ID="lblMoPath" runat="server" CssClass="gsp_msgfriendly" />
                    </p>
                    <p class="gsp_addtopmargin5">
                        <asp:CheckBox ID="chkPathIsReadOnly" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_MoPathIsReadOnly_Label %>" />
                    </p>
                    <p>
                        <asp:CheckBox ID="chkSynchAlbumTitleAndDirectoryName" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_SynchAlbumTitleAndDirectoryName_Label %>" />
                    </p>
                </div>
                <p class="gsp_bold gsp_addtopmargin10">
                    <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_Thumbnail_Storage_Label %>" />
                </p>
                <div class="gsp_addleftpadding6">
                    <p>
                        <asp:Label ID="lblThumbCachePath" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_ThumbnailCachePath_Label %>" />
                    </p>
                    <p class="gsp_collapse">
                        <asp:TextBox ID="txtThumbnailCachePath" runat="server" CssClass="gsp_textbox" />
                    </p>
                    <p class="gsp_addtopmargin5">
                        <asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_CurrentThumbnailCachePath_Label %>" />
                    </p>
                    <p class="gsp_addleftpadding6">
                        <asp:Label ID="lblThumbnailCachePath" runat="server" CssClass="gsp_msgfriendly" />
                    </p>
                </div>
                <p class="gsp_bold gsp_addtopmargin10">
                    <asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_Compressed_Storage_Label %>" />
                </p>
                <div class="gsp_addleftpadding6">
                    <p>
                        <asp:Label ID="lblOptCachePath" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_OptimizedCachePath_Label %>" />
                    </p>
                    <p class="gsp_collapse">
                        <asp:TextBox ID="txtOptimizedCachePath" runat="server" CssClass="gsp_textbox" />
                    </p>
                    <p class="gsp_addtopmargin5">
                        <asp:Literal ID="l10" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_CurrentOptimizedCachePath_Label %>" />
                    </p>
                    <p class="gsp_addleftpadding6">
                        <asp:Label ID="lblOptimizedCachePath" runat="server" CssClass="gsp_msgfriendly" />
                    </p>
                </div>
            </div>
        </div>

        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l12" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_Download_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p class="gsp_addtopmargin5 gsp_addleftmargin5">
                    <asp:Label ID="lblMediaViewSize" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_MediaViewSize_Label %>" />
                    <asp:DropDownList ID="ddlMediaViewSize" runat="server" />
                </p>
                <p class="gsp_addtopmargin5 gsp_addleftmargin5">
                    <asp:CheckBox ID="chkEnableAnonMoDownload" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableAnonMoDownload_Label %>" />
                </p>
                <p class="gsp_addleftmargin5">
                    <asp:CheckBox ID="chkEnableMoDownload" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableMoDownload_Label %>" />
                </p>
                <div class="gsp_addleftmargin5">
                    <p class="gsp_addleftmargin5">
                        <asp:CheckBox ID="chkEnableGoZipDownload" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableMoZipDownload_Label %>" />
                    </p>
                    <div class="gsp_addleftmargin5">
                        <p class="gsp_addleftmargin5">
                            <asp:CheckBox ID="chkEnableAlbumZipDownload" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableAlbumZipDownload_Label %>" />
                        </p>
                    </div>
                </div>
            </div>
        </div>

        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l17" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_Upload_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p class="gsp_addtopmargin5">
                    <asp:CheckBox ID="chkAllowAddLocalContent" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_AllowAddLocalContent_Label %>" />
                </p>
                <p>
                    <asp:CheckBox ID="chkAllowAddExternalContent" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_AllowAddExternalContent_Label %>" />
                </p>
                <p class="gsp_addtopmargin5">
                    <asp:Label ID="lblMaxUploadSize" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_MaxUploadSize_Label %>" />
                    <asp:TextBox ID="txtMaxUploadSize" runat="server" />&nbsp;<asp:RangeValidator ID="rvMaxUploadSize"
                        runat="server" Display="Dynamic" ControlToValidate="txtMaxUploadSize" Type="Integer"
                        MinimumValue="0" MaximumValue="2147483647" Text="<%$ Resources:GalleryServer, Validation_Positive_Int_Text %>" />
                </p>
            </div>
        </div>

        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l11" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_TransitionEffects_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p>
                    <asp:CheckBox ID="chkEnableSlideShow" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableSlideShow_Label %>" />
                </p>
                <p class="gsp_addleftpadding10">
                    <asp:Label ID="lblSlideShowType" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_SlideShowType_Label %>" />
                    <asp:DropDownList ID="ddlSlideShowType" runat="server" Style="margin-right: 30px;" /><asp:CheckBox ID="chkSlideShowLoop" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_SlideShowLoop_Label %>" ToolTip="<%$ Resources:GalleryServer, Admin_MediaObjects_SlideShowLoop_Tt %>" />
                </p>
                <p class="gsp_addleftmargin10">
                    <asp:Label ID="lblSlideShowInterval" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_SlideShowInterval_Label %>" />
                    <asp:TextBox ID="txtSlideShowInterval" runat="server" />&nbsp;<asp:RangeValidator
                        ID="rvSlideShowInterval" runat="server" Display="Dynamic" ControlToValidate="txtSlideShowInterval"
                        Type="Integer" MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServer, Validation_Positive_Int_Text %>"
                        ErrorMessage="<%$ Resources:GalleryServer, Validation_Positive_Int_Text %>" />
                </p>
                <p class="gsp_addtopmargin5">
                    <asp:Label ID="lblTransType" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_TransType_Label %>" />
                    <asp:DropDownList ID="ddlTransType" runat="server" />
                </p>
                <p>
                    <asp:Label ID="lblTransDuration" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_TransDuration_Label %>" />
                    <asp:TextBox ID="txtTransDuration" runat="server" />&nbsp;<asp:RangeValidator ID="rvTransDuration"
                        runat="server" Display="Dynamic" ControlToValidate="txtTransDuration" Type="Double"
                        CultureInvariantValues="true" MinimumValue=".000001" MaximumValue="2147483647"
                        Text="<%$ Resources:GalleryServer, Validation_Positive_Double_Text %>" ErrorMessage="<%$ Resources:GalleryServer, Validation_Positive_Double_Text %>" />
                </p>
            </div>
        </div>

        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l14" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_Thumbnail_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <table class="gsp_addtopmargin5 gsp_standardTable">
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblMaxThumbnailLength" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_MaxThumbnailLength_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtMaxThumbnailLength" runat="server" />&nbsp;<asp:RangeValidator
                                ID="rvMaxThumbnailLength" runat="server" Display="Dynamic" ControlToValidate="txtMaxThumbnailLength"
                                Type="Integer" MinimumValue="10" MaximumValue="100000" Text="<%$ Resources:GalleryServer, Validation_Int_10_To_100000_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblThumbJpegQuality" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_ThumbnailJpegQuality_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtThumbJpegQuality" runat="server" />&nbsp;<asp:RangeValidator
                                ID="rvThumbJpegQuality" runat="server" Display="Dynamic" ControlToValidate="txtThumbJpegQuality"
                                Type="Integer" MinimumValue="1" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_1_To_100_Text %>" />
                            &nbsp;<asp:Label ID="l15" runat="server" CssClass="gsp_fs" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_ThumbnailJpegQuality_Label2 %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblThumbFileNamePrefix" runat="server" Text="<%$ Resources:GalleryServer, Admin_MediaObjects_ThumbFileNamePrefix_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtThumbFileNamePrefix" runat="server" />
                            <asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtThumbFileNamePrefix"
                                Display="Static" ErrorMessage="<%$ Resources:GalleryServer, Site_Field_Required_Text %>"
                                ForeColor="" CssClass="gsp_msgfailure">
                            </asp:RequiredFieldValidator>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
    <tis:wwDataBinder ID="wwDataBinder" runat="server" OnValidateControl="wwDataBinder_ValidateControl"
        OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl">
        <DataBindingItems>
            <tis:wwDataBindingItem ID="WwDataBindingItem1" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MediaObjectPath" ControlId="txtMoPath" IsRequired="True" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_MoPath_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem1d" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MediaObjectPathIsReadOnly" ControlId="chkPathIsReadOnly" BindingProperty="Checked"
                UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_MoPathIsReadOnly_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem1b" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="SynchAlbumTitleAndDirectoryName" ControlId="chkSynchAlbumTitleAndDirectoryName"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_SynchAlbumTitleAndDirectoryName_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem2" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="ThumbnailPath" ControlId="txtThumbnailCachePath" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_ThumbnailCachePath_Label %>"
                IsRequired="false" />
            <tis:wwDataBindingItem ID="WwDataBindingItem3" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="OptimizedPath" ControlId="txtOptimizedCachePath" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_OptimizedCachePath_Label %>"
                IsRequired="false" />
            <tis:wwDataBindingItem ID="WwDataBindingItem9e" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="EnableMediaObjectDownload" ControlId="chkEnableMoDownload"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableMoDownload_Label %>" />
            <tis:wwDataBindingItem ID="wbi5" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="EnableAnonymousOriginalMediaObjectDownload" ControlId="chkEnableAnonMoDownload"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableAnonMoDownload_Label %>" />

            <tis:wwDataBindingItem ID="WwDataBindingItem9f" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="EnableGalleryObjectZipDownload" ControlId="chkEnableGoZipDownload"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableMoZipDownload_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem9j" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="EnableAlbumZipDownload" ControlId="chkEnableAlbumZipDownload"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableMoZipDownload_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem9h" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="EnableSlideShow" ControlId="chkEnableSlideShow" BindingProperty="Checked"
                UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_EnableSlideShow_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem4a" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MediaViewSize" ControlId="ddlMediaViewSize" BindingProperty="SelectedValue"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_MediaViewSize_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem4b" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="SlideShowType" ControlId="ddlSlideShowType" BindingProperty="SelectedValue"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Control_Settings_SlideShowType_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem4d" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="SlideShowLoop" ControlId="chkSlideShowLoop" BindingProperty="Checked"
                UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_SlideShowLoop_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem9i" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="SlideshowInterval" ControlId="txtSlideShowInterval" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_SlideShowInterval_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem4" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MediaObjectTransitionType" ControlId="ddlTransType" BindingProperty="SelectedValue"
                UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_TransType_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem5" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MediaObjectTransitionDuration" ControlId="txtTransDuration"
                UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_TransDuration_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem6" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MaxThumbnailLength" ControlId="txtMaxThumbnailLength" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_MaxThumbnailLength_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem8" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="AllowAddLocalContent" ControlId="chkAllowAddLocalContent"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_AllowAddLocalContent_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem9" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="AllowAddExternalContent" ControlId="chkAllowAddExternalContent"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_AllowAddExternalContent_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem10" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MaxUploadSize" ControlId="txtMaxUploadSize" UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_MaxUploadSize_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem11" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="ThumbnailImageJpegQuality" ControlId="txtThumbJpegQuality"
                UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_ThumbnailJpegQuality_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem4c" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="ThumbnailFileNamePrefix" ControlId="txtThumbFileNamePrefix"
                UserFieldName="<%$ Resources:GalleryServer, Admin_MediaObjects_ThumbFileNamePrefix_Label %>" />
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
                $('#<%= chkEnableSlideShow.ClientID %>, #<%= chkEnableGoZipDownload.ClientID %>, #<%= chkEnableMoDownload.ClientID %>').click(updateUi);
      };

        var updateUi = function () {
            var chkEnableSlideShow = $('#<%= chkEnableSlideShow.ClientID %>');

          $('#<%= txtSlideShowInterval.ClientID %>').prop('disabled', !chkEnableSlideShow.prop('checked') || chkEnableSlideShow.prop('disabled'));
          $('#<%= ddlSlideShowType.ClientID %>').prop('disabled', !chkEnableSlideShow.prop('checked') || chkEnableSlideShow.prop('disabled'));

          var chkEnableMoDownload = $('#<%= chkEnableMoDownload.ClientID %>');
          if (!chkEnableMoDownload.prop('checked')) {
              $('#<%= chkEnableGoZipDownload.ClientID %>, #<%= chkEnableAlbumZipDownload.ClientID %>').prop('checked', false);
        }

          $('#<%= chkEnableGoZipDownload.ClientID %>').prop('disabled', (!chkEnableMoDownload.prop('checked') || chkEnableMoDownload.prop('disabled')));

          var chkEnableGoZipDownload = $('#<%= chkEnableGoZipDownload.ClientID %>');
          if (!chkEnableGoZipDownload.prop('checked')) {
              $('#<%= chkEnableAlbumZipDownload.ClientID %>').prop('checked', false);
        }

          $('#<%= chkEnableAlbumZipDownload.ClientID %>').prop('disabled', !chkEnableGoZipDownload.prop('checked') || chkEnableGoZipDownload.prop('disabled'));
      };

        var configTooltips = function () {
            $('#<%= lblMediaPath.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_mediaObjectPath_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_mediaObjectPath_Bdy.JsEncode() %>'
        });

          $('#<%= chkPathIsReadOnly.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_mediaObjectPathIsReadOnly_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_mediaObjectPathIsReadOnly_Bdy.JsEncode() %>'
        });

          $('#<%= chkSynchAlbumTitleAndDirectoryName.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_synchAlbumTitleAndDirectoryName_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_synchAlbumTitleAndDirectoryName_Bdy.JsEncode() %>'
        });

          $('#<%= lblThumbCachePath.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_thumbnailPath_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_thumbnailPath_Bdy.JsEncode() %>'
        });

          $('#<%= lblOptCachePath.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_optimizedPath_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_optimizedPath_Bdy.JsEncode() %>'
        });

          $('#<%= lblMediaViewSize.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_mediaViewSize_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_mediaViewSize_Bdy.JsEncode() %>'
        });

          $('#<%= chkEnableMoDownload.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_enableMoDownload_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_enableMoDownload_Bdy.JsEncode() %>'
        });

          $('#<%= chkEnableAnonMoDownload.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_allowAnonMoDownload_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_allowAnonMoDownload_Bdy.JsEncode() %>'
        });

          $('#<%= chkEnableGoZipDownload.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_enableMoZipDownload_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_enableMoZipDownload_Bdy.JsEncode() %>'
        });

          $('#<%= chkEnableAlbumZipDownload.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_enableAlbumZipDownload_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_enableAlbumZipDownload_Bdy.JsEncode() %>'
        });

          $('#<%= chkEnableSlideShow.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_enableSlideShow_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_enableSlideShow_Bdy.JsEncode() %>'
        });

          $('#<%= lblSlideShowType.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_slideShowType_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_slideShowType_Bdy.JsEncode() %>'
        });

          $('#<%= lblTransType.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_mediaObjectTransitionType_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_mediaObjectTransitionType_Bdy.JsEncode() %>'
        });

          $('#<%= lblTransDuration.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_mediaObjectTransitionDuration_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_mediaObjectTransitionDuration_Bdy.JsEncode() %>'
        });

          $('#<%= chkAllowAddLocalContent.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_AllowAddLocalContent_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_AllowAddLocalContent_Bdy.JsEncode() %>'
        });

          $('#<%= chkAllowAddExternalContent.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_AllowAddExternalContent_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_AllowAddExternalContent_Bdy.JsEncode() %>'
        });

          $('#<%= lblMaxUploadSize.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_MaxUploadSize_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_MaxUploadSize_Bdy.JsEncode() %>'
        });

          $('#<%= lblMaxThumbnailLength.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_maxThumbnailLength_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_maxThumbnailLength_Bdy.JsEncode() %>'
        });

          $('#<%= lblThumbJpegQuality.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_thumbnailImageJpegQuality_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_thumbnailImageJpegQuality_Bdy.JsEncode() %>'
        });

          $('#<%= lblThumbFileNamePrefix.ClientID %>').gsTooltip({
              title: '<%= Resources.GalleryServer.Cfg_thumbnailFileNamePrefix_Hdr.JsEncode() %>',
            content: '<%= Resources.GalleryServer.Cfg_thumbnailFileNamePrefix_Bdy.JsEncode() %>'
        });
      };
    })(jQuery);
    </script>
</asp:PlaceHolder>

