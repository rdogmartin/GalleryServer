<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="filetypes.ascx.cs" Inherits="GalleryServer.Web.gs.pages.admin.filetypes" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content">
    <asp:PlaceHolder ID="phAdminHeader" runat="server" />
    <div class="gsp_addleftpadding5" runat="server">
        <p><a href="javascript:void(0)" class="gs_mt_add_new">Add new...</a></p>

        <div class="gs_ft_add_dg" title="Add MIME Type">
            <fieldset>
                <p>
                    <label for="gs_ft_add_ext">File name extension:</label></p>
                <p>
                    <input type="text" name="gs_ft_add_ext" id="gs_ft_add_ext" value="" class="gs_ft_add_ext"></p>
                <p>
                    <label for="gs_ft_add_mime">MIME type:</label></p>
                <p>
                    <input type="text" name="gs_ft_add_mime" id="gs_ft_add_mime" value="" class="gs_ft_add_mime"></p>
            </fieldset>
        </div>

        <div id="<%= cid %>_mimeTypes" class="gs_mimeTypes"></div>

        <asp:HiddenField ID="hdnMimeTypes" runat="server" ClientIDMode="Static" />
        <asp:PlaceHolder ID="phAdminFooter" runat="server" />
    </div>
</div>

<asp:PlaceHolder runat="server">
    <script>
        (function ($) {

            var clientId = '<%= cid %>';;
            var $scope, grid, data;

            var bindData = function () {
                // Configure the formatters needed by Slick.Grid.

                var configureCheckboxHandler = function () {
                    // When user clicks a cell containing a checkbox, toggle the corresponding data value and update CSS to reflect new value.
                    $('.gs_mimeTypes', $scope).on('click', '.slick-cell:has(input[type=checkbox])', function (e) {
                        var cell = grid.getCellFromEvent(e);
                        var column = grid.getColumns()[cell.cell];

                        if (column['editable'] === false || column['autoEdit'] === false)
                            return;

                        var newValue = !data.MimeTypes[cell.row][column.field];

                        data.MimeTypes[cell.row][column.field] = newValue;

                        $('input', this).prop('checked', newValue);

                        var tooltip = newValue ? 'This file type is allowed in the gallery. Click to disable it.' : 'This file type is NOT allowed in the gallery. Click to enable it.';

                        $('.gs_ft_enabled_btn', this).toggleClass('fa-check-square-o gsp_msgfriendly fa-square-o').attr('title', tooltip);
                    });
                };

                var configureDeleteHandler = function () {
                    $('.gs_mimeTypes', $scope).on('click', '.fa-close,.fa-undo', function (e) {
                        var cell = grid.getCellFromEvent(e);
                        var mimeType = data.MimeTypes[cell.row];

                        var shouldDelete = $(e.currentTarget).hasClass('fa-close');

                        mimeType.IsDeleted = shouldDelete;
                        grid.invalidate();

                        e.preventDefault();
                        e.stopPropagation();
                    });
                };

                $.extend(true, window, {
                    "Slick": {
                        "Formatters": {

                            "TextWithDeleteStrikeThrough": function (row, cell, value, columnDef, dataContext) {
                                if (dataContext.IsDeleted) // dataContext is a MimeType
                                    return '<span class="gs_ft_text_lt gsp_msgattention">' + value + '</span>';

                                return value;
                            },

                            "GsCheckmark": function (row, cell, value, columnDef, dataContext) {
                                var enabledIconClasses = value ? 'fa-check-square-o gsp_msgfriendly' : 'fa-square-o';
                                if (dataContext.IsDeleted) {
                                    enabledIconClasses = 'fa-minus-square-o gsp_msgattention';
                                }

                                var deleteIconClass = dataContext.IsDeleted ? 'fa-undo' : 'fa-close gsp_msgattention';
                                var deleteIconTooltip = dataContext.IsDeleted ? 'Click to undo the deletion of this MIME type' : 'Click to delete this MIME type';

                                if (value)
                                    return '<span class="fa fa-lg ' + deleteIconClass + ' fa-btn" title="' + deleteIconTooltip + '"></span><input type="checkbox" name="" class="gsp_invisible" value="' + value + '" checked /><span class="fa fa-lg ' + enabledIconClasses + ' fa-btn gs_ft_enabled_btn" title="This file type is allowed in the gallery. Click to disable it."></span>';
                                else
                                    return '<span class="fa fa-lg ' + deleteIconClass + ' fa-btn" title="' + deleteIconTooltip + '"></span><input type="checkbox" name="" class="gsp_invisible" value="' + value + '" /><span class="fa fa-lg ' + enabledIconClasses + ' fa-btn gs_ft_enabled_btn" title="This file type is NOT allowed in the gallery. Click to enable it."></span>';
                            }
                        }
                    }
                });

                var columns = [
                    { id: "Enabled", name: "<span class='fa fa-lg fa-close gsp_hide'></span><input type='checkbox' class='gsp_hide' /><input id='gs_mt_enabled' type='checkbox' class='gs_use_font gs_mt_enabled_hdr_chk' /><label for='gs_mt_enabled'></label>", field: "Enabled", focusable: false, width: 100, cssClass: "mt_ctr", headerCssClass: "mt_col1 mt_ctr gs_mt_hdr_enabled", toolTip: "Click to enable/disable all file types", formatter: Slick.Formatters.GsCheckmark },
                    { id: "Extension", name: "File Extension", field: "Extension", width: 125, focusable: false, cssClass: "mt_ctr", headerCssClass: "mt_ctr gs_mt_hdr_extension", toolTip: "The file extension", formatter: Slick.Formatters.TextWithDeleteStrikeThrough },
                    { id: "FullType", name: "MIME Type", field: "FullType", width: 400, cssClass: "gs_mt_mime_type", editor: Slick.Editors.Text, formatter: Slick.Formatters.TextWithDeleteStrikeThrough }
                ];

                var options = {
                    editable: true,
                    autoEdit: true,
                    enableRowReordering: false,
                    enableColumnReorder: false,
                    enableAddRow: false,
                    enableCellNavigation: true,
                    asyncEditorLoading: false,
                    autoHeight: true,
                    autoWidth: true
                };

                data = { MimeTypes: JSON.parse($("#hdnMimeTypes").val()) };

                grid = new Slick.Grid("#" + clientId + "_mimeTypes", data.MimeTypes, columns, options);

                configureCheckboxHandler();
                configureDeleteHandler();
            };

            var bindEventHandlers = function () {

                $('form:first').submit(function (e) {
                    // Serialize the metadata definitions and store in hidden field,
                    // where it will be accessed by server code after form submission.

                    // Move out of active cell to force update of data model. Without this currently edited cell changes would be lost.
                    grid.navigateRight();

                    var mimeTypeData = grid.getData(); // MimeType[]

                    var serializedStr = JSON.stringify(mimeTypeData);
                    $("#hdnMimeTypes").val(serializedStr);

                    return true;
                });

                $('.gs_mt_enabled_hdr_chk', $scope).click(function (e) {
                    // Update all checkboxes to match
                    var isChecked = $(e.currentTarget).prop('checked');
                    var mimeTypeData = grid.getData(); // MimeType[]

                    $.each(mimeTypeData, function (i, mt) { mt.Enabled = isChecked; });

                    grid.invalidate();
                });

                $('.gs_mt_add_new', $scope).click(function (e) {

                    var $dg = $(".gs_ft_add_dg", $scope).dialog({
                        appendTo: '#' + clientId,
                        autoOpen: true,
                        modal: true,
                        closeOnEscape: true,
                        buttons: {
                            OK: function () {
                                // Add MIME type to grid
                                var extension = $('.gs_ft_add_ext', $dg).val();
                                if (extension.charAt(0) !== '.') {
                                    extension = '.' + extension; // Prepend with . if not already there
                                }

                                // Look for existing MIME type with the extension the user entered
                                var existingMimeType = $.grep(data.MimeTypes, function (mt) { return mt.Extension === extension; })[0];

                                if (existingMimeType == null) {
                                    // Add to data array and force grid to redraw
                                    data.MimeTypes.unshift({ Extension: extension, FullType: $('.gs_ft_add_mime', $dg).val(), Enabled: true, Id: Gs.Constants.IntMinValue });
                                    grid.invalidate();
                                } else {
                                    // File extension is already in the list
                                    Gs.Msg.show('Action Aborted', 'The file extension ' + extension + ' is already in the table.', { msgType: 'error', autoCloseDelay: 0 });
                                }

                                $dg.dialog("close");
                            },
                            Cancel: function () {
                                $dg.dialog("close");
                            }
                        },
                        close: function () {
                            $('input[type=text]', $scope).val('');
                        }
                    });

                    $(".gs_ft_add_dg", $scope).keydown(function (e1) {
                        if (e1.keyCode === 13) {
                            $(e1.currentTarget).parent().find(".ui-dialog-buttonpane button:first").trigger("click");
                            return false;
                        }
                        return true;
                    });
                });
            };

            var configTooltips = function () {
                $('.gsp_admin_h2_txt', '#<%= cid %>').gsTooltip({
                      title: '<%= Resources.GalleryServer.Admin_MimeTypes_Overview_Hdr.JsEncode() %>',
                    content: '<%= Resources.GalleryServer.Admin_MimeTypes_Overview_Bdy.JsEncode() %>'
                });
              };

            $(document).ready(function () {
                $scope = $('#' + clientId);

                bindData();
                bindEventHandlers();
                configTooltips();
            });
        })(jQuery);
    </script>
</asp:PlaceHolder>
