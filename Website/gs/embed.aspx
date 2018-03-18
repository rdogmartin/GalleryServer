<%@ Page Language="C#" UnobtrusiveValidationMode="None" %>
<%@ Register TagPrefix="gs" Namespace="GalleryServer.Web" Assembly="GalleryServer.Web" %>
<!DOCTYPE html>
<html>
<head runat="server">
	<meta charset="utf-8" />
	<meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" />
	<style type="text/css">
		html { height: 100%; }
		body { margin: 0; height: 100%; /* overflow: hidden; */ }
		.gsp_footer { display: none; }
	</style>
</head>
<body>
	<form id="form1" runat="server">
	<gs:Gallery ID="g" runat="server" ShowHeader="false" ShowRibbonToolbar="false" ShowAlbumBreadCrumb="false"
		ShowMediaObjectNavigation="false" ShowMediaObjectIndexPosition="false" ShowLeftPaneForAlbum="false" 
		ShowLeftPaneForMediaObject="false" ShowRightPane="false" ShowMediaObjectTitle="true" />
	</form>
</body>
</html>
