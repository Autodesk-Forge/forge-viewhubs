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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using RestSharp;
using Newtonsoft.Json;
using Hangfire;
using System.Diagnostics;

namespace forgeSample.Controllers
{
    public class DataManagementController : ControllerBase
    {
        private IWebHostEnvironment _env;
        private static RestClient client = new RestClient("https://developer.api.autodesk.com");
        public DataManagementController(IWebHostEnvironment env)
        {
            _env = env;
        }

        /// <summary>
        /// Credentials on this request
        /// </summary>
        private Credentials Credentials { get; set; }

        public string CallbackUrl { get { return Credentials.GetAppSetting("FORGE_WEBHOOK_URL") + "/api/forge/callback/webhook"; } }
        public string VersionId { get { return Credentials.GetAppSetting("VERSION_ID"); } }

        /// <summary>
        /// GET TreeNode passing the ID
        /// </summary>
        [HttpGet]
        [Route("api/forge/datamanagement")]
        public async Task<IList<jsTreeNode>> GetTreeNodeAsync(string id)
        {
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            if (Credentials == null) { return null; }

            IList<jsTreeNode> nodes = new List<jsTreeNode>();

            if (id == "#") // root
                return await GetHubsAsync();
            else
            {
                string[] idParams = id.Split('/');
                string resource = idParams[idParams.Length - 2];
                switch (resource)
                {
                    case "hubs": // hubs node selected/expanded, show projects
                        return await GetProjectsAsync(id);
                    case "projects": // projects node selected/expanded, show root folder contents
                        return await GetProjectContents(id);
                    case "folders": // folders node selected/expanded, show folder contents
                        return await GetFolderContents(id);
                    case "items":
                        return await GetItemVersions(id);
                }
            }

            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetHubsAsync()
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();

            // the API SDK
            HubsApi hubsApi = new HubsApi();
            hubsApi.Configuration.AccessToken = Credentials.TokenInternal;

            var hubs = await hubsApi.GetHubsAsync();
            foreach (KeyValuePair<string, dynamic> hubInfo in new DynamicDictionaryItems(hubs.data))
            {
                // check the type of the hub to show an icon
                string nodeType = "hubs";
                switch ((string)hubInfo.Value.attributes.extension.type)
                {
                    case "hubs:autodesk.core:Hub":
                        nodeType = "hubs"; // if showing only BIM 360, mark this as 'unsupported'
                        break;
                    case "hubs:autodesk.a360:PersonalHub":
                        nodeType = "personalHub"; // if showing only BIM 360, mark this as 'unsupported'
                        break;
                    case "hubs:autodesk.bim360:Account":
                        nodeType = "bim360Hubs";
                        break;
                }

                // create a treenode with the values
                jsTreeNode hubNode = new jsTreeNode(hubInfo.Value.links.self.href, hubInfo.Value.attributes.name, nodeType, !(nodeType == "unsupported"));
                nodes.Add(hubNode);
            }

            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetProjectsAsync(string href)
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();

            // the API SDK
            ProjectsApi projectsApi = new ProjectsApi();
            projectsApi.Configuration.AccessToken = Credentials.TokenInternal;

            // extract the hubId from the href
            string[] idParams = href.Split('/');
            string hubId = idParams[idParams.Length - 1];

            var projects = await projectsApi.GetHubProjectsAsync(hubId);
            foreach (KeyValuePair<string, dynamic> projectInfo in new DynamicDictionaryItems(projects.data))
            {
                // check the type of the project to show an icon
                string nodeType = "projects";
                switch ((string)projectInfo.Value.attributes.extension.type)
                {
                    case "projects:autodesk.core:Project":
                        nodeType = "a360projects";
                        break;
                    case "projects:autodesk.bim360:Project":
                        nodeType = "bim360projects";
                        break;
                }

                // create a treenode with the values
                jsTreeNode projectNode = new jsTreeNode(projectInfo.Value.links.self.href, projectInfo.Value.attributes.name, nodeType, true);
                nodes.Add(projectNode);
            }

            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetProjectContents(string href)
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();

            // the API SDK
            ProjectsApi projectApi = new ProjectsApi();
            projectApi.Configuration.AccessToken = Credentials.TokenInternal;

            // extract the hubId & projectId from the href
            string[] idParams = href.Split('/');
            string hubId = idParams[idParams.Length - 3];
            string projectId = idParams[idParams.Length - 1];

            var folders = await projectApi.GetProjectTopFoldersAsync(hubId, projectId);
            foreach (KeyValuePair<string, dynamic> folder in new DynamicDictionaryItems(folders.data))
            {
                nodes.Add(new jsTreeNode(folder.Value.links.self.href, folder.Value.attributes.displayName, "folders", true));
            }
            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetFolderContents(string href)
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();

            // the API SDK
            FoldersApi folderApi = new FoldersApi();
            folderApi.Configuration.AccessToken = Credentials.TokenInternal;

            // extract the projectId & folderId from the href
            string[] idParams = href.Split('/');
            string folderId = idParams[idParams.Length - 1];
            string projectId = idParams[idParams.Length - 3];

            // check if folder specifies visible types
            JArray visibleTypes = null;
            dynamic folder = (await folderApi.GetFolderAsync(projectId, folderId)).ToJson();
            if (folder.data.attributes != null && folder.data.attributes.extension != null && folder.data.attributes.extension.data != null && !(folder.data.attributes.extension.data is JArray) && folder.data.attributes.extension.data.visibleTypes != null)
            {
                visibleTypes = folder.data.attributes.extension.data.visibleTypes;
                visibleTypes.Add("items:autodesk.bim360:C4RModel"); // C4R models are not returned on visibleTypes, therefore add them here
            }

            var folderContents = await folderApi.GetFolderContentsAsync(projectId, folderId);
            // the GET Folder Contents has 2 main properties: data & included (not always available)
            var folderData = new DynamicDictionaryItems(folderContents.data);
            var folderIncluded = (folderContents.Dictionary.ContainsKey("included") ? new DynamicDictionaryItems(folderContents.included) : null);

            // let's start iterating the FOLDER DATA
            foreach (KeyValuePair<string, dynamic> folderContentItem in folderData)
            {
                // do we need to skip some items? based on the visibleTypes of this folder
                string extension = folderContentItem.Value.attributes.extension.type;
                if (extension.IndexOf("Folder") /*any folder*/ == -1 && visibleTypes != null && !visibleTypes.ToString().Contains(extension)) continue;

                // if the type is items:autodesk.bim360:Document we need some manipulation...
                if (extension.Equals("items:autodesk.bim360:Document"))
                {
                    // as this is a DOCUMENT, lets interate the FOLDER INCLUDED to get the name (known issue)
                    foreach (KeyValuePair<string, dynamic> includedItem in folderIncluded)
                    {
                        // check if the id match...
                        if (includedItem.Value.relationships.item.data.id.IndexOf(folderContentItem.Value.id) != -1)
                        {
                            // found it! now we need to go back on the FOLDER DATA to get the respective FILE for this DOCUMENT
                            foreach (KeyValuePair<string, dynamic> folderContentItem1 in folderData)
                            {
                                if (folderContentItem1.Value.attributes.extension.type.IndexOf("File") == -1) continue; // skip if type is NOT File

                                // check if the sourceFileName match...
                                if (folderContentItem1.Value.attributes.extension.data.sourceFileName == includedItem.Value.attributes.extension.data.sourceFileName)
                                {
                                    // ready!

                                    // let's return for the jsTree with a special id:
                                    // itemUrn|versionUrn|viewableId
                                    // itemUrn: used as target_urn to get document issues
                                    // versionUrn: used to launch the Viewer
                                    // viewableId: which viewable should be loaded on the Viewer
                                    // this information will be extracted when the user click on the tree node, see ForgeTree.js:136 (activate_node.jstree event handler)
                                    string treeId = string.Format("{0}|{1}|{2}",
                                        folderContentItem.Value.id, // item urn
                                        Base64Encode(folderContentItem1.Value.relationships.tip.data.id), // version urn
                                        includedItem.Value.attributes.extension.data.viewableId // viewableID
                                    );
                                    nodes.Add(new jsTreeNode(treeId, WebUtility.UrlDecode(includedItem.Value.attributes.name), "bim360documents", false));
                                }
                            }
                        }
                    }
                }
                else
                {
                    // non-Plans folder items
                    //if (folderContentItem.Value.attributes.hidden == true) continue;
                    nodes.Add(new jsTreeNode(folderContentItem.Value.links.self.href, folderContentItem.Value.attributes.displayName, (string)folderContentItem.Value.type, true));
                }
            }

            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetItemVersions(string href)
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();

            // the API SDK
            ItemsApi itemApi = new ItemsApi();
            itemApi.Configuration.AccessToken = Credentials.TokenInternal;

            // extract the projectId & itemId from the href
            string[] idParams = href.Split('/');
            string itemId = idParams[idParams.Length - 1];
            string projectId = idParams[idParams.Length - 3];

            var versions = await itemApi.GetItemVersionsAsync(projectId, itemId);
            foreach (KeyValuePair<string, dynamic> version in new DynamicDictionaryItems(versions.data))
            {
                DateTime versionDate = version.Value.attributes.lastModifiedTime;
                string verNum = version.Value.id.Split("=")[1];
                string userName = version.Value.attributes.lastModifiedUserName;

                string urn = string.Empty;
                try { urn = (string)version.Value.relationships.derivatives.data.id; }
                catch { urn = Base64Encode(version.Value.id); } // some BIM 360 versions don't have viewable

                jsTreeNode node = new jsTreeNode(
                    urn,
                    string.Format("v{0}: {1} by {2}", verNum, versionDate.ToString("dd/MM/yy HH:mm:ss"), userName),
                    "versions",
                    false);
                nodes.Add(node);
            }

            return nodes;
        }



        public class jsTreeNode
        {
            public jsTreeNode(string id, string text, string type, bool children)
            {
                this.id = id;
                this.text = text;
                this.type = type;
                this.children = children;
            }

            public string id { get; set; }
            public string text { get; set; }
            public string type { get; set; }
            public bool children { get; set; }
        }

        private const int UPLOAD_CHUNK_SIZE = 5; // Mb

        /// <summary>
        /// Receive a file from the client and upload to the bucket
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("api/forge/datamanagement")]
        public async Task<dynamic> UploadObject([FromForm] UploadFile input)
        {
            // get the uploaded file and save on the server
            var fileSavePath = Path.Combine(_env.ContentRootPath, input.fileToUpload.FileName);
            using (var stream = new FileStream(fileSavePath, FileMode.Create))
                await input.fileToUpload.CopyToAsync(stream);

            // user credentials
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);

            // extract projectId and folderId from folderHref
            string[] hrefParams = input.folderHref.Split("/");
            string projectId = hrefParams[hrefParams.Length - 3];
            string folderId = hrefParams[hrefParams.Length - 1];

            // prepare storage
            ProjectsApi projectApi = new ProjectsApi();
            projectApi.Configuration.AccessToken = Credentials.TokenInternal;
            StorageRelationshipsTargetData storageRelData = new StorageRelationshipsTargetData(StorageRelationshipsTargetData.TypeEnum.Folders, folderId);
            CreateStorageDataRelationshipsTarget storageTarget = new CreateStorageDataRelationshipsTarget(storageRelData);
            CreateStorageDataRelationships storageRel = new CreateStorageDataRelationships(storageTarget);
            BaseAttributesExtensionObject attributes = new BaseAttributesExtensionObject(string.Empty, string.Empty, new JsonApiLink(string.Empty), null);
            CreateStorageDataAttributes storageAtt = new CreateStorageDataAttributes(input.fileToUpload.FileName, attributes);
            CreateStorageData storageData = new CreateStorageData(CreateStorageData.TypeEnum.Objects, storageAtt, storageRel);
            CreateStorage storage = new CreateStorage(new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0), storageData);
            dynamic storageCreated = await projectApi.PostStorageAsync(projectId, storage);

            string[] storageIdParams = ((string)storageCreated.data.id).Split('/');
            string[] bucketKeyParams = storageIdParams[storageIdParams.Length - 2].Split(':');
            string bucketKey = bucketKeyParams[bucketKeyParams.Length - 1];
            string objectName = storageIdParams[storageIdParams.Length - 1];

            // upload the file/object, which will create a new object
            ObjectsApi objects = new ObjectsApi();
            objects.Configuration.AccessToken = Credentials.TokenInternal;

            // get file size
            long fileSize = (new FileInfo(fileSavePath)).Length;

            // decide if upload direct or resumable (by chunks)
            if (fileSize > UPLOAD_CHUNK_SIZE * 1024 * 1024) // upload in chunks
            {
                long chunkSize = 2 * 1024 * 1024; // 2 Mb
                long numberOfChunks = (long)Math.Round((double)(fileSize / chunkSize)) + 1;

                long start = 0;
                chunkSize = (numberOfChunks > 1 ? chunkSize : fileSize);
                long end = chunkSize;
                string sessionId = Guid.NewGuid().ToString();

                // upload one chunk at a time
                using (BinaryReader reader = new BinaryReader(new FileStream(fileSavePath, FileMode.Open)))
                {
                    for (int chunkIndex = 0; chunkIndex < numberOfChunks; chunkIndex++)
                    {
                        string range = string.Format("bytes {0}-{1}/{2}", start, end, fileSize);

                        long numberOfBytes = chunkSize + 1;
                        byte[] fileBytes = new byte[numberOfBytes];
                        MemoryStream memoryStream = new MemoryStream(fileBytes);
                        reader.BaseStream.Seek((int)start, SeekOrigin.Begin);
                        int count = reader.Read(fileBytes, 0, (int)numberOfBytes);
                        memoryStream.Write(fileBytes, 0, (int)numberOfBytes);
                        memoryStream.Position = 0;

                        await objects.UploadChunkAsync(bucketKey, objectName, (int)numberOfBytes, range, sessionId, memoryStream);

                        start = end + 1;
                        chunkSize = ((start + chunkSize > fileSize) ? fileSize - start - 1 : chunkSize);
                        end = start + chunkSize;
                    }
                }
            }
            else // upload in a single call
            {
                using (StreamReader streamReader = new StreamReader(fileSavePath))
                {
                    await objects.UploadObjectAsync(bucketKey, objectName, (int)streamReader.BaseStream.Length, streamReader.BaseStream, "application/octet-stream");
                }
            }

            // cleanup
            string fileName = input.fileToUpload.FileName;
            System.IO.File.Delete(fileSavePath);

            // check if file already exists...
            FoldersApi folderApi = new FoldersApi();
            folderApi.Configuration.AccessToken = Credentials.TokenInternal;
            var filesInFolder = await folderApi.GetFolderContentsAsync(projectId, folderId);
            string itemId = string.Empty;
            foreach (KeyValuePair<string, dynamic> item in new DynamicDictionaryItems(filesInFolder.data))
                if (item.Value.attributes.displayName == fileName)
                    itemId = item.Value.id; // this means a file with same name is already there, so we'll create a new version

            // now decide whether create a new item or new version
            if (string.IsNullOrWhiteSpace(itemId))
            {
                // create a new item
                BaseAttributesExtensionObject baseAttribute = new BaseAttributesExtensionObject(projectId.StartsWith("a.") ? "items:autodesk.core:File" : "items:autodesk.bim360:File", "1.0");
                CreateItemDataAttributes createItemAttributes = new CreateItemDataAttributes(fileName, baseAttribute);
                CreateItemDataRelationshipsTipData createItemRelationshipsTipData = new CreateItemDataRelationshipsTipData(CreateItemDataRelationshipsTipData.TypeEnum.Versions, CreateItemDataRelationshipsTipData.IdEnum._1);
                CreateItemDataRelationshipsTip createItemRelationshipsTip = new CreateItemDataRelationshipsTip(createItemRelationshipsTipData);
                StorageRelationshipsTargetData storageTargetData = new StorageRelationshipsTargetData(StorageRelationshipsTargetData.TypeEnum.Folders, folderId);
                CreateStorageDataRelationshipsTarget createStorageRelationshipTarget = new CreateStorageDataRelationshipsTarget(storageTargetData);
                CreateItemDataRelationships createItemDataRelationhips = new CreateItemDataRelationships(createItemRelationshipsTip, createStorageRelationshipTarget);
                CreateItemData createItemData = new CreateItemData(CreateItemData.TypeEnum.Items, createItemAttributes, createItemDataRelationhips);
                BaseAttributesExtensionObject baseAttExtensionObj = new BaseAttributesExtensionObject(projectId.StartsWith("a.") ? "versions:autodesk.core:File" : "versions:autodesk.bim360:File", "1.0");
                CreateStorageDataAttributes storageDataAtt = new CreateStorageDataAttributes(fileName, baseAttExtensionObj);
                CreateItemRelationshipsStorageData createItemRelationshipsStorageData = new CreateItemRelationshipsStorageData(CreateItemRelationshipsStorageData.TypeEnum.Objects, storageCreated.data.id);
                CreateItemRelationshipsStorage createItemRelationshipsStorage = new CreateItemRelationshipsStorage(createItemRelationshipsStorageData);
                CreateItemRelationships createItemRelationship = new CreateItemRelationships(createItemRelationshipsStorage);
                CreateItemIncluded includedVersion = new CreateItemIncluded(CreateItemIncluded.TypeEnum.Versions, CreateItemIncluded.IdEnum._1, storageDataAtt, createItemRelationship);
                CreateItem createItem = new CreateItem(new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0), createItemData, new List<CreateItemIncluded>() { includedVersion });

                ItemsApi itemsApi = new ItemsApi();
                itemsApi.Configuration.AccessToken = Credentials.TokenInternal;
                var newItem = await itemsApi.PostItemAsync(projectId, createItem);
                return newItem;
            }
            else
            {
                // create a new version
                BaseAttributesExtensionObject attExtensionObj = new BaseAttributesExtensionObject(projectId.StartsWith("a.") ? "versions:autodesk.core:File" : "versions:autodesk.bim360:File", "1.0");
                CreateStorageDataAttributes storageDataAtt = new CreateStorageDataAttributes(fileName, attExtensionObj);
                CreateVersionDataRelationshipsItemData dataRelationshipsItemData = new CreateVersionDataRelationshipsItemData(CreateVersionDataRelationshipsItemData.TypeEnum.Items, itemId);
                CreateVersionDataRelationshipsItem dataRelationshipsItem = new CreateVersionDataRelationshipsItem(dataRelationshipsItemData);
                CreateItemRelationshipsStorageData itemRelationshipsStorageData = new CreateItemRelationshipsStorageData(CreateItemRelationshipsStorageData.TypeEnum.Objects, storageCreated.data.id);
                CreateItemRelationshipsStorage itemRelationshipsStorage = new CreateItemRelationshipsStorage(itemRelationshipsStorageData);
                CreateVersionDataRelationships dataRelationships = new CreateVersionDataRelationships(dataRelationshipsItem, itemRelationshipsStorage);
                CreateVersionData versionData = new CreateVersionData(CreateVersionData.TypeEnum.Versions, storageDataAtt, dataRelationships);
                //MetaData metaData = new MetaData();
                CreateVersion newVersionData = new CreateVersion(new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0), versionData);

                VersionsApi versionsApis = new VersionsApi();
                versionsApis.Configuration.AccessToken = Credentials.TokenInternal;
                dynamic newVersion = await versionsApis.PostVersionAsync(projectId, newVersionData);
                return newVersion;
            }

            

        }

        public async Task<string> GetHubRegion(string hubId)
        {
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            HubsApi hubsApi = new HubsApi();
            hubsApi.Configuration.AccessToken = Credentials.TokenInternal;
            var hub = await hubsApi.GetHubAsync(hubId);
            return hub.data.attributes.region;
        }

        public class HookInputData
        {
            public static string ExtractFolderIdFromHref(string href)
            {
                string[] idParams = href.Split('/');
                Console.WriteLine(href);
                string resource = idParams[idParams.Length - 2];
                string folderId = idParams[idParams.Length - 1];
                if (!resource.Equals("folders")) return string.Empty;
                return folderId;
            }

            public static string ExtractProjectIdFromHref(string href)
            {
                string[] idParams = href.Split('/');
                string resource = idParams[idParams.Length - 4];
                string folderId = idParams[idParams.Length - 3];
                if (!resource.Equals("projects")) return string.Empty;
                return folderId;
            }

            public static string ExtractHubIdFromHref(string href)
            {
                string[] idParams = href.Split('/');
                string resource = idParams[idParams.Length - 2];
                string hubId = idParams[idParams.Length - 1];
                if (!resource.Equals("hubs")) return string.Empty;
                return hubId;
            }
            public string folder { get; set; }
            public string hub { get; set; }

            public string FolderId { get { return ExtractFolderIdFromHref(folder); } }
            public string ProjectId { get { return ExtractProjectIdFromHref(folder); } }
            public string HubId { get { return ExtractHubIdFromHref(hub); } }
        }

        [HttpPost]
        [Route("api/forge/webhook")]
        public async Task<HttpStatusCode> CreateWebHook(string workflowId)
        {
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            if (Credentials == null) { return (HttpStatusCode)401; ; }

            WebhookObject webhook = new WebhookObject();
            webhook.callbackUrl =  CallbackUrl;
            webhook.scope.workflow = workflowId;

            string json = JsonConvert.SerializeObject(webhook);

            dynamic jsonObject = JsonConvert.DeserializeObject<WebhookObject>(json);
            var request = new RestRequest("webhooks/v1/systems/derivative/events/{supportedEvent}/hooks");
            request.AddHeader("Authorization", "Bearer " + Credentials.TokenInternal);
            request.AddHeader("x-ads-region", "US");
            request.AddUrlSegment("supportedEvent", ConvertToString(SupportedEvents.ExtractionFinished));
            request.AddJsonBody(jsonObject);
            var response = client.Post(request);

            return response.StatusCode;

        }

        [HttpGet]
        [Route("api/forge/webhook")]
        public async Task<IList<GetHookData.Hook>> GetHooks()
        {
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            if (Credentials == null) { return null; }
            IList<GetHookData.Hook> hooks = await Hooks();
            return hooks;
        }


        [HttpGet]
        [Route("api/forge/webhook/delete")]
        public async Task<IDictionary<string, HttpStatusCode>> DeleteHook()
        {
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);

            IList<GetHookData.Hook> hooks = await Hooks();
            IDictionary<string, HttpStatusCode> status = new Dictionary<string, HttpStatusCode>();

            foreach (GetHookData.Hook hook in hooks)
            {

                RestRequest request = new RestRequest("/webhooks/v1/systems/data/events/{supportedEvent}/hooks/{webhookId}", Method.DELETE);
                request.AddUrlSegment("supportedEvent", ConvertToString(SupportedEvents.ExtractionFinished));
                request.AddUrlSegment("webhookId", hook.hookId);
                request.AddHeader("Authorization", "Bearer " + Credentials.TokenInternal);
                IRestResponse response = await client.ExecuteAsync(request);
                status.Add(hook.hookId, response.StatusCode);
            }

            return status;

        }


        [HttpPost]
        [Route("api/forge/newItem")]
        public async Task<string> PostNewItem(string folderId, string name, string workflowId, string projectId)
        {
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            if (Credentials == null) { return null ; }

            CreateItemObject createItemObject = new CreateItemObject();
            createItemObject.jsonapi.version = "1.0";
            createItemObject.data.type = "items";
            createItemObject.data.relationships.tip.data.type = "versions";
            createItemObject.data.relationships.tip.data.id = "1";
            createItemObject.data.relationships.parent.data.type = "folders";
            createItemObject.data.relationships.parent.data.id = folderId;
            CreateIncludedObject createIncludedObject = new CreateIncludedObject();
            createIncludedObject.type = "versions";
            createIncludedObject.id = "1";
            createIncludedObject.attributes.name = name;
            createItemObject.included = new List<CreateIncludedObject>() { createIncludedObject };
            createItemObject.meta.workflow = workflowId;
            createItemObject.meta.workflowAttribute.myfoo = 33;
            createItemObject.meta.workflowAttribute.projectId = projectId;
            createItemObject.meta.workflowAttribute.myobject.nested = true;

            string json = JsonConvert.SerializeObject(createItemObject);

            dynamic jsonObject = JsonConvert.DeserializeObject<CreateItemObject>(json);
            var request = new RestRequest("/data/v1/projects/{project_id}/items");
            request.AddHeader("Authorization", "Bearer " + Credentials.TokenInternal);
            request.AddHeader("Content-Type", "application/vnd.api+json");
            request.AddHeader("x-ads-region", "US");
            request.AddUrlSegment("project_id", projectId);
            request.AddQueryParameter("copyFrom", VersionId);
            request.AddJsonBody(jsonObject);
            var response = client.Post(request);

            Console.WriteLine(response.Content.ToString());

            return response.Content;

        }

        public class UploadFile
        {
            //[ModelBinder(BinderType = typeof(FormDataJsonBinder))]
            public string folderHref { get; set; }
            public IFormFile fileToUpload { get; set; }
            // Other properties
        }

        public async Task<IList<GetHookData.Hook>> Hooks()
        {
            Credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            if (Credentials == null) { return null; }

            RestRequest request = new RestRequest("/webhooks/v1/hooks", Method.GET);
            request.AddHeader("Authorization", "Bearer " + Credentials.TokenInternal);
            request.AddUrlSegment("supportedEvent", ConvertToString(SupportedEvents.ExtractionFinished));
            IRestResponse<GetHookData> response = await client.ExecuteAsync<GetHookData>(request);

            return response.Data.data;
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes).Replace("/", "_");
        }

        public class CreateItemObject
        {
            public CreateItemObject()
            {
                this.jsonapi = new CreateJsonAPIObject();
                this.data = new CreateDataObject();
                this.included = new List<CreateIncludedObject>();
                this.meta = new MetaObject();
            }

            public CreateJsonAPIObject jsonapi { get; set; }
            public CreateDataObject data { get; set; }
            public List<CreateIncludedObject> included { get; set; }
            public MetaObject meta { get; set; }
        }

        public class CreateJsonAPIObject
        {
            public string version { get; set; }
        }

        public class CreateDataObject
        {
            public CreateDataObject()
            {
                this.relationships = new CreateDataRelationshipsObject();
            }
            public string type { get; set; }
            public CreateDataRelationshipsObject relationships { get; set; }
        }
     
  
        public class CreateDataRelationshipsObject
        {
            public CreateDataRelationshipsObject()
            {
                this.tip = new CreateRelationshipsTipObject();
                this.parent = new CreateRelationshipsParentObject();
            }
            public CreateRelationshipsTipObject tip { get; set; }
            public CreateRelationshipsParentObject parent { get; set; }

        }
        public class CreateRelationshipsTipObject
        {
            public CreateRelationshipsTipObject()
            {
                this.data = new CreateTipDataObject();
            }

            public CreateTipDataObject data { get; set; }
        }
        public class CreateTipDataObject
        {
            public string type { get; set; }
            public string id { get; set; }
        }
        public class CreateRelationshipsParentObject
        {
            public CreateRelationshipsParentObject()
            {
                this.data = new CreateParentDataObject();
            }

            public CreateParentDataObject data { get; set; }
        }

        public class CreateParentDataObject
        {
            public string type { get; set; }
            public string id { get; set; }
        }

        public class CreateIncludedObject
        {
            public CreateIncludedObject()
            {
                this.attributes = new CreateIncludedObjectAttributes();
            }
            public string type { get; set; }
            public string id { get; set; }
            public CreateIncludedObjectAttributes attributes { get; set; }
        }
        public class CreateIncludedObjectAttributes
        {
            public string name { get; set; }
        }
  

        public class MetaObject
        {
            public MetaObject()
            {
                this.workflowAttribute = new MetaWorkflowAttributeObject();
            }
            public string workflow { get; set; }
            public MetaWorkflowAttributeObject workflowAttribute { get; set; }
        }

        public class MetaWorkflowAttributeObject
        {
            public MetaWorkflowAttributeObject()
            {
                this.myobject = new MetaWorkflowAttributeMyObjectObject();
            }
            public int myfoo { get; set; }
            public string projectId { get; set; }

            public MetaWorkflowAttributeMyObjectObject myobject { get; set; }
        }


        public class MetaWorkflowAttributeMyObjectObject
        {
            public bool nested { get; set; }
        }



        public class WebhookObject
        {

            public WebhookObject()
            {
                this.scope = new Scope();

            }
            //[ModelBinder(BinderType = typeof(FormDataJsonBinder))]
            public string callbackUrl { get; set; }
            public Scope scope { get; set; }
            // Other properties
        }

        public class Scope
        {
            //[ModelBinder(BinderType = typeof(FormDataJsonBinder))]
            public string workflow { get; set; }
            // Other properties
        }

        public class MetaData
        { 
            public string workflow { get; set; }
        }


        public class JobPayloadObject
        {
            public JobPayloadObject()
            {
                this.input = new InputObject();
                this.output = new OutputObject();
                this.misc = new MiscObject();
            }

            public InputObject input { get; set; }
            public OutputObject output { get; set; }
            public MiscObject misc { get; set; }
        }

        public class InputObject
        {
            public string urn { get; set; }
        }

        public class OutputObject
        {
            public OutputObject()
            {
                this.formats = new List<FormatsObject>();
            }

            public List<FormatsObject> formats {get; set;}
        }

        public class FormatsObject
        {
            public string type { get; set; }
        }

        public class MiscObject
        {
            public string workflow { get; set; }

        }

        public enum SupportedEvents
        {
            ExtractionFinished,
            ExtractionUpdated,
        }

        public enum FormatType
        {
            Svf,
            Svf2,
            Thumbnail,
            Stl,
            Step,
            Iges,
            Obj,
            Ifc,
            Dwg 
        }

        private string ConvertToString(SupportedEvents supportedEvents)
        {
            var supportedEvent = "";

            switch (supportedEvents)
            {
                case SupportedEvents.ExtractionFinished:
                    supportedEvent = "extraction.finished";
                    break;
                case SupportedEvents.ExtractionUpdated:
                    supportedEvent = "extraction.updated";
                    break;
            }
            return supportedEvent;
        }

        public static string URNBase64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public class GetHookData
        {
            public Links links { get; set; }
            public List<Hook> data { get; set; }

            public class Links
            {
                public object next { get; set; }
            }

            public class Hook
            {
                public string hookId { get; set; }
                public string tenant { get; set; }
                public string callbackUrl { get; set; }
                public string createdBy { get; set; }
                public string @event { get; set; }
                public DateTime createdDate { get; set; }
                public string system { get; set; }
                public string creatorType { get; set; }
                public string status { get; set; }
                public Scope scope { get; set; }
                public string urn { get; set; }
                public string __self__ { get; set; }

                public class Scope
                {
                    public string folder { get; set; }
                }
            }
        }

    }
}