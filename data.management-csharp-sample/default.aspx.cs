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
using Autodesk.Forge;

namespace DataManagementSample
{
  public partial class _default : System.Web.UI.Page
  {
    private bool IsForgeAuthorized
    {
      get
      {
        // check if there is a Cookie with the Access Token (this is a simple approach, not entirely safe)
        return (Request.Cookies[ConfigVariables.FORGE_OAUTH] != null && !string.IsNullOrEmpty(Request.Cookies[ConfigVariables.FORGE_OAUTH].Value));
      }
    }

    private string AccessToken
    {
      get
      {
        return Request.Cookies[ConfigVariables.FORGE_OAUTH].Value;
      }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsForgeAuthorized)
      {
        // redirect to Autodesk Accounts Sign-in page
        ThreeLeggedApi _threeLeggedApi = new ThreeLeggedApi();
        string oauthUrl = _threeLeggedApi.Authorize(
          ConfigVariables.FORGE_CLIENT_ID,
          oAuthConstants.CODE,
          ConfigVariables.FORGE_CALLBACK_URL,
          new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.DataWrite, Scope.AccountWrite, Scope.BucketDelete });
        Response.Redirect(oauthUrl);
      }

      return;
    }
  }
}