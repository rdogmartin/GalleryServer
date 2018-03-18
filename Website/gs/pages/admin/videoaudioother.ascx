<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="videoaudioother.ascx.cs" Inherits="GalleryServer.Web.Pages.Admin.videoaudioother" %>
<%@ Import Namespace="GalleryServer.Web" %>
<%@ Register Assembly="GalleryServer.Web" Namespace="GalleryServer.WebControls" TagPrefix="tis" %>
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
			    <asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_General_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
			  <p>
				  <asp:CheckBox ID="chkAutoStart" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_AutoStart_Label %>" />
			  </p>
			  <p>
				  <asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_VideoThumbnailPosition_Part1_Label %>" />&nbsp;
				  <asp:TextBox ID="txtVideoThumbnailPosition" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					  ID="rvVideoThumbnailPosition" runat="server" Display="Dynamic" ControlToValidate="txtVideoThumbnailPosition"
					  Type="Integer" MinimumValue="0" MaximumValue="86400" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_Validation_VideoThumbnailPosition_Text %>" />
				  &nbsp;<asp:Label ID="lblVideoThumbnailPositionPart2" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_VideoThumbnailPosition_Part2_Label %>" />
			  </p>
			  <p class="gsp_bold gsp_addtopmargin5">
				  <asp:Label ID="lblMediaCtrDim" runat="server" Text="<%$ Resources:GalleryServer, Admin_Media_Container_Dimensions_Label %>" />
			  </p>
			  <p class="gsp_addleftpadding5">
				  <asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_MediaContainer_Video_Lbl %>" />
				  <asp:TextBox ID="txtVideoPlayerWidth" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					  ID="rvVideoPlayerWidth" runat="server" Display="Dynamic" ControlToValidate="txtVideoPlayerWidth"
					  Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_10000_Text %>" />
				  x&nbsp;
				  <asp:TextBox ID="txtVideoPlayerHeight" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					  ID="rvVideoPlayerHeight" runat="server" Display="Dynamic" ControlToValidate="txtVideoPlayerHeight"
					  Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_10000_Text %>" />
				  <asp:Literal ID="l6" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_Pixels_Lbl %>" />
			  </p>
			  <p class="gsp_addleftpadding5">
				  <asp:Literal ID="l7" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_MediaContainer_Audio_Lbl %>" />
				  <asp:TextBox ID="txtAudioPlayerWidth" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					  ID="rvAudioPlayerWidth" runat="server" Display="Dynamic" ControlToValidate="txtAudioPlayerWidth"
					  Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_10000_Text %>" />
				  x&nbsp;
				  <asp:TextBox ID="txtAudioPlayerHeight" runat="server" CssClass="gsp_textcenter gsp_textbox_narrow" />&nbsp;<asp:RangeValidator
					  ID="rvAudioPlayerHeight" runat="server" Display="Dynamic" ControlToValidate="txtAudioPlayerHeight"
					  Type="Integer" MinimumValue="0" MaximumValue="10000" Text="<%$ Resources:GalleryServer, Validation_Int_0_To_10000_Text %>" />
				  <asp:Literal ID="l9" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_Pixels_Lbl %>" />
			  </p>
      </div>
    </div>

    <div class="gsp_single_tab" style="max-width:100%;">
      <div class="gsp_single_tab_hdr">
        <span class="gsp_single_tab_hdr_txt">
			    <asp:Label ID="lblEncoderSettingsHdr" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_EncoderSettings_Hdr %>" />
        </span>
      </div>
      <div class="gsp_single_tab_bdy gsp_dropshadow3">
		    <p class="gsp_addleftmargin4">
			    <asp:Literal ID="l19" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_EncoderSettings_Status_Hdr %>" />&nbsp;<asp:Label
				    ID="lblEncoderStatus" runat="server" />
		    </p>
		    <p class="gsp_addleftmargin4">
			    <asp:Label ID="lblEncoderTimeout" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_EncoderTimeout_Lbl %>" />
			    <asp:TextBox ID="txtMediaEncoderTimeoutMs" runat="server" />
		    </p>
		    <p class="gsp_addleftmargin4 gsp_va_tip">
			    <asp:Literal ID="l20" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_EncoderSettings_Tip %>" />
		    </p>
		    <ul id="gsp_encoderSettingsCtr">
		    </ul>
		    <p class="gsp_addleftmargin4">
			    <a href="#" id="gsp_addEncoderSetting">
				    <asp:Literal ID="l4" runat="server" Text="<%$ Resources:GalleryServer, Admin_VidAudOther_AddEncoder_CmdText %>" />
			    </a>
		    </p>
      </div>
    </div>
	</div>
	<asp:HiddenField ID="hdnEncoderSettings" runat="server" ClientIDMode="Static" />
	<asp:HiddenField ID="hdnSourceAvailableFileExtensions" runat="server" ClientIDMode="Static" />
	<asp:HiddenField ID="hdnDestinationAvailableFileExtensions" runat="server" ClientIDMode="Static" />
	<tis:wwDataBinder ID="wwDataBinder" runat="server">
		<DataBindingItems>
			<tis:wwDataBindingItem ID="wbi1" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="AutoStartMediaObject" ControlId="chkAutoStart" BindingProperty="Checked"
				UserFieldName="<%$ Resources:GalleryServer, Admin_VidAudOther_AutoStart_Label %>" />
			<tis:wwDataBindingItem ID="wbi2" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultVideoPlayerWidth" ControlId="txtVideoPlayerWidth" UserFieldName="<%$ Resources:GalleryServer, Admin_VidAudOther_MediaContainer_Video_Lbl %>" />
			<tis:wwDataBindingItem ID="wbi3" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultVideoPlayerHeight" ControlId="txtVideoPlayerHeight"
				UserFieldName="<%$ Resources:GalleryServer, Admin_VidAudOther_MediaContainer_Video_Lbl %>" />
			<tis:wwDataBindingItem ID="wbi4" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="VideoThumbnailPosition" ControlId="txtVideoThumbnailPosition"
				UserFieldName="Video Thumbnail Position" />
			<tis:wwDataBindingItem ID="wbi5" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="MediaEncoderTimeoutMs" ControlId="txtMediaEncoderTimeoutMs"
				UserFieldName="<%$ Resources:GalleryServer, Admin_VidAudOther_EncoderTimeout_Lbl %>" />
			<tis:wwDataBindingItem ID="wbi6" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultAudioPlayerWidth" ControlId="txtAudioPlayerWidth" UserFieldName="<%$ Resources:GalleryServer, Admin_VidAudOther_MediaContainer_Audio_Lbl %>" />
			<tis:wwDataBindingItem ID="wbi7" runat="server" BindingSource="GallerySettingsUpdateable"
				BindingSourceMember="DefaultAudioPlayerHeight" ControlId="txtAudioPlayerHeight"
				UserFieldName="<%$ Resources:GalleryServer, Admin_VidAudOther_MediaContainer_Audio_Lbl %>" />
		</DataBindingItems>
	</tis:wwDataBinder>
	<asp:PlaceHolder ID="phAdminFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
	<script id='tmplEncoderSetting' type='text/x-jsrender'>
<li class="gsp_va_et">
<table class="gsp_encoderSettingRowCtr">
<tr><td style="white-space:nowrap;">
	<a href="#" title='Delete' class="gsp_va_et_d_btn gsp_hoverLink"><img src='<%= Utils.GetSkinnedUrl("images/delete-red-s.png") %>' alt='Delete' /></a>
    <span class="gsp_va_et_m_btn gsp_hoverLink" title='<%= EsMoveTooltip %>'></span>
	 <%= EsConvertString %> <select name="sltSourceFileExtension">{{for SourceAvailableFileExtensions}}
		<option {{if Value === #parent.parent.data.SourceFileExtension}}selected="selected"{{/if}} value="{{:Value}}">{{:Text}}</option>
		{{/for}}</select>
	 <%= EsToString %> <select name="sltDestinationFileExtension">{{for DestinationAvailableFileExtensions}}
		<option {{if Value === #parent.parent.data.DestinationFileExtension}}selected="selected"{{/if}} value="{{:Value}}">{{:Text}}</option>
		{{/for}}</select>
	 <%= EsFFmpegArgsString %> </td>
	<td width="100%"><input type="text" name="iptArgs" value="{{>EncoderArguments}}" style="width:100%;" /></td></tr>
</table>
</li>
	</script>
	<script>
	    (function ($) {
	        var updateHiddenField = function () {
	            // Convert data in encoder settings HTML to MediaEncoderSettings array, then serialize and store in hidden field,
	            // where it will be accessed by server code after form submission.
	            var encoderSettings = new Array();

	            $("#gsp_encoderSettingsCtr li").each(function () {
	                var sourceFileExt = $("select[name=sltSourceFileExtension]", $(this)).val();
	                var destFileExt = $("select[name=sltDestinationFileExtension]", $(this)).val();
	                var args = $("input[name=iptArgs]", $(this)).val();

	                var encoderSetting = {};
	                encoderSetting.SourceFileExtension = sourceFileExt;
	                encoderSetting.DestinationFileExtension = destFileExt;
	                encoderSetting.EncoderArguments = args;

	                encoderSettings.push(encoderSetting);
	            });

	            var serializedStr = JSON.stringify(encoderSettings);
	            $("#hdnEncoderSettings").val(serializedStr);
	        };

	        var bindEncoderSettings = function () {
	            var encoderSettings = JSON.parse($("#hdnEncoderSettings").val());

	            // Get available file extensions and add as property on each encoder setting
	            var sourceAvailableFileExtensions = JSON.parse($("#hdnSourceAvailableFileExtensions").val());
	            var destinationAvailableFileExtensions = JSON.parse($("#hdnDestinationAvailableFileExtensions").val());

	            $.each(encoderSettings, function (i, item) {
	                item.SourceAvailableFileExtensions = sourceAvailableFileExtensions;
	                item.DestinationAvailableFileExtensions = destinationAvailableFileExtensions;
	            });

	            var tmplData = $("#tmplEncoderSetting").render(encoderSettings); // Generate the HTML from the template
	            $("#gsp_encoderSettingsCtr").html(tmplData) // Add the HTML to the page
	                .find('.gsp_va_et_d_btn').click(function () {
	                    $(this).parents("li").remove();
	                    return false;
	                });

	            $("#gsp_encoderSettingsCtr").sortable({
	                axis: 'y'
	            });
	        };

	        var onAddEncoderRow = function (e) {
	            updateHiddenField();

	            // Add an item to the encoder settings stored in the hidden field, then re-bind the template
	            var encoderSetting = {};
	            encoderSetting.SourceFileExtension = "";
	            encoderSetting.DestinationFileExtension = "";
	            encoderSetting.EncoderArguments = "";

	            var encoderSettings = JSON.parse($("#hdnEncoderSettings").val());
	            encoderSettings.push(encoderSetting);
	            var serializedStr = JSON.stringify(encoderSettings);
	            $("#hdnEncoderSettings").val(serializedStr);

	            bindEncoderSettings();

	            return false;
	        };

	        var bindEventHandlers = function () {
	            $("#gsp_addEncoderSetting").click(onAddEncoderRow);
	            $('form:first').submit(function (e) {
	                updateHiddenField();
	                //e.preventDefault();
	                return true;
	            });
	        };

	        var configTooltips = function () {
	            $('#<%= chkAutoStart.ClientID %>').gsTooltip({
	                title: '<%= Resources.GalleryServer.Cfg_autoStartMediaObject_Hdr.JsEncode() %>',
	                content: '<%= Resources.GalleryServer.Cfg_autoStartMediaObject_Bdy.JsEncode() %>'
	            });

	            $('#<%= lblVideoThumbnailPositionPart2.ClientID %>').gsTooltip({
	                title: '<%= Resources.GalleryServer.Cfg_VideoThumbnailPosition_Hdr.JsEncode() %>',
	                content: '<%= Resources.GalleryServer.Cfg_VideoThumbnailPosition_Bdy.JsEncode() %>'
	            });

	            $('#<%= lblMediaCtrDim.ClientID %>').gsTooltip({
	                title: '<%= Resources.GalleryServer.Cfg_MediaContainerDimensions_Hdr.JsEncode() %>',
	                content: '<%= Resources.GalleryServer.Cfg_MediaContainerDimensions_Bdy.JsEncode() %>'
	            });

	            $('#<%= lblEncoderSettingsHdr.ClientID %>').gsTooltip({
	                title: '<%= Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_Overview_Hdr.JsEncode() %>',
	                content: '<%= Resources.GalleryServer.Admin_VidAudOther_EncoderSettings_Overview_Bdy.JsEncode() %>'
	            });

	            $('#<%= lblEncoderTimeout.ClientID %>').gsTooltip({
	                title: '<%= Resources.GalleryServer.Admin_VidAudOther_EncoderTimeout_Hdr.JsEncode() %>',
	                content: '<%= Resources.GalleryServer.Admin_VidAudOther_EncoderTimeout_Bdy.JsEncode() %>'
	            });
	        };

	      $(document).ready(function () {
	          bindEventHandlers();
	          bindEncoderSettings();
	          configTooltips();
	      });

	  })(jQuery);
	</script>
</asp:PlaceHolder>
