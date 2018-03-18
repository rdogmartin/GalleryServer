<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="taskfooter.ascx.cs"
	Inherits="GalleryServer.Web.Controls.Task.taskfooter" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_rightBottom">
	<p class="gsp_minimargin">
		<span class="gsp_spinner_msg"></span>&nbsp;<img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" class="gsp_spinner" alt=""/>&nbsp;<asp:Button ID="btnOkBottom" runat="server" Text="<%$ Resources:GalleryServer, Default_Task_Ok_Button_Text %>"
			CssClass="gsp_btnOkBottom" />
		<asp:Button ID="btnCancelBottom" runat="server" OnClick="btnCancel_Click" CausesValidation="false"
			Text="<%$ Resources:GalleryServer, Default_Task_Cancel_Button_Text %>" ToolTip="<%$ Resources:GalleryServer, Default_Task_Cancel_Button_Tooltip %>"
			CssClass="gsp_btnCancelBottom" />&nbsp;</p>
</div>
