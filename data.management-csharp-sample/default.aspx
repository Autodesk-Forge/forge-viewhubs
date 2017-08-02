<%@ Page Async="true" Language="C#" AutoEventWireup="true" CodeBehind="default.aspx.cs" Inherits="DataManagementSample._default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
  <title>Forge Tools</title>
  <link runat="server" rel="shortcut icon" href="~/favicon.ico" type="image/x-icon" />
  <link runat="server" rel="icon" href="~/favicon.ico" type="image/ico" />

  <link rel="stylesheet" href="https://developer.api.autodesk.com/viewingservice/v1/viewers/style.min.css?v=v2.16" type="text/css" />
  <script src="https://developer.api.autodesk.com/viewingservice/v1/viewers/three.min.js"></script>
  <script src="https://developer.api.autodesk.com/viewingservice/v1/viewers/viewer3D.min.js?v=v2.16"></script>

  <link href="Content/bootstrap.min.css" rel="stylesheet" />
  <link href="Content/jsTree/themes/default/style.min.css" rel="stylesheet" />
  <script src="Scripts/Libraries/jquery-3.1.1.min.js"></script>
  <script src="Scripts/Libraries/bootstrap.min.js"></script>
  <script src="Scripts/Libraries/bootstrap-select.min.js"></script>
  <link href="Content/bootstrap-select.min.css" rel="stylesheet" />
  <script src="Scripts/Libraries/jsTree3/jstree.min.js"></script>
  <script src="Scripts/DataManagementTree.js"></script>
  <script src="Scripts/ForgeViewer.js"></script>
  <script src="Scripts/Libraries/clipboard.min.js"></script>
  <link href="Content/Main.css" rel="stylesheet" />

  <script src="Scripts/Libraries/Blob.js"></script>
  <script src="Scripts/Libraries/FileSaver.min.js"></script>
  <script src="Scripts/Libraries/xlsx.core.min.js"></script>
  <script src="Scripts/ExcelExtension.js"></script>

  <script src="Scripts/CivilExtension.js"></script>
</head>
<body>
  <form id="form1" runat="server"></form>

  <nav class="navbar navbar-default navbar-fixed-top">
    <div class="container-fluid">
      <ul class="nav navbar-nav left">
        <li>
          <a href="http://developer.autodesk.com" target="_blank">
            <img alt="Autodesk Forge" src="/Images/autodesk-forge.png" height="20" />
            Forge Tools Sample App
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
          <h4 class="modal-title">Create new BIM 360 Project</h4>
        </div>
        <div class="modal-body">
          <form id="newBIM360Project">
            <input type="hidden" name="hubId" id="hubId" value="" />
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
  <!-- Modal Provision BIM360  -->
  <div class="modal fade" id="provisionAccountModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
    <div class="modal-dialog" role="document">
      <div class="modal-content">
        <div class="modal-header">
          <button type="button" class="close" data-dismiss="modal" aria-label="Cancel"><span aria-hidden="true">&times;</span></button>
          <h4 class="modal-title">Thanks for using Forge Tools Sample App!</h4>
        </div>
        <div class="modal-body">
          <p>To view your BIM 360 Docs files on this app please authorize my Forge Client ID with your BIM 360 Docs Account.</p>
          <p>
            <button type="button" class="btn btn-info" data-toggle="modal" data-target="#provisionAccountStepsModal">Show me the steps <span class="glyphicon glyphicon-new-window"></span></button>
          </p>
          Use this as Forge Client ID:
        <div class="input-group">
          <input type="text" readonly="true" aria-describedby="CopyClientID" id="ClientID" class="form-control" value="" />
          <span class="input-group-addon" style="cursor: pointer" data-clipboard-target="#ClientID" id="CopyClientID">Copy to clipboard</span>
        </div>
          And this App Name:
        <div class="input-group">
          <input type="text" readonly="true" aria-describedby="CopyAppName" id="AppName" class="form-control" value="Forge Tools Sample App" />
          <span class="input-group-addon" style="cursor: pointer" data-clipboard-target="#AppName" id="CopyAppName">Copy to clipboard</span>
        </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
          <button type="button" class="btn btn-primary" id="provisionAccountSave">Done</button>
        </div>
      </div>
    </div>
  </div>
  <!-- Modal Provision BIM360 Help  -->
  <div class="modal fade" id="provisionAccountStepsModal" tabindex="-1" role="dialog" aria-labelledby="myModalLabel">
    <div class="modal-dialog modal-lg" role="document">
      <div class="modal-content">
        <div class="modal-header">
          <button type="button" class="close" data-dismiss="modal" aria-label="Cancel"><span aria-hidden="true">&times;</span></button>
          <h4 class="modal-title" id="myModalLabel1">Step-by-step guide</h4>
        </div>
        <div class="modal-body">
          <p>To access your BIM 360 Docs files with this app, you need to connect your BIM 360 account with the app.</p>
          <ol>
            <li>Log in to <a href="https://bim360enterprise.autodesk.com/" target="_blank">BIM 360 Account Administration</a>. Note that you need account administrator permissions.</li>
            <li>If you have more than one account, select the account you want to integrate with the app.</li>
            <li>From the toolbar, select <strong>SETTING &gt; Apps & Integrations</strong>.<br />
              <img src="/Images/Step1.png" width="500" /></li>
            <li>Click the <strong>Add Integration</strong> button.<br />
              <img src="/Images/Step2.png" />
            </li>
            <li>Select <strong>BIM 360 Account Administration</strong> and <strong>BIM 360 Docs</strong>, and click <strong>Next</strong>.</li>
            <li>Select <strong>I’m the developer</strong>, and click <strong>Next</strong>.</li>
            <li>In the <strong>Forge Client ID</strong> and <strong>App Name</strong> fields, enter the Forge client ID and app name provided by the app retailer.</li>
            <li>Select the <strong>I have saved the Account ID information securely</strong> checkbox.</li>
            <li>Click <strong>Save</strong>.<br/>
            <img src="/Images/Step3.png" width="500" />
            </li>
            <p></p>
            <p>Congratulations! You can now access your BIM 360 Docs files.</p>
          </ol>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-default" data-dismiss="modal">Understood, thanks!</button>
        </div>
      </div>
    </div>
  </div>
  <script>
    new Clipboard('.input-group-addon');
  </script>
</body>
</html>
