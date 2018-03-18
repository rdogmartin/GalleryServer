<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="login.ascx.cs" Inherits="GalleryServer.Web.Pages.login" %>
<%@ Import Namespace="GalleryServer.Web" %>
<asp:Panel ID="pnlLoginContainer" runat="server" CssClass="gsp_loginContainerPage gsp_rounded10"
	DefaultButton="Login1$LoginButton">
	<p class="gsp_loginTitle gsp_roundedtop6">
		<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Login_Title_Text %>" /></p>
    <span class="fa fa-user fa-4x gs_login_icon"></span>
	<asp:Login ID="Login1" runat="server" CssClass="gsp_login gsp_addbottommargin2" BorderPadding="0"
		RememberMeSet="false" DisplayRememberMe="True" Width="100%" Orientation="Vertical"
		LabelStyle-CssClass="gsp_loginlabel" TextBoxStyle-CssClass="gsp_logintextbox" TextLayout="TextOnLeft"
		TitleText="" LoginButtonType="Button" LoginButtonStyle-CssClass="gsp_loginbutton"
		EnableViewState="true" OnLoggedIn="Login1_LoggedIn" OnLoginError="Login1_LoginError" FailureText="<%$ Resources:GalleryServer, Login_Failure_Text %>"
		LoginButtonText="<%$ Resources:GalleryServer, Login_Button_Text %>" PasswordLabelText="<%$ Resources:GalleryServer, Login_Password_Label_Text %>"
		PasswordRequiredErrorMessage="<%$ Resources:GalleryServer, Login_Password_Required_Error_Msg %>"
		RememberMeText="<%$ Resources:GalleryServer, Login_Remember_Me_Text %>" UserNameLabelText="<%$ Resources:GalleryServer, Login_UserName_Label_Text %>"
		UserNameRequiredErrorMessage="<%$ Resources:GalleryServer, Login_UserName_Required_Error_Msg %>"
		CheckBoxStyle-CssClass="gsp_rememberme" HyperLinkStyle-CssClass="gsp_loginhyperlinks"
		PasswordRecoveryText="<%$ Resources:GalleryServer, Login_Password_Recovery_Text %>">
		<FailureTextStyle CssClass="gsp_msgwarning gsp_addtoppadding2" ForeColor="" />
		<ValidatorTextStyle CssClass="gsp_msgwarning" ForeColor="" />
	</asp:Login>
</asp:Panel>
