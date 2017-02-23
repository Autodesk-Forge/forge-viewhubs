using Autodesk.Forge;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace DataManagementSample.WebAPI
{
  public class OAuthController : ApiController
  {
    [HttpGet]
    [Route("api/forge/callback/oauth")] // see Web.Config 
    public async Task OAuthCallback(string code)
    {
      ThreeLeggedApi oauth = new ThreeLeggedApi();
      dynamic bearer = await oauth.GettokenAsync(Code.Config. FORGE_CLIENT_ID, Code.Config. FORGE_CLIENT_SECRET, oAuthConstants.AUTHORIZATION_CODE, code, Code.Config.FORGE_CALLBACK_URL);
      OAuth auth = JsonConvert.DeserializeObject<OAuth>(bearer.ToString());

      // store the OAuth somewhere...

      HttpContext.Current.Response.Redirect("/");
    }

    public class OAuth
    {
      [JsonProperty("access_token")]
      public string AccessToken { get; set; }

      [JsonProperty("refresh_token")]
      public string RefreshToken { get; set; }

      [JsonProperty("token_type")]
      public string TokenType { get; set; }

      private int _expires_in = 0;

      [JsonProperty("expires_in")]
      public int ExpiresIn
      {
        set
        {
          _expires_in = value;
          _expiresAt = DateTime.Now.AddSeconds(value);

        }
      }

      private DateTime _expiresAt;
      public DateTime ExpiresAt
      {
        get
        {
          return _expiresAt;
        }
      }
    }
  }
}