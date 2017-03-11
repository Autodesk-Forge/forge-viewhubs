using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Autodesk.Forge;
using System.Threading.Tasks;
using Autodesk.Forge.Model;
using System.Text.RegularExpressions;

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
          new Scope[] { Scope.DataRead, Scope.DataCreate, Scope.AccountWrite });
        Response.Redirect(oauthUrl);
      }

      return;
    }
  }
}