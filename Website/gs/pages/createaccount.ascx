<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="createaccount.ascx.cs" Inherits="GalleryServer.Web.Pages.createaccount" %>
<%@ Import Namespace="GalleryServer.Web" %>
<asp:Panel ID="pnlCreateUser" runat="server" CssClass="gsp_createuser gsp_rounded10"
  DefaultButton="btnCreateAccount">
    <p class="gsp_h1">
        <span class="fa fa-user fa-2x gs_login_icon"></span><asp:Literal ID="litHeader" runat="server" Text="<%$ Resources:GalleryServer, CreateAccount_HeaderNormal %>" />
  </p>
  <table class="gsp_addmargin5">
    <tr style="height: 60px;">
      <td class="gsp_col1">
        <asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, CreateAccount_Username_Header_Text %>" /><span
          class="gsp_fs gsp_msgwarning">*</span>
      </td>
      <td>
        <asp:TextBox ID="txtNewUserUserName" runat="server" required="required" autofocus="autofocus" data-required="true"></asp:TextBox>
        <p class="gsp_ca_name_val"></p>
      </td>
    </tr>
    <tr id="trEmail" runat="server">
      <td class="gsp_col1">
        <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, CreateAccount_Email_Header_Text %>" /><asp:Label
          ID="lblEmailReqd" runat="server" Visible="false" CssClass="gsp_fs gsp_msgwarning"
          Text="*" />
      </td>
      <td>
        <asp:TextBox ID="txtNewUserEmail" runat="server" type="email" />
      </td>
    </tr>
    <tr>
      <td class="gsp_col1">
        <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, CreateAccount_Password_Header_Text %>" /><span
          class="gsp_fs gsp_msgwarning">*</span>
      </td>
      <td>
        <asp:TextBox ID="txtNewUserPassword1" runat="server" TextMode="Password" required="required" data-required="true"></asp:TextBox>
      </td>
    </tr>
    <tr>
      <td class="gsp_col1">
        <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, CreateAccount_Confirm_Password_Header_Text %>" /><span
          class="gsp_fs gsp_msgwarning">*</span>
      </td>
      <td>
        <asp:TextBox ID="txtNewUserPassword2" runat="server" TextMode="Password" required="required" data-required="true"></asp:TextBox>
        <asp:CompareValidator ID="cvPasswordsEqual" runat="server" CssClass="gsp_msgwarning"
          ForeColor="" ControlToCompare="txtNewUserPassword1" ControlToValidate="txtNewUserPassword2"
          Display="Dynamic" ErrorMessage="<%$ Resources:GalleryServer, CreateAccount_Passwords_Not_Matching_Error %>"
          SetFocusOnError="True"></asp:CompareValidator>
      </td>
    </tr>
    <tr>
      <td colspan="2">
        <p class="gsp_fs gsp_msgwarning">
          <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, Site_Field_Required_Text %>" />
        </p>
      </td>
    </tr>
  </table>
  <p class="gsp_rightBottom">
    <asp:Button ID="btnCreateAccount" runat="server" Text="<%$ Resources:GalleryServer, Login_Create_Account_Text %>"
      OnClick="btnCreateAccount_Click" />
  </p>
</asp:Panel>
<asp:PlaceHolder ID="phEula" runat="server"></asp:PlaceHolder>

<asp:PlaceHolder runat="server">
  <script>
    (function ($) {
      $(document).ready(function () {
        configureControls();
        bindEventHandlers();
      });

      var configureControls = function() {
        var emailReqd = <%= EnableEmailVerification.ToString().ToLowerInvariant() %>;
        if (emailReqd) {
          $('#<%= txtNewUserEmail.ClientID %>').prop('required', true).attr('data-required', true);
        }
      };
      
      var bindEventHandlers = function () {
        $('#<%= txtNewUserUserName.ClientID %>').change(function () {
          var userName = $(this).val();

          if (userName.length == 0) {
            $('.gsp_ca_name_val').html('');
            return;
          }

          var msg;
          $.ajax(({
            type: "GET",
            url: Gs.Vars.AppRoot + '/api/users/exists?userName=' + encodeURIComponent(userName),
            success: function (userExists) {
              if (userExists)
                msg = '<span class="gsp_addleftpadding1 gsp_msgwarning"><%= Resources.GalleryServer.Site_UserNameIsDuplicate %></span>';
              else
                msg = '<span class="gsp_addleftpadding1 gsp_msgfriendly"><%= Resources.GalleryServer.Site_UserNameAvailable %></span>';

              $('.gsp_ca_name_val', $('#<%= cid %>')).html(msg);
            }
          }));
        });
      };
      
    })(jQuery);
  </script>
</asp:PlaceHolder>
