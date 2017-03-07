﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
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
        return (Request.Cookies[Code.Config.FORGE_OAUTH] != null && !string.IsNullOrEmpty(Request.Cookies[Code.Config.FORGE_OAUTH].Value));
      }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsForgeAuthorized)
      {
        // redirect to Autodesk Accounts Sign-in page
        ThreeLeggedApi _threeLeggedApi = new ThreeLeggedApi();
        string oauthUrl = _threeLeggedApi.Authorize(
          Code.Config.FORGE_CLIENT_ID, 
          oAuthConstants.CODE,
          Code.Config.FORGE_CALLBACK_URL,
          new Scope[] { Scope.DataRead });
        Response.Redirect(oauthUrl);
      }
    }
  }
}