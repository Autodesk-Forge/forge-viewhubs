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
  // first, check if current visitor is signed in
  jQuery.ajax({
    url: '/api/forge/oauth/token',
    success: function (res) {
      // yes, it is signed in...
      $('#signOut').show();
      $('#refreshHubs').show();

      // prepare sign out
      $('#signOut').click(function () {
        $('#hiddenFrame').on('load', function (event) {
          location.href = '/api/forge/oauth/signout';
        });
        $('#hiddenFrame').attr('src', 'https://accounts.autodesk.com/Authentication/LogOut');
        // learn more about this signout iframe at
        // https://forge.autodesk.com/blog/log-out-forge
      })

      // and refresh button
      $('#refreshHubs').click(function () {
        $('#userHubs').jstree(true).refresh();
      });

      // finally:
      prepareUserHubsTree();
      showUser();
    }
  });

  $('#autodeskSigninButton').click(function () {
    jQuery.ajax({
      url: '/api/forge/oauth/url',
      success: function (url) {
        location.href = url;
      }
    });
  })

  $.getJSON("/api/forge/clientid", function (res) {
    $("#ClientID").val(res.id);
    $("#provisionAccountSave").click(function () {
      $('#provisionAccountModal').modal('toggle');
      $('#userHubs').jstree(true).refresh();
    });
  });

  $('#hiddenUploadField').change(function () {
    var node = $('#userHubs').jstree(true).get_selected(true)[0];
    var _this = this;
    if (_this.files.length == 0) return;
    var file = _this.files[0];
    switch (node.type) {
      case 'folders':
        var formData = new FormData();
        formData.append('fileToUpload', file);
        formData.append('folderHref', node.id);
        _this.value = '';

        $.ajax({
          url: '/api/forge/datamanagement',
          data: formData,
          processData: false,
          contentType: false,
          type: 'POST',
          success: function (data) {
            $('#userHubs').jstree(true).refresh_node(node);
            _this.value = '';
          }
        });
        break;
    }
  });
});

function prepareUserHubsTree() {
  var haveBIM360Hub = false;
  $('#userHubs').jstree({
    'core': {
      'themes': { "icons": true },
      'multiple': false,
      'data': {
        "url": '/api/forge/datamanagement',
        "dataType": "json",
        'cache': false,
        'data': function (node) {
          $('#userHubs').jstree(true).toggle_node(node);
          return { "id": node.id };
        },
        "success": function (nodes) {
          nodes.forEach(function (n) {
            if (n.type === 'bim360Hubs' && n.id.indexOf('b.') > 0)
              haveBIM360Hub = true;
          });

          if (!haveBIM360Hub) {
            $("#provisionAccountModal").modal();
            haveBIM360Hub = true;
          }
        }
      }
    },
    'types': {
      'default': {
        'icon': 'glyphicon glyphicon-question-sign'
      },
      '#': {
        'icon': 'glyphicon glyphicon-user'
      },
      'hubs': {
        'icon': 'https://github.com/Autodesk-Forge/learn.forge.viewhubmodels/raw/master/img/a360hub.png'
      },
      'personalHub': {
        'icon': 'https://github.com/Autodesk-Forge/learn.forge.viewhubmodels/raw/master/img/a360hub.png'
      },
      'bim360Hubs': {
        'icon': 'https://github.com/Autodesk-Forge/learn.forge.viewhubmodels/raw/master/img/bim360hub.png'
      },
      'bim360projects': {
        'icon': 'https://github.com/Autodesk-Forge/learn.forge.viewhubmodels/raw/master/img/bim360project.png'
      },
      'a360projects': {
        'icon': 'https://github.com/Autodesk-Forge/learn.forge.viewhubmodels/raw/master/img/a360project.png'
      },
      'items': {
        'icon': 'glyphicon glyphicon-file'
      },
      'bim360documents': {
        'icon': 'glyphicon glyphicon-file'
      },
      'folders': {
        'icon': 'glyphicon glyphicon-folder-open'
      },
      'versions': {
        'icon': 'glyphicon glyphicon-time'
      },
      'unsupported': {
        'icon': 'glyphicon glyphicon-ban-circle'
      }
    },
    "sort": function (a, b) {
      var a1 = this.get_node(a);
      var b1 = this.get_node(b);
      var parent = this.get_node(a1.parent);
      if (parent.type === 'items') { // sort by version number
        var id1 = Number.parseInt(a1.text.substring(a1.text.indexOf('v') + 1, a1.text.indexOf(':')))
        var id2 = Number.parseInt(b1.text.substring(b1.text.indexOf('v') + 1, b1.text.indexOf(':')));
        return id1 > id2 ? 1 : -1;
      }
      else if (a1.type !== b1.type) return a1.icon < b1.icon ? 1 : -1; // types are different inside folder, so sort by icon (files/folders)
      else return a1.text > b1.text ? 1 : -1; // basic name/text sort
    },
    "plugins": ["types", "state", "sort", "contextmenu"],
    "contextmenu": { items: autodeskCustomMenu },
    "state": { "key": "autodeskHubs" }// key restore tree state
  }).bind("activate_node.jstree", function (evt, data) {
    if (data != null && data.node != null && (data.node.type == 'versions' || data.node.type == 'bim360documents')) {
      if (data.node.id.indexOf('|') > -1) {
        // let's split the id of the tree node in case there is a geometryId to show
        var urn = data.node.id.split('|')[1];
        var geometryId = data.node.id.split('|')[2];
        launchViewer(urn, geometryId);
      }
      else {
        launchViewer(data.node.id);
      }
    }
  });
}

function autodeskCustomMenu(autodeskNode) {
  var items;

  switch (autodeskNode.type) {
    case "versions":
      var parent = $('#userHubs').jstree(true).get_node(autodeskNode.parent);
      if (parent.text.indexOf('.rvt') == -1) return;
      items = {
        translateIfc: {
          label: "Translate to IFC",
          action: function () {
            jQuery.post({
              url: '/api/forge/modelderivative/jobs',
              contentType: 'application/json',
              data: JSON.stringify({ 'urn': autodeskNode.id, 'output': 'ifc' }),
              success: function (res) {
                $("#forgeViewer").html('Translation started! Please wait and <a href="/api/forge/modelderivative/' + autodeskNode.id + '/ifc" target="_blank">click here</a> to download');
              },
            });
          },
          icon: 'glyphicon glyphicon-cloud-upload'
        }
      };
      break;
    case "folders":
      items = {
        uploadFile: {
          label: "Upload file",
          action: function () {
            uploadFile();
          },
          icon: 'glyphicon glyphicon-cloud-upload'
        }
      }
      break;
  }

  return items;
}

function uploadFile() {
  $('#hiddenUploadField').click();
}

function showUser() {
  jQuery.ajax({
    url: '/api/forge/user/profile',
    success: function (profile) {
      var img = '<img src="' + profile.picture + '" height="30px">';
      $('#userInfo').html(img + profile.name);
    }
  });
}