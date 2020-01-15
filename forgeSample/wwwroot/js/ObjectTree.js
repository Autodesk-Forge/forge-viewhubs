// *******************************************
// Custom Object Tree Extension
// *******************************************
function CustomObjectTreeExtension(viewer, options) {
    Autodesk.Viewing.Extension.call(this, viewer, options);
    this.viewer = viewer;
    this.options = options;
}
 
CustomObjectTreeExtension.prototype = Object.create(Autodesk.Viewing.Extension.prototype);
CustomObjectTreeExtension.prototype.constructor = CustomObjectTreeExtension;
 
CustomObjectTreeExtension.prototype.load = function () {
    Autodesk.Viewing.Private.InstanceTree.prototype.enumNodeChildren = function (node, callback, recursive) {
        var dbId;
        if (typeof node == "number")
            dbId = node; else
            if (node)
                dbId = node.dbId;
 
        var self = this;
 
        if (recursive) {
            if (callback(dbId))
                return dbId;
        }
 
        function traverse(dbId) {
            var res = self.nodeAccess.enumNodeChildren(dbId, function (childId) {
 
                // *** start of changes
                var name = self.getNodeName(childId);
                if (name.indexOf('Generic') == 0) return; // ignore anything starting with 'Generic'
                // *** end of changes
 
                if (callback(childId)){
                    return childId;
                }
 
                if (recursive)
                    return traverse(childId);
            });
            if (res)
                return res;
        }
 
        return traverse(dbId);
    };
    return true;
};
 
CustomObjectTreeExtension.prototype.unload = function () {
    return true;
};
 
Autodesk.Viewing.theExtensionManager.registerExtension('CustomStructurePanelExtension', CustomObjectTreeExtension);
