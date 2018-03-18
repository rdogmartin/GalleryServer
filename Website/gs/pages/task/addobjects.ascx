<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="addobjects.ascx.cs" Inherits="GalleryServer.Web.Pages.Task.addobjects" %>
<%@ Import Namespace="System.Globalization" %>
<%@ Import Namespace="GalleryServer.Business" %>
<%@ Import Namespace="GalleryServer.Web" %>
<div class="gsp_content" runat="server">
	<asp:PlaceHolder ID="phTaskHeader" runat="server" />
	<div class="gsp_addleftpadding1">
		<div id="<%= cid %>_addObjTabContainer" class="<%= AddObjectsCssClass %>">
			<ul>
				<li class="<%= AddFilesTabCssClass %>"><a href="#<%= cid %>_addObjTabFiles"><asp:Literal ID="l1" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_Local_Media_Tab_Title %>" /></a></li>
				<li class="<%= AddHtmlTabCssClass %>"><a href="#<%= cid %>_addObjTabExtContent"><asp:Literal ID="l2" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Title %>" /></a></li>
			</ul>
			<div id="<%= cid %>_addObjTabFiles" class="ui-corner-all">
				<p class="gsp_ao_hdr">
					<asp:Label ID="lblAddFileHdr" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_Local_Media_Tab_Hdr %>" />
				</p>
				<div id="uploader" class="gsp_addbottommargin2">
					<p style="width: 100%; height: 150px; text-align: center; padding-top: 100px;">Loading...&nbsp;<img src="<%=Utils.GetSkinnedUrl("/images/wait-squares.gif")%>" alt="" /></p>
				</div>
				<div id="gsp_optionsHdr" class="gsp_optionsHdr gsp_collapsed ui-corner-top">
					<p title='<asp:Literal ID="l1b" runat="server" Text="<%$ Resources:GalleryServer, Site_Options_Tooltip %>" />'>
						<asp:Literal ID="l3" runat="server" Text="<%$ Resources:GalleryServer, Site_Options_Hdr %>" />
					</p>
				</div>
				<section id="gsp_optionsDtl" class="gsp_optionsDtl ui-corner-bottom">
					<section>
						<p>
							<asp:CheckBox ID="chkDiscardOriginal" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_Discard_Original_File_Option_Text %>"
								ClientIDMode="Static" />
						</p>
						<p>
							<asp:CheckBox ID="chkDoNotExtractZipFile" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_Do_Not_Extract_Zip_File_Option_Text %>"
								ClientIDMode="Static" />
						</p>
					</section>
				</section>
			</div>
			<div id="<%= cid %>_addObjTabExtContent" class="ui-corner-all">
				<p class="gsp_ao_hdr">
					<asp:Label ID="lblAddExtHdr" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Hdr %>" />
				</p>
				<p>
					<asp:Label ID="lblExternalOverview" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Dtl %>" />
				</p>
				<table class="gsp_standardTable gsp_addtopmargin5" style="width: 100%;">
					<tr>
						<td style="width: 125px;">
							<asp:Literal ID="l5" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Type_Label %>" />
						</td>
						<td>
							<asp:DropDownList ID="ddlMediaTypes" runat="server" Width="200">
								<asp:ListItem Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Type_Audio %>"
									Value="Audio" />
								<asp:ListItem Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Type_Image %>"
									Value="Image" />
								<asp:ListItem Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Type_Video %>"
									Value="Video" Selected="True" />
								<asp:ListItem Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Type_Other %>"
									Value="Other" />
							</asp:DropDownList>
						</td>
					</tr>
					<tr>
						<td style="width: 125px;">
							<asp:Label ID="lblTitle" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Title_Label %>" />
						</td>
						<td>
							<asp:TextBox ID="txtTitle" runat="server" Style="width: 92%;" />
						</td>
					</tr>
					<tr>
						<td class="gsp_aligntop">
							<asp:Label ID="lblExternalHtmlSource" runat="server" Text="<%$ Resources:GalleryServer, Task_Add_Objects_External_Tab_Html_Label %>" />
						</td>
						<td>
							<asp:TextBox ID="txtExternalHtmlSource" runat="server" Rows="5" TextMode="MultiLine"
								Style="width: 92%;" />
						</td>
					</tr>
				</table>
			</div>
		</div>
	</div>
	<asp:PlaceHolder ID="phTaskFooter" runat="server" />
</div>
<asp:PlaceHolder runat="server">
	<script>

		(function ($) {
			var fileProcessedCount = 0;
			var isError = false; // Set to true if any of the uploads failed (or a file is skipped)
			var isAsync = false; // Set to true if the uploads are being processed on the server asyncronously (will be true for ZIP files)
		  var plUploadStarted = false;
				
			jQuery(document).ready(function () {
				configureEventHandlers();
			  configTabs();
				configureAddFile();
				configureAddHtmlSnippet();
				configTooltips();

			});

		  var configTabs = function() {
			  $("#<%= cid %>_addObjTabContainer").not(".gsp_invisible").tabs({
			    active: (Gs.Vars.Cookies.get('<%= SelectedTabCookieName %>') || 0),
			    activate: function (e, ui) { Gs.Vars.Cookies.get('<%= SelectedTabCookieName %>', ui.newTab.index()); }
				})
					.show();
			};

		  var getSelectedTabIdx = function() {
		    return $("#<%= cid %>_addObjTabContainer").tabs('option', 'active');
			};
		
			var configureEventHandlers = function() {
				$(".gsp_btnOkTop, .gsp_btnOkBottom").click(function(e) {
					$(".gsp_spinner").show();
					$(".gsp_spinner_msg").text("<%= AddObjectsUploadingText %>").show();

				  if (getSelectedTabIdx() == 0) {
						uploadFiles(e);
						e.preventDefault();
					}
					else {
						uploadHtmlSnippet(e);	
					}
				});
			
				$('.gsp_optionsHdr').click(function () {
					$(this).toggleClass("gsp_expanded gsp_collapsed");
					$('.gsp_optionsDtl').slideToggle('fast');
				});
			};

			var uploadFiles = function() {
				// Begin the upload of files to the server.
			  plUploadStarted = true;
				var discardOriginal = $("#chkDiscardOriginal").prop("checked");
				var uploader = $("#uploader").plupload('getUploader');
				
				if (discardOriginal) {
					uploader.settings.resize = {
						width : <%= GallerySettings.MaxOptimizedLength %>, 
						height : <%= GallerySettings.MaxOptimizedLength %>,
						quality : <%= GallerySettings.OptimizedImageJpegQuality %>,
						enabled: false
					};
				}

				// Files in queue upload them first
			    if (uploader.files.length > 0) {
			        isError = false;
					uploader.start();
				} else {
					$(".gsp_spinner, .gsp_spinner_msg").hide();
					Gs.Msg.show('Upload canceled', '<%= AddObjectsInstruction %>', { msgType: 'warning' });
				}
			};

			var uploadHtmlSnippet = function(e) {
				// Validate that the user entered an HTML snippet, cancelling the postback if necessary.
				var htmlSnippet = $("#<%= txtExternalHtmlSource.ClientID %>").val();
				if ((htmlSnippet == null) || (htmlSnippet.length == 0)) {
					e.preventDefault();
					$(".gsp_spinner, .gsp_spinner_msg").hide();
					Gs.Msg.show('Upload canceled', 'Enter an HTML fragment / embed code.', { msgType: 'warning'});
				}
			};
		
			var configureAddFile = function() {
				$('#pnlOptions').hide();
				$('#pnlOptionsHdr').click(function () {
					$('#pnlOptions').slideToggle('fast');
				});

				$("#uploader").plupload({
				  runtimes: 'html5,silverlight,flash,html4',
					url: '<%=Utils.GetUrl(String.Format(CultureInfo.InvariantCulture, "/handler/upload.ashx?aid={0}", GetAlbumId()))%>',
					flash_swf_url: '<%=Utils.GetUrl("/script/plupload/Moxie.swf")%>',
				  silverlight_xap_url: '<%=Utils.GetUrl("/script/plupload/Moxie.xap")%>',
				  filters: <%= GetFileFilters() %>,
					unique_names: true,
					max_file_size : '<%= String.Format("{0} KB", GallerySettings.MaxUploadSize) %>',
				  chunk_size: '2mb',
				  views: {
				  	list: true,
				  	thumbs: true,
				  	active: 'thumbs'
				  },
					uploaded: onFileUpload,
					complete: onComplete,
					error: onError
				});
			};

			var configureAddHtmlSnippet = function() {
				// Nothing to do here...
			};

			var redirectToAlbum = function(msgId) {
				var albumUrl = '<%= Utils.GetUrl(PageId.album, "aid={0}", GetAlbumId()) %>';
				if (msgId && msgId > 0)
					window.location = albumUrl + '&msg=' + msgId;
				else 
					window.location = albumUrl;
			};
		
			var onComplete = function(event, args) {
				// Fail-safe: Normally, we redirect after the web service finishes processing the file (onFileProcessed),
				// but just in case it doesn't return or the file counter gets messed up, set a timer to redirect to 
				// the album a few seconds after the last file upload is complete, thus guaranteeing the page will never
				// get stuck with a "Processing. Please wait..." message.
				// NOTE: args.up.state shows a STOPPED value for normal completions as well as when upload.ashx returns an error (like when validation fails),
				// Since we should catch most validation issues on the client before the upload starts (eg. max file size, disabled file extensions),
				// we'll set the timeout whenever we get here. If we discover the timeout occurs in normal workflows, we may need to revisit this.
				if (args.up.state == plupload.STOPPED) {
					window.setTimeout(redirectToAlbum, 20000);
				}
			};
		
			var onError = function(up, args) {
				// args.error.code can be any of these values:
			  //STOPPED:1,STARTED:2,QUEUED:1,UPLOADING:2,FAILED:4,DONE:5,GENERIC_ERROR:-100,HTTP_ERROR:-200,IO_ERROR:-300,SECURITY_ERROR:-400,INIT_ERROR:-500,FILE_SIZE_ERROR:-600,FILE_EXTENSION_ERROR:-601,IMAGE_FORMAT_ERROR:-700,IMAGE_MEMORY_ERROR:-701,IMAGE_DIMENSIONS_ERROR:-702
				isError = true;
				$(".gsp_spinner, .gsp_spinner_msg").hide();

				Gs.Msg.show("Cannot Upload File", getErrMsg(args), { msgType: 'error', autoCloseDelay: 0 });

				if (plUploadStarted)
				  onFileComplete();
			};
		
			var onFileUpload = function(event, args) {
				// File has been transferred to the server; now call web service to copy file to destination album and create media object record.
				var getData = function() {
					var settings = { };
				
					settings.FileName = args.file.name;
					settings.FileNameOnServer = args.file.target_name;
					settings.AlbumId = <%=this.GetAlbumId()%>;
					settings.DiscardOriginalFile = $("#chkDiscardOriginal").prop("checked");
					settings.ExtractZipFile = !$("#chkDoNotExtractZipFile").prop("checked");

					return settings;
				};
				
				// Call web service to move uploaded file to destination and add to gallery.
				$.ajax(({
					type: "POST",
					url: Gs.Vars.AppRoot + '/api/mediaitems/createfromfile',
					data: JSON.stringify(getData()),
					contentType: "application/json; charset=utf-8",
					dataType: "json",
					success: function(response) { onFileProcessed(args.file, response); },
					error: function(response) { isError = true;onFileComplete(); }
				}));
			};

			var onFileProcessed = function(file, actionResults) {
				// Invoked after the web service has successfully processed an uploaded file. actionResults is an array 
				// of ActionResult objects.
				for (var i = 0; i < actionResults.length; i++) {
					if (actionResults[i].Status == "Error") {
						isError = true;
					}
					else if (actionResults[i].Status == "Async") {
						isAsync = true;
					}
				}
			
				fileProcessedCount++;
				onFileComplete();
			};

			var onFileComplete = function(event, args) {
				// Invoked when a plUpload error occurs, file has either failed to upload/be processed in some way or has successfully been uploaded and processed
				var uploader = $('#uploader').plupload('getUploader');
				if (fileProcessedCount + uploader.total.failed >= uploader.files.length) {
				  if (isAsync)
						redirectToAlbum(<%= (int)MessageType.ObjectsBeingProcessedAsyncronously %>);
					else if (isError)
					  redirectToAlbum(<%= (int)MessageType.ObjectsSkippedDuringUpload %>);
				  else
					  redirectToAlbum();
			  }
			};
			
		    var getErrMsg = function(args) {
				if (args.error.code == plupload.FILE_SIZE_ERROR) {
					return 'File size must be less than ' + args.up.settings.max_file_size + '. The file \'' + args.error.file.name + '\' is ' + Globalize.format(args.error.file.size / 1024 / 1024, 'n1') + ' MB.';
				}
				
				if (args.error.code == plupload.FILE_EXTENSION_ERROR) {
					return 'The file \'' + args.error.file.name + '\' has an extension not currently allowed by the gallery configuration. If you are an administrator, you can enable this extension in the site administration.';
				}

				var msg = '<p>' + args.error.message + ' Code ' + args.error.code + '.</p>';

				if (args.error.file != null) {
					msg += '<p>File: ' + args.error.file.name + '.</p>';
				}

				if (args.error.response != null) {
				    // Unfortunately, the response is invalid json and can't be parsed into an object, so we just show the raw text
					msg += '<p>HTTP response data: ' + args.error.response + '</p>';
				}

				return msg;
			};

		  var configTooltips = function() {
		    $('#<%= lblAddFileHdr.ClientID %>').gsTooltip({
		      title: '<%= Resources.GalleryServer.Task_Add_Objects_Local_Media_Overview_Hdr.JsEncode() %>',
		      content: '<%= String.Format(CultureInfo.CurrentCulture, Resources.GalleryServer.Task_Add_Objects_Local_Media_Overview_Bdy, GallerySettings.MaxUploadSize / 1024).JsEncode() %>'
		    });

		    $('#<%= chkDiscardOriginal.ClientID %>').gsTooltip({
		      title: '<%= Resources.GalleryServer.Task_Add_Objects_Discard_Original_File_Hdr.JsEncode() %>',
		      content: '<%= Resources.GalleryServer.Task_Add_Objects_Discard_Original_File_Bdy.JsEncode() %>'
		    });

		    $('#<%= chkDoNotExtractZipFile.ClientID %>').gsTooltip({
		      title: '<%= Resources.GalleryServer.Task_Add_Objects_ZipOption_Hdr.JsEncode() %>',
		      content: '<%= Resources.GalleryServer.Task_Add_Objects_ZipOption_Bdy.JsEncode() %>'
		    });

		    $('#<%= lblAddExtHdr.ClientID %>').gsTooltip({
		      title: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Overview_Hdr.JsEncode() %>',
		      content: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Overview_Bdy.JsEncode() %>'
		    });

		    $('#<%= ddlMediaTypes.ClientID %>').gsTooltip({
		      title: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Type_Hdr.JsEncode() %>',
		      content: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Type_Bdy.JsEncode() %>'
		    });

		    $('#<%= lblTitle.ClientID %>').gsTooltip({
		      title: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Title_Hdr.JsEncode() %>',
		      content: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Title_Bdy.JsEncode() %>'
		    });

		    $('#<%= lblExternalHtmlSource.ClientID %>').gsTooltip({
		      title: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Html_Hdr.JsEncode() %>',
		      content: '<%= Resources.GalleryServer.Task_Add_Objects_External_Object_Html_Bdy.JsEncode() %>'
		    });
		  };
		  
		})(jQuery);
		
	</script>
</asp:PlaceHolder>
