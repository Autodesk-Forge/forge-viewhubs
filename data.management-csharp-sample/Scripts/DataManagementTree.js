/////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved
// Written by Forge Partner Development
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////

$(document).ready(function () {
  prepareDataManagementTree();
  registerFormChangeEvent();
  registerCreateProject();
});

var haveBIM360Hub = false;

function prepareDataManagementTree() {
  $('#dataManagementHubs').jstree({
    'core': {
      'themes': { "icons": true },
      'data': {
        "url": '/api/forge/dataManagement/tree',
        "dataType": "json",
        'multiple': false,
        "data": function (node) {
          return { "id": node.id };
        },
        "success": function (nodes) {
          nodes.forEach(function (n) {
            if (n.type === 'bim360hubs' && n.id.indexOf('b.') > 0)
              haveBIM360Hub = true;
          });

          if (!haveBIM360Hub) {
            $.getJSON("/api/forge/oauth/clientID", function (clientID) {
              $("#ClientID").val(clientID);
              $("#provisionAccountModal").modal();
              $("#provisionAccountSave").click(function () {
                $('#provisionAccountModal').modal('toggle');
                $('#dataManagementHubs').jstree(true).refresh();
              });
              haveBIM360Hub = true;
            });
          }
        }
      }
    },
    'types': {
      'default': {
        'icon': 'glyphicon glyphicon-question-sign'
      },
      '#': {
        'icon': 'glyphicon glyphicon-cloud'
      },
      'hubs': {
        'icon': '/Images/a360hub.png'
      },
      'bim360hubs': {
        'icon': '/Images/bim360.png'
      },
      'personalhub': {
        'icon': '/Images/a360hub.png'
      },
      'projects': {
        'icon': 'glyphicon glyphicon-list-alt'
      },
      'projectunavailable': {
        'icon': 'glyphicon glyphicon-remove'
      },
      'folders': {
        'icon': 'glyphicon glyphicon-folder-open'
      },
      'items': {
        'icon': 'glyphicon glyphicon-file'
      },
      'versions': {
        'icon': 'glyphicon glyphicon-time'
      }
    },
    "plugins": ["types", "state", "sort", "contextmenu"],
    contextmenu: { items: dataManagementContextMenu }
  }).on('loaded.jstree', function () {

  }).bind("activate_node.jstree", function (evt, data) {
    if (data != null && data.node != null && data.node.type == 'versions') {
      if (data.node.id === 'not_available') { alert('No viewable available for this version'); return; }
      var parent_node = $('#dataManagementHubs').jstree(true).get_node(data.node.parent);
      launchViewer(data.node.id, parent_node.text);
    }
  });
}

function dataManagementContextMenu(node) {
  var items;

  switch (node.type) {
    case "projects": case "folders":
      items = {
        uploadFile: {
          label: "Upload file",
          icon: "/Images/upload.png",
          action: function () {
            var treeNode = $('#dataManagementHubs').jstree(true).get_selected(true)[0];
            uploadFile(treeNode);
          }
        }
      };
      break;
    case 'bim360hubs':
      var treeNode = $('#dataManagementHubs').jstree(true).get_selected(true)[0];
      if (treeNode.id.indexOf('/hubs/b.') == -1) return;
      items = {
        uploadFile: {
          label: "Create project",
          icon: "/Images/upload.png",
          action: function () {
            var treeNode = $('#dataManagementHubs').jstree(true).get_selected(true)[0];

            var start = new Date(); // today
            var end = new Date(start);
            end.setDate(end.getDate() + 30);
            $('#startdate').val(start.toDateInputValue());
            $('#enddate').val(end.toDateInputValue());
            $('#hubId').val(treeNode.id.split('b.')[1]);

            $("#createBIM360ProjectForm").modal();
          }
        }
      };
      break;
  }

  return items;
}

// source: http://stackoverflow.com/questions/6982692/html5-input-type-date-default-value-to-today
Date.prototype.toDateInputValue = (function () {
  var local = new Date(this);
  local.setMinutes(this.getMinutes() - this.getTimezoneOffset());
  return local.toJSON().slice(0, 10);
});

function uploadFile(theNode) {
  $('#hiddenUploadField').click();
}

function registerFormChangeEvent() {
  $('#hiddenUploadField').change(function () {
    var selectedNode = $('#dataManagementHubs').jstree(true).get_selected(true)[0];
    if (this.files.length != 1) return;

    var file = this.files[0];
    //size = file.size;
    //type = file.type;
    var formData = new FormData();
    formData.append('fileToUpload', file);
    formData.append('href', selectedNode.id);

    $.ajax({
      url: 'api/forge/folders/uploadObject',
      data: formData,
      processData: false,
      contentType: false,
      type: 'POST',
      success: function (data) {
        $('#dataManagementHubs').jstree(true).refresh_node(selectedNode);
      },
      complete: function (data) {
        $('#uploadFile')[0].reset();
      },
      fail: function (data) {
        alert('Error uploading file.');
      }
    });

    /*
     // upload with progress bar ToDo
     var xhr = new XMLHttpRequest();
     xhr.open('post', '/api/upload', true);
     xhr.upload.onprogress = function (e) {
     if (e.lengthComputable) {
     //var percentage = (e.loaded / e.total) * 100;
     //$('div.progress div.bar').css('width', percentage + '%');
     }
     };
     xhr.onload = function () {
     }
     xhr.send(formData);
     */
  });
}

function registerCreateProject() {
  $("#createBIM360Project").click(function () {
    $('#createBIM360ProjectForm').modal('toggle');
    jQuery.post({
      url: '/api/forge/BIM360/project',
      data: serializeFormJSON($('#newBIM360Project')),
      success: function (data) {
        $("#newBIM360ProjectName").val('');
        $('#newBIM360Project')[0].reset();

        var selectedNode = $('#dataManagementHubs').jstree(true).get_selected(true)[0];
        $('#dataManagementHubs').jstree(true).refresh_node(selectedNode);
      },
      fail: function (error) {
        alert('Cannot create project.');
      }
    });
  });
}

// Credits to https://jsfiddle.net/gabrieleromanato/bynaK/
function serializeFormJSON(form) {
  var o = {};
  var a = form.serializeArray();
  $.each(a, function () {
    if (o[this.name]) {
      if (!o[this.name].push) {
        o[this.name] = [o[this.name]];
      }
      o[this.name].push(this.value || '');
    } else {
      o[this.name] = this.value || '';
    }
  });
  return o;
}