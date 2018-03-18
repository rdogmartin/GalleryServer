<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="myaccount.ascx.cs" Inherits="GalleryServer.Web.Pages.myaccount" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gs_myaccount gsp_addpadding1" runat="server">
    <p class="gsp_h1">
        <span class="fa fa-user fa-2x gs_login_icon"></span>
        <asp:Literal ID="lit1" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_Hdr %>" />
    </p>
    <asp:Panel ID="pnlAccountInfo" runat="server">
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_Info_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <table class="gsp_standardTable">
                    <tr>
                        <td class="gsp_bold gsp_fll">
                            <asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_UserName_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblUserName" runat="server" CssClass="gsp_fll"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold">
                            <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_Password_Label %>" />
                        </td>
                        <td>
                            <asp:HyperLink ID="hlChangePwd" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_Change_Pwd_Hyperlink_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold">
                            <asp:Label ID="lblProfileSettings" runat="server" AssociatedControlID="txtEmail" Text="Profile settings:" />
                        </td>
                        <td>
                            <a class="gs_profile_clear_btn" href="javascript:void(0)">Restore default settings</a>&nbsp;<span style="display: none;" class="fa fa-spinner fa-pulse gs_profile_clear_btn_spnr"></span>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold">
                            <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" Text="<%$ Resources:GalleryServer, MyAccount_Email_Address_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtEmail" runat="server" CssClass="gsp_textbox" />
                            <asp:RegularExpressionValidator ID="revEmail" runat="server" Display="Dynamic" CssClass="gsp_msgwarning"
                                ForeColor="" ControlToValidate="txtEmail" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                                SetFocusOnError="true" ErrorMessage="<%$ Resources:GalleryServer, Site_Invalid_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold gsp_aligntop">
                            <asp:Label ID="lblComment" runat="server" AssociatedControlID="txtComment" Text="<%$ Resources:GalleryServer, MyAccount_Comment_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" CssClass="gsp_textarea1"></asp:TextBox>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold">
                            <asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_LastActivityDate_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblLastActivityDate" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold">
                            <asp:Literal ID="Literal3" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_LastLogonDate_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblLastLogOnDate" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold">
                            <asp:Literal ID="Literal4" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_LastPwdChangeDate_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblLastPasswordChangedDate" runat="server"></asp:Label>
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_bold">
                            <asp:Literal ID="Literal5" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_AccountCreatedDate_Label %>" />
                        </td>
                        <td>
                            <asp:Label ID="lblCreationDate" runat="server"></asp:Label>
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlUserAlbum" runat="server">
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_UserAlbum_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p>
                    <asp:CheckBox ID="chkEnableUserAlbum" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_EnableUserAlbum_Label %>" />
                </p>
                <p class='gsp_ma_ua_wrng gsp_msgwarning gsp_addleftpadding4 gsp_invisible'>
                    <%= Resources.GalleryServer.MyAccount_EnableUserAlbum_Warning %>
                </p>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlDeleteAccount" runat="server" Visible="False">
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_Delete_Account_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p>
                    <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_Delete_Account_Overview %>" />
                </p>
                <p class="gsp_msgwarning" style="max-width: 550px;">
                    <asp:Literal ID="litDeleteAccountWarning" runat="server" />
                </p>
                <p>
                    <img src="<%= Utils.SkinPath %>/images/error-s.png" alt="" />
                    <asp:LinkButton ID="lbDeleteAccount" runat="server" Text="<%$ Resources:GalleryServer, MyAccount_Delete_Account_Command_Text %>"
                        ToolTip="<%$ Resources:GalleryServer, MyAccount_Delete_Account_Command_Text %>"
                        OnClick="lbDeleteAccount_Click" />
                </p>
            </div>
        </div>
    </asp:Panel>
</div>
<div class="gsp_rightBottom">
    <p class="gsp_minimargin">
        <asp:Button ID="btnSave" runat="server" OnClick="btnSave_Click" Text="<%$ Resources:GalleryServer, Default_Task_Ok_Button_Text %>" />
        <asp:Button ID="btnCancel" runat="server" OnClick="btnCancel_Click" CausesValidation="false"
            Text="<%$ Resources:GalleryServer, Default_Task_Cancel_Button_Text %>" ToolTip="<%$ Resources:GalleryServer, Default_Task_Cancel_Button_Tooltip %>" />&nbsp;
    </p>
</div>
<tis:wwDataBinder ID="wwDataBinder" runat="server" OnAfterBindControl="wwDataBinder_AfterBindControl">
    <DataBindingItems>
        <tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="CurrentUser" BindingSourceMember="UserName"
            ControlId="lblUserName" BindingMode="OneWay" />
        <tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="CurrentUser" BindingSourceMember="Email"
            ControlId="txtEmail" />
        <tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="CurrentUser" BindingSourceMember="Comment"
            ControlId="txtComment" />
        <tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="CurrentUser" BindingSourceMember="LastActivityDate"
            ControlId="lblLastActivityDate" BindingMode="OneWay" DisplayFormat="{0:MMM dd, yyyy h:mm:ss tt (UTCzzz)}" />
        <tis:wwDataBindingItem ID="wbi5" runat="server" BindingSource="CurrentUser" BindingSourceMember="LastLoginDate"
            ControlId="lblLastLogOnDate" BindingMode="OneWay" DisplayFormat="{0:MMM dd, yyyy h:mm:ss tt (UTCzzz)}" />
        <tis:wwDataBindingItem ID="wbi6" runat="server" BindingSource="CurrentUser" BindingSourceMember="LastPasswordChangedDate"
            ControlId="lblLastPasswordChangedDate" BindingMode="OneWay" DisplayFormat="{0:MMM dd, yyyy h:mm:ss tt (UTCzzz)}" />
        <tis:wwDataBindingItem ID="wbi7" runat="server" BindingSource="CurrentUser" BindingSourceMember="CreationDate"
            ControlId="lblCreationDate" BindingMode="OneWay" DisplayFormat="{0:MMM dd, yyyy h:mm:ss tt (UTCzzz)}" />
        <tis:wwDataBindingItem ID="wbi8" runat="server" BindingSource="CurrentGalleryProfile" BindingSourceMember="EnableUserAlbum"
            ControlId="chkEnableUserAlbum" BindingProperty="Checked" />
    </DataBindingItems>
</tis:wwDataBinder>
<asp:PlaceHolder runat="server">
    <script>
        (function ($) {
            var bindEventHandlers = function () {
                $('#<%= chkEnableUserAlbum.ClientID %>').click(function () {
                    // Toggle warning when user unchecks 'Enable user album'
                    $('.gsp_ma_ua_wrng', $('#<%= cid %>')).toggleClass('gsp_invisible', $(this).prop('checked'));
                });

                $('.gs_profile_clear_btn', $('#<%= cid %>')).click(function (e) {
                    
                    $('.gs_profile_clear_btn_spnr', $('#<%= cid %>')).show();

                    // Delete all cookies starting with "gsp_". Most importantly this resets the left and right pane widths.
                    var cookies = Gs.Vars.Cookies.get();
                    for (var cookie in cookies) {
                        if (cookies.hasOwnProperty(cookie) && cookie.startsWith("gsp_")) {
                            Gs.Vars.Cookies.remove(cookie);
                        }
                    }

                    Gs.DataService.clearUserProfile('admin', function () {
                        // DONE: Hide spinner
                        $('.gs_profile_clear_btn_spnr', $('#<%= cid %>')).hide();
                    }, function (actionResult) {
                        // SUCCESS
                        switch (actionResult.Status) {
                            case 'Success':
                                Gs.Msg.show(actionResult.Title, actionResult.Message, { msgType: 'success' });
                                break;
                            default:
                                Gs.Msg.show(actionResult.Title, actionResult.Message, { msgType: 'error', autoCloseDelay: 0 });
                                break;
                        }
                    }, function(jqXHR) { // AJAX ERROR
                        Gs.Msg.show('Cannot Reset Profile Settings', Gs.Utils.parseJqXhrMsg(jqXHR), { msgType: 'error', autoCloseDelay: 0 });
                    });
                });
            };

          var configTooltips = function () {
              $('#<%= lblEmail.ClientID %>').gsTooltip({
                title: '<%= Resources.GalleryServer.MyAccount_Email_Address_Hdr.JsEncode() %>',
              content: '<%= Resources.GalleryServer.MyAccount_Email_Address_Bdy.JsEncode() %>'
          });

            $('#<%= chkEnableUserAlbum.ClientID %>').gsTooltip({
                title: '<%= Resources.GalleryServer.MyAccount_EnableUserAlbum_Hdr.JsEncode() %>',
                content: '<%= Resources.GalleryServer.MyAccount_EnableUserAlbum_Bdy.JsEncode() %>'
            });

            $('#<%= lblProfileSettings.ClientID %>').gsTooltip({
                title: '<%= Resources.GalleryServer.MyAccount_ResetProfile_Hdr.JsEncode() %>',
                content: '<%= Resources.GalleryServer.MyAccount_ResetProfile_Bdy.JsEncode() %>'
            });
        };

          $(document).ready(function () {
              bindEventHandlers();
              configTooltips();
          });


      })(jQuery);
    </script>
</asp:PlaceHolder>
