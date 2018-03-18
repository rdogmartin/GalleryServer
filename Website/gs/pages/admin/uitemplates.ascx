<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="uitemplates.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.uitemplates" %>
<%@ Import Namespace="GalleryServer.Web" %>
<%@ Register Src="../../controls/albumtreeview.ascx" TagName="albumtreeview" TagPrefix="uc1" %>
<div class="gsp_content" runat="server">
    <p class="gsp_a_ap_to">
        <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
            EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server" EnableViewState="false" />
    </p>
    <asp:PlaceHolder ID="phAdminHeader" runat="server" />
    <div class="gsp_addleftpadding5">
        <p class="">
            Gallery item:&nbsp;
            <asp:DropDownList ID="ddlGalleryItem" runat="server" AutoPostBack="True" OnSelectedIndexChanged="ddlGalleryItem_SelectedIndexChanged" CssClass="gsp_mt_b_slt" />
            <span>Name:</span>&nbsp;<asp:DropDownList ID="ddlTemplateName" runat="server" AutoPostBack="True"
                OnSelectedIndexChanged="ddlTemplateName_SelectedIndexChanged" CssClass="gsp_mt_b_slt" />
            &nbsp;
            <asp:LinkButton ID="lbCreate" runat="server" Text="Copy as new..." ToolTip="Create a new template from the current one"
                OnClick="lbCreate_Click" />
        </p>
        <hr style="max-width: 1000px;" />
        <p class="gsp_addtopmargin10" style="margin-bottom: 10px;">
            Name:
            <asp:TextBox ID="txtTemplateName" runat="server" CssClass="gsp_textbox" />&nbsp;<asp:Button
                ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" ToolTip="Save the template" />&nbsp;<asp:Button
                    ID="btnCancel" runat="server" Text="Cancel" OnClick="btnCancel_Click" ToolTip="Cancel changes" />&nbsp;<asp:Button
                        ID="btnDelete" runat="server" Text="Delete" OnClick="btnDelete_Click" ToolTip="Delete this template"
                        OnClientClick="return confirm('Are you sure you want to delete this template?');" />
        </p>
        <div id="<%= cid %>_tmplTabContainer" class="gsp_tabContainer">
            <ul>
                <li><a href="#<%= cid %>_tmplTabHtml">HTML</a></li>
                <li><a href="#<%= cid %>_tmplTabScript">JavaScript</a></li>
                <li><a href="#<%= cid %>_tmplTabAlbums">Target Albums</a></li>
                <li><a href="#<%= cid %>_tmplTabPreview">Preview</a></li>
            </ul>
            <div id="<%= cid %>_tmplTabHtml" class="ui-corner-all">
                <p>
                    <span class="gsp_uit_h">HTML:</span>
                </p>
                <div>
                    <asp:TextBox ID="txtTemplate" runat="server" TextMode="MultiLine" Rows="20" CssClass="gsp_a_tmpl_html_txtarea" />
                </div>
            </div>
            <div id="<%= cid %>_tmplTabScript" class="ui-corner-all">
                <p>
                    <span class="gsp_uit_js">JavaScript:</span>
                </p>
                <p>
                    <asp:TextBox ID="txtScript" runat="server" TextMode="MultiLine" Rows="20" CssClass="gsp_a_tmpl_script_txtarea" />
                </p>
            </div>
            <div id="<%= cid %>_tmplTabAlbums" class="ui-corner-all">
                <p>
                    <span class="gsp_uit_alb">Target Albums:</span>
                </p>
                <uc1:albumtreeview ID="tvUC" runat="server" AllowMultiCheck="true" />
            </div>
            <div id="<%= cid %>_tmplTabPreview" class="ui-corner-all gsp_tmpl_output_ctr">
            </div>
        </div>
    </div>
    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
    <script>
        (function ($) {
            jQuery(document).ready(function () {
                configControls();
                configTooltips();
            });
         
            var configControls = function () {
                if (!Gs.Vars.IsPostBack) {
                    Gs.Vars.Cookies.remove('<%= SelectedTabCookieName %>');
                }

                var $selectedTabIndex = $("#<%= cid %>_tmplTabContainer").tabs({
                    active: (Gs.Vars.Cookies.get('<%= SelectedTabCookieName %>') || 0),
                    activate: function(e, ui) {
                        Gs.Vars.Cookies.set('<%= SelectedTabCookieName %>', ui.newTab.index());
                    
                        if (ui.newTab.index() == 3) {
                            renderPreview(); // User selected preview tab.
                        }
                    }
                })
                    .show()
                    .tabs('option', 'active');
                
                if ($selectedTabIndex == 3) {
                    renderPreview(); // Preview tab is selected, so render template.
                }

                $(".gsp_a_tmpl_html_txtarea,.gsp_a_tmpl_script_txtarea").textareafullscreen();
            };
            
            var renderPreview = function() {
                // Compile the HTML and javascript templates
                $.templates({
                    <%= cid %>_a_tmpl_html : $("#<%= cid %>_tmplTabHtml .gsp_a_tmpl_html_txtarea").val(),
                    <%= cid %>_a_tmpl_script : $("#<%= cid %>_tmplTabScript .gsp_a_tmpl_script_txtarea").val()
                });

                prepareDataForPreview();

                // Pass the script template to the renderer and then execute. It is expected the script contains code for
                // rendering the HTML template.
                var script = $.render["<%= cid %>_a_tmpl_script"](Gs.Vars['<%= cid %>'].gsData);
                if (console) console.log(script); // Send to console (useful for debugging)
                (new Function(script))(); // Execute the script

                revertDataAfterPreview();
            };
            
            var prepareDataForPreview = function() {
                // Make a copy of the data (to be restored later) and update a few properties so that the template will
                // render correctly on the preview tab.
                $("#<%= cid %>").data("gsDataTmp", Gs.Utils.deepCopy(Gs.Vars['<%= cid %>'].gsData));
                Gs.Vars['<%= cid %>'].gsData.Settings.HeaderTmplName = '<%= cid %>_a_tmpl_html';
                Gs.Vars['<%= cid %>'].gsData.Settings.HeaderClientId = '<%= cid %>_tmplTabPreview';
                Gs.Vars['<%= cid %>'].gsData.Settings.ThumbnailTmplName = '<%= cid %>_a_tmpl_html';
                Gs.Vars['<%= cid %>'].gsData.Settings.ThumbnailClientId = '<%= cid %>_tmplTabPreview';
                Gs.Vars['<%= cid %>'].gsData.Settings.MediaTmplName = '<%= cid %>_a_tmpl_html';
                Gs.Vars['<%= cid %>'].gsData.Settings.MediaClientId = '<%= cid %>_tmplTabPreview';
                Gs.Vars['<%= cid %>'].gsData.Settings.LeftPaneTmplName = '<%= cid %>_a_tmpl_html';
                Gs.Vars['<%= cid %>'].gsData.Settings.LeftPaneClientId = '<%= cid %>_tmplTabPreview';
                Gs.Vars['<%= cid %>'].gsData.Settings.RightPaneTmplName = '<%= cid %>_a_tmpl_html';
                Gs.Vars['<%= cid %>'].gsData.Settings.RightPaneClientId = '<%= cid %>_tmplTabPreview';
            };

            var revertDataAfterPreview = function() {
                // Restore data that we modified in prepareDataForPreview()
                Gs.Vars['<%= cid %>'].gsData = $("#<%= cid %>").data("gsDataTmp");
            };

            var configTooltips = function() {
                $('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_UiTemplate_Overview_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_UiTemplate_Overview_Bdy.JsEncode() %>'
                });
                
                $('.gsp_uit_h', '#<%= cid %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_UiTemplate_Tmpl_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_UiTemplate_Tmpl_Bdy.JsEncode() %>'
                });
                
                $('.gsp_uit_js', '#<%= cid %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_UiTemplate_Tmpl_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_UiTemplate_Tmpl_Bdy.JsEncode() %>'
                });
                
                $('.gsp_uit_alb', '#<%= cid %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_UiTemplate_Target_Album_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_UiTemplate_Target_Album_Bdy.JsEncode() %>'
                });
            };

        })(jQuery);
    </script>
</asp:PlaceHolder>
