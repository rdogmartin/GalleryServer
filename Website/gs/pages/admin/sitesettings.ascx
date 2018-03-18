<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="sitesettings.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.sitesettings" %>
<%@ Import Namespace="GalleryServer.Web" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
<div class="gsp_content">
    <p class="gsp_a_ap_to">
        <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
            EnableViewState="false" />&nbsp;<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
    </p>
    <asp:PlaceHolder ID="phAdminHeader" runat="server" />
    <div class="gsp_addleftpadding5">
        <div class="gsp_single_tab ">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_ProductKey_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p class="gsp_addtopmargin5">
                    <asp:Label ID="lblLicenseEmail" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_LicenseEmail_Label %>" />&nbsp;
          <asp:TextBox ID="txtLicenseEmail" runat="server" TextMode="Email" CssClass="gsp_textbox" />
                </p>
                <p class="gsp_addtopmargin5">
                    <asp:Label ID="lblProductKey" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_ProductKey_Label %>" />&nbsp;
          <asp:TextBox ID="txtLicenseKey" runat="server" CssClass="gsp_textbox" />&nbsp;<asp:LinkButton id="hlDeactivate" runat="server" Text="Deactivate" OnClick="hlDeactivate_OnClick"></asp:LinkButton>
                </p>
                <p class="gsp_nopadding">
                    <span class="gs_a_ss_lk_lbl_ico fa fa-lg"></span>
                    <asp:Label ID="lblProductKeyValidationMsg" runat="server" CssClass="gsp_msgfriendly" />
                </p>
                <p class="gsp_bold">
                    <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_GSP_Hdr %>" />
                    <asp:Label ID="lblVersion" runat="server" CssClass="gsp_msgfriendly" />
                </p>
                <p class="gsp_addtopmargin5">
                    <asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_GSP_Home_Page %>" />
                </p>
            </div>
        </div>
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_AppSettings_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p>
                    <asp:CheckBox ID="chkEnableCache" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_EnableCache_Label %>" />&nbsp;<asp:Button
                        ID="btnPurgeCache" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_PurgeCache_Btn_Lbl %>"
                        OnClick="btnPurgeCache_Click" />
                </p>
                <p>
                    <asp:CheckBox ID="chkAllowGalleryAdminToManageUsersAndRoles" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_AllowGalleryAdminToManageUsersAndRoles_Label %>" />
                </p>
                <p>
                    <asp:CheckBox ID="chkAllowGalleryAdminToViewAllUsersAndRoles" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_AllowGalleryAdminToViewAllUsersAndRoles_Label %>" />
                </p>
                <p>
                    <asp:CheckBox ID="chkAutoTrimLog" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_MaxNumberErrorItems_Lbl %>" />
                </p>
                <p class="gsp_addleftmargin10">
                    <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_MaxNumberErrorItems_Label %>" />:
          <asp:TextBox ID="txtMaxErrorItems" runat="server" />&nbsp;<asp:RangeValidator ID="rvMaxErrorItems"
              runat="server" Display="Dynamic" ControlToValidate="txtMaxErrorItems" Type="Integer"
              MinimumValue="1" MaximumValue="2147483647" Text="<%$ Resources:GalleryServer, Validation_Positive_Int_Text %>" CssClass="gsp_msgattention" />
                </p>
                <table class="gsp_standardTable gsp_addtopmargin5">
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblSkin" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_Skin_Label %>" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlSkin" runat="server" CssClass="gsp_a_ss_s_ddl" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblJQuery" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_JQuery_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtJQueryScriptPath" runat="server" CssClass="gsp_textbox" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblJQueryMigrate" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_JQuery_Migrate_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtJQueryMigrateScriptPath" runat="server" CssClass="gsp_textbox" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblJQueryUI" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_JQueryUI_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtJQueryUiScriptPath" runat="server" CssClass="gsp_textbox" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblImageMagick" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_ImageMagickPath_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtImageMagickPath" runat="server" CssClass="gsp_textbox" />&nbsp;<span class="fa fa-lg gs_a_ss_im_vry" title="Verified"></span>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Email_Settings_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <div class="gsp_addleftpadding6">
                    <table class="gsp_standardTable gsp_addtopmargin5">
                        <tr>
                            <td class="gsp_col1">
                                <asp:Label ID="lblSmtpServer" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_SmtpServer_Label %>" />
                            </td>
                            <td>
                                <asp:TextBox ID="txtSmtpServer" runat="server" CssClass="gsp_textbox" />
                            </td>
                        </tr>
                        <tr>
                            <td class="gsp_col1">
                                <asp:Label ID="lblSmtpPort" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_SmtpPort_Label %>" />
                            </td>
                            <td>
                                <asp:TextBox ID="txtSmtpPort" runat="server" CssClass="gsp_textbox" />
                            </td>
                        </tr>
                    </table>
                    <p class="gsp_addtopmargin5">
                        <asp:CheckBox ID="chkUseSsl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_UseSsl_Label %>" />
                    </p>
                </div>
                <p class="gsp_bold gsp_addtopmargin5">
                    <asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_From_Hdr %>" />
                </p>
                <table class="gsp_addleftpadding6 gsp_standardTable">
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblEmailFromName" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_EmailName_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtEmailFromName" runat="server" CssClass="gsp_textbox" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblEmailFromAddress" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_EmailAddress_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtEmailFromAddress" runat="server" TextMode="Email" CssClass="gsp_textbox" />
                        </td>
                    </tr>
                </table>
                <p class="gsp_addleftpadding6">
                    <asp:Button ID="btnEmailTest" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Sent_Test_Email_Text %>"
                        OnClick="btnEmailTest_Click" />
                </p>
            </div>
        </div>

        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l10" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_AppInfo_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <table class="gsp_addleftpadding6 gsp_standardTable">
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblTrustLevelLbl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_TrustLevel_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblTrustLevel" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblNetVersionLbl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_NetVersion_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblNetVersion" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblIisAppPoolLbl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_IisAppPoolIdentity_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblIisAppPool" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblFFmpegLbl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_FFmpeg_Label %>" />
                        </td>
                        <td><span class="gsp_msgfriendly" runat="server">
                            <%= String.IsNullOrEmpty(AppSettings.FFmpegPath) ? Resources.GalleryServer.Site_No : Resources.GalleryServer.Site_Yes %>
                        </span></td>
                    </tr>
                </table>
            </div>
        </div>
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Label ID="lblProviderLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <table class="gsp_addleftpadding6 gsp_standardTable">
                    <tr>
                        <td colspan="2">
                            <p class="gsp_bold">
                                <asp:Label ID="lblGalleryDataLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_GalleryData_Hdr %>" />
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblGalleryDataProviderLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_Description_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblGalleryDataProvider" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                    <tr id="trCompact" runat="server" visible="false">
                        <td class="gsp_col1 gsp_aligntop">Data file: </td>
                        <td class="gsp_msgfriendly">
                            <asp:Label ID="lblDbFilename" runat="server" />
                            <asp:Label ID="lblDbFileSize" runat="server" /><p>
                                <asp:LinkButton ID="lbCompactDb" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_CompactDb_Btn_Lbl %>"
                                    ToolTip="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_CompactDb_Btn_Lbl %>"
                                    OnClick="lbCompactDb_Click" />
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <p class="gsp_bold gsp_addtopmargin5">
                                <asp:Label ID="lblMembershipLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_Membership_Hdr %>" />
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblMembershipDataProviderLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_Description_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblMembershipDataProvider" runat="server" CssClass="gsp_msgfriendly" />
                            -
              <asp:Label ID="lblMembershipDataProviderDesc" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblMembershipAppNameLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_AppName_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblMembershipAppName" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                    <tr>
                        <td colspan="2">
                            <p class="gsp_bold gsp_addtopmargin5">
                                <asp:Label ID="lblRoleLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_Roles_Hdr %>" />
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblRoleDataProviderLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_Description_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblRoleDataProvider" runat="server" CssClass="gsp_msgfriendly" />
                            -
              <asp:Label ID="lblRoleDataProviderDesc" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblRoleAppNameLabel" runat="server" Text="<%$ Resources:GalleryServer, Admin_Site_Settings_DataProvider_AppName_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblRoleAppName" runat="server" CssClass="gsp_msgfriendly" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
    <tis:wwDataBinder ID="wwDataBinder" runat="server" OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl"
        OnAfterBindControl="wwDataBinder_AfterBindControl" OnValidateControl="wwDataBinder_ValidateControl">
        <DataBindingItems>
            <tis:wwDataBindingItem ID="wbi1a" runat="server" ControlId="txtLicenseEmail" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="LicenseEmail" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_LicenseEmail_Label %>"
                IsRequired="False" />
            <tis:wwDataBindingItem ID="wbi1b" runat="server" ControlId="txtLicenseKey" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="LicenseKey" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_ProductKey_Label %>"
                IsRequired="False" />
            <tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="EnableCache" ControlId="chkEnableCache" BindingProperty="Checked"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_EnableCache_Label %>" />
            <tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="AllowGalleryAdminToManageUsersAndRoles" ControlId="chkAllowGalleryAdminToManageUsersAndRoles"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_AllowGalleryAdminToManageUsersAndRoles_Label %>" />
            <tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="AllowGalleryAdminToViewAllUsersAndRoles" ControlId="chkAllowGalleryAdminToViewAllUsersAndRoles"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_AllowGalleryAdminToViewAllUsersAndRoles_Label %>" />
            <tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="MaxNumberErrorItems" ControlId="txtMaxErrorItems" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_MaxNumberErrorItems_Label %>" />
            <tis:wwDataBindingItem ID="wbi5" runat="server" ControlId="chkAutoTrimLog">
            </tis:wwDataBindingItem>
            <tis:wwDataBindingItem ID="wbi5b" runat="server" BindingProperty="SelectedValue" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="Skin" ControlId="ddlSkin" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_Skin_Label %>" />
            <tis:wwDataBindingItem ID="wbi6" runat="server" ControlId="txtJQueryScriptPath" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="JQueryScriptPath" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_JQuery_Label %>"
                IsRequired="False" />
            <tis:wwDataBindingItem ID="wbi6b" runat="server" ControlId="txtJQueryMigrateScriptPath" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="JQueryMigrateScriptPath" UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_JQuery_Migrate_Label %>"
                IsRequired="False" />
            <tis:wwDataBindingItem ID="wbi7" runat="server" ControlId="txtJQueryUiScriptPath"
                BindingSource="AppSettingsUpdateable" BindingSourceMember="JQueryUiScriptPath"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_JQueryUI_Label %>"
                IsRequired="False" />
            <tis:wwDataBindingItem ID="wbi7g" runat="server" ControlId="txtImageMagickPath"
                BindingSource="AppSettingsUpdateable" BindingSourceMember="ImageMagickPath"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Site_Settings_ImageMagickPath_Label %>"
                IsRequired="False" />
            <tis:wwDataBindingItem ID="wbi7a" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="EmailFromName" ControlId="txtEmailFromName" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_EmailName_Label %>" />
            <tis:wwDataBindingItem ID="wbi7b" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="EmailFromAddress" ControlId="txtEmailFromAddress" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_EmailAddress_Label %>" />
            <tis:wwDataBindingItem ID="wbi7c" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="SmtpServer" ControlId="txtSmtpServer" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_SmtpServer_Label %>" />
            <tis:wwDataBindingItem ID="wbi7d" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="SmtpServerPort" ControlId="txtSmtpPort" UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_SmtpPort_Label %>" />
            <tis:wwDataBindingItem ID="wbi7e" runat="server" BindingSource="AppSettingsUpdateable"
                BindingSourceMember="SendEmailUsingSsl" ControlId="chkUseSsl" BindingProperty="Checked"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Gallery_Settings_UseSsl_Label %>" />
            <tis:wwDataBindingItem ID="wbi7f" runat="server" ControlId="btnEmailTest">
            </tis:wwDataBindingItem>

            <tis:wwDataBindingItem ID="wbi8" runat="server" ControlId="lblTrustLevel" BindingSource="AppSettings"
                BindingSourceMember="AppTrustLevel" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi9" runat="server" ControlId="lblNetVersion" BindingSource="AppSettings"
                BindingSourceMember="DotNetFrameworkVersion" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi10" runat="server" ControlId="lblIisAppPool" BindingSource="AppSettings"
                BindingSourceMember="IisAppPoolIdentity" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi13" runat="server" BindingSource="ConnectionStringSettings"
                BindingSourceMember="ProviderName" ControlId="lblGalleryDataProvider" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi16" runat="server" BindingSource="MembershipGsp" BindingSourceMember="Name"
                ControlId="lblMembershipDataProvider" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi17" runat="server" BindingSource="MembershipGsp" BindingSourceMember="Description"
                ControlId="lblMembershipDataProviderDesc" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi18" runat="server" BindingSource="MembershipGsp" BindingSourceMember="ApplicationName"
                ControlId="lblMembershipAppName" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi19" runat="server" BindingSource="RoleGsp" BindingSourceMember="Name"
                ControlId="lblRoleDataProvider" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi20" runat="server" BindingSource="RoleGsp" BindingSourceMember="Description"
                ControlId="lblRoleDataProviderDesc" BindingMode="OneWay" />
            <tis:wwDataBindingItem ID="wbi21" runat="server" BindingSource="RoleGsp" BindingSourceMember="ApplicationName"
                ControlId="lblRoleAppName" BindingMode="OneWay" />
        </DataBindingItems>
    </tis:wwDataBinder>
    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
    <script>
        (function ($) {
            var $scope;
            var clientId = '<%= cid %>';
            var imageMagickPathResolved = '<%= AppSettings.ImageMagickPathResolved.JsEncode() %>';
            var licenseKeyLblIconCssClass = '<%= LicenseKeyIconCssClass %>';

            var updateUi = function () {
                var txtMaxErrItems = $('#<%= txtMaxErrorItems.ClientID %>');
                var chkAutoTrimLog = $('#<%= chkAutoTrimLog.ClientID %>');

                txtMaxErrItems.prop('disabled', (!chkAutoTrimLog.prop('checked') || chkAutoTrimLog.prop('disabled')));
            };

            var configControls = function () {
                $('#<%= chkAutoTrimLog.ClientID %>').click(updateUi);

                $('.gs_a_ss_lk_lbl_ico', $scope).addClass(licenseKeyLblIconCssClass);

                if (imageMagickPathResolved.length > 0) {
                    $('.gs_a_ss_im_vry', $('#<%= cid %>')).addClass('fa-check-circle gsp_msgfriendly').attr('title', 'ImageMagick found at ' + imageMagickPathResolved);
                } else {
                    $('.gs_a_ss_im_vry', $('#<%= cid %>')).addClass('fa-exclamation-circle gsp_msgwarning').attr('title', 'MISSING: convert.exe not found at this location. Refer to the Admin Guide for information about installing ImageMagick.');
                }
            };

            var configTooltips = function () {
                $('#<%= lblLicenseEmail.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_licenseEmail_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_licenseEmail_Bdy.JsEncode() %>'
                });

                $('#<%= lblProductKey.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_productKey_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_productKey_Bdy.JsEncode() %>'
                });

                $('#<%= chkEnableCache.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_enableCache_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_enableCache_Bdy.JsEncode() %>'
                });

                $('#<%= chkAllowGalleryAdminToManageUsersAndRoles.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_allowGalleryAdminToManageUsersAndRole_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_allowGalleryAdminToManageUsersAndRole_Bdy.JsEncode() %>'
                });

                $('#<%= chkAllowGalleryAdminToViewAllUsersAndRoles.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_allowGalleryAdminViewAllUsersAndRoles_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_allowGalleryAdminViewAllUsersAndRoles_Bdy.JsEncode() %>'
                });

                $('#<%= chkAutoTrimLog.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_maxNumberErrorItems_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_maxNumberErrorItems_Bdy.JsEncode() %>'
                });

                $('#<%= lblSkin.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_skin_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_skin_Bdy.JsEncode() %>'
                });

                $('#<%= lblJQuery.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_jQueryScriptPath_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_jQueryScriptPath_Bdy.JsEncode() %>'
                });

                $('#<%= lblJQueryMigrate.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_jQueryMigrateScriptPath_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_jQueryMigrateScriptPath_Bdy.JsEncode() %>'
                });

                $('#<%= lblJQueryUI.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_jQueryUIScriptPath_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_jQueryUIScriptPath_Bdy.JsEncode() %>'
                });

                $('#<%= lblImageMagick.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Site_Settings_ImageMagick_Info_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Site_Settings_ImageMagick_Info_Bdy.JsEncode() %>'
                });

                $('#<%= lblFFmpegLbl.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Site_Settings_FFmpeg_Info_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Site_Settings_FFmpeg_Info_Bdy.JsEncode() %>'
                });

                $('#<%= lblGalleryDataLabel.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_GalleryData_Popup_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_GalleryData_Popup_Bdy.JsEncode() %>'
                });

                $('#<%= lblMembershipLabel.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_Membership_Popup_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_Membership_Popup_Bdy.JsEncode() %>'
                });

                $('#<%= lblRoleLabel.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_Role_Popup_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_Role_Popup_Bdy.JsEncode() %>'
                });

                $('#<%= lbCompactDb.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_Compact_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Site_Settings_DataProvider_Compact_Bdy.JsEncode() %>'
                });

                $('#<%= lblEmailFromName.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_emailFromName_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_emailFromName_Bdy.JsEncode() %>'
                });

                $('#<%= lblEmailFromAddress.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_emailFromAddress_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_emailFromAddress_Bdy.JsEncode() %>'
                });

                $('#<%= lblSmtpServer.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_smtpServer_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_smtpServer_Bdy.JsEncode() %>'
                });

                $('#<%= lblSmtpPort.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_smtpPort_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_smtpPort_Bdy.JsEncode() %>'
                });

                $('#<%= chkUseSsl.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_sendEmailUsingSsl_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_sendEmailUsingSsl_Bdy.JsEncode() %>'
                });
            };

            $(document).ready(function () {
                $scope = $('#' + clientId);

                configControls();
                updateUi();
                configTooltips();
            });

        })(jQuery);
    </script>
</asp:PlaceHolder>

