<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="manageusers.ascx.cs"
    Inherits="GalleryServer.Web.Pages.Admin.manageusers" %>
<%@ Import Namespace="GalleryServer.Web" %>
<%@ Import Namespace="GalleryServer.Web.Controller" %>
<asp:PlaceHolder runat="server">
    <div class="gsp_content">
        <p class="gsp_a_ap_to">
            <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
                EnableViewState="false" />&nbsp;<asp:Literal runat="server" Text="<%$ Resources:GalleryServer, Admin_All_Galleries_Label %>" />
        </p>
        <asp:PlaceHolder ID="phAdminHeader" runat="server" />
        <div class="gsp_addleftpadding5 gsp_usr_ctr">
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
                        <p id="P1" runat="server">
                            <a id="gsp_chkDeleteUsers" class="gsp_disabled">
                                <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_Manage_Users_Delete_Selected_Users_Lbl %>" />
                            </a>&nbsp;<span class="gsp_waitCtr"></span>
                        </p>
                    </section>
                </section>
            </section>
         <div class="ui-widget ui-front">
            <p class="gsp_addleftmargin10"><label for="gsp_usr_srch">Find user: </label><input id="gsp_usr_srch" type="text" class="gsp_mu_srch" /></p>
         </div>
         <div id="<%= cid %>_users" class="gsp_users">
            </div>
            <asp:ListBox ID="lbRoles" runat="server" EnableViewState="false" SelectionMode="Multiple" CssClass="gsp_j_rolelist gsp_invisible" />
            <asp:HiddenField ID="hdnUserRoles" runat="server" />
        </div>
        <asp:PlaceHolder ID="phAdminFooter" runat="server" />
    </div>
    <script id="<%= cid %>_userTmpl" type="text/x-jsrender">
        <h3 data-username="{{if #index > 0}}{{: ~htmlEscape(#data)}}{{/if}}">
            <a href="#" class="gs_user_link"><input type="checkbox" class="gsp_chkUser" />{{>#data}}</a>&nbsp;<input type="button" value="Save" class="gsp_userSaveBtn" title="Save this user" />&nbsp;<input type="button" value="Delete" class="gsp_userDeleteBtn" title="Delete this user" />&nbsp;<span class="gsp_waitCtr"></span></h3>
        <div>
        </div>
    </script>
    <script id="<%= cid %>_userNewTmpl" type="text/x-jsrender">
        <div id="gsp_userTabContainer" class="gsp_tabContainer">
            <ul>
                <li><a href="#gsp_userTabGeneral">New user info</a></li>
            </ul>
            <div id="gsp_userTabGeneral">
                <table class="gsp_standardTable gsp_mu_eu_tbl">
                    <tr><td class="gsp_col1">User name:</td><td><input type="text" required class="gsp_textbox gsp_mu_eu_n" placeholder="Enter user name" value="" /></td></tr>
                    <tr><td class="gsp_col1">E-mail:</td><td><input type="email" class="gsp_textbox gsp_mu_eu_e" value="" /></td></tr>
                    <tr><td class="gsp_col1">Password:</td><td><input type="password" required class="gsp_textbox gsp_mu_eu_pwd1" value="" /></td></tr>
                    <tr><td class="gsp_col1">Confirm:</td><td><input type="password" required class="gsp_textbox gsp_mu_eu_pwd2" value="" /></td></tr>
                    <tr><td class="gsp_col1">Roles:</td><td class="gsp_col2 gsp_userRolesCell"></td></tr>
                </table>
            </div>
        </div>
    </script>
    <script id="<%= cid %>_userEditTmpl" type="text/x-jsrender">
        <div id="gsp_userTabContainer" class="gsp_tabContainer">
            <ul>
                <li><a href="#gsp_userTabGeneral">General</a></li>
                <li><a href="#gsp_userTabPassword">Password</a></li>
            </ul>
            <div id="gsp_userTabGeneral">
                <table class="gsp_standardTable gsp_mu_eu_tbl">
        {{if IsLockedOut}}
                    <tr><td class="gsp_col1"></td><td class="gsp_col2"><img src="<%= Utils.GetSkinnedUrl("/images/warning-s.png") %>" alt="Warning icon" style="vertical-align:top;" /><span class="gsp_msgattention"> Account is locked from too many failed login attempts.</span> <a href="#" class="gsp_mu_usr_unlck" title="Unlock this user">Unlock</a></td></tr>
        {{/if}}
                    <tr><td class="gsp_col1">Roles:</td><td class="gsp_col2 gsp_userRolesCell"></td></tr>
                    <tr><td class="gsp_col1 gsp_aligntop">Description:</td><td><textarea class="gsp_textarea1 gsp_mu_eu_d" cols="20" rows="2">{{>Comment}}</textarea></td></tr>
                    <tr><td class="gsp_col1">E-mail:</td><td><input type="email" class="gsp_textbox gsp_mu_eu_e" value="{{:Email}}" /></td></tr>
                    <tr><td class="gsp_col1{{if !IsApproved}} gsp_msgattention{{/if}}">Approved?</td><td>
                        <input type="radio" id="gsp_userApprovalYes" name="gspUserApproval" value="approvedYes" {{if IsApproved}} checked{{/if}} class="gsp_mu_eu_app" /><label for="gsp_userApprovalYes">Yes</label>
                        <input type="radio" id="gsp_userApprovalNo" name="gspUserApproval" value="approvedNo" {{if !IsApproved}} checked{{/if}} /><label for="gsp_userApprovalNo">No</label>
                    </td></tr>
        {{if ~isUserAlbumEnabled()}}
                    <tr><td class="gsp_col1">Enable user album:</td><td>
                        <input type="radio" id="gsp_userUAYes" name="gspUserAlbum" value="enableUserAlbum" {{if EnableUserAlbum}} checked{{/if}} class="gsp_mu_eu_ua" /><label for="gsp_userUAYes">Yes</label>
                        <input type="radio" id="gsp_userUANo" name="gspUserAlbum" value="enableUserAlbum" {{if !EnableUserAlbum}} checked{{/if}} /><label for="gsp_userUANo">No</label>
                    </td></tr>
        {{/if}}
                    <tr><td class="gsp_col1">Last activity date:</td><td class="gsp_msgfriendly">{{: ~getDateAsFormattedString(LastActivityDate)}}</td></tr>
                    <tr><td class="gsp_col1">Last logon date:</td><td class="gsp_msgfriendly">{{: ~getDateAsFormattedString(LastLoginDate)}}</td></tr>
                    <tr><td class="gsp_col1">Last password changed date:</td><td class="gsp_msgfriendly">{{: ~getDateAsFormattedString(LastPasswordChangedDate)}}</td></tr>
                    <tr><td class="gsp_col1">Account created:</td><td class="gsp_msgfriendly">{{: ~getDateAsFormattedString(CreationDate)}}</td></tr>
                </table>
            </div>
            <div id="gsp_userTabPassword" class="gsp_mu_pwd_tab">
                <p>
                    <input type="checkbox" id="gsp_pwdOptionReset" name="gspPwdOptionReset" class="gsp_mu_pwd_rst" /><label for="gsp_pwdOptionReset">Reset Password</label>
                </p>
                <p class="gsp_mu_pwd_rst_dtl">Resets a user's password to a new, automatically generated password. You will be given the new password so you can notify the user.</p>
                <p>
                    <input type="checkbox" id="gsp_pwdOptionChange" name="gspPwdOptionChange" class="gsp_mu_pwd_chg" /><label for="gsp_pwdOptionChange">Change Password</label>
                </p>
                <section class="gsp_mu_pwd_chg_dtl">
                    <table class="gsp_standardTable gsp_mu_pwd_tbl">
                        <tr>
                            <td class="gsp_col1">New Password:</td>
                            <td class="gsp_col2">
                                <input type="password" required class="gsp_textbox gsp_mu_eu_pwd1" value="" /></td>
                        </tr>
                        <tr>
                            <td class="gsp_col1">Confirm:</td>
                            <td class="gsp_col2">
                                <input type="password" required class="gsp_textbox gsp_mu_eu_pwd2" value="" /></td>
                        </tr>
                    </table>
                </section>
                <p class="gsp_addtopmargin10">
                    <input type="checkbox" id="gsp_emailPwd" name="gspEmailPwd" class="gsp_mu_ntf_on_pwd_chg"  /><label for="gsp_emailPwd">E-mail new password to user</label>
                </p>
            </div>
        </div>
        <div></div>
    </script>

    <script>
        (function ($) {
            jQuery(document).ready(function () {
                bindOptions();
                bindUserList();
                bindUserSearch();
                configTooltips();
                $("#gsp_usr_srch").focus(); // Don't use autofocus attribute. For some reason it causes the accordian headers to require two clicks
            });

            var gspUsers = JSON.parse('<%= GetUserNames() %>');
            var gspUserData = null;
            var $userHtmlCtr = $('#<%= cid %>_users');
            var $allRoles = $('#<%= lbRoles.ClientID %>');
            var userSettings = {
                requiresQuestionAndAnswer: <%= UserController.RequiresQuestionAndAnswer.ToString().ToLowerInvariant() %>, 
                enablePasswordReset: <%= UserController.EnablePasswordReset.ToString().ToLowerInvariant() %>, 
                enablePasswordRetrieval: <%= UserController.EnablePasswordRetrieval.ToString().ToLowerInvariant() %>,
                useEmailForAccountName: <%= GallerySettings.UseEmailForAccountName.ToString().ToLowerInvariant() %>
                };

            var bindUserSearch = function() {
                var dg;
                $("#gsp_usr_srch").autocomplete({
                    source: gspUsers,
                    autoFocus: true,
                    change: function(e, ui) {
                        var userName = $(this).val().trim();
                        
                        if (userName.length == 0) {
                            bindUserList();
                            return;
                        }
                        
                        if ($.inArray(userName, gspUsers) > -1) {
                            if (dg != null)
                                dg.dialog("destroy"); // close dialog from previous 'user not found' messages
                            
                            var users = new Array();
                            users.push(userName);
                            bindUserList(users);
                            $('.ui-accordion-header[data-username="' + userName.replace(/\"/g, '\\"').replace(' ', '\\ ') + '"]').click();
                        } else {
                            dg = Gs.Msg.show('User Not Found', 'The user ' + userName + ' does not exist.', { msgType: 'info', show: 'none', autoCloseDelay: 0 });
                            $(this).focus();
                        }
                    }
                });

                $("#gsp_usr_srch").keydown(function (e) {
                    if (e.keyCode === $.ui.keyCode.ENTER) {
                        $('.gs_user_link')[0].focus();
                    }
                    return true;
                });
            };
            
            var bindOptions = function () {
                
                $('.gsp_optionsHdr').click(function () {
                    $(this).toggleClass("gsp_expanded gsp_collapsed");
                    $('.gsp_optionsDtl').slideToggle('fast');
                });

                $("#gsp_showCheckboxes").prop("checked", false).click(function () {
                    setUserCheckboxVisibility($(this).prop("checked"));

                    if ($(this).prop("checked")) {
                        // Enable the hyperlinks within the 'show checkboxes' section
                        $(".gsp_checkboxOptions a", $("#gsp_optionsDtl")).attr("href", "javascript:void(0);").removeClass("gsp_disabled");

                        $("#gsp_chkCheckUncheckAll").click(function () {
                            var checkAll = !$(this).data("isChecked"); // true when we want to check all; otherwise false
                            $(this).data("isChecked", checkAll);
                            if (checkAll)
                                $(".gsp_chkUser:visible", $userHtmlCtr).prop("checked", checkAll); // Check all visible roles
                            else
                                $(".gsp_chkUser", $userHtmlCtr).prop("checked", checkAll); // Uncheck all, including hidden (aka owner) roles
                        });

                        $("#gsp_chkDeleteUsers").click(function (e) {
                            var onUsersDeleted = function () {
                                gspUsers = $.grep(gspUsers, function (value) {
                                    return ($.inArray(value, usersToDelete) == -1);
                                });

                                bindUserList();
                                $('#gsp_usr_srch').val('');
                            };

                            if (!confirm("Are you sure you want to delete the selected users?")) {
                                return;
                            }

                            // Collect the checked items, build an array of role names, then send to server
                            var imgWait = $(this).next('.gsp_waitCtr').addClass('gsp_wait_center');
                            var usersToDelete = $.map($("h3:has(.gsp_chkUser:checked)", $userHtmlCtr), function (value) { return $(value).data("username"); });

                            // Create an array of ajax requests that we'll send to the server
                            var ajaxArray = $.map(usersToDelete, function(userName, idx) {
                                return $.ajax({
                                    type: "DELETE",
                                    url: Gs.Vars.AppRoot + '/api/users/deletebyusername?userName=' + encodeURIComponent(userName),
                                    error: function(response) {
                                        Gs.Msg.show("User '" + userName + "' not deleted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                                        usersToDelete.gspRemove(idx); // Remove from array so it isn't deleted from screen later in onUsersDeleted
                                    }
                                });
                            });
                            
                            // Execute all ajax requests, then continue when all have completed
                            $.when.apply($, ajaxArray).then(function() {
                                // All ajax requests succeeded
                                Gs.Msg.show("Users Deleted", "The users were successfully deleted.", { msgType: 'success' });
                                onUsersDeleted();
                                imgWait.removeClass('gsp_wait_center');
                            }, function() {
                                // One or more ajax requests failed. The ajax error handler already displayed an error msg, so just clean up
                                onUsersDeleted();
                                imgWait.removeClass('gsp_wait_center');
                            });
                        });
                    } else {
                        $(".gsp_checkboxOptions a", $("#gsp_optionsDtl")).removeAttr("href").addClass("gsp_disabled").unbind("click");
                    }
                });
            };

            var usersAccordianOnBeforeActivate = function (event, ui) {
                $(".gsp_waitCtr", $(ui.newHeader)).addClass('gsp_wait_center');
                $(ui.oldPanel).children().remove();
                if (ui.newHeader.length > 0) {
                    bindUserData(ui.newHeader.data("username"), ui.newHeader, ui.newPanel);
                }

                $("input:button", ui.oldHeader).unbind("click").hide();
            };

            var usersAccordianOnActivate = function (event, ui) {
            };

            var usersAccordianOnCreate = function (event, ui) {
                $("input:button", $(this)).button();
                $(this).data("isAccordion", true);
            };

            var setUserCheckboxVisibility = function (isVisible) {
                // Show checkbox next to each user if the 'show checkbox' option is selected
                $(".gsp_chkUser:gt(0)", $userHtmlCtr).toggle(isVisible);
            };

            var bindUserList = function (users) {

                if (users == null) {
                    if (gspUsers.length < <%= MaxNumberOfUsersToRender %>) {
                        users = Gs.Utils.deepCopy(gspUsers); // If no users have been specified default to full list
                    } else {
                        users = new Array();
                    }
                }

                // Add the 'Add new user' item
                users.splice(0, 0, '<%= Resources.GalleryServer.Admin_Manage_Users_Add_User_Link_Text %>');
                
                var tmplData = $("#<%= cid %>_userTmpl").render(users); // Generate the HTML from the template

                if ($userHtmlCtr.data("isAccordion")) {
                    $userHtmlCtr.accordion("destroy").data("isAccordion", false);
                }

                $userHtmlCtr.html(tmplData).show().accordion({
                    active: false,
                    animated: false,
                    collapsible: true,
                    heightStyle: "content", // content, auto, fill
                    create: usersAccordianOnCreate,
                    beforeActivate: usersAccordianOnBeforeActivate,
                    activate: usersAccordianOnActivate
                });

                setUserCheckboxVisibility($("#gsp_showCheckboxes").prop("checked"));

                $userHtmlCtr.find("input.gsp_chkUser:checkbox").click(function (e) { e.stopPropagation(); });
            };

            var bindUserData = function (userName, userHeader, userContent) {
                $.ajax(({
                    type: "GET",
                    url: Gs.Vars.AppRoot + '/api/users/getbyusername?userName=' + encodeURIComponent(userName) + '&galleryId=' + Gs.Vars['<%= cid %>'].gsData.Settings.GalleryId,
                    dataType: "json",
                success: function (user) {
                    bindUserDataReceived(user, userHeader, userContent);
                },
                error: function (response) {
                    Gs.Msg.show("Action Aborted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                },
                complete: function () {
                    $(".gsp_waitCtr", userHeader).removeClass('gsp_wait_center');
                }
            }));
        };
            
        var bindUserDataReceived = function (data, userHeader, userContent) {
            var $rolesEl;
            
            var generateHtmlFromTemplate = function () {
                var $tmpl = gspUserData.IsNew ? $("#<%= cid %>_userNewTmpl") : $("#<%= cid %>_userEditTmpl");
                var tmplData = $tmpl.render(gspUserData, { // Generate the HTML from the template
                    getDateAsFormattedString: function(dateValue) {
                        if (dateValue != null) {
                            return Globalize.format(new Date(dateValue), "MMMM dd, yyyy h:mm:ss tt (UTCzzz)");
                        } else return "";
                    },
                    isUserAlbumEnabled: function() {
                        return Gs.Vars['<%= cid %>'].gsData.Settings.EnableUserAlbum;
                    }
                });
                
                $(userContent).html(tmplData); // Add the HTML to the page
            };

            var bindRoles = function () {
                $rolesEl = $allRoles.clone().removeAttr('id').removeClass('gsp_invisible');

                // Select the roles that are assigned to this user.
                $.each(gspUserData.Roles, function(idx, roleName) {
                    $("option[value='" + roleName.replace(/\'/g, '\\\'') + "']", $rolesEl).prop('selected', true);
                });

                // Add HTML to page
                $rolesEl.appendTo($('.gsp_userRolesCell', $userHtmlCtr));
                
                // Now convert the roles list to a jQuery multi-select
                $rolesEl
                    .multiselect({
                        minWidth: Gs.Utils.isWidthLessThan(750) ? 300 : 750,
                        height: Gs.Utils.isWidthLessThan(750) ? 250 : 500,
                        header: '<input id="chkShowOwnerRoles" type="checkbox" /><label for="chkShowOwnerRoles"><%= ShowAlbumOwnerRolesLabel %></label>',
                        noneSelectedText: '&lt;No roles selected&gt;',
                        selectedList: 5,
                        classes: 'gsp_j_rolelist',
                        close: function() {
                            // Assign selected roles to hidden field
                            $("#<%= hdnUserRoles.ClientID %>").val(JSON.stringify($rolesEl.val()));
                        }
                    })
                    .multiselect('widget')
                    .appendTo($('#<%= cid %>')); // Move to .gsp_ns namespace so it'll inherit the jQuery UI CSS classes

                // Override the explicit width assigned by multiselect. For some reason when we say 94% it turns into the desired 100% 
                // when we look at it in the browser. We need a timeout to wait for the timeouts in the multiselect widget.
                setTimeout(function() { $rolesEl.next().width('94%'); },15);
             
                // Add click handler for the owner role toggle button
                $('#chkShowOwnerRoles').click(function() {
                    if ($(this).prop('checked'))
                        $(".gsp_j_rolelist .gsp_j_albumownerrole").fadeIn();
                    else
                        $(".gsp_j_rolelist .gsp_j_albumownerrole").fadeOut();
                });
                
                $('.gsp_j_rolelist .gsp_j_albumownerrole').hide(); // Hide owner roles by default
            };
                
            var saveUserData = function (e) {
                var updateUserDataFromHtml = function () {
                    if (gspUserData.IsNew) {
                        gspUserData.UserName = $('.gsp_mu_eu_n', $userHtmlCtr).val().trim();
                        
                        if (gspUserData.UserName.length == 0)
                            throw new Error("Enter a user name.");
                    } else {
                        gspUserData.Comment = $('.gsp_mu_eu_d', $userHtmlCtr).val();
                        gspUserData.IsApproved = $('.gsp_mu_eu_app', $userHtmlCtr).prop('checked');
                        
                        if (Gs.Vars['<%= cid %>'].gsData.Settings.EnableUserAlbum)
                        gspUserData.EnableUserAlbum = $('.gsp_mu_eu_ua', $userHtmlCtr).prop('checked');
                    }

                    if (gspUserData.IsNew || $('.gsp_mu_pwd_chg', $userHtmlCtr).prop('checked')) {
                        // This is either a new user or the admin is changing the pwd for an existing user. Grab the new pwd.
                        gspUserData.Password = $('.gsp_mu_eu_pwd1', $userHtmlCtr).val();
                         
                        if (gspUserData.Password.length == 0)
                            throw new Error("Enter a password.");
                    }
                    
                    if (!gspUserData.IsNew ) {
                        gspUserData.PasswordResetRequested = $('.gsp_mu_pwd_rst', $userHtmlCtr).prop('checked');
                        gspUserData.PasswordChangeRequested = $('.gsp_mu_pwd_chg', $userHtmlCtr).prop('checked');
                        gspUserData.NotifyUserOnPasswordChange = $('.gsp_mu_ntf_on_pwd_chg', $userHtmlCtr).prop('checked');
                        
                        if (gspUserData.PasswordChangeRequested && $('.gsp_mu_eu_pwd1', $userHtmlCtr).val() != $('.gsp_mu_eu_pwd2', $userHtmlCtr).val())
                            throw new Error("Passwords must match.");
                    }
                    
                    gspUserData.Email = $('.gsp_mu_eu_e', $userHtmlCtr).val();
                    gspUserData.Roles = $rolesEl.val() || [];
                };

                var onUserSaved = function (response) {
                    var msgOptions = { msgType: 'success' };

                    if (gspUserData.PasswordResetRequested)
                        msgOptions.autoCloseDelay = 0;
                    
                    Gs.Msg.show("Save Successful", response, msgOptions);
                    
                    if (gspUserData.IsNew) {
                        gspUserData.IsNew = false;
                        gspUsers.splice(1, 0, gspUserData.UserName);

                        if (gspUsers.length < <%= MaxNumberOfUsersToRender %>) {
                            bindUserList();
                        } else {
                            var users = new Array();
                            users.push(gspUserData.UserName);
                            bindUserList(users);
                        }
                        
                        $('.ui-accordion-header[data-username="' + gspUserData.UserName.replace(/\"/g, '\\"').replace(' ', '\\ ') + '"]').click();
                        $("#gsp_usr_srch").val('');
                    }
                    else
                        bindExistingUser();

                    gspUserData.PasswordResetRequested = false;
                    gspUserData.PasswordChangeRequested = false;
                };

                var imgWait = $(".ui-accordion-header.ui-state-active .gsp_waitCtr", $userHtmlCtr).addClass('gsp_wait_center');

                try {
                    updateUserDataFromHtml();

                    // Send role data to the server for saving
                    return $.ajax({
                        type: "POST",
                        url: Gs.Vars.AppRoot + '/api/users/post',
                        data: JSON.stringify(gspUserData),
                        contentType: "application/json; charset=utf-8"
                    })
                    .done(function(response) {
                        onUserSaved(response);						
                    })
                    .fail(function(response) {
                        Gs.Msg.show("Action Aborted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });					
                    })
                    .always(function() {
                        imgWait.removeClass('gsp_wait_center');					
                    });
                } catch (ex) {
                    Gs.Msg.show("Action Aborted", ex.message, { msgType: 'error', autoCloseDelay: 0 });
                    imgWait.removeClass('gsp_wait_center');
                }
            };

            var userDeleteClick = function (e) {
                var deleteUser = function (user) {

                    var onUserDeleted = function (response) {
                        Gs.Msg.show("User Deleted", response, { msgType: 'success', show: 'none' });

                        gspUsers = $.grep(gspUsers, function (value) {
                            return value != user.UserName;
                        });

                        bindUserList();
                        $('#gsp_usr_srch').val('').autocomplete('option', 'source', gspUsers).focus();
                    };

                    var imgWait = $(".gsp_waitCtr", userHeader).addClass('gsp_wait_center');

                    $.ajax(({
                        type: "DELETE",
                        url: Gs.Vars.AppRoot + '/api/users/deletebyusername?userName=' + encodeURIComponent(gspUserData.UserName),
                        success: function(response) {
                            onUserDeleted(response);
                        },
                        error: function (response) {
                            Gs.Msg.show("Action Aborted", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                        },
                        complete: function() {
                            imgWait.removeClass('gsp_wait_center');
                        }
                    }));
                };

                if (confirm("Are you sure you want to delete the user '" + gspUserData.UserName + "'?")) {
                    deleteUser(gspUserData);
                }

                $(this).removeClass("ui-state-focus");
                return false;
            };
            
            var bindPwdInputs = function() {
                $('.gsp_mu_eu_pwd1, .gsp_mu_eu_pwd2', $userHtmlCtr).change(function() {
                    var $pwd1 = $('.gsp_mu_eu_pwd1', $userHtmlCtr); 
                    var $pwd2 = $('.gsp_mu_eu_pwd2', $userHtmlCtr);
                    var pwd1 = $pwd1.val();
                    var pwd2 = $pwd2.val();
                    
                    $pwd1.next('.gsp_msgwarning').remove();
                    
                    if (pwd1.length > 0 && pwd2.length > 0 && pwd1 !== pwd2)
                        $pwd1.after('<span class="gsp_addleftpadding1 gsp_msgwarning">Passwords must match</span>');
                });
            };
                
            var bindNewUser = function() {
                // Configure the new user form
                bindPwdInputs();
                
                $('.gsp_mu_eu_n', $userHtmlCtr).change(function() {
                    var $userName = $(this);
                    $.ajax(({
                        type: "GET",
                        url: Gs.Vars.AppRoot + '/api/users/exists?userName=' + encodeURIComponent($userName.val()),
                        success: function (userExists) {
                            $userName.next('.gsp_mu_eu_n_msg').remove();
                            if (userExists)
                                $userName.after('<span class="gsp_addleftpadding1 gsp_mu_eu_n_msg gsp_msgwarning"><%= Resources.GalleryServer.Site_UserNameIsDuplicate %></span>');
                            else
                                $userName.after('<span class="gsp_addleftpadding1 gsp_mu_eu_n_msg gsp_msgfriendly"><%= Resources.GalleryServer.Site_UserNameAvailable %></span>');
                        }
                    }));
                }).focus();
            };
            
        var bindExistingUser = function() {
                // Configure the edit user form
                var bindPwdTab = function() {
                    var setChgPwdDtlEnabled = function(isEnabled) {
                        $('.gsp_mu_pwd_chg_dtl').toggleClass('gsp_disabledtext', !isEnabled);
                        $('.gsp_mu_pwd_chg_dtl input').prop('disabled', !isEnabled);
                    };

                    var setRstPwdDtlEnabled = function(isEnabled) {
                        $('.gsp_mu_pwd_rst_dtl').toggleClass('gsp_disabledtext', !isEnabled);
                    };

                    var setChgPwdEnabled = function(isEnabled) {
                        $('.gsp_mu_pwd_chg').prop('disabled', !isEnabled).next('label').toggleClass('gsp_disabledtext', !isEnabled);
                    };

                    var setRstPwdEnabled = function(isEnabled) {
                        $('.gsp_mu_pwd_rst').prop('disabled', !isEnabled).next('label').toggleClass('gsp_disabledtext', !isEnabled);
                    };

                    // Alert admin when there is an incompatible membership setting in web.config
                    var showPwdMsg = function() {          
                        var needToShowMsg = false;
                        var msg = "<section class='gsp_mu_pwd_msg gsp_msgfriendly'><p>ATTENTION</p><ul>";
                        
                        if (userSettings.requiresQuestionAndAnswer) // Can't change or reset password
                        {
                            setChgPwdEnabled(false);
                            setRstPwdEnabled(false);
                            msg += '<li><%= QuestionAnswerEnabledMsg %></li>';
                            needToShowMsg = true;
                        }
                        else
                        {
                            if (!userSettings.enablePasswordReset) // Can't reset password
                            {
                                setRstPwdEnabled(false);
                                msg += '<li><%= PwdResetDisabledMsg %></li>';
                                needToShowMsg = true;
                            }

                            if (!userSettings.enablePasswordRetrieval) // Can't change password
                            {
                                setChgPwdEnabled(false);
                                msg += '<li><%= PwdRetrievalDisabledMsg %></li>';
                                needToShowMsg = true;
                            }
                        }

                        msg += "</ul></section>";
            
                        if (needToShowMsg)
                            $userHtmlCtr.find('.gsp_mu_pwd_msg').remove().end().find('.gsp_mu_pwd_tab').prepend(msg);
                    };

                    // When 'reset pwd' is checked, enable child elements and disable change pwd
                    $('.gsp_mu_pwd_rst', $userHtmlCtr).change(function() {
                        var isRstPwdChkd = $(this).prop('checked');

                        if (isRstPwdChkd) {
                            $('.gsp_mu_pwd_chg', $userHtmlCtr).prop('checked', false);
                            setChgPwdDtlEnabled(false);
                        }
                        setRstPwdDtlEnabled(isRstPwdChkd);
                    });

                    // When 'change pwd' is checked, enable child elements and disable reset pwd
                    $('.gsp_mu_pwd_chg', $userHtmlCtr).change(function() {
                        var isChgPwdChkd = $(this).prop('checked');

                        if (isChgPwdChkd) {
                            $('.gsp_mu_pwd_rst', $userHtmlCtr).prop('checked', false);
                            setRstPwdDtlEnabled(false);
                        }
                        setChgPwdDtlEnabled(isChgPwdChkd);
                    });

                    // Enable/disable the 'notify user' checkbox based on whether a valid email exists
                    $('.gsp_mu_eu_e', $userHtmlCtr).change(function() {
                        if (!Gs.Utils.hasFormValidation())
                            return; // Browser doesn't support form validation, so just leave the checkbox enabled.
                        
                        if ($(this).val().length > 0 && $(this)[0].checkValidity())
                            $('.gsp_mu_ntf_on_pwd_chg', $userHtmlCtr).prop('disabled', false).next('label').removeClass('gsp_disabledtext').text('E-mail new password to user');
                        else
                            $('.gsp_mu_ntf_on_pwd_chg', $userHtmlCtr).prop('disabled', true).next('label').addClass('gsp_disabledtext').text('E-mail new password to user (Disabled because user does not have an e-mail)');
                    }).change();

                    setRstPwdDtlEnabled(false);
                    setChgPwdDtlEnabled(false);
                    bindPwdInputs();
                    showPwdMsg();
                };

                var bindGeneralTab = function() {
                    $('.gsp_mu_usr_unlck', $userHtmlCtr).click(function() {
                        // Unlock the user
                        gspUserData.IsLockedOut = false;
                        var unlockBtn = this;
                        saveUserData().done(function() {
                            $(unlockBtn).closest("tr").find("td.gsp_col2").html("<img src='<%= Utils.GetSkinnedUrl("/images/ok-s.png") %>' alt='Success icon' style='vertical-align:top;' /><span class='gsp_msgfriendly'> User has been unlocked.</span>").end().delay(3000).fadeOut("slow"); // Delete message about locked user
                        });
                });
            };

                bindGeneralTab();
                bindPwdTab();
            };

            gspUserData = data;
            generateHtmlFromTemplate();
            $("div:first", userContent).tabs().show();
            bindRoles();
                
            $("input.gsp_userSaveBtn", userHeader).show().click(function () {
                saveUserData();
                return false;
            });

            $("input.gsp_userDeleteBtn", userHeader).show().click(userDeleteClick);

            if (gspUserData.IsNew)
                bindNewUser();
            else
                bindExistingUser();
        };
        
        var configTooltips = function () {
            $('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
                title: '<%= Resources.GalleryServer.Admin_Manage_Users_Overview_Hdr.JsEncode() %>',
                content: '<%= Resources.GalleryServer.Admin_Manage_Users_Overview_Bdy.JsEncode() %>'
            });
        };

        })(jQuery);
    </script>

</asp:PlaceHolder>
