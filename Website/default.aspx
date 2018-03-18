<%@ Page Language="C#" UnobtrusiveValidationMode="None" %>
<%@ Register TagPrefix="gs" Namespace="GalleryServer.Web" Assembly="GalleryServer.Web" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" />
    <meta name="viewport" content="width=device-width" />
    <link rel="apple-touch-icon-precomposed" href="favicon_ios.png">
    <link rel="icon" href="favicon.png">
</head>
<body>
    <form runat="server">
        <gs:Gallery ID="g" runat="server" />
    </form>
</body>
</html>
