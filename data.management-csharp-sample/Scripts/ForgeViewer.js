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

// This script file is based on the tutorial:
// https://developer.autodesk.com/en/docs/viewer/v2/tutorials/basic-application/

var viewerApp;
var fileName;

function launchViewer(urn, name) {
    var options = {
        env: 'AutodeskProduction',
    };
    fileName = name;
    var documentId = 'urn:' + urn;
    Autodesk.Viewing.Initializer(options, function onInitialized() {
        Autodesk.Viewing.endpoint.setEndpointAndApi(window.location.origin + '/api/forge/viewerproxy', '', true);
        viewerApp = new Autodesk.Viewing.ViewingApplication('forgeViewer');
        viewerApp.registerViewer(viewerApp.k3D, Autodesk.Viewing.Private.GuiViewer3D);
        viewerApp.loadDocument(documentId, onDocumentLoadSuccess, onDocumentLoadFailure);
    });
}

var viewer;

function onDocumentLoadSuccess(doc) {

    // We could still make use of Document.getSubItemsWithProperties()
    // However, when using a ViewingApplication, we have access to the **bubble** attribute,
    // which references the root node of a graph that wraps each object from the Manifest JSON.
    var viewables = viewerApp.bubble.search({ 'type': 'geometry' });
    if (viewables.length === 0) {
        console.error('Document contains no viewables.');
        return;
    }

    // Choose any of the avialble viewables
    viewerApp.selectItem(viewables[0].data, onItemLoadSuccess, onItemLoadFail);

    NOP_VIEWER.loadExtension('Autodesk.Sample.ExportExcel', { 'fileName': fileName, 'getToken': getForgeToken, 'urn': doc.myPath });
    NOP_VIEWER.loadExtension('Autodesk.Sample.CivilExtension');
    NOP_VIEWER.loadExtension('Autodesk.Viewing.WebVR');
}

function onDocumentLoadFailure(viewerErrorCode) { }

function onItemLoadSuccess(viewer, item) { }

function onItemLoadFail(errorCode) { }

function getForgeToken() {
    var token = '';
    jQuery.ajax({
        url: '/api/forge/oauth/token',
        success: function (res) {
            token = res;
        },
        async: false
    });
    return token;
}