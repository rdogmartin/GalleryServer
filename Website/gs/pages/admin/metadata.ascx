<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="metadata.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.metadata" %>
<%@ Import Namespace="GalleryServer.Web" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
<div class="gsp_content">
    <p class="gsp_a_ap_to">
        <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
            EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server" EnableViewState="false" />
    </p>
    <asp:PlaceHolder ID="phAdminHeader" runat="server" />
    <div class="gsp_addleftpadding5" runat="server">
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Admin_Metadata_Options_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p class="gsp_addtopmargin5">
                    <asp:CheckBox ID="chkExtractMetadata" runat="server" Text="<%$ Resources:GalleryServer, Admin_Metadata_ExtractMetadata_Label %>" />
                </p>
                <p class="gsp_addleftmargin5">
                    <asp:CheckBox ID="chkExtractMetadataUsingWpf" runat="server" Text="<%$ Resources:GalleryServer, Admin_Metadata_ExtractMetadataUsingWpf_Label %>" />
                </p>
                <p>
                    <asp:Label ID="lblDateTimeFormatString" runat="server" Text="<%$ Resources:GalleryServer, Admin_Metadata_DateTimeFormatString_Label %>" />
                    <asp:TextBox ID="txtDateTimeFormatString" runat="server" CssClass="gsp_textbox" />
                </p>
            </div>
        </div>

        <div class="gsp_single_tab" style="max-width: 100%;">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Label ID="lblMetadata" runat="server" Text="<%$ Resources:GalleryServer, Admin_Metadata_Display_Settings_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <asp:HiddenField ID="hdnMetadataDefinitions" runat="server" ClientIDMode="Static" />
                <div id="<%= cid %>_mdOptions" class="mdOptions"></div>
            </div>
        </div>
    </div>
    <tis:wwDataBinder ID="wwDataBinder" runat="server" OnValidateControl="wwDataBinder_ValidateControl"
        OnBeforeUnbindControl="wwDataBinder_BeforeUnbindControl">
        <DataBindingItems>
            <tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="ExtractMetadata" ControlId="chkExtractMetadata" BindingProperty="Checked"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Metadata_ExtractMetadata_Label %>" />
            <tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="ExtractMetadataUsingWpf" ControlId="chkExtractMetadataUsingWpf"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_Metadata_ExtractMetadataUsingWpf_Label %>" />
            <tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MetadataDateTimeFormatString" ControlId="txtDateTimeFormatString"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Metadata_DateTimeFormatString_Label %>" />
        </DataBindingItems>
    </tis:wwDataBinder>
    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
    <script>
        (function ($) {
            var grid;
            
            $(document).ready(function () {
                bindData();
                bindEventHandlers();
                configUi();
                configTooltips();
            });
            
            var bindEventHandlers = function () {
                $("#<%= chkExtractMetadata.ClientID %>").click(updateUi);
                        
                $('form:first').submit(function(e) {
                    // Serialize the metadata definitions and store in hidden field,
                    // where it will be accessed by server code after form submission.

                    // Move out of active cell to force update of data model. Without this currently edited cell changes would be lost.
                    grid.navigateRight();

                    var metaDefData = grid.getData();
                
                    // Update sequence to reflect current ordering of array (which may have been reordered by user)
                    var seq = 0;
                    $.each(metaDefData, function(n, v) { v.Sequence = seq++; });
                    
                    var serializedStr = JSON.stringify(metaDefData);
                    $("#hdnMetadataDefinitions").val(serializedStr);
                    
                    return true;
                });
                
                $(".gsp_m_write_btn", $('#<%= cid %>')).click(function (e) {
                    // User clicked the write metadata button. Send ajax request.
                    if (Gs.Vars['<%= cid %>'].gsData.Settings.IsReadOnlyGallery) {
                        Gs.Msg.show("Metadata file writing canceled", "You cannot modify the original media files when the gallery is read only. This setting is configured on the Media Settings page.", { msgType: 'info', autoCloseDelay: 0 });
                        return;
                    }

                    $(e.currentTarget).addClass('gsp_wait'); // Show wait animated gif
                    
                    var metaNameId = $(e.currentTarget).data("id");
                    var galleryId = Gs.Vars['<%= cid %>'].gsData.Settings.GalleryId;
                    var urlParms = "metaNameId=" + metaNameId + "&galleryId=" + galleryId;
                    
                    $.ajax({
                        type: 'POST',
                        url: Gs.Vars.AppRoot + '/api/meta/writemetaitem/?' + urlParms,
                    })
                    .done(function (x, y, z) {
                        var msg = "The writing of metadata item '" + metaNameId + "' to the media files has been started on a background thread. The event log will contain additional details of its progress.";
                        Gs.Msg.show("Metadata writing initiated", msg, { autoCloseDelay: 10000 });
                    })
                    .always(function() {
                        $(e.currentTarget).removeClass('gsp_wait');
                    })
                    .fail(function (response) {
                        Gs.Msg.show("Ajax Error", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                    });
                });
                
                $(".gsp_m_extract_btn", $('#<%= cid %>')).click(function (e) {
                    // User clicked the extract metadata button. Send ajax request.
                    $(e.currentTarget).addClass('gsp_wait'); // Show wait animated gif
                    
                    var metaNameId = $(e.currentTarget).data("id");
                    var galleryId = Gs.Vars['<%= cid %>'].gsData.Settings.GalleryId;
                    var urlParms = "metaNameId=" + metaNameId + "&galleryId=" + galleryId;
                    
                    $.ajax({
                        type: 'POST',
                        url: Gs.Vars.AppRoot + '/api/meta/rebuildmetaitem/?' + urlParms,
                    })
                    .done(function (x, y, z) {
                        var msg = "The extraction of metadata item '" + metaNameId + "' from the media files has been started on a background thread. The event log will contain additional details of its progress.";
                        Gs.Msg.show("Metadata extraction initiated", msg, { autoCloseDelay: 10000 });
                    })
                    .always(function() {
                        $(e.currentTarget).removeClass('gsp_wait');
                    })
                    .fail(function (response) {
                        Gs.Msg.show("Ajax Error", response.responseText, { msgType: 'error', autoCloseDelay: 0 });
                    });
                });
            };

            var bindData = function() {
                // Configure the formatters needed by Slick.Grid.
                $.extend(true, window, {
                    "Slick": {
                        "Formatters": {

                            "MetadataSync": function (row, cell, value, columnDef, dataContext) {
                                // Note: The format() function is a custom function defined in gallery.ts.
                                var isWritable = (dataContext.PersistToFile == null || dataContext.PersistToFile);

                                var writeBtnHtml = "<a href='javascript:void(0)' data-id='{0}' class='{1} gs_adm_m_btn gsp_m_write_btn gs_rel' title='Write database metadata to the media files for all writable assets in the gallery'><span class='fa fa-lg fa-sign-out fa-rotate-270'></span></a>&nbsp;".format(value, isWritable ? "" : "gsp_hide");

                                var extractBtnHtml = "<a href='javascript:void(0)' data-id='" + value + "' class='gs_adm_m_btn gsp_m_extract_btn' title='Re-extract metadata for all assets in the gallery'><span class='fa fa-lg fa-sign-in fa-rotate-90'></span></a>";

                                return writeBtnHtml + extractBtnHtml;
                            },

                            "NullableCheckmark": function (row, cell, value, columnDef, dataContext) {
                                if (value == null)
                                    return '<input type="checkbox" name="" class="gsp_invisible" value="' + value + '" /><span class="fa fa-lg gsp_msgfriendly"></span>';
                                else if (value)
                                    return '<input type="checkbox" name="" class="gsp_invisible" value="' + value + '" checked /><span class="fa fa-lg fa-check-circle gsp_msgfriendly"></span>';
                                else
                                    return '<span class="fa fa-lg fa-minus-circle" title="This property cannot be written to the media file"></span>';
                            },

                            "GsCheckmark": function (row, cell, value, columnDef, dataContext) {
                                if (value)
                                    return '<input type="checkbox" name="" class="gsp_invisible" value="' + value + '" checked /><span class="fa fa-lg fa-check-circle gsp_msgfriendly"></span>';
                                else
                                    return '<input type="checkbox" name="" class="gsp_invisible" value="' + value + '" /><span class="fa fa-lg gsp_msgfriendly"></span>';
                            },

                            "GsEditMode": function (row, cell, value, columnDef, dataContext) {
                                switch (value) {
                                    case 2:
                                        return '<span class="fa fa-lg fa-check-circle gsp_msgfriendly" title="Editing enabled"></span><span class="fa fa-fw fa-lg fa-pencil gsp_addleftpadding4 gs_edit_icon" title="Plain text editor"></span>';
                                    case 3:
                                        return '<span class="fa fa-lg fa-check-circle gsp_msgfriendly" title="Editing enabled"></span><span class="fa fa-fw fa-lg fa-code gsp_addleftpadding4 gs_edit_icon" title="HTML editor"></span>';
                                    default:
                                        return '<span class="fa fa-lg fa-check-circle gsp_msgfriendly gsp_invisible" title="Editing enabled"></span><span class="fa fa-fw fa-lg fa-pencil gsp_addleftpadding4 gsp_invisible gs_edit_icon"></span>';
                                }
                            }
                        }
                    }
                });

                var configureRowDragDrop = function () {
                    // The code in this function was copied from the example at
                    // http://mleibman.github.com/SlickGrid/examples/example9-row-reordering.html
                    grid.setSelectionModel(new Slick.RowSelectionModel());

                    var moveRowsPlugin = new Slick.RowMoveManager();

                    moveRowsPlugin.onBeforeMoveRows.subscribe(function (e, thedata) {
                        for (var i = 0; i < thedata.rows.length; i++) {
                            // no point in moving before or after itself
                            if (thedata.rows[i] == thedata.insertBefore || thedata.rows[i] == thedata.insertBefore - 1) {
                                e.stopPropagation();
                                return false;
                            }
                        }
                        return true;
                    });

                    moveRowsPlugin.onMoveRows.subscribe(function (e, args) {
                        var extractedRows = [], left, right;
                        var rows = args.rows;
                        var insertBefore = args.insertBefore;
                        left = data.slice(0, insertBefore);
                        right = data.slice(insertBefore, data.length);

                        rows.sort(function (a, b) { return a - b; });

                        for (var i = 0; i < rows.length; i++) {
                            extractedRows.push(data[rows[i]]);
                        }

                        rows.reverse();

                        for (var i = 0; i < rows.length; i++) {
                            var row = rows[i];
                            if (row < insertBefore) {
                                left.splice(row, 1);
                            } else {
                                right.splice(row - insertBefore, 1);
                            }
                        }

                        data = left.concat(extractedRows.concat(right));

                        var selectedRows = [];
                        for (var i = 0; i < rows.length; i++)
                            selectedRows.push(left.length + i);

                        grid.resetActiveCell();
                        grid.setData(data);
                        grid.setSelectedRows(selectedRows);
                        grid.render();
                    });

                    grid.registerPlugin(moveRowsPlugin);
                };

                var configureCheckboxHandler = function() {
                    // When user clicks a cell containing a checkbox, toggle the corresponding data value and update CSS to reflect new value.
                    $('#<%= cid %>_mdOptions').on('click', '.slick-cell:has(input[type=checkbox])', function (e) {
                        var cell = grid.getCellFromEvent(e);
                        var column = grid.getColumns()[cell.cell];

                        if (column['editable'] === false || column['autoEdit'] === false)
                            return;

                        var newValue = !data[cell.row][column.field];

                        data[cell.row][column.field] = newValue;

                        $('input', this).prop('checked', newValue);

                        $('.fa', this).toggleClass('fa-check-circle');
                    });
                };

                var configureEditModeHandler = function() {
                    // When user clicks the 'editable' cell, cycle to the next edit mode value and update CSS to reflect new value.
                    $('#<%= cid %>_mdOptions').on('click', '.slick-cell:has(.gs_edit_icon)', function (e) {
                        var cell = grid.getCellFromEvent(e);
                        var column = grid.getColumns()[cell.cell];

                        if (column['editable'] === false || column['autoEdit'] === false)
                            return;

                        // userEditMode maps to enum PropertyEditorMode
                        var userEditMode = data[cell.row][column.field] + 1;

                        if (userEditMode > 3) {
                            userEditMode = 1;
                        }

                        data[cell.row][column.field] = userEditMode;

                        switch (userEditMode) {
                            case 2:
                                $('.fa', this).removeClass('gsp_invisible fa-code');
                                $('.fa-lock', this).addClass('gsp_invisible');
                                $('.gs_edit_icon', this).addClass('fa-pencil').attr('title', 'Plain text editor');
                                break;

                            case 3:
                                $('.fa', this).removeClass('gsp_invisible fa-pencil');
                                $('.gs_edit_icon', this).addClass('fa-code').attr('title', 'HTML editor');
                                break;

                            default:
                                $('.gs_edit_icon', this).removeClass('fa-pencil fa-code').attr('title', '');
                                $('.fa', this).addClass('gsp_invisible');
                        }
                    });
                };
                    
                var columns = [
                    { id: "#", name: "&nbsp;", focusable: false, width: 40, behavior: "selectAndMove", resizable: false,cssClass: "cell-reorder dnd", headerCssClass:"md_col1 md_sort" },
                    { id: "MetadataItem", name: "ID", field: "MetadataItem", focusable: false, cssClass:"md_ctr", headerCssClass:"md_ctr md_id", toolTip:"The unique ID. This value can be used in templates to find specific items." },
                    { id: "Name", name: "Name", field: "Name", focusable: false, width: 200, toolTip:"Unique name of the item. These can be used in the default value template." },
                    { id: "DisplayName", name: "Display name", field: "DisplayName", width: 200, toolTip:"Text to display in UI", headerCssClass:"md_dname", editor: Slick.Editors.Text },
                    { id: "IsVisibleForAlbum", name: "Album", field: "IsVisibleForAlbum", focusable: false, width: 100, cssClass: "md_ctr", headerCssClass: "md_ctr md_album", toolTip: "Indicates whether to display the item for albums", formatter: Slick.Formatters.GsCheckmark },
                    { id: "IsVisibleForGalleryObject", name: "Media", field: "IsVisibleForGalleryObject", focusable: false, width: 110, cssClass: "md_ctr", headerCssClass: "md_ctr md_go", toolTip: "Indicates whether to display the item for media objects", formatter: Slick.Formatters.GsCheckmark },
                    { id: "DefaultValue", name: "Default value", field: "DefaultValue", width: 200, toolTip:"The template to use when adding a media object", headerCssClass:"md_dvalue", editor: Slick.Editors.Text },
                    { id: "EditMode", name: "Editable", field: "UserEditMode", focusable: false, width: 100, cssClass: "md_ctr", headerCssClass: "md_ctr md_isEditableHdr", toolTip: "Specifies whether the item is user-editable and whether the editor is plain text or HTML", formatter: Slick.Formatters.GsEditMode },
                    { id: "PersistToFile", name: "Write", field: "PersistToFile", focusable: false, width: 100, cssClass: "md_ctr", headerCssClass: "md_ctr md_persistHdr", toolTip: "Indicates whether to write user-entered values to the original file (JPG/JPEG only)", formatter: Slick.Formatters.NullableCheckmark },
                    { id: "Sync", name: "", field: "MetadataItem", focusable: false, cssClass: "md_ctr", headerCssClass: "md_ctr md_rebuild", formatter: Slick.Formatters.MetadataSync }
                ];

                var options = {
                    editable: true,
                    autoEdit: true,
                    enableRowReordering: true,
                    enableColumnReorder: false,
                    enableAddRow: false,
                    enableCellNavigation: true,
                    asyncEditorLoading: false,
                    autoHeight: true,
                    autoWidth:true
                };

                var data = JSON.parse($("#hdnMetadataDefinitions").val(), true);

                grid = new Slick.Grid("#<%= cid %>_mdOptions", data, columns, options);

                configureCheckboxHandler();

                configureEditModeHandler();

                configureRowDragDrop();
            };
            
            var configUi = function() {
                var extractMetadataSelected = $('#<%= chkExtractMetadata.ClientID %>').prop('checked');
                
                $('#<%= chkExtractMetadataUsingWpf.ClientID %>').prop('disabled', !extractMetadataSelected);

                // Add lock icon to the 'Write' header, but only when in trial mode or running the free license.
                var license = Gs.Vars['<%= cid %>'].gsData.App.License;

                if (license < 3 || license === 6) {
                    var lockIcon = "&nbsp;<span class='fa fa-lock gsp_gold' title='Requires Gallery Server Home &amp; Nonprofit or higher'></span>";
                    $('.md_persistHdr', '#<%= cid %>').append(lockIcon);
                }
            };

            var updateUi = function() {
                var extractMetadataSelected = $('#<%= chkExtractMetadata.ClientID %>').prop('checked');
                
                $('#<%= chkExtractMetadataUsingWpf.ClientID %>').prop('checked', extractMetadataSelected).prop('disabled', !extractMetadataSelected);
            };
            
            var configTooltips = function() {
                $('#<%= chkExtractMetadata.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_extractMetadata_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_extractMetadata_Bdy.JsEncode() %>'
                });

                $('#<%= chkExtractMetadataUsingWpf.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_extractMetadataUsingWpf_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_extractMetadataUsingWpf_Bdy.JsEncode() %>'
                });

                $('#<%= lblDateTimeFormatString.ClientID %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_metadataDateTimeFormatString_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_metadataDateTimeFormatString_Bdy.JsEncode() %>'
                });

                $('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
                    title: '<%= Resources.GalleryServer.Cfg_metadataDisplaySettings_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Cfg_metadataDisplaySettings_Bdy.JsEncode() %>'
                });

                $('.md_sort span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Sort_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Sort_Bdy.JsEncode() %>'
                });

                $('.md_id span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Id_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Id_Bdy.JsEncode() %>'
                });

                $('.md_dname span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_DisName_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_DisName_Bdy.JsEncode() %>'
                });

                $('.md_album span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Album_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Album_Bdy.JsEncode() %>'
                });

                $('.md_go span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Media_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Media_Bdy.JsEncode() %>'
                });

                $('.md_dvalue span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_DefValue_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_DefValue_Bdy.JsEncode() %>'
                });

                $('.md_isEditableHdr span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Editable_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Editable_Bdy.JsEncode() %>'
                });

                $('.md_persistHdr > span:last').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Writable_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Writable_Bdy.JsEncode() %>'
                });

                $('.md_rebuild span').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_WriteToFiles_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_WriteToFiles_Bdy.JsEncode() %>'
                });

                $('.md_rebuild .gsp_tt_tgr').gsTooltip({
                    title: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Rebuild_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_Metadata_Gd_Rebuild_Bdy.JsEncode() %>'
                });

            };
        })(jQuery);
    </script>
</asp:PlaceHolder>
