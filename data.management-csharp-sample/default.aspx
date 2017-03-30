<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="DataManagementSample._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title></title>
  <link href="Content/bootstrap.min.css" rel="stylesheet" />
  <link href="Content/jsTree/themes/default/style.min.css" rel="stylesheet" />
  <script src="Scripts/Libraries/jquery-3.1.1.min.js"></script>
  <script src="Scripts/Libraries/bootstrap.min.js"></script>
  <script src="Scripts/Libraries/bootstrap-select.min.js"></script>
  <link href="Content/bootstrap-select.min.css" rel="stylesheet" />
  <script src="Scripts/Libraries/jsTree3/jstree.min.js"></script>
  <script src="Scripts/DataManagementTree.js"></script>
  <script src="Scripts/ForgeViewer.js"></script>
  <link href="Content/Main.css" rel="stylesheet" />

  <link rel="stylesheet" href="https://developer.api.autodesk.com/viewingservice/v1/viewers/style.min.css" type="text/css">
  <script src="https://developer.api.autodesk.com/viewingservice/v1/viewers/three.js"></script>
  <script src="https://developer.api.autodesk.com/viewingservice/v1/viewers/viewer3D.js"></script>
</head>
<body>
  <form id="form1" runat="server"></form>

  <nav class="navbar navbar-default navbar-fixed-top">
    <div class="container-fluid">
      <ul class="nav navbar-nav left">
        <li>
          <a href="http://developer.autodesk.com" target="_blank">
            <img alt="Autodesk Forge" src="/Images/autodesk-forge.png" height="20" />
            Autodesk Forge
          </a>
        </li>
      </ul>
    </div>
  </nav>
  <div id="dataManagementHubs" class="dataManagementHubs">
    tree here
  </div>
  <!-- End of navbar -->
  <div id="forgeViewer" class="forgeviewer"></div>
  <form id="uploadFile" method='post' enctype="multipart/form-data">
    <input id="hiddenUploadField" type="file" name="theFile" style="visibility: hidden" />
  </form>
  <!-- Modal Create BIM 360 Project -->
  <div class="modal fade" id="createBIM360ProjectForm" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
    <div class="modal-dialog" role="document">
      <div class="modal-content">
        <div class="modal-header">
          <button type="button" class="close" data-dismiss="modal" aria-label="Cancel"><span aria-hidden="true">&times;</span></button>
          <h4 class="modal-title" id="myModalLabel">Create new BIM 360 Project</h4>
        </div>
        <div class="modal-body">
          <form id="newBIM360Project">
            <label for="name">Project name:</label><input type="text" name="name" class="form-control" />
            <label for="startdate">Start date:</label><input type="date" name="startdate" id="startdate" class="form-control" />
            <label for="enddate">End date:</label><input type="date" name="enddate" id="enddate" class="form-control" />
            <label for="projecttype">Project Type:</label><input type="text" name="projecttype" class="form-control" value="Office" />
            <label for="projecttype">Value:</label>
            <div class="input-group">
              <input type="text" name="value" class="form-control" value="0" />
              <select class="selectpicker" title="Currency" name="currency">
                <option value="USD" selected>USD</option>
                <option value="EUR">EUR</option>
                <option value="BRL">BRL</option>
              </select>
            </div>
          </form>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
          <button type="button" class="btn btn-primary" id="createBIM360Project">Create BIM 360 Project</button>
        </div>
      </div>
    </div>
  </div>
</body>
</html>
