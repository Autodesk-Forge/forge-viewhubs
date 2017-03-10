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

It allow upload of files to **Projects** and **Folders**. For new files, a version 1.0 is automatically created, following [this tutorial](https://developer.autodesk.com/en/docs/data/v2/tutorials/upload-file/), until step 5.

## Run Locally

Open the **web.config** file and adjust the Forge Client ID & Secret. If you plan to deploy to Appharbor, configure the variables on the host settings (no need to change this web.config file).

```xml
<appSettings>
  <add key="FORGE_CLIENT_ID" value="" />
  <add key="FORGE_CLIENT_SECRET" value="" />
</appSettings>
```

No need to adjust the **FORGE\_CALLBACK\_URL** appsetting to run it locally. Compile the solution, Visual Studio should download the NUGET packages ([Autodesk Forge](https://www.nuget.org/packages/Autodesk.Forge/), [RestSharp](https://www.nuget.org/packages/RestSharp) and [Newtonsoft.Json](https://www.nuget.org/packages/newtonsoft.json/))

To use, right-click on a Project or Folder to **Upload** files. The tree view should reload once the file is uploaded. 

# Deployment

For Appharbor deployment, following [this steps to configure your Forge Client ID & Secret](http://adndevblog.typepad.com/cloud_and_mobile/2017/01/deploying-forge-aspnet-samples-to-appharbor.html).

# License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).
Please see the [LICENSE](LICENSE) file for full details.

## Written by

Augusto Goncalves [@augustomaia](https://twitter.com/augustomaia), [Forge Partner Development](http://forge.autodesk.com)