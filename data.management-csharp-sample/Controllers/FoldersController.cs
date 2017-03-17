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


using Autodesk.Forge;
using Autodesk.Forge.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace DataManagementSample.Controllers
{
  public class FoldersController : ApiController
  {
    private string AccessToken
    {
      get
      {
        var cookies = Request.Headers.GetCookies();
        var accessToken = cookies[0].Cookies[0].Value;
        return accessToken;
      }
    }

    [HttpPost]
    [Route("api/forge/folders/uploadObject")]
    public async Task<Object> UploadObject()//[FromBody]UploadObjectModel obj)
    {
      // basic input validation
      HttpRequest req = HttpContext.Current.Request;
      if (string.IsNullOrWhiteSpace(req.Params["href"]))
        throw new System.Exception("Folder href parameter was not provided.");

      if (req.Files.Count != 1)
        throw new System.Exception("Missing file to upload"); // for now, let's support just 1 file at a time

      string href = req.Params["href"];
      string[] idParams = href.Split('/');
      string folderId = idParams[idParams.Length - 1];
      string projectId = idParams[idParams.Length - 3];
      HttpPostedFile file = req.Files[0];

      // save the file on the server
      var fileSavePath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), file.FileName);
      file.SaveAs(fileSavePath);

      StorageRelationshipsTargetData storageRelData = new StorageRelationshipsTargetData(StorageRelationshipsTargetData.TypeEnum.Folders, folderId);
      CreateStorageDataRelationshipsTarget storageTarget = new CreateStorageDataRelationshipsTarget(storageRelData);
      CreateStorageDataRelationships storageRel = new CreateStorageDataRelationships(storageTarget);
      BaseAttributesExtensionObject attributes = new BaseAttributesExtensionObject(string.Empty, string.Empty, new JsonApiLink(string.Empty), null);
      CreateStorageDataAttributes storageAtt = new CreateStorageDataAttributes(file.FileName, attributes);
      CreateStorageData storageData = new CreateStorageData(CreateStorageData.TypeEnum.Objects, storageAtt, storageRel);
      CreateStorage storage = new CreateStorage(new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0), storageData);

      ProjectsApi projectApi = new ProjectsApi();
      projectApi.Configuration.AccessToken = AccessToken;
      dynamic storageCreated = await projectApi.PostStorageAsync(projectId, storage);

      string[] storageIdParams = ((string)storageCreated.data.id).Split('/');
      var objectName = storageIdParams[storageIdParams.Length - 1];
      string[] bucketIdParams = storageIdParams[storageIdParams.Length - 2].Split(':');
      var bucketKey = bucketIdParams[bucketIdParams.Length - 1];

      // upload the file/object
      ObjectsApi objects = new ObjectsApi();
      objects.Configuration.AccessToken = AccessToken;
      dynamic uploadedObj;
      using (StreamReader streamReader = new StreamReader(fileSavePath))
      {
        uploadedObj = await objects.UploadObjectAsync(bucketKey,
               objectName, (int)streamReader.BaseStream.Length, streamReader.BaseStream,
               "application/octet-stream");
      }

      string type = string.Format(":autodesk.{0}:File", (href.IndexOf("projects/b.") > 0 ? "bim360" : "core"));

      CreateItem item = new CreateItem(new JsonApiVersionJsonapi(JsonApiVersionJsonapi.VersionEnum._0),
        new CreateItemData(CreateItemData.TypeEnum.Items, 
        new CreateStorageDataAttributes(file.FileName, 
        new BaseAttributesExtensionObject("items" + type, "1.0", new JsonApiLink(string.Empty))), 
        new CreateItemDataRelationships(
          new CreateItemDataRelationshipsTip(
            new CreateItemDataRelationshipsTipData(CreateItemDataRelationshipsTipData.TypeEnum.Versions, CreateItemDataRelationshipsTipData.IdEnum._1)), 
          new CreateStorageDataRelationshipsTarget(
            new StorageRelationshipsTargetData(StorageRelationshipsTargetData.TypeEnum.Folders, folderId)))), 
        new System.Collections.Generic.List<CreateItemIncluded>()
        {
          new CreateItemIncluded(CreateItemIncluded.TypeEnum.Versions, CreateItemIncluded.IdEnum._1,     
            new CreateStorageDataAttributes(file.FileName, new BaseAttributesExtensionObject("versions" + type, "1.0", new JsonApiLink(string.Empty))),  
            new CreateItemRelationships(
              new CreateItemRelationshipsStorage(
                new CreateItemRelationshipsStorageData(CreateItemRelationshipsStorageData.TypeEnum.Objects, storageCreated.data.id))))
        }
       );

      ItemsApi itemsApi = new ItemsApi();
      itemsApi.Configuration.AccessToken = AccessToken;
      dynamic newItem = itemsApi.PostItem(projectId, item);

      // cleanup
      File.Delete(fileSavePath);

      return uploadedObj;
    }
  }
}
