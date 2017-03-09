<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="DataManagementSample._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title></title>
  <link href="Content/bootstrap.min.css" rel="stylesheet" />
  <link href="Content/jsTree/themes/default/style.min.css" rel="stylesheet" />
  <script src="Scripts/Libraries/jquery-3.1.1.min.js"></script>
  <script src="Scripts/Libraries/jsTree3/jstree.min.js"></script>
  <script src="Scripts/DataManagementTree.js"></script>
</head>
<body>
  <form id="form1" runat="server">
    <div id="dataManagementHubs" class="foldertree">
      tree here
    </div>
  </form>
  <form id="uploadFile" method='post' enctype="multipart/form-data">
    <input id="hiddenUploadField" type="file" name="theFile" style="visibility: hidden" />
  </form>
</body>
</html>
