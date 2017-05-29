

function CivilExtension(viewer, options) {
  Autodesk.Viewing.Extension.call(this, viewer, options);
}

CivilExtension.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
CivilExtension.prototype.constructor = CivilExtension;

CivilExtension.prototype.load = function () {
  var _viewer = this.viewer;

  // list of alignments on this model
  var _alignments = {}

  var _panel = null;
  var _panelGuid = newGUID();

  var COLOR = new THREE.Vector4(1, 0, 0, 1); //red

  _viewer.addEventListener(Autodesk.Viewing.GEOMETRY_LOADED_EVENT, function (e) {
    _panel = null;
    _alignments = [];
    _viewer.search('Alignment', function (aligns) {
      aligns.forEach(function (alignDbId) {
        _viewer.getProperties(alignDbId, function (props) {
          if (props.name.indexOf('Alignment [') == 0)
            _alignments[props.dbId] = props;

          if (Object.keys(_alignments).length == 1)
            if (_viewer.toolbar)
              createUI(_viewer);
        })
      });
    }, null, ['name']);
  });

  createUI =  function (){
    // Button 1
    var button1 = new Autodesk.Viewing.UI.Button('toolbarC3DDesignCheck');
    button1.onClick = function (e) {
      if (_panel == null) {
        _panel = new Autodesk.Viewing.UI.DockingPanel(_viewer.container, 'designCheckPanel', 'Alignments Design Check');
        _panel.container.style.top = "10px";
        _panel.container.style.left = "10px";
        _panel.container.style.width = "auto";
        _panel.container.style.height = "auto";
        _panel.container.style.resize = "auto";
      
        var div = document.createElement('div');
        div.style.margin = '5px';
        div.id = _panelGuid;
        _panel.container.appendChild(div);

        var ul = document.createElement('ul');
        ul.id = _panelGuid + 'ul'
        ul.className = 'list-group';
        div.appendChild(ul);
      }

      // show docking panel
      _panel.setVisible(true);

      // clear current list of aligments
      var panelContent = $('#' +  _panelGuid + 'ul');
      while (panelContent.firstChild) panelContent.removeChild(panelContent.firstChild);
        
      // list alignments
      _alignments.forEach(function (align) {
        var type;
        var li = document.createElement('li');
        var approved = true;
        align.properties.forEach(function (prop) {
          if (prop.attributeName === 'Name') {
            li.className = 'list-group-item alignItem';
            li.id = align.dbId;
            li.onclick = onPanelSelectAlignment;
            li.innerText = prop.displayValue.split('(')[0];
            panelContent.append(li);
          }
                
          if (prop.displayCategory != null && prop.displayCategory.match('^Sub-entity') && prop.displayCategory.match('Data - Curve$') && prop.attributeName === 'Radius') {
            var radius = parseFloat(prop.displayValue);
            var curveNumber = parseInt(prop.displayCategory.replace(/^\D+/g, ''));
            li.innerText += '\nSegment ' + curveNumber + ' with radius of ' + Math.round(radius) + prop.units + ' is ' + (radius < 1000 ? 'not approved' : 'approved');
            if (approved && radius < 1000) approved = false;
          }
        })
        var badge = document.createElement('span');
        badge.className = (approved ? 'glyphicon glyphicon-ok' : 'glyphicon glyphicon-remove') + ' pull-right';

        li.appendChild(badge);
      })
    };
    button1.addClass('toolbarC3DDesignCheck');
    button1.setToolTip('Alignment Design Check');

    // SubToolbar
    this.subToolbar = new Autodesk.Viewing.UI.ControlGroup('myAppGroup1');
    this.subToolbar.addControl(button1);

    _viewer.toolbar.addControl(this.subToolbar);
  }

  onPanelSelectAlignment = function (e) {
    var dbId = parseInt(e.srcElement.id);

    //_viewer.isolate(-1);
    //_viewer.clearThemingColors(_viewer.model);
    //_viewer.impl.visibilityManager.show(dbId, _viewer.model);
    //_viewer.setThemingColor(dbId, COLOR, _viewer.model);
    _viewer.select(dbId);
    _viewer.fitToView([dbId], _viewer.model);
  }

  return true;
};



function newGUID() {
  var d = new Date().getTime();
  var guid = 'xxxx-xxxx-xxxx-xxxx-xxxx'.replace(
  /[xy]/g,
  function (c) {
    var r = (d + Math.random() * 16) % 16 | 0;
    d = Math.floor(d / 16);
    return (c == 'x' ? r : (r & 0x7 | 0x8)).toString(16);
  });

  return guid;
};

CivilExtension.prototype.unload = function () {
  alert('CivilExtension is now unloaded!');
  return true;
};

Autodesk.Viewing.theExtensionManager.registerExtension('Autodesk.Sample.CivilExtension', CivilExtension);