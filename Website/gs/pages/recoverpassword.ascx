<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="recoverpassword.ascx.cs" Inherits="GalleryServer.Web.Pages.recoverpassword" %>
<asp:Panel ID="pnlPwdRecoverContainer" runat="server" CssClass="pwdrecover gsp_rounded10" DefaultButton="btnRetrievePassword">
	<p class="pwdrecoverTitle gsp_roundedtop6">
		<asp:Literal ID="lit1" runat="server" Text="<%$ Resources:GalleryServer, Anon_Pwd_Recovery_Header %>" /></p>
	<p>
		<asp:Literal ID="lit2" runat="server" Text="<%$ Resources:GalleryServer, Anon_Pwd_Recovery_Instructions %>" />
	</p>
	<p>
		<asp:Literal ID="lit3" runat="server" Text="<%$ Resources:GalleryServer, Anon_Pwd_Recovery_UserName_Label %>" />
		<asp:TextBox ID="txtUserName" runat="server" MaxLength="256" />
		<asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtUserName"
			Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServer, Anon_Pwd_Recovery_Username_Required_Text %>" CssClass="gsp_msgwarning">
		</asp:RequiredFieldValidator>
	</p>
	<p class="pwdRecoverRetrieve">
		<asp:Button ID="btnRetrievePassword" runat="server" Text="<%$ Resources:GalleryServer, Anon_Pwd_Recovery_Retrieve_Pwd_Button_Text %>" OnClick="btnRetrievePassword_Click" /></p>
</asp:Panel>
