<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="usersettings.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.usersettings" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
<%@ Register Src="../../controls/albumtreeview.ascx" TagName="albumtreeview" TagPrefix="uc1" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content">
  <p class="gsp_a_ap_to">
    <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
      EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server" EnableViewState="false" /></p>
  <asp:PlaceHolder ID="phAdminHeader" runat="server" />
  <div class="gsp_addleftpadding5">
    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Self_Registration_Hdr %>" />
       </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p>
          <asp:CheckBox ID="chkEnableSelfRegistration" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_EnableSelfRegistration_Lbl %>" /></p>
        <div class="gsp_addleftmargin10">
          <p>
            <asp:CheckBox ID="chkRequireEmailValidation" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_RequireEmailValidation_Lbl %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkRequireAdminApproval" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_RequireAdminApproval_Lbl %>" />
          </p>
          <p>
            <asp:CheckBox ID="chkUseEmailForAccountName" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_UseEmailForAccountName_Lbl %>" />
          </p>
        </div>
      </div>
    </div>

    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Accounts_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
          <p>
            <asp:Label ID="lblUserRoles" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Default_Roles_Lbl %>" />
          </p>
          <p>
            <asp:ListBox ID="lbUserRoles" runat="server" EnableViewState="false" SelectionMode="Multiple" CssClass="gsp_j_rolelist"/>
            <asp:HiddenField ID="hdnUserRoles" runat="server" />
          </p>
        <p class="gsp_addtopmargin10">
          <asp:Label ID="lblUsersToNotify" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Users_To_Notify_For_New_Accounts_Lbl %>" />
        </p>
        <p>
          <asp:ListBox ID="lbUsersToNotify" runat="server" EnableViewState="false" SelectionMode="Multiple" />
          <asp:HiddenField ID="hdnUsersToNotify" runat="server" />
        </p>
        <p class="gsp_addtopmargin10">
          <asp:CheckBox ID="chkAllowAnonymousBrowsing" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_AllowAnonymousBrowsing_Label %>" />
        </p>
        <p>
          <asp:CheckBox ID="chkAllowAnonymousRating" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_AllowAnonymousRating_Label %>" />
        </p>
        <p>
          <asp:CheckBox ID="chkAllowHtml" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_AllowHtml_Label %>" />
        </p>
        <div class="gsp_addleftmargin10">
          <p class="gsp_addtopmargin5">
            <asp:Label ID="lblAllowedHtmlTags" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Allowed_HTML_Tags_Label %>" />
          </p>
          <p>
            <asp:TextBox ID="txtAllowedHtmlTags" runat="server" CssClass="gsp_textarea3" TextMode="MultiLine" />
          </p>
          <p class="gsp_addtopmargin5">
            <asp:Label ID="lblAllowedHtmlAttributes" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Allowed_HTML_Attributes_Label %>" />
          </p>
          <p>
            <asp:TextBox ID="txtAllowedHtmlAttributes" runat="server" CssClass="gsp_textarea3"
              TextMode="MultiLine" />
          </p>
        </div>
        <p class="gsp_addtopmargin5">
          <asp:CheckBox ID="chkAllowJavascript" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_AllowJavascript_Label %>" />
        </p>
        <p>
          <asp:CheckBox ID="chkAllowCopyingReadOnlyObjects" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_AllowCopyingReadOnlyObjects_Label %>" />
        </p>
        <p>
          <asp:CheckBox ID="chkAllowManageAccount" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_AllowManageAccount_Lbl %>" /></p>
        <div class="gsp_addleftmargin10">
          <p>
            <asp:CheckBox ID="chkAllowDeleteOwnAccount" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_AllowDeleteOwnAccount_Label %>" />
          </p>
        </div>
      </div>
    </div>

    <div class="gsp_single_tab">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
          <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_User_Albums_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
        <p>
          <asp:CheckBox ID="chkEnableUserAlbums" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_EnableUserAlbums_Lbl %>" /></p>
        <div class="gsp_addleftmargin10">
          <p class="gsp_addleftpadding6">
            <asp:CheckBox ID="chkRedirectAfterLogin" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_RedirectAfterLogin_Lbl %>" />
          </p>
          <p>
            <asp:Label ID="lblAlbumNameTemplate" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_User_Album_Name_Template_Label %>" />
            <asp:TextBox ID="txtAlbumNameTemplate" runat="server" CssClass="gsp_textbox" />
          </p>
          <p class="gsp_addtopmargin10">
            <asp:Label ID="lblAlbumSummaryTemplate" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_User_Album_Summary_Template_Label %>" />
          </p>
          <p>
            <asp:TextBox ID="txtAlbumSummaryTemplate" runat="server" CssClass="gsp_textarea3"
              TextMode="MultiLine" />
          </p>
          <p class="gsp_addtopmargin10">
            <asp:Label ID="lblUserAlbumParent" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_User_Album_Parent_Album_Lbl %>" /></p>
          <div class="gsp_addleftpadding10" runat="server">
            <p id="gsp_userAlbum" class="gsp_us_ua"><input id="gsp_iptUserAlbum" type="text" class="gsp_textbox" value="<%= UserAlbumTitle %>"/><img id="gsp_imgUserAlbum" alt="" src="<%= Utils.GetSkinnedUrl("/images/down-arrow-s-o.png") %>"/></p>
            <section id="gsp_userAlbumDropDown" class="gsp_us_ua_dd ui-corner-bottom ui-corner-tr">
              <uc1:albumtreeview ID="tvUC" runat="server" AllowMultiCheck="false" />
            </section>
          </div>
          <p class="gsp_bold gsp_addtopmargin5">
            <asp:Literal ID="l9" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_User_Album_NewAccount_Hdr %>" />
          </p>
          <p class="gsp_addleftpadding6">
            <asp:CheckBox ID="chkEnableUserAlbumDefaultForUser" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_EnableUserAlbumDefaultForUser_Lbl %>" />
          </p>
          <p class="gsp_bold gsp_addtopmargin5">
            <asp:Label ID="lblUserAlbumsExistingAccounts" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_User_Album_ExistingAccount_Hdr %>" />
          </p>
          <div class="gsp_addleftpadding5">
            <p>
              <asp:Button ID="btnEnableUserAlbums" runat="server" OnClick="btnEnableUserAlbums_Click" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Turn_On_User_Album_Btn_Text %>" />
              &nbsp;<asp:Label ID="lblEnableUserAlbums" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Turn_On_User_Album_Lbl_Text %>" />
            </p>
            <p>
              <asp:Button ID="btnDisableUserAlbums" runat="server" OnClick="btnDisableUserAlbums_Click" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Turn_Off_User_Album_Btn_Text %>" />
              &nbsp;<asp:Label ID="lblDisableUserAlbums" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Turn_Off_User_Album_Lbl_Text %>" />
              </p>
          </div>
          <asp:Panel ID="pnlOrphanUserAlbums" runat="server" Width="575" CssClass="gsp_p_a_us_oac gsp_rounded10"
            Visible="false">
            <img style="float: left;" class="gsp_p_a_us_ii" src="<%= Utils.GetSkinnedUrl("/images/info-l.png") %>" alt="" />
            <asp:Label ID="lblOrphanUserAlbumsMsg" runat="server" CssClass="gsp_msgattention" />
            <div class="gsp_p_a_us_doa">
              <img src="<%= Utils.GetSkinnedUrl("/images/delete-red-s.png") %>" alt="" /><a class="gsp_p_a_us_doa_hl" href="#"><asp:Literal
                  ID="l7" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Delete_Orphan_User_Albums_Lbl %>" /></a></div>
            <div class="gsp_p_a_us_t">
              <input id="gsp_orphan_ua_sa" type="checkbox" /> <label for="gsp_orphan_ua_sa"><asp:Literal Text="<%$ Resources:GalleryServer, Admin_User_Settings_Orphan_User_Albums_Select_All_Lbl %>" runat="server" /></label>
            </div>
            <div id="gsp_chk_container" class="gsp_p_a_us_oal">
              <asp:Repeater ID="rptrOrphanUserAlbums" runat="server">
                <ItemTemplate>
                  <p class="gsp_p_a_us_oa">
                    <input type="checkbox" id='<%# "chk" + (Eval("ID")) %>' />&nbsp;<a href="<%# GetAlbumUrl((int)Eval("ID")) %>" 
                    title='<asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_View_Orphan_User_Album_Tooltip %>" />'><%# Utils.RemoveHtmlTags(Eval("Title").ToString()) %></a>
                  </p>
                </ItemTemplate>
              </asp:Repeater>
            </div>
          </asp:Panel>
        </div>
      </div>
    </div>

    <tis:wwDataBinder ID="wwDataBinder" runat="server" OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl" OnValidateControl="wwDataBinder_ValidateControl">
      <DataBindingItems>
        <tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="EnableSelfRegistration"
          ControlId="chkEnableSelfRegistration" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_EnableSelfRegistration_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="RequireEmailValidationForSelfRegisteredUser"
          ControlId="chkRequireEmailValidation" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_RequireEmailValidation_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="RequireApprovalForSelfRegisteredUser"
          ControlId="chkRequireAdminApproval" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_RequireAdminApproval_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="UseEmailForAccountName"
          ControlId="chkUseEmailForAccountName" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_UseEmailForAccountName_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi6" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="EnableUserAlbum"
          ControlId="chkEnableUserAlbums" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_EnableUserAlbums_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi7" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="UserAlbumNameTemplate"
          ControlId="txtAlbumNameTemplate" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_User_Album_Name_Template_Label %>" />
        <tis:wwDataBindingItem ID="wbi8" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="UserAlbumSummaryTemplate"
          ControlId="txtAlbumSummaryTemplate" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_User_Album_Summary_Template_Label %>" />
        <tis:wwDataBindingItem ID="wbi9" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="EnableUserAlbumDefaultForUser"
          ControlId="chkEnableUserAlbumDefaultForUser" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_EnableUserAlbumDefaultForUser_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi10" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="RedirectToUserAlbumAfterLogin"
          ControlId="chkRedirectAfterLogin" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_RedirectAfterLogin_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi11" runat="server" BindingSource="GallerySettingsUpdateable"
          BindingSourceMember="AllowAnonymousBrowsing" ControlId="chkAllowAnonymousBrowsing"
          BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_AllowAnonymousBrowsing_Label %>" />
        <tis:wwDataBindingItem ID="wbi12" runat="server" BindingSource="GallerySettingsUpdateable"
          BindingSourceMember="AllowAnonymousRating" ControlId="chkAllowAnonymousRating"
          BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_AllowAnonymousRating_Label %>" />
        <tis:wwDataBindingItem ID="wbi22" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="AllowUserEnteredHtml"
          ControlId="chkAllowHtml" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_AllowHtml_Label %>" />
        <tis:wwDataBindingItem ID="wbi23" runat="server" BindingSource="this" BindingSourceMember="AllowedHtmlTags"
          ControlId="txtAllowedHtmlTags" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_Allowed_HTML_Tags_Label %>" />
        <tis:wwDataBindingItem ID="wbi24" runat="server" BindingSource="this" BindingSourceMember="AllowedHtmlAttributes"
          ControlId="txtAllowedHtmlAttributes" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_Allowed_HTML_Attributes_Label %>" />
        <tis:wwDataBindingItem ID="wbi25" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="AllowUserEnteredJavascript"
          ControlId="chkAllowJavascript" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_AllowJavascript_Label %>" />
        <tis:wwDataBindingItem ID="wbi26" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="AllowCopyingReadOnlyObjects"
          ControlId="chkAllowCopyingReadOnlyObjects" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_AllowCopyingReadOnlyObjects_Label %>" />
        <tis:wwDataBindingItem ID="wbi27" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="AllowManageOwnAccount"
          ControlId="chkAllowManageAccount" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_AllowManageAccount_Lbl %>" />
        <tis:wwDataBindingItem ID="wbi28" runat="server" BindingSource="GallerySettingsUpdateable" BindingSourceMember="AllowDeleteOwnAccount"
          ControlId="chkAllowDeleteOwnAccount" BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_User_Settings_AllowDeleteOwnAccount_Label %>" />
      </DataBindingItems>
    </tis:wwDataBinder>
    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
  </div>
</div>
<asp:PlaceHolder runat="server">
  <script>
    (function ($) {

      $(document).ready(function () {
        configControls();
        updateUi();
        configTooltips();
      });
      
      var configControls = function() {
        var mouseIsInsideUserAlbumSection = false;
        
        $("body").mouseup(function () {
          if (!mouseIsInsideUserAlbumSection) $('#gsp_userAlbumDropDown').slideUp();
        });

        $('#gsp_orphan_ua_sa').click(function() {
          // Check/uncheck all checkboxes for orphaned albums
          $('#<%= pnlOrphanUserAlbums.ClientID %> :input[type=checkbox]').prop('checked', this.checked);
        });
        
        $('#gsp_userAlbum, #gsp_userAlbumDropDown').hover(function () {
          mouseIsInsideUserAlbumSection = true;
        }, function () {
          mouseIsInsideUserAlbumSection = false;
        });

        $('#<%= chkEnableSelfRegistration.ClientID %>, #<%= chkEnableUserAlbums.ClientID %>, #<%= chkAllowHtml.ClientID %>, #<%= chkAllowManageAccount.ClientID %>').click(updateUi);
        
        // Convert 'default roles' list to a jQuery multi-select
        $('#<%= lbUserRoles.ClientID %>')
          .multiselect({
            minWidth: Gs.Utils.isWidthLessThan(750) ? 300 : 750,
            header: '<input id="chkShowOwnerRoles" type="checkbox" /><label for="chkShowOwnerRoles"><%= ShowAlbumOwnerRolesLabel %></label>',
            noneSelectedText: '&lt;No roles selected&gt;',
            selectedList: 2,
            classes: 'gsp_j_rolelist',
            close: function() {
              // Assign selected roles to hidden field
              $("#<%= hdnUserRoles.ClientID %>").val(JSON.stringify($('#<%= lbUserRoles.ClientID %>').val()));
            }
          })
          .multiselect('widget')
          .appendTo($('#<%= cid %>')); // Move to .gsp_ns namespace so it'll inherit the jQuery UI CSS classes

        $('#chkShowOwnerRoles').click(function() {
          if ($(this).prop('checked'))
            $(".gsp_j_rolelist .gsp_j_albumownerrole").fadeIn();
          else
            $(".gsp_j_rolelist .gsp_j_albumownerrole").fadeOut();
        });
              
        // Convert 'users to notify when account is created' list to a jQuery multi-select
        $('#<%= lbUsersToNotify.ClientID %>')
          .multiselect({
            minWidth: Gs.Utils.isWidthLessThan(500) ? 300 : 500,
            header: false,
            noneSelectedText: '&lt;No users selected&gt;',
            selectedList: 5,
            close: function() {
              // Assign selected users to hidden field
              $("#<%= hdnUsersToNotify.ClientID %>").val(JSON.stringify($('#<%= lbUsersToNotify.ClientID %>').val()));
            }
          })
          .multiselect('widget')
          .appendTo($('#<%= cid %>')); // Move to .gsp_ns namespace so it'll inherit the jQuery UI CSS classes

        // Wire up 'delete orphan user albums' link
        $('.gsp_p_a_us_doa_hl', $('#<%= cid %>')).click(function() {
          var $self = $(this);
          
          var elsToDelete = $('#gsp_chk_container :input:checked');
          
          if (elsToDelete.length == 0) {
            Gs.Msg.show("Action Aborted", "No albums were marked for deletion", { msgType: 'info' });
            return false;
          } 
          else
            $self.addClass('gsp_wait');

          if (!confirm('<asp:Literal ID="l10" runat="server" Text="<%$ Resources:GalleryServer, Admin_User_Settings_Delete_Orphan_User_Albums_Confirm_Msg %>" />'))
            return false;
          
          elsToDelete.each(function () {
            var albumId = this.id.substring(3);
            
            $.ajax({
              type: "DELETE",
              url: Gs.Vars.AppRoot + '/api/albums/' + albumId,
              error: function (response) {
                Gs.Msg.show("Action Aborted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
              }
            }).then(function() {
              $self.removeClass('gsp_wait');
              Gs.Msg.show("Success", "The albums were successfully deleted.", { msgType: 'success' });
              $('#gsp_chk_container p:has(:input:checked)').fadeOut();
            }, function() {
              $self.removeClass('gsp_wait');
            });
          });

          return false;
        });
      };
      
      var updateUi = function () {
        var hideOwnerRoles = function () {
          $(".gsp_j_rolelist .gsp_j_albumownerrole").hide();
        };

        // Self registration section
        var chkEnableSelfRegistration = $('#<%= chkEnableSelfRegistration.ClientID %>');
        var chkRequireEmailValidation = $('#<%= chkRequireEmailValidation.ClientID %>');
        var chkRequireAdminApproval = $('#<%= chkRequireAdminApproval.ClientID %>');
        var chkUseEmailForAccountName = $('#<%= chkUseEmailForAccountName.ClientID %>');

        chkRequireEmailValidation.prop('disabled', (!chkEnableSelfRegistration.prop('checked') || chkEnableSelfRegistration.prop('disabled')));
        chkUseEmailForAccountName.prop('disabled', chkRequireEmailValidation.prop('disabled'));
        chkRequireAdminApproval.prop('disabled', chkRequireEmailValidation.prop('disabled'));

        // User album section
        var chkEnableUserAlbums = $('#<%= chkEnableUserAlbums.ClientID %>');
        var chkEnableUserAlbumDefaultForUser = $('#<%= chkEnableUserAlbumDefaultForUser.ClientID %>');
        var chkRedirectAfterLogin = $('#<%= chkRedirectAfterLogin.ClientID %>');
        var txtAlbumNameTemplate = $('#<%= txtAlbumNameTemplate.ClientID %>');
        var txtAlbumSummaryTemplate = $('#<%= txtAlbumSummaryTemplate.ClientID %>');
        var btnEnableUserAlbums = $('#<%= btnEnableUserAlbums.ClientID %>');
        var btnDisableUserAlbums = $('#<%= btnDisableUserAlbums.ClientID %>');

        var userAlbumsDisabled = (!chkEnableUserAlbums.prop('checked') || chkEnableUserAlbums.prop('disabled'));

        txtAlbumNameTemplate.prop('disabled', userAlbumsDisabled);
        txtAlbumSummaryTemplate.prop('disabled', userAlbumsDisabled);
        chkEnableUserAlbumDefaultForUser.prop('disabled', userAlbumsDisabled);
        chkRedirectAfterLogin.prop('disabled', userAlbumsDisabled);
        btnEnableUserAlbums.prop('disabled', userAlbumsDisabled);
        btnDisableUserAlbums.prop('disabled', userAlbumsDisabled);

        if (userAlbumsDisabled) {
          $("#gsp_userAlbum").unbind("click");
          $("#gsp_iptUserAlbum").prop('disabled', true);
        }
        else {
          $("#gsp_userAlbum").click(function () {
            $("#gsp_userAlbumDropDown").slideToggle();
          });

          $("#gsp_iptUserAlbum").prop('disabled', false);

          $("#<%= tvUC.TreeViewClientId %>").on("changed.jstree", function (e, data) {
            switch (data.action) {
              case 'select_node':
                $("#gsp_iptUserAlbum").val(data.node.text);
                $("#gsp_userAlbumDropDown").slideUp();
                break;
              case 'deselect_node':
                $("#gsp_userAlbumDropDown").slideUp();
                break;
          }
          });
        }

        // User permissions section
        var chkAllowHtml = $('#<%= chkAllowHtml.ClientID %>');
        var txtAllowedHtmlTags = $('#<%= txtAllowedHtmlTags.ClientID %>');
        var txtAllowedHtmlAttributes = $('#<%= txtAllowedHtmlAttributes.ClientID %>');
        var chkAllowManageAccount = $('#<%= chkAllowManageAccount.ClientID %>');
        var chkAllowDeleteOwnAccount = $('#<%= chkAllowDeleteOwnAccount.ClientID %>');

        txtAllowedHtmlTags.prop('disabled', (!chkAllowHtml.prop('checked') || chkAllowHtml.prop('disabled')));
        txtAllowedHtmlTags.attr('disabled', (!chkAllowHtml.prop('checked') || chkAllowHtml.prop('disabled')));	      
        txtAllowedHtmlAttributes.prop('disabled', txtAllowedHtmlTags.prop('disabled'));
        chkAllowDeleteOwnAccount.prop('disabled', (!chkAllowManageAccount.prop('checked') || chkAllowManageAccount.prop('disabled')));
                
        // Disable default roles setting when current user is only a gallery admin and one or both of the gallery admin permissions are disabled
        if (<%= (UserCanEditUsersAndRoles && UserCanViewUsersAndRolesInOtherGalleries && !ActiveDirectoryRoleProviderIsBeingUsed).ToString().ToLowerInvariant() %>)
            $('#<%= lbUserRoles.ClientID %>').multiselect('enable');
        else
            $('#<%= lbUserRoles.ClientID %>').multiselect('disable');

        if (!$('#chkShowOwnerRoles').prop('checked'))
          hideOwnerRoles();
      };

      var configTooltips = function() {
        $('#<%= chkEnableSelfRegistration.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_enableSelfRegistration_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_enableSelfRegistration_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkRequireAdminApproval.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_requireApprovalForSelfRegisteredUser_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_requireApprovalForSelfRegisteredUser_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkRequireEmailValidation.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_requireEmailValidationForSelfRegisteredUser_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_requireEmailValidationForSelfRegisteredUser_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkUseEmailForAccountName.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_useEmailForAccountName_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_useEmailForAccountName_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblUserRoles.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_defaultRolesForUser_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_defaultRolesForUser_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkEnableUserAlbums.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_enableUserAlbum_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_enableUserAlbum_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblAlbumNameTemplate.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_userAlbumNameTemplate_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_userAlbumNameTemplate_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblAlbumSummaryTemplate.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_userAlbumSummaryTemplate_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_userAlbumSummaryTemplate_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkEnableUserAlbumDefaultForUser.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_enableUserAlbumDefaultForUser_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_enableUserAlbumDefaultForUser_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkRedirectAfterLogin.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_redirectToUserAlbumAfterLogin_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_redirectToUserAlbumAfterLogin_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblUserAlbumParent.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_userAlbumParentAlbumId_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_userAlbumParentAlbumId_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblUserAlbumsExistingAccounts.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_User_Settings_User_Album_Existing_Accounts_Lbl_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_User_Settings_User_Album_Existing_Accounts_Lbl_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblEnableUserAlbums.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_User_Settings_Turn_On_User_Album_Lbl_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_User_Settings_Turn_On_User_Album_Lbl_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblDisableUserAlbums.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_User_Settings_Turn_Off_User_Album_Lbl_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_User_Settings_Turn_Off_User_Album_Lbl_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblUsersToNotify.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_usersToNotifyWhenAccountIsCreated_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_usersToNotifyWhenAccountIsCreated_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkAllowAnonymousBrowsing.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowAnonymousBrowsing_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_allowAnonymousBrowsing_Bdy.JsEncode() %>'
        });

        $('#<%= chkAllowAnonymousRating.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowAnonymousRating_Hdr.JsEncode() %>', 
          content: '<%= Resources.GalleryServer.Cfg_allowAnonymousRating_Bdy.JsEncode() %>'
        });

        $('#<%= chkAllowHtml.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowUserEnteredHtml_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_allowUserEnteredHtml_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblAllowedHtmlTags.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowedHtmlTags_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_allowedHtmlTags_Bdy.JsEncode() %>'
        });
        
        $('#<%= lblAllowedHtmlAttributes.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowedHtmlAttributes_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_allowedHtmlAttributes_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkAllowJavascript.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowUserEnteredJavascript_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_allowUserEnteredJavascript_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkAllowCopyingReadOnlyObjects.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowCopyingReadOnlyObjects_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_allowCopyingReadOnlyObjects_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkAllowManageAccount.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowManageOwnAccount_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_allowManageOwnAccount_Bdy.JsEncode() %>'
        });
        
        $('#<%= chkAllowDeleteOwnAccount.ClientID %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Cfg_allowDeleteOwnAccount_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Cfg_allowDeleteOwnAccount_Bdy.JsEncode() %>'
        });
        
      };
    })(jQuery);
  </script>
</asp:PlaceHolder>

