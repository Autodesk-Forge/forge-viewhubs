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
});

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
      }
    },
    "plugins": ["types", "state", "sort", "contextmenu"],
    contextmenu: { items: dataManagementContextMenu }
  }).on('loaded.jstree', function () {

  }).bind("activate_node.jstree", function (evt, data) {
    if (data != null && data.node != null && data.node.type == 'items') {
      console.log(data);
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
            alert('Not implemented - WIP');
          }
        }
      };
      break;
  }

  return items;
}

function uploadFile(node) {
  $('#hiddenUploadField').click();
  $('#hiddenUploadField').change(function () {
    var file = this.files[0];
    //size = file.size;
    //type = file.type;
    var formData = new FormData();
    formData.append('fileToUpload', file);
    formData.append('href', node.id);

    $.ajax({
      url: 'api/forge/folders/uploadObject',
      data: formData,
      processData: false,
      contentType: false,
      type: 'POST',
      success: function (data) {
        $('#dataManagementHubs').jstree(true).refresh_node(node);
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