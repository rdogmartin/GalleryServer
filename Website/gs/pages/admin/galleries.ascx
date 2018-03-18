<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="galleries.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.galleries" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content">
  <p class="gsp_a_ap_to">
    <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
      EnableViewState="false" />&nbsp;<asp:Literal ID="Literal2" runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
  </p>
  <asp:PlaceHolder ID="phAdminHeader" runat="server" />
  <div class="gsp_addleftpadding5">
    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Label ID="lblCurrentGalleryHdr" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_CurrentGallery_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p class="gsp_addtopmargin5">
          <asp:Label ID="lblCurrentGallery" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_CurrentGallery_Label %>" />&nbsp;<asp:DropDownList
            ID="ddlCurrentGallery" runat="server" />
          &nbsp;
				  <asp:LinkButton ID="lbChangeGallery" runat="server" OnClick="lbChangeGallery_Click"
            Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Change_Gallery_Label %>"
            ToolTip="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Change_Gallery_Tooltip %>"></asp:LinkButton>
        </p>
      </div>
    </div>
    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Label ID="lblManageGalleriesHdr" runat="server" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Galleries_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <asp:GridView ID="gvGalleries" runat="server" DataSourceID="odsGalleries" EnableViewState="true"
          AllowPaging="False" AutoGenerateColumns="False" ShowFooter="True" DataKeyNames="GalleryId"
          OnRowCommand="gvGalleries_RowCommand" OnRowDataBound="gvGalleries_RowDataBound"
          CssClass="gsp_adm_g_tbl">
          <Columns>
            <asp:TemplateField ShowHeader="False">
              <EditItemTemplate>
                <div class="gsp_g_edit_cell">
                  <asp:LinkButton ID="LinkButton1" runat="server" CausesValidation="True" CommandName="Update"
                    Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Update_Gallery_Text %>"
                    ToolTip="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Update_Gallery_Tooltip %>"></asp:LinkButton>
                  <asp:LinkButton ID="LinkButton2" runat="server" CausesValidation="False" CommandName="Cancel"
                    Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Cancel_Edit_Gallery_Text %>"
                    ToolTip="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Cancel_Edit_Gallery_Tooltip %>"></asp:LinkButton>
                </div>
              </EditItemTemplate>
              <FooterTemplate>
                <div class="gsp_g_edit_cell">
                  <asp:LinkButton ID="lbInsert" runat="server" CommandName="Insert" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Create_Gallery_Link_Text %>"
                    ToolTip="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Create_Gallery_Link_ToolTip %>" />
                </div>
              </FooterTemplate>
              <ItemTemplate>
                <div class="gsp_g_edit_cell">
                  <asp:HyperLink ID="hlViewGallery" runat="server" Visible="False" Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_View_Gallery_Link_Text %>"
                    ToolTip="<%$ Resources:GalleryServer, Admin_Gallery_Settings_View_Gallery_Link_Tooltip %>" />
                  <asp:LinkButton ID="lbEditGallery" runat="server" CausesValidation="False" CommandName="Edit"
                    Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Edit_Gallery_Link_Text %>"
                    ToolTip="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Edit_Gallery_Link_ToolTip %>"></asp:LinkButton>
                  <asp:LinkButton ID="lbDeleteGallery" runat="server" CausesValidation="False" CommandName="Delete"
                    Text="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Delete_Gallery_Link_Text %>"
                    ToolTip="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Delete_Gallery_Link_ToolTip %>"></asp:LinkButton>
                </div>
              </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="GalleryId" HeaderText="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Gallery_ID_Hdr_Text %>"
              InsertVisible="False" ReadOnly="True" SortExpression="GalleryId">
              <ItemStyle HorizontalAlign="Center" />
            </asp:BoundField>
            <asp:TemplateField HeaderText="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Gallery_Description_Hdr_Text %>"
              SortExpression="Description" HeaderStyle-HorizontalAlign="Left">
              <EditItemTemplate>
                <asp:TextBox ID="txtDescriptionUpdate" runat="server" Text='<%# Bind("Description") %>'></asp:TextBox>
              </EditItemTemplate>
              <ItemTemplate>
                <asp:Label ID="Label1" runat="server" Text='<%# Utils.HtmlEncode((string)Eval("Description")) %>'></asp:Label>
              </ItemTemplate>
              <FooterTemplate>
                <asp:TextBox ID="txtDescriptionInsert" runat="server" Text="" />
              </FooterTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="CreationDate" HeaderText="<%$ Resources:GalleryServer, Admin_Gallery_Settings_Gallery_DateCreated_Hdr_Text %>"
              InsertVisible="false" ReadOnly="true" SortExpression="CreationDate" DataFormatString="{0:G}" />
            <asp:TemplateField HeaderText="<%$ Resources:GalleryServer, Admin_Gallery_Settings_MediaPath_Hdr_Text %>"
              SortExpression="Description" HeaderStyle-HorizontalAlign="Left">
              <ItemTemplate>
                <asp:Label ID="lblMediaPath" runat="server"></asp:Label>
              </ItemTemplate>
              <EditItemTemplate>
                <asp:Label ID="lblMediaPath" runat="server"></asp:Label>
              </EditItemTemplate>
            </asp:TemplateField>
          </Columns>
        </asp:GridView>
      </div>
    </div>
  </div>
  <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:ObjectDataSource ID="odsGalleries" runat="server" DataObjectTypeName="GalleryServer.Business.Gallery"
  DeleteMethod="DeleteGallery" SelectMethod="GetGalleriesCurrentUserCanAdminister"
  TypeName="GalleryServer.Web.Controller.GalleryController" UpdateMethod="UpdateGallery"
  OnUpdated="odsGalleries_Updated" OnUpdating="odsGalleries_Updating" OnDeleted="odsGalleries_Deleted"
  OnDeleting="odsGalleries_Deleting"></asp:ObjectDataSource>
<asp:PlaceHolder runat="server">
  <script>
    (function ($) {
      jQuery(document).ready(function () {
        configTooltips();
      });

      var configTooltips = function () {
        $('#<%= lblCurrentGalleryHdr.ClientID %>').gsTooltip({
			    title: '<%= Resources.GalleryServer.Admin_Gallery_Settings_CurrentGallery_Info_Hdr.JsEncode() %>',
				  content: '<%= Resources.GalleryServer.Admin_Gallery_Settings_CurrentGallery_Info_Bdy.JsEncode() %>'
				});

			  $('#<%= lblManageGalleriesHdr.ClientID %>').gsTooltip({
			    title: '<%= Resources.GalleryServer.Admin_Gallery_Settings_ManageGalleries_Info_Hdr.JsEncode() %>',
			  content: '<%= Resources.GalleryServer.Admin_Gallery_Settings_ManageGalleries_Info_Bdy.JsEncode() %>'
			});
			};

		})(jQuery);
  </script>
</asp:PlaceHolder>
