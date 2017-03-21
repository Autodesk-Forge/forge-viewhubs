# Attention

This is a work in progres, use carefully.

# data.management-csharp-sample

Basic 3-legged OAuth and Data Management API access

![Platforms](https://img.shields.io/badge/platform-Windows-lightgray.svg)
![.NET](https://img.shields.io/badge/.NET-4.5.2-blue.svg)
[![ASP.NET](https://img.shields.io/badge/ASP.NET-4.5.2-blue.svg)](https://asp.net/)
[![License](http://img.shields.io/:license-mit-blue.svg)](http://opensource.org/licenses/MIT)

[![oAuth2](https://img.shields.io/badge/oAuth2-v1-green.svg)](http://developer.autodesk.com/)
[![Data-Management](https://img.shields.io/badge/Data%20Management-v1-green.svg)](http://developer.autodesk.com/)
[![OSS](https://img.shields.io/badge/OSS-v2-green.svg)](http://developer.autodesk.com/)
[![Model-Derivative](https://img.shields.io/badge/Model%20Derivative-v2-green.svg)](http://developer.autodesk.com/)

# Description

This sample show a basic tree view with Hubs, Projects, Folders and Items. It does not use the ASP.NET native [TreeView](https://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.treeview.aspx) due its limitations, but the [jsTree](https://www.jstree.com/) library, that have support for menus, reload, among other features.

The tree view will display personal enterprise (shared) hubs, with respective projects. All BIM 360 projects under the specified account are listed, but an authenticated user can only expand/interact if he/she is added to the project. The following image demonstrate a tipical tree view:

![](DM_BIM360.png) 

It allow upload of files to **Projects** and **Folders**. For new files, a version 1.0 is automatically created, following [this tutorial](https://developer.autodesk.com/en/docs/data/v2/tutorials/upload-file/), until step 5.

## Run Locally

#### Setup

Open the **web.config** file and adjust the Forge Client ID & Secret. If you plan to deploy to Appharbor, configure the variables on the host settings (no need to change this web.config file).

```xml
<appSettings>
  <add key="FORGE_CLIENT_ID" value="" />  <add key="FORGE_CLIENT_SECRET" value="" />  <add key="FORGE_CALLBACK_URL" value="http://localhost:3000/api/forge/callback/oauth" />  <add key="FORGE_BIM360_ACCOUNT_ID" value="" />
</appSettings>
```

No need to adjust the **FORGE\_CALLBACK\_URL** appSettings to run it locally. Optionally, the **FORGE\_BIM360\_ACCOUNT\_ID** is the Account ID [provisioned](https://developer.autodesk.com/en/docs/bim360/v1/tutorials/get-access-to-account/) to this Client ID & Secret pair and it's used to list and manipulate projects, [see more](https://developer.autodesk.com/en/docs/bim360/v1/reference/http/).

Compile the solution, Visual Studio should download the NUGET packages ([Autodesk Forge](https://www.nuget.org/packages/Autodesk.Forge/), [RestSharp](https://www.nuget.org/packages/RestSharp) and [Newtonsoft.Json](https://www.nuget.org/packages/newtonsoft.json/)). 

#### Usage

To use, right-click on a Project or Folder to **Upload** files. The tree view should reload once the file is uploaded. If available, right-click on "BIM 360 Projects" to create new BIM 360 Projects.

# Deployment

This sample still a work in progress, not ready for production. For Appharbor deployment, following [this steps to configure your Forge Client ID & Secret](http://adndevblog.typepad.com/cloud_and_mobile/2017/01/deploying-forge-aspnet-samples-to-appharbor.html).

# License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for full details.

## Written by

Augusto Goncalves [@augustomaia](https://twitter.com/augustomaia), [Forge Partner Development](http://forge.autodesk.com)