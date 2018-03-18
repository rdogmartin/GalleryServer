<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="backuprestore.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.backuprestore" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content">
    <p class="gsp_a_ap_to">
        <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
            EnableViewState="false" />&nbsp;<asp:Literal ID="Literal1" runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
    </p>
    <asp:PlaceHolder ID="phAdminHeader" runat="server" />
    <div class="gsp_addleftpadding5">
        <asp:PlaceHolder runat="server">
            <div id="<%= cid %>_bakRestoreTabContainer" class="gsp_addtoppadding5 gsp_tabContainer">
                <ul>
                    <li><a href="#<%= cid %>_bakRestoreTabBackup">
                        <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Backup_Tab_Title %>" /></a></li>
                    <li><a href="#<%= cid %>_bakRestoreTabRestore">
                        <asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Title %>" /></a></li>
                </ul>
                <div id="<%= cid %>_bakRestoreTabBackup" class="ui-corner-all">
                    <p class="gsp_h3">
                        <asp:Literal ID="l1b" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Backup_Tab_Hdr %>" />
                    </p>
                    <p class="gsp_addtopmargin5">
                        <asp:Label ID="lblBackupDtl" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Backup_Tab_Dtl %>" />
                    </p>
                    <div class="gsp_msgBoxInfo">
                        <asp:Literal ID="l16" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Backup_Tab_Note %>" />
                    </div>
                    <p class="gsp_addtopmargin5">
                        <asp:CheckBox ID="chkExportMembership" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Backup_Tab_Export_Users_Checkbox_Text %>"
                            Checked="true" />
                    </p>
                    <p>
                        <asp:CheckBox ID="chkExportGalleryData" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Backup_Tab_Export_Gallery_Data_Checkbox_Text %>"
                            Checked="true" />
                    </p>
                    <p>
                        <asp:Button ID="btnExportData" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Backup_Tab_Export_Btn_Text %>"
                            OnClick="btnExportData_Click" />
                    </p>
                </div>
                <div id="<%= cid %>_bakRestoreTabRestore" class="ui-corner-all">
                    <p class="gsp_h3">
                        <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Hdr %>" />
                    </p>
                    <p>
                        <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Dtl %>" />
                    </p>
                    <div class="gsp_msgBoxInfo">
                        <asp:Literal ID="l15" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Note %>" />
                    </div>
                    <p>
                        <asp:FileUpload ID="fuRestoreFile" runat="server" size="45" CssClass="gsp_textbox" ToolTip="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Upload_File_Browse_Button_Tooltip %>" />&nbsp;
                        <asp:Button ID="btnUpload" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Upload_File_Button_Text %>" ToolTip="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Upload_File_Button_Tooltip %>" OnClick="btnUpload_Click" />
                    </p>
                    <asp:PlaceHolder ID="phUpload" runat="server" />
                    <asp:Panel ID="pnlRestoreFileInfo" runat="server" Visible="true">
                        <table id="restoreFileContainer" class="gsp_rounded10" cellpadding="0" cellspacing="0">
                            <tr class="gsp_tableSummaryRow gsp_roundedtop6">
                                <td colspan="3">
                                    <p>
                                        <asp:LinkButton ID="lbRemoveRestoreFile" runat="server" Text="Remove" CssClass="gsp_fs"
                                            Visible="false" OnClick="lbRemoveRestoreFile_Click" />
                                        <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Filename_Hdr %>" />
                                        <asp:Label ID="lblRestoreFilename" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Empty_Filename %>"
                                            CssClass="gsp_msgwarning" />
                                    </p>
                                    <p>
                                        <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Schema_Hdr %>" />&nbsp;<asp:Label
                                            ID="lblSchemaVersion" runat="server" CssClass="gsp_msginfo" />
                                    </p>
                                    <p>
                                        <asp:Image ID="imgValidationResult" runat="server" Width="16" Height="16" Style="vertical-align: middle;"
                                            Visible="false" />&nbsp;<asp:Label ID="lblValidationResult" runat="server" />
                                    </p>
                                </td>
                            </tr>
                            <tr class="gsp_tableHeaderRow">
                                <td class="gsp_topBorder gsp_bottomBorder">&nbsp;
                                </td>
                                <td class="gsp_topBorder gsp_bottomBorder">
                                    <asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Table_Column_Hdr %>" />
                                </td>
                                <td class="gsp_numRecords gsp_topBorder gsp_bottomBorder">
                                    <asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_NumRecords_Column_Hdr %>" />
                                </td>
                            </tr>
                            <tr>
                                <td rowspan="6" style="width: 150px;" class="gsp_bottomBorder">
                                    <asp:CheckBox ID="chkImportMembership" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Import_Users_Checkbox_Text %>"
                                        Checked="True" />
                                </td>
                                <td style="width: 250px;">Applications
                                </td>
                                <td style="width: 100px;" class="gsp_numRecords">
                                    <asp:Label ID="lblNumApps" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Profiles
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumProfiles" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Roles
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumRoles" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Memberships
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumMembers" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Users
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumUsers" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr class="gsp_bottomBorder">
                                <td class="gsp_bottomBorder">UsersInRoles
                                </td>
                                <td class="gsp_numRecords gsp_bottomBorder">
                                    <asp:Label ID="lblNumUsersInRoles" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td rowspan="17">
                                    <asp:CheckBox ID="chkImportGalleryData" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Import_Gallery_Data_Checkbox_Text %>"
                                        Checked="True" />
                                </td>
                                <td>Gallery
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumGalleries" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Album
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumAlbums" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>MediaObject
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumMediaObjects" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Metadata
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumMetadata" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Tag
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumTag" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>MetadataTag
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumMetadataTag" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>RoleAlbum
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumRoleAlbums" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>Role
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumGalleryRoles" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>AppSetting
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumAppSettings" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>GalleryControlSetting
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumGalleryControlSettings" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>GallerySetting
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumGallerySettings" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>MediaTemplate
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumBrowserTemplates" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>MimeType
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumMimeTypes" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>MimeTypeGallery
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumMimeTypeGalleries" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>UiTemplate
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumUiTemplates" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>UiTemplateAlbum
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumUiTemplateAlbums" runat="server"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td>UserGalleryProfile
                                </td>
                                <td class="gsp_numRecords">
                                    <asp:Label ID="lblNumUserGalleryProfiles" runat="server"></asp:Label>
                                </td>
                            </tr>
                        </table>
                        <div>
                            <div style="height: 80px; float: left; margin-top: 10px; margin-right: 5px;">
                                <asp:Button ID="btnRestore" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Restore_Btn_Text %>"
                                    Enabled="False" OnClick="btnRestore_Click" />
                            </div>
                            <p style="width: 400px;">
                                <span class="gsp_fs gsp_msgwarning"><span class="gsp_bold">
                                    <asp:Literal ID="l9" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Restore_Warning_Hdr %>" /></span>&nbsp;<asp:Literal
                                        ID="l10" runat="server" Text="<%$ Resources:GalleryServer, Admin_Backup_Restore_Restore_Tab_Restore_Warning_Dtl %>" /></span>
                            </p>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </asp:PlaceHolder>
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

                $("#<%= cid %>_bakRestoreTabContainer").tabs({
                    active: (Gs.Vars.Cookies.get('<%= SelectedTabCookieName %>') || 0),
                    activate: function (e, ui) { Gs.Vars.Cookies.set('<%= SelectedTabCookieName %>', ui.newTab.index()); }
                })
                .show();
            };

            var configTooltips = function () {
                $('#<%= lblBackupDtl.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_BackupRestore_Backup_Overview_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_BackupRestore_Backup_Overview_Bdy.JsEncode() %>'
                });

                $('#<%= chkExportMembership.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_BackupRestore_ExportUserAccounts_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_BackupRestore_ExportUserAccounts_Bdy.JsEncode() %>'
                });

                $('#<%= chkExportGalleryData.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_BackupRestore_ExportGalleryData_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_BackupRestore_ExportGalleryData_Bdy.JsEncode() %>'
                });
            };

        })(jQuery);
    </script>
</asp:PlaceHolder>
