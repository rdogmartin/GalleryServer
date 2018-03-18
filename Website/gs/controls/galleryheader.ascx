<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="galleryheader.ascx.cs"
  Inherits="GalleryServer.Web.Controls.galleryheader" %>
<%@ Import Namespace="GalleryServer.Web" %>
<header id='<%= GalleryPage.HeaderClientId %>' class='gsp_header'></header>
<asp:LoginView ID="lv" runat="server" EnableViewState="false">
  <AnonymousTemplate>
    <div id="<%= cid %>_loginDlg" class="gsp_dlg gsp_login_dlg">
      <span class="fa fa-user fa-3x gs_login_icon"></span>
      <asp:Login ID="Login1" runat="server" CssClass="gsp_login" RememberMeSet="false"
        DisplayRememberMe="True" Width="100%" Orientation="Vertical" LabelStyle-CssClass="gsp_login_label"
        TextBoxStyle-CssClass="gsp_login_textbox" TextLayout="TextOnLeft" TitleText=""
        LoginButtonType="Button" LoginButtonStyle-CssClass="gsp_login_button" EnableViewState="false"
        OnLoginError="Login1_LoginError" OnLoggedIn="Login1_LoggedIn" FailureText="<%$ Resources:GalleryServer, Login_Failure_Text %>"
        LoginButtonText="<%$ Resources:GalleryServer, Login_Button_Text %>" PasswordLabelText="<%$ Resources:GalleryServer, Login_Password_Label_Text %>"
        PasswordRequiredErrorMessage="" RememberMeText="<%$ Resources:GalleryServer, Login_Remember_Me_Text %>"
        UserNameLabelText="<%$ Resources:GalleryServer, Login_UserName_Label_Text %>"
        UserNameRequiredErrorMessage="" CheckBoxStyle-CssClass="gsp_rememberme" HyperLinkStyle-CssClass="gsp_login_hyperlinks"
        PasswordRecoveryText="<%$ Resources:GalleryServer, Login_Password_Recovery_Text %>">
      </asp:Login>
    </div>
  </AnonymousTemplate>
</asp:LoginView>

