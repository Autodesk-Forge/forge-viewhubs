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
    return true;
};

MiniMapExtension.prototype.onToolbarCreated = function () {
    // Create a new toolbar group if it doesn't exist
    this._group = this.viewer.toolbar.getControl('customExtensions');
    if (!this._group) {
        this._group = new Autodesk.Viewing.UI.ControlGroup('customExtensions');
        this.viewer.toolbar.addControl(this._group);
    }

    // Add a new button to the toolbar group
    this._button = new Autodesk.Viewing.UI.Button('showMiniMap');
    this._button.onClick = (ev) => {
        if (this.geoExtension === null || !this.geoExtension.hasGeolocationData()) {
            alert('Model does not contain geo location information');
            return;
        }

        this._enabled = !this._enabled;
        this._button.setState(this._enabled ? 0 : 1);

        // if null, create it
        if (this.panel == null) {
            this.panel = new MiniMapPanel(this.viewer, this.viewer.container, 'miniMapPanel', 'Mini Map');
        }

        // show/hide docking panel
        this.panel.setVisible(!this.panel.isVisible());

        if (!this._enabled) return;

        // initialize the map
        if (this.map == null) {
            this.map = new google.maps.Map(document.getElementById('googlemap'), {
                zoom: 16,
                center: { lat: 0, lng: 0 },
                mapTypeId: 'satellite',
                rotateControl: false,
                streetViewControl: false,
                tilt: 0
            });
        }

        // draw model boundoung box & center
        var bb = this.viewer.model.getBoundingBox();
        this.drawBoundingBox(bb.min, bb.max);
        this.cameraChanged(this.viewer.autocam); // first run (center of the model)

    };
    this._button.setToolTip('Show map location');
    this._button.container.children[0].classList.add('fas', 'fa-map-marked');
    this._group.addControl(this._button);

    // listen to camera changes
    this.viewer.addEventListener(Autodesk.Viewing.CAMERA_CHANGE_EVENT, (e) => { this.cameraChanged(e.target.autocam) });
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
    if (this.viewer.toolbar !== null) this.viewer.toolbar.removeControl(this.subToolbar);
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