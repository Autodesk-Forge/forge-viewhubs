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

// *******************************************
// MiniMap Extension
// *******************************************
function MiniMapExtension(viewer, options) {
    Autodesk.Viewing.Extension.call(this, viewer, options);
    this.viewer = viewer;
    this.panel = null; // dock panel
    this.map = null; // Google Map
    this.geoExtension = null; // Autodesk.Geolocation extension

    var _this = this;
    // load extension...
    viewer.loadExtension('Autodesk.Geolocation').then(function (ext) { _this.geoExtension = ext });
}

MiniMapExtension.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
MiniMapExtension.prototype.constructor = MiniMapExtension;

MiniMapExtension.prototype.load = function () {
    if (this.viewer.toolbar) {
        // Toolbar is already available, create the UI
        this.createUI();
    } else {
        // Toolbar hasn't been created yet, wait until we get notification of its creation
        this.onToolbarCreatedBinded = this.onToolbarCreated.bind(this);
        this.viewer.addEventListener(Autodesk.Viewing.TOOLBAR_CREATED_EVENT, this.onToolbarCreatedBinded);
    }
    return true;
};

MiniMapExtension.prototype.onToolbarCreated = function () {
    this.viewer.removeEventListener(Autodesk.Viewing.TOOLBAR_CREATED_EVENT, this.onToolbarCreatedBinded);
    this.onToolbarCreatedBinded = null;
    this.createUI();
};

MiniMapExtension.prototype.createUI = function () {
    var _this = this;

    // button to show the docking panel
    var toolbarButtonShowDockingPanel = new Autodesk.Viewing.UI.Button('showMiniMap');
    toolbarButtonShowDockingPanel.onClick = function (e) {
        if (_this.geoExtension === null || !_this.geoExtension.hasGeolocationData()) {
            alert('Model does not contain geo location information');
            return;
        }

        // if null, create it
        if (_this.panel == null) {
            _this.panel = new MiniMapPanel(_this.viewer, _this.viewer.container, 'miniMapPanel', 'Mini Map');
        }

        // show/hide docking panel
        _this.panel.setVisible(!_this.panel.isVisible());

        // initialize the map
        if (_this.map == null) {
            _this.map = new google.maps.Map(document.getElementById('googlemap'), {
                zoom: 16,
                center: { lat: 0, lng: 0 },
                mapTypeId: 'satellite',
                rotateControl: false,
                streetViewControl: false,
                tilt: 0
            });
            // draw model boundoung box & center
            var bb = _this.viewer.model.getBoundingBox();
            _this.drawBoundingBox(bb.min, bb.max);
            _this.cameraChanged(_this.viewer.autocam); // first run (center of the model)
        }
    };
    // CSS for the toolbar
    toolbarButtonShowDockingPanel.addClass('miniMapExtension');
    toolbarButtonShowDockingPanel.setToolTip('Show minimap');

    // SubToolbar
    this.subToolbar = new Autodesk.Viewing.UI.ControlGroup('CustomGeoTools');
    this.subToolbar.addControl(toolbarButtonShowDockingPanel);
    this.viewer.toolbar.addControl(this.subToolbar);

    // listen to camera changes
    this.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, function (e) { _this.cameraChanged(e.target.autocam) });
};

MiniMapExtension.prototype.drawBoundingBox = function (min, max) {
    // basic check...
    if (this.map == null) return;
    if (this.geoExtension == null) return;

    // prepare a polygon with the bounding box information
    var polygon = [];
    polygon.push({ x: min.x, y: min.y });
    polygon.push({ x: min.x, y: max.y });
    polygon.push({ x: max.x, y: max.y });
    polygon.push({ x: max.x, y: min.y });

    this.drawPolygon(polygon);
}

MiniMapExtension.prototype.drawPolygon = function (polygon) {
    // basic check...
    var _this = this;
    if (_this.map == null) return;
    if (_this.geoExtension == null) return;

    // prepare the polygon coordinate to draw it
    var coords = [];
    polygon.forEach(function (point) {
        var geoLoc = _this.geoExtension.lmvToLonLat(point);
        coords.push({ lat: geoLoc.y, lng: geoLoc.x });
    });
    var polyOptions = {
        path: coords,
        strokeColor: '#FF0000',
        strokeOpacity: 0.8,
        strokeWeight: 2,
        fillColor: '#FF0000',
        fillOpacity: 0.1,
    };
    var polygon = new google.maps.Polygon(polyOptions);
    polygon.setMap(_this.map);
}

MiniMapExtension.prototype.cameraChanged = function (camera) {
    // basic check...
    if (this.map == null) return;
    if (this.geoExtension == null) return;

    // adjust the center of the map
    var geoLoc = this.geoExtension.lmvToLonLat(camera.center);
    this.map.setCenter({ lat: geoLoc.y, lng: geoLoc.x });
}


MiniMapExtension.prototype.unload = function () {
    this.viewer.toolbar.removeControl(this.subToolbar);
    if (this.panel !== null) this.panel.setVisible(false);
    return true;
};

Autodesk.Viewing.theExtensionManager.registerExtension('Autodesk.Sample.MiniMapExtension', MiniMapExtension);

// *******************************************
// MiniMap Panel
// *******************************************
function MiniMapPanel(viewer, container, id, title, options) {
    this.viewer = viewer;
    Autodesk.Viewing.UI.DockingPanel.call(this, container, id, title, options);

    // the style of the docking panel
    // use this built-in style to support Themes on Viewer 4+
    this.container.classList.add('docking-panel-container-solid-color-a');
    this.container.style.top = "10px";
    this.container.style.left = "10px";
    this.container.style.width = "300px";
    this.container.style.height = "300px";
    this.container.style.resize = "auto";

    // this is where we should place the content of our panel
    var div = document.createElement('div');
    div.id = 'googlemap';
    this.container.appendChild(div);
}
MiniMapPanel.prototype = Object.create(Autodesk.Viewing.UI.DockingPanel.prototype);
MiniMapPanel.prototype.constructor = MiniMapPanel;