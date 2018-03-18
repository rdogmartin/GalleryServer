<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="images.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.images" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content">
    <p class="gsp_a_ap_to">
        <asp:Label ID="lbl1" runat="server" CssClass="gsp_bold" Text="<%$ Resources:GalleryServer, Admin_Settings_Apply_To_Label %>"
            EnableViewState="false" />&nbsp;<asp:Label ID="lblGalleryDescription" runat="server" EnableViewState="false" />
    </p>
    <asp:PlaceHolder ID="phAdminHeader" runat="server" />
    <div class="gsp_addleftpadding5">
        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Label ID="lblCompressedHdr" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_Compressed_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <table class="gsp_standardTable">
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblOptTriggerSize" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_OptTriggerSize_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtOptTriggerSize" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator ID="rvOptTriggerSize"
                                runat="server" Display="Dynamic" ControlToValidate="txtOptTriggerSize" Type="Integer"
                                MinimumValue="0" MaximumValue="2147483647" Text="<%$ Resources:GalleryServer, Validation_Positive_Int_Include_0_Text %>" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblOptMaxLength" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_OptMaxLength_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtOptMaxLength" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator ID="rvOptMaxLength"
                                runat="server" Display="Dynamic" ControlToValidate="txtOptMaxLength" Type="Integer"
                                MinimumValue="10" MaximumValue="100000" Text="<%$ Resources:GalleryServer, Validation_Int_10_To_100000_Text %>" />
                        </td>
                        <td></td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblOptJpegQuality" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_OptJpegQuality_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtOptJpegQuality" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator ID="rvOptJpegQuality"
                                runat="server" Display="Dynamic" ControlToValidate="txtOptJpegQuality" Type="Integer"
                                MinimumValue="1" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_1_To_100_Text %>" />
                        </td>
                        <td class="gsp_fs">
                            <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_OptJpegQuality_Label2 %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblOptFileNamePrefix" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_OptFileNamePrefix_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtOptFileNamePrefix" runat="server" CssClass="gsp_textcenter gsp_tb_mobile" />
                        </td>
                        <td>
                            <asp:RequiredFieldValidator ID="rfv1" runat="server" ControlToValidate="txtOptFileNamePrefix"
                                Display="Static" ErrorMessage="<%$ Resources:GalleryServer, Site_Field_Required_Text %>"
                                ForeColor="" CssClass="gsp_msgfailure">
                            </asp:RequiredFieldValidator>
                        </td>
                    </tr>
                </table>
            </div>
        </div>

        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_Original_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <asp:Label ID="lblOriginalJpegQuality" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_OriginalJpegQuality_Label %>" />&nbsp;
              <asp:TextBox ID="txtOriginalJpegQuality" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator
                  ID="rvOriginalJpegQuality" runat="server" Display="Dynamic" ControlToValidate="txtOriginalJpegQuality"
                  Type="Integer" MinimumValue="1" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_1_To_100_Text %>" />&nbsp;
              <asp:Label ID="lblOriginalJpegQuality2" runat="server" CssClass="gsp_fs" Text="<%$ Resources:GalleryServer, Admin_Images_OriginalJpegQuality_Label2 %>" />
                <p>
                    <asp:CheckBox ID="chkDiscardOriginal" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_DiscardOriginal_Label %>" />
                </p>
            </div>
        </div>

        <div class="gsp_single_tab">
            <div class="gsp_single_tab_hdr">
                <span class="gsp_single_tab_hdr_txt">
                    <asp:Literal ID="l8" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_Watermark_Hdr %>" />
                </span>
            </div>
            <div class="gsp_single_tab_bdy gsp_dropshadow3">
                <p>
                    <asp:CheckBox ID="chkApplyWatermark" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_ApplyWatermark_Label %>" />
                </p>
                <p class="gsp_addleftpadding6">
                    <asp:CheckBox ID="chkApplyWmkToThumb" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_ApplyWmkToThumb_Label %>" />
                </p>
                <p class="gsp_bold gsp_addtopmargin5">
                    <asp:Literal ID="l9" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_Watermark_Text_Hdr %>" />
                </p>
                <table class="gsp_addleftpadding6 gsp_standardTable">
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkText" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkText_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkText" runat="server" class="gsp_textbox" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkFontName" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkFontName_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkFontName" runat="server" CssClass="gsp_textcenter" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkFontColor" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkFontColor_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkFontColor" runat="server" CssClass="gsp_textcenter" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkFontSize" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkFontSize_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkFontSize" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator ID="rvWmkFontSize"
                                runat="server" Display="Dynamic" ControlToValidate="txtWmkFontSize" Type="Integer"
                                MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_10000_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkTextWidthPct" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkTextWidthPct_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkTextWidthPct" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator ID="rvWmkTextWidthPct"
                                runat="server" Display="Dynamic" ControlToValidate="txtWmkTextWidthPct" Type="Integer"
                                MinimumValue="0" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_100_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkTextOpacity" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkTextOpacity_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkTextOpacity" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator ID="rvWmkTextOpacity"
                                runat="server" Display="Dynamic" ControlToValidate="txtWmkTextOpacity" Type="Integer"
                                MinimumValue="0" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_100_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkTextLocation" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkTextLocation_Label %>" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlWmkTextLocation" runat="server" />
                        </td>
                    </tr>
                </table>
                <p class="gsp_bold gsp_addtopmargin5">
                    <asp:Literal ID="l17" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_Watermark_Image_Hdr %>" />
                </p>
                <table class="gsp_addleftpadding6 gsp_standardTable">
                    <tr>
                        <td class="gsp_col1 gsp_aligntop">
                            <p>
                                <asp:Label ID="lblWmkImagePath" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkImagePath_Label %>" />
                            </p>
                        </td>
                        <td class="gsp_aligntop gs_images_wm_img_container">
                            <p id="cur_watermark_container" style="display: none;"><asp:Image id="imgWatermarkImage" runat="server" CssClass="gs_images_wm_img" /></p>
                            <div id="watermark_image_preview" class="gs_images_wm_img_preview">
                                <p class="gs_images_wm_img_drop_text"><asp:Literal ID="l18" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkImage_Drop_Text %>" /></p>
                                <p class="gs_images_wm_img_sf_text"><a id="hlSelectFile" href="javascript:void(0)"><asp:Literal ID="l19" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkImage_SelectFile_Text %>" /></a></p>
                            </div>
                            <p id="watermark_file_info_container" class="gsp_msgfriendly gsp_textcenter gs_images_wm_img_fi_ctr"><span id="watermark_file_info"></span>
                                <a id="hl_remove_watermark" href="javascript:void(0)">
                                    <asp:Literal ID="l20" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkImage_ReplaceFile_Text %>" />
                                </a>
                            </p>
                            <p id="gs_images_wm_img_p_c" class="gs_images_wm_img_p_c" style="display: none;">
                                <progress id="gs_images_wm_img_progress" max="100" value="0"></progress>
                            </p>
                       </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkImageWidthPct" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkImageWidthPct_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkImageWidthPct" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator
                                ID="rvWmkImageWidthPct" runat="server" Display="Dynamic" ControlToValidate="txtWmkImageWidthPct"
                                Type="Integer" MinimumValue="0" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_100_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkImageOpacity" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkImageOpacity_Label %>" />
                        </td>
                        <td>
                            <asp:TextBox ID="txtWmkImageOpacity" runat="server" CssClass="gsp_textbox_narrow gsp_textcenter" />&nbsp;<asp:RangeValidator ID="rvWmkImageOpacity"
                                runat="server" Display="Dynamic" ControlToValidate="txtWmkImageOpacity" Type="Integer"
                                MinimumValue="0" MaximumValue="100" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_100_Text %>" />
                        </td>
                    </tr>
                    <tr>
                        <td class="gsp_col1">
                            <asp:Label ID="lblWmkImageLocation" runat="server" Text="<%$ Resources:GalleryServer, Admin_Images_WmkImageLocation_Label %>" />
                        </td>
                        <td>
                            <asp:DropDownList ID="ddlWmkImageLocation" runat="server" />
                        </td>
                    </tr>
                </table>
            </div>
        </div>
    </div>
    <tis:wwDataBinder ID="wwDataBinder" runat="server">
        <DataBindingItems>
            <tis:wwDataBindingItem ID="WwDataBindingItem2" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="OptimizedImageTriggerSizeKB" ControlId="txtOptTriggerSize"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_OptTriggerSize_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem3" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="MaxOptimizedLength" ControlId="txtOptMaxLength" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_OptMaxLength_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem4" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="OptimizedImageJpegQuality" ControlId="txtOptJpegQuality" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_OptJpegQuality_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem4b" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="OptimizedFileNamePrefix" ControlId="txtOptFileNamePrefix"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_OptFileNamePrefix_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem5" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="OriginalImageJpegQuality" ControlId="txtOriginalJpegQuality"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_OriginalJpegQuality_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem5b" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="DiscardOriginalImageDuringImport" ControlId="chkDiscardOriginal"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_DiscardOriginal_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem6" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="ApplyWatermark" ControlId="chkApplyWatermark" BindingProperty="Checked"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_ApplyWatermark_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem7" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="ApplyWatermarkToThumbnails" ControlId="chkApplyWmkToThumb"
                BindingProperty="Checked" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_ApplyWmkToThumb_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem8" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkText" ControlId="txtWmkText" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkText_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem9" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkTextFontName" ControlId="txtWmkFontName" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkFontName_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem10" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkTextFontSize" ControlId="txtWmkFontSize" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkFontSize_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem11" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkTextWidthPercent" ControlId="txtWmkTextWidthPct"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkTextWidthPct_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem12" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkTextColor" ControlId="txtWmkFontColor" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkFontColor_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem13" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkTextOpacityPercent" ControlId="txtWmkTextOpacity"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkTextOpacity_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem14" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkTextLocation" ControlId="ddlWmkTextLocation" BindingProperty="SelectedValue"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkTextLocation_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem16" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkImageWidthPercent" ControlId="txtWmkImageWidthPct"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkImageWidthPct_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem17" runat="server" BindingSource="GallerySettingsUpdateable"
                BindingSourceMember="WatermarkImageOpacityPercent" ControlId="txtWmkImageOpacity"
                UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkImageOpacity_Label %>" />
            <tis:wwDataBindingItem ID="WwDataBindingItem18" runat="server" BindingProperty="SelectedValue"
                BindingSource="GallerySettingsUpdateable" BindingSourceMember="WatermarkImageLocation"
                ControlId="ddlWmkImageLocation" UserFieldName="<%$ Resources:GalleryServer, Admin_Images_WmkImageLocation_Label %>" />
        </DataBindingItems>
    </tis:wwDataBinder>
    <asp:HiddenField ID="hdnWatermarkTempFileName" runat="server" ClientIDMode="Static" />
    <asp:HiddenField ID="hdnWatermarkFileName" runat="server" ClientIDMode="Static" />
    <asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
    <script>
        (function ($) {
            $(document).ready(function () {
                var chkApplyWatermarkId = '#<%= chkApplyWatermark.ClientID %>';
                var chkApplyWmkToThumbId = '#<%= chkApplyWmkToThumb.ClientID %>';
                var watermarkUploadUrl = '<%=Utils.GetUrl(string.Format(System.Globalization.CultureInfo.InvariantCulture, "/handler/upload.ashx?aid={0}", GetAlbumId()))%>';
                var flashSwfUrl = '<%=Utils.GetUrl("/script/plupload/Moxie.swf")%>';
                var silverlightXapUrl = '<%=Utils.GetUrl("/script/plupload/Moxie.xap")%>';
                var watermarkImageFilename = '<%= GallerySettingsUpdateable.WatermarkImagePath %>';
                var watermarkImageFileSizeBytes = <%= WatermarkImageFileSizeBytes %>;
                var watermark_image_preview_empty_html;

                var configTooltips = function () {
                    $('#<%= lblOptTriggerSize.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_optimizedImageTriggerSizeKB_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_optimizedImageTriggerSizeKB_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblOptMaxLength.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_maxOptimizedLength_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_maxOptimizedLength_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblOptJpegQuality.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_optimizedImageJpegQuality_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_optimizedImageJpegQuality_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblOptFileNamePrefix.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_optimizedFileNamePrefix_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_optimizedFileNamePrefix_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblOriginalJpegQuality.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_originalImageJpegQuality_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_originalImageJpegQuality_Bdy.JsEncode() %>'
                    });

                    $('#<%= chkDiscardOriginal.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_discardOriginalImageDuringImport_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_discardOriginalImageDuringImport_Bdy.JsEncode() %>'
                    });

                    $('#<%= chkApplyWatermark.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_applyWatermark_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_applyWatermark_Bdy.JsEncode() %>'
                    });

                    $('#<%= chkApplyWmkToThumb.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_applyWatermarkToThumbnails_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_applyWatermarkToThumbnails_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkText.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkText_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkText_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkFontName.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkTextFontName_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkTextFontName_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkFontSize.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkTextFontSize_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkTextFontSize_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkTextWidthPct.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkTextWidthPercent_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkTextWidthPercent_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkFontColor.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkTextColor_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkTextColor_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkTextOpacity.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkTextOpacityPercent_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkTextOpacityPercent_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkTextLocation.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkTextLocation_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkTextLocation_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkImagePath.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkImagePath_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkImagePath_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkImageWidthPct.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkImageWidthPercent_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkImageWidthPercent_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkImageOpacity.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkImageOpacityPercent_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkImageOpacityPercent_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblWmkImageLocation.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Cfg_watermarkImageLocation_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Cfg_watermarkImageLocation_Bdy.JsEncode() %>'
                    });

                    $('#<%= lblCompressedHdr.ClientID %>').gsTooltip({
                        title: '<%= Resources.GalleryServer.Admin_Images_About_Compressed_Images_Hdr.JsEncode() %>',
                        content: '<%= Resources.GalleryServer.Admin_Images_About_Compressed_Images_Bdy.JsEncode() %>'
                    });
                };

                var updateUi = function () {
                    var applyWatermarkSelected = $(chkApplyWatermarkId).prop('checked');

                    if (!applyWatermarkSelected) {
                        $(chkApplyWmkToThumbId).prop('checked', applyWatermarkSelected);
                    }

                    $(chkApplyWmkToThumbId).prop('disabled', !applyWatermarkSelected);
                };

                var formatSize = function(size) {
                    function round(num, precision) {
                        return Math.round(num * Math.pow(10, precision)) / Math.pow(10, precision);
                    }

                    var boundary = Math.pow(1024, 4);

                    if (size > boundary) {
                        return round(size / boundary, 1) + " TB";
                    }

                    if (size > (boundary /= 1024)) {
                        return round(size / boundary, 1) + " GB";
                    }

                    if (size > (boundary /= 1024)) {
                        return round(size / boundary, 1) + " MB";
                    }

                    if (size > 1024) {
                        return Math.round(size / 1024) + " KB";
                    }

                    return size + " bytes";
                };

                // Configure the file uploader for the watermark image
                var uploader = new plupload.Uploader({

                    runtimes: 'html5,silverlight,flash,html4',
                    browse_button: 'hlSelectFile',
                    drop_element: 'watermark_image_preview',
                    url: watermarkUploadUrl,
                    flash_swf_url: flashSwfUrl,
                    silverlight_xap_url: silverlightXapUrl,
                    unique_names: true,
                    filters: {
                        mime_types: [
                            { title: "Image files", extensions: "jpg,jpeg,gif,png,bmp,tif,tiff" }
                        ]
                    },

                    init: {
                        FilesAdded: function (up, files) {
                            // User selected a new watermark image. Grab info, show preview, and upload to App_Data\_Temp on server
                            var file = files[0];

                            $('#gs_images_wm_img_progress').text('0%')[0].value = '0';
                            $('#watermark_file_info').text(file.name + ' (' + formatSize(file.size) + ')');
                            $('#watermark_file_info_container,#gs_images_wm_img_p_c').show();

                            var img = new moxie.image.Image();

                            img.onload = function () {
                                watermark_image_preview_empty_html = $('#watermark_image_preview').children().detach();
                                var thumb = $('#watermark_image_preview');

                                this.embed(thumb[0], {
                                    width: 400,
                                    height: 200,
                                    crop: false,
                                    preserveHeaders: false
                                });
                            };

                            img.load(file.getSource());

                            uploader.start();

                            $('#hdnWatermarkTempFileName').val(file.target_name); // e.g. o_1atuapgq5cqmqos1jl0144mieja.png
                            $('#hdnWatermarkFileName').val(file.name); // e.g. logo.png
                        },

                        UploadProgress: function (up, file) {
                            //console.log('UploadProgress event: ' + file.percent + '%');
                            if (file.percent < 100) {
                                $('#gs_images_wm_img_progress').text(file.percent + '%')[0].value = file.percent;
                            } else {
                                $('#gs_images_wm_img_p_c').hide();
                            }
                        },

                        Error: function (up, err) {
                            var msg = 'An error occurred while uploading the watermark image to the server. The event log may have additional details. ';

                            if (err.response) msg += err.response;
                            if (err.code) msg += '; Code: ' + err.code;
                            if (err.status) msg += '; Status: ' + err.status;

                            Gs.Msg.show('Cannot upload to server', msg, { msgType: 'error', autoCloseDelay: 0 });
                        }
                    }
                });

                var configUi = function () {
                    var applyWatermarkSelected = $(chkApplyWatermarkId).prop('checked');

                    $(chkApplyWmkToThumbId).prop('disabled', !applyWatermarkSelected);

                    uploader.init();

                    if (watermarkImageFilename.length > 0) {
                        // A watermark image exists. Show it and it's info.
                        $('#cur_watermark_container,#watermark_file_info_container').show();
                        $('#watermark_image_preview').hide();
                        $('#watermark_file_info').text(watermarkImageFilename + ' (' + formatSize(watermarkImageFileSizeBytes) + ')');
                    } else {
                        // No watermark image has been configured. Show the file upload box.
                        $('#cur_watermark_container,#watermark_file_info_container').hide();
                        $('#watermark_image_preview').show();
                    }
                };

                var removeWatermark = function () {
                    // Hide the current watermark and show the file upload box. Clear out the hidden field, which is used on the server.
                    $('#hdnWatermarkFileName').val('');
                    $('#cur_watermark_container,#watermark_file_info_container').hide();

                    if ($('#watermark_image_preview canvas').length > 0) {
                        $('#watermark_image_preview').empty().append(watermark_image_preview_empty_html).show();
                    } else {
                        $('#watermark_image_preview').show();
                    }
                };

                var bindEventHandlers = function () {
                    $(chkApplyWatermarkId).click(updateUi);
                    $('#hl_remove_watermark').click(removeWatermark);
                };

                bindEventHandlers();
                configUi();
                configTooltips();
            });

        })(jQuery);
    </script>
</asp:PlaceHolder>
