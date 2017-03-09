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
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace DataManagementSample.Controllers
{
  public class DataManagementTreeController : ApiController
  {
    public class TreeNode
    {
      public TreeNode(string id, string text, string type, bool children)
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

    private string AccessToken
    {
      get
      {
        var cookies = Request.Headers.GetCookies();
        var accessToken = cookies[0].Cookies[0].Value;
        return accessToken;
      }
    }

    [HttpGet]
    [Route("api/forge/dataManagement/tree")]
    public async Task<IList<TreeNode>> GetDataManagementTreeAsync([FromUri]string id)
    {
      IList<TreeNode> nodes = new List<TreeNode>();

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
        }
      }

      return nodes;
    }

    private async Task<IList<TreeNode>> GetHubsAsync()
    {
      IList<TreeNode> nodes = new List<TreeNode>();

      HubsApi hubsApi = new HubsApi();
      hubsApi.Configuration.AccessToken = AccessToken;

      var hubs = await hubsApi.GetHubsAsync();
      string urn = string.Empty;
      foreach (KeyValuePair<string, dynamic> hubInfo in new DynamicDictionaryItems(hubs.data))
      {
        TreeNode hubNode = new TreeNode(hubInfo.Value.links.self.href, hubInfo.Value.attributes.name, "hubs", true);
        nodes.Add(hubNode);
      }

      return nodes;
    }

    private async Task<IList<TreeNode>> GetProjectsAsync(string href)
    {
      IList<TreeNode> nodes = new List<TreeNode>();
      string[] idParams = href.Split('/');

      string hubId = idParams[idParams.Length - 1];
      ProjectsApi projectsApi = new ProjectsApi();
      projectsApi.Configuration.AccessToken = AccessToken;
      var projects = await projectsApi.GetHubProjectsAsync(hubId);
      foreach (KeyValuePair<string, dynamic> projectInfo in new DynamicDictionaryItems(projects.data))
      {
        TreeNode projectNode = new TreeNode(projectInfo.Value.links.self.href, projectInfo.Value.attributes.name, "projects", true);
        nodes.Add(projectNode);
      }

      return nodes;
    }

    private async Task<IList<TreeNode>> GetProjectContents(string href)
    {
      IList<TreeNode> nodes = new List<TreeNode>();
      string[] idParams = href.Split('/');

      string hubId = idParams[idParams.Length - 3];
      string projectId = idParams[idParams.Length - 1];

      ProjectsApi projectApi = new ProjectsApi();
      projectApi.Configuration.AccessToken = AccessToken;
      var project = await projectApi.GetProjectAsync(hubId, projectId);
      var rootFolderHref = project.data.relationships.rootFolder.meta.link.href;

      return await GetFolderContents(rootFolderHref);
    }
    
    private async Task<IList<TreeNode>> GetFolderContents(string href)
    {
      IList<TreeNode> nodes = new List<TreeNode>();
      string[] idParams = href.Split('/');

      string folderId = idParams[idParams.Length - 1];
      string projectId = idParams[idParams.Length - 3];


      FoldersApi folderApi = new FoldersApi();
      folderApi.Configuration.AccessToken = AccessToken;
      var folderContents = await folderApi.GetFolderContentsAsync(projectId, folderId);
      foreach (KeyValuePair<string, dynamic> folderContentItem in new DynamicDictionaryItems(folderContents.data))
      {
        TreeNode itemNode = new TreeNode(folderContentItem.Value.links.self.href, folderContentItem.Value.attributes.displayName, (string)folderContentItem.Value.type, ((string)folderContentItem.Value.type) == "folders");
        nodes.Add(itemNode);
      }

      return nodes;
    }
  }
}
