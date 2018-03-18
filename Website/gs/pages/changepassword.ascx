<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="changepassword.ascx.cs" Inherits="GalleryServer.Web.Pages.changepassword" %>
<div class="cpwdcontainer gsp_rounded10">
  <asp:ChangePassword ID="cp1" runat="server" BorderPadding="0" CssClass="cpwd"
    CancelButtonText="<%$ Resources:GalleryServer, Change_Pwd_CancelButtonText %>"
    ChangePasswordButtonText="<%$ Resources:GalleryServer, Change_Pwd_ChangePasswordButtonText %>"
    ChangePasswordFailureText="<%$ Resources:GalleryServer, Change_Pwd_ChangePasswordFailureText %>"
    ChangePasswordTitleText="<%$ Resources:GalleryServer, Change_Pwd_ChangePasswordTitleText %>"
    ConfirmNewPasswordLabelText="<%$ Resources:GalleryServer, Change_Pwd_ConfirmNewPasswordLabelText %>"
    ConfirmPasswordCompareErrorMessage="<%$ Resources:GalleryServer, Change_Pwd_ConfirmPasswordCompareErrorMessage %>"
    ConfirmPasswordRequiredErrorMessage="<%$ Resources:GalleryServer, Change_Pwd_ConfirmPasswordRequiredErrorMessage %>"
    ContinueButtonText="<%$ Resources:GalleryServer, Change_Pwd_ContinueButtonText %>"
    NewPasswordLabelText="<%$ Resources:GalleryServer, Change_Pwd_NewPasswordLabelText %>"
    NewPasswordRequiredErrorMessage="<%$ Resources:GalleryServer, Change_Pwd_NewPasswordRequiredErrorMessage %>"
    PasswordLabelText="<%$ Resources:GalleryServer, Change_Pwd_PasswordLabelText %>"
    PasswordRequiredErrorMessage="<%$ Resources:GalleryServer, Change_Pwd_PasswordRequiredErrorMessage %>"
    SuccessText="<%$ Resources:GalleryServer, Change_Pwd_SuccessText %>" SuccessTitleText="<%$ Resources:GalleryServer, Change_Pwd_SuccessTitleText %>"
    UserNameLabelText="<%$ Resources:GalleryServer, Change_Pwd_UserNameLabelText %>"
    UserNameRequiredErrorMessage="<%$ Resources:GalleryServer, Change_Pwd_UserNameRequiredErrorMessage %>"
    OnSendingMail="cp1_SendingMail" LabelStyle-CssClass="gsp_cpwdlabel" CancelButtonStyle-CssClass="gsp_cpwdcancelbtn"
    ChangePasswordButtonStyle-CssClass="gsp_cpwdbutton" SuccessTextStyle-CssClass="gsp_cpwdsuccess"
    ContinueButtonStyle-CssClass="gsp_cpwdcontinue" TextBoxStyle-CssClass="gsp_cpwdtxtbox" ValidatorTextStyle-ForeColor=""
    ValidatorTextStyle-CssClass="gsp_msgwarning" OnCancelButtonClick="cp1_CancelButtonClick">
    <TitleTextStyle CssClass="cpwdTitle gsp_roundedtop6" />
    <FailureTextStyle CssClass="gsp_msgwarning gsp_addbottompadding5" ForeColor="" />
  </asp:ChangePassword>
</div>
