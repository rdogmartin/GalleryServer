<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="manageroles.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.manageroles" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content">
  <p class="gsp_a_ap_to">
    <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
      EnableViewState="false" />&nbsp;<asp:Literal runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
  </p>
  <asp:PlaceHolder ID="phAdminHeader" runat="server" />
  <div class="gsp_addleftpadding5">
    <div id="gsp_optionsHdr" class="gsp_optionsHdr gsp_collapsed ui-corner-top">
      <p title='<asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Site_Options_Tooltip %>" />'>
        <asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, Site_Options_Hdr %>" />
      </p>
    </div>
    <section id="gsp_optionsDtl" class="gsp_optionsDtl ui-corner-bottom">
      <section>
        <p>
          <input type="checkbox" id="gsp_showCheckboxes" />
          <label for="gsp_showCheckboxes">
            <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Site_Show_Checkboxes_Lbl %>" />
          </label>
        </p>
        <section class="gsp_checkboxOptions">
          <p>
            <a id="gsp_chkCheckUncheckAll" data-ischecked="false" class="gsp_disabled">
              <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Site_ToggleCheckAll_Lbl %>" />
            </a>
          </p>
          <p runat="server">
            <a id="gsp_chkDeleteRoles" class="gsp_disabled">
              <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_Manage_Roles_Delete_Selected_Roles_Lbl %>" />
            </a>&nbsp;<span class="gsp_waitCtr"></span>
          </p>
        </section>
      </section>
      <p>
        <input type="checkbox" id="gsp_ShowOwnerRoles" />
        <label for="gsp_ShowOwnerRoles">
          <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, Admin_Manage_Roles_Show_Owner_Roles_Lbl %>" />
        </label>
      </p>
    </section>
    <div id="gsp_roles" class="gsp_roles">
    </div>
  </div>
  <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
  <script id="<%= cid %>_roleTmpl" type="text/x-jsrender">
    <h3 data-rolename="{{: ~htmlEscape(Name)}}"{{if IsOwner}}class="gsp_roleOwner"{{/if}}>
      <a href="#" {{if Members}}{{if Members.length == 0}} title="Members: None"{{else}} title="Members:{{for Members ~count=Members.length}} {{:#data}}{{if #index === ~count-2}} and{{else #index < ~count-2}}, {{/if}}{{/for}}"{{/if}}{{/if}}><input type="checkbox" class="gsp_chkRole" />{{>Name}}</a>&nbsp;<input type="button" value="Save" class="gsp_roleSaveBtn" title="Save this role" />&nbsp;<input type="button" value="Delete" class="gsp_roleDeleteBtn" title="Delete this role" />&nbsp;<span class="gsp_waitCtr"></span></h3>
    <div>
    </div>
  </script>
  <script id="<%= cid %>_roleBodyTmpl" type="text/x-jsrender">
    <div id="gsp_roleTabContainer" class="gsp_tabContainer">
      <p id="gsp_newRoleName" class='gsp_invisible'>Role name: <input id='gsp_roleName' type='text' placeholder='Type role name' style='width: 400px;' /></p>
      <ul>
        <li><a href="#gsp_roleTabPermission">Permissions</a></li>
        <li><a href="#gsp_roleTabAlbums">Albums</a></li>
        <li><a href="#gsp_roleTabUsers">Users</a></li>
      </ul>
      <div id="gsp_roleTabPermission">
        <div id="gsp_RolePerms"></div>
        <input id="hdnSelectedRolePerms" type="hidden" />
      </div>
      <div id="gsp_roleTabAlbums">
        <div id="gsp_RoleAlbums"></div>
        <input id="hdnSelectedRoleAlbums" type="hidden" />
      </div>
      <div id="gsp_roleTabUsers">
        <p>This role contains the following users:</p>
        {{for Members}}
        <p>{{:#data}}</p>
        {{/for}}
      </div>
    </div>
    <div></div>
  </script>
  <script>
    (function ($) {
      jQuery(document).ready(function () {
        configControls();
        configTooltips();
      });

      var configControls = function() {
        var gspRolePermData = <%= GetPermissionTreeviewData() %>;
        var gspRoles = JSON.parse('<%= GetRoleNames() %>');
        var gspRoleData = null;

        var bindOptions = function () {
          var roleCtr = $('#gsp_roles');

          $('.gsp_optionsHdr').click(function () {
            $(this).toggleClass("gsp_expanded gsp_collapsed");
            $('.gsp_optionsDtl').slideToggle('fast');
          });

          $("#gsp_showCheckboxes").prop("checked", false).click(function () {
            setRoleCheckboxVisibility($(this).prop("checked"));

            if ($(this).prop("checked")) {
              // Enable the hyperlinks within the 'show checkboxes' section
              $(".gsp_checkboxOptions a", $("#gsp_optionsDtl")).attr("href", "javascript:void(0);").removeClass("gsp_disabled");

              $("#gsp_chkCheckUncheckAll").click(function () {
                var checkAll = !$(this).data("isChecked"); // true when we want to check all; otherwise false
                $(this).data("isChecked", checkAll);
                if (checkAll)
                  $(".gsp_chkRole:visible", roleCtr).prop("checked", checkAll); // Check all visible roles
                else
                  $(".gsp_chkRole", roleCtr).prop("checked", checkAll); // Uncheck all, including hidden (aka owner) roles
              });

              $("#gsp_chkDeleteRoles").click(function (e) {
                var onRolesDeleted = function () {
                  gspRoles = $.grep(gspRoles, function (value) {
                    return ($.inArray(value.Name, rolesToDelete) == -1);
                  });

                  bindRoleList();
                };

                if (!confirm("Are you sure you want to delete the selected roles?")) {
                  return;
                }

                // Collect the checked items, build an array of role names, then send to server
                var imgWait = $(this).next('.gsp_waitCtr').addClass('gsp_wait_center');
                var rolesToDelete = $.map($("h3:has(.gsp_chkRole:checked)", roleCtr), function (value) { return $(value).data("rolename"); });

                // Create an array of ajax requests that we'll send to the server
                var ajaxArray = $.map(rolesToDelete, function(roleName, idx) {
                  return $.ajax({
                    type: "DELETE",
                    url: Gs.Vars.AppRoot + '/api/roles/deletebyrolename?roleName=' + encodeURIComponent(roleName),
                    error: function(response) {
                      Gs.Msg.show("Role '" + roleName + "' not deleted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                      rolesToDelete.gspRemove(idx); // Remove from array so it isn't deleted from screen later in onRolesDeleted
                    }
                  });
                });
              
                // Execute all ajax requests, then continue when all have completed
                $.when.apply($, ajaxArray).then(function() {
                  // All ajax requests succeeded
                  Gs.Msg.show("Roles Deleted", "The roles were successfully deleted.", { msgType: 'success' });
                  onRolesDeleted();
                  imgWait.removeClass('gsp_wait_center');
                }, function() {
                  // One or more ajax requests failed. The ajax error handler already displayed an error msg, so just clean up
                  onRolesDeleted();
                  imgWait.removeClass('gsp_wait_center');
                });
              });
            } else {
              $(".gsp_checkboxOptions a", $("#gsp_optionsDtl")).removeAttr("href").addClass("gsp_disabled").unbind("click");
            }
          });

          $("#gsp_ShowOwnerRoles").click(function () {
            if (this.checked) {
              $(".gsp_roleOwner", roleCtr).fadeIn();
            } else {
              $(".gsp_roleOwner", roleCtr).fadeOut(function () { $(this).find(".gsp_chkRole").prop("checked", false); });
            }
          });
        };

        var rolesAccordianOnBeforeActivate = function (event, ui) {
          $(".gsp_waitCtr", $(ui.newHeader)).addClass('gsp_wait_center');
          $(ui.oldPanel).children().remove();
          if (ui.newHeader.length > 0) {
            bindRoleData(ui.newHeader.data("rolename"), ui.newHeader, ui.newPanel);
          }

          $("input:button", ui.oldHeader).unbind("click").hide();
        };

        var rolesAccordianOnActivate = function (event, ui) {
        };

        var rolesAccordianOnCreate = function (event, ui) {
          $("input:button", $(this)).button();
          $(this).data("isAccordion", true);
        };

        var setRoleCheckboxVisibility = function (isVisible) {
          // Show checkbox next to each role if the 'show checkbox' option is selected
          $(".gsp_chkRole:gt(0)", $('#gsp_roles')).toggle(isVisible);
        };

        var bindRoleList = function () {
          var tmplData = $("#<%= cid %>_roleTmpl").render(gspRoles); // Generate the HTML from the template

          var roleCtr = $('#gsp_roles');
          if (roleCtr.data("isAccordion")) {
            roleCtr.accordion("destroy").data("isAccordion", false);
          }

          roleCtr.html(tmplData).show().accordion({
            active: false,
            animated: false,
            collapsible: true,
            heightStyle: "content", // content, auto, fill
            create: rolesAccordianOnCreate,
            beforeActivate: rolesAccordianOnBeforeActivate,
            activate: rolesAccordianOnActivate
          });

          if ($("#gsp_ShowOwnerRoles").prop('checked')) {
            $(".gsp_roleOwner", roleCtr).show();
          }

          setRoleCheckboxVisibility($("#gsp_showCheckboxes").prop("checked"));

          roleCtr.find("input.gsp_chkRole:checkbox").click(function (e) { e.stopPropagation(); });
        };

      	var bindRoleData = function (roleName, roleHeader, roleContent) {
          $.ajax(({
            type: "GET",
            url: Gs.Vars.AppRoot + '/api/roles/getbyrolename?roleName=' + encodeURIComponent(roleName),
            dataType: "json",
            success: function (role) {
              bindRoleDataReceived(role, roleHeader, roleContent);
            },
            error: function (response) {
              Gs.Msg.show("Action Aborted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
            },
            complete: function() {
              $(".gsp_waitCtr", roleHeader).removeClass('gsp_wait_center');
            }
          }));
        };

        var bindRoleDataReceived = function (data, roleHeader, roleContent) {
          var generateHtmlFromTemplate = function () {
            var tmplData = $("#<%= cid %>_roleBodyTmpl").render(gspRoleData); // Generate the HTML from the template
            $(roleContent).html(tmplData); // Add the HTML to the page
          };

          var bindRolePermissions = function () {    
            var options = {
              allowMultiSelect: true,
              albumIdsToSelect: $.map(gspRoleData.Permissions, function (permValue, permName) { if (permValue) return permName; else return null; }),
              checkedAlbumIdsHiddenFieldClientId: 'hdnSelectedRolePerms',
              navigateUrl: '',
              enableCheckboxPlugin: true
            };
            
            $('#gsp_RolePerms').jstree('destroy')
              .gsTreeView(Gs.Utils.deepCopy(gspRolePermData), options)
              .on('ready.jstree', bindRolePermissionTooltips)
              .on('after_open.jstree', bindRolePermissionTooltips); // Required because jstree deletes the HTML when collapsing
          };

          var bindRoleAlbums = function () {
            var options = {
                allowMultiSelect: true,
              checkedAlbumIdsHiddenFieldClientId: 'hdnSelectedRoleAlbums',
              navigateUrl: '',
              enableCheckboxPlugin: true
            };
          
            $('#gsp_RoleAlbums').gsTreeView(JSON.parse(gspRoleData.AlbumTreeDataJson), options);
          };

          var bindRoleUsers = function () {
          };

          var bindNewRole = function () {
            $("#gsp_newRoleName").show().find("#gsp_roleName").focus();
          };

          var saveRoleData = function (e) {
            var updateRoleDataFromHtml = function () {
              // To improve POST speed, null out the data the server won't need. We'll need to retrieve fresh data from the server before rebinding.
              gspRoleData.AlbumTreeDataJson = null;

              if (gspRoleData.IsNew) {
                gspRoleData.Name = $('#gsp_roleName').val().trim();
                if (gspRoleData.Name.length == 0)
                  throw new Error("Enter a role name.");
              }

              // Iterate all role permission nodes. For each one, update the corresponding perm value.
              var tv = $('#gsp_RolePerms');
              var lis = $('li', tv);
              $.each(lis, function () {
                gspRoleData.Permissions[$(this).data('id')] = tv.jstree("is_selected", $(this));
              });
              var idStr = $('#hdnSelectedRoleAlbums').val();
              gspRoleData.SelectedRootAlbumIds = idStr.length > 0 ? idStr.split(",") : [];
            };

            var onRoleSaved = function (response) {
              Gs.Msg.show("Save Successful", response, { msgType: 'success' });

              if (gspRoleData.IsNew) {
                gspRoleData.IsNew = false;
                gspRoles.splice(1, 0, gspRoleData);
                bindRoleList();
              }
            };

            var imgWait = $(".ui-accordion-header.ui-state-active .gsp_waitCtr", $("#gsp_roles")).addClass('gsp_wait_center');

            try {
              updateRoleDataFromHtml();

              // Send role data to the server for saving
              $.ajax(({
                type: "POST",
                url: Gs.Vars.AppRoot + '/api/roles/',
                data: JSON.stringify(gspRoleData),
                contentType: "application/json; charset=utf-8",
                success: function(response) {
                  onRoleSaved(response);
                },
                error: function (response) {
                  Gs.Msg.show("Action Aborted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                },
                complete: function() {
                  imgWait.removeClass('gsp_wait_center');
                }
              }));
            } catch (ex) {
              Gs.Msg.show("Action Aborted", ex.message, { msgType: 'error', autoCloseDelay: 0 });
              imgWait.removeClass('gsp_wait_center');
            }

            return false;
          };

          var roleDeleteClick = function (e) {
            var deleteRole = function (role) {

              var onRoleDeleted = function (response) {
                Gs.Msg.show("Role Deleted", response, { msgType: 'success' });

                gspRoles = $.grep(gspRoles, function (value) {
                  return value.Name != role.Name;
                });

                bindRoleList();
              };

              var imgWait = $(".gsp_waitCtr", roleHeader).addClass('gsp_wait_center');

              $.ajax(({
                type: "DELETE",
                url: Gs.Vars.AppRoot + '/api/roles/deletebyrolename?roleName=' + encodeURIComponent(role.Name),
                success: function(response) {
                  onRoleDeleted(response);
                },
                error: function (response) {
                  Gs.Msg.show("Action Aborted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                },
                complete: function() {
                  imgWait.removeClass('gsp_wait_center');
                }
              }));
            };

            if (confirm("<%= Resources.GalleryServer.Admin_Manage_Roles_Confirm_Delete_Text %> '" + gspRoleData.Name + "'?")) {
              deleteRole(gspRoleData);
            }

            $(this).removeClass("ui-state-focus");
            return false;
          };

          gspRoleData = data;
          generateHtmlFromTemplate();
          bindRolePermissions();
          $("div:first", roleContent).tabs().show();
          bindRoleAlbums();
          bindRoleUsers();
        
          if (!gspRoleData.Name)
            bindNewRole();
        
          $("input.gsp_roleSaveBtn", roleHeader).show().click(saveRoleData);
        
          if (!gspRoleData.IsNew)
            $("input.gsp_roleDeleteBtn", roleHeader).show().click(roleDeleteClick);
        
          if (<%= (GalleryServer.Business.AppSetting.Instance.License.LicenseType == GalleryServer.Business.LicenseLevel.TrialExpired).ToString().ToLowerInvariant() %>)
          $("input", roleHeader).prop('disabled', true).attr('title', 'Disabled - Enter a license key to restore functionality');
        };
      
        bindOptions();
        bindRoleList();
      };

      var configTooltips = function () {
        $('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_Overview_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_Overview_Bdy.JsEncode() %>'
        });
        
        $('#gsp_ShowOwnerRoles').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_ShowOwnerRoles_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_ShowOwnerRoles_Bdy.JsEncode() %>'
        });
        
        $('#gsp_ShowOwnerRoles').gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_ShowOwnerRoles_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_ShowOwnerRoles_Bdy.JsEncode() %>'
        });
      };

      var bindRolePermissionTooltips = function () {
        var permSelector = 'li[data-id="{0}"] a:first';
        var $permContainer = $('#gsp_RolePerms');
        
        $(permSelector.format('AdministerSite'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_AdminSite_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_AdminSite_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('AdministerGallery'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_AdminGallery_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_AdminGallery_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('ViewAlbumOrMediaObject'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_ViewObject_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_ViewObject_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('ViewOriginalMediaObject'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_ViewHiResImage_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_ViewHiResImage_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('AddChildAlbum'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_AddAlbum_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_AddAlbum_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('AddMediaObject'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_AddMediaObject_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_AddMediaObject_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('EditAlbum'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_EditAlbum_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_EditAlbum_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('EditMediaObject'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_EditMediaObject_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_EditMediaObject_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('DeleteChildAlbum'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_DeleteChildAlbum_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_DeleteChildAlbum_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('DeleteMediaObject'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_DeleteMediaObject_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_DeleteMediaObject_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('Synchronize'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_Synchronize_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_Synchronize_Bdy.JsEncode() %>'
        });
        
        $(permSelector.format('HideWatermark'), $permContainer).gsTooltip({
          title: '<%= Resources.GalleryServer.Admin_Manage_Roles_HideWatermark_Hdr.JsEncode() %>',
          content: '<%= Resources.GalleryServer.Admin_Manage_Roles_HideWatermark_Bdy.JsEncode() %>'
        });
      };

    })(jQuery);
  </script>
</asp:PlaceHolder>
