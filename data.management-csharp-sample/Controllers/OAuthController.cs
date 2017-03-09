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
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace DataManagementSample.WebAPI
{
  public class OAuthController : ApiController
  {
    [HttpGet]
    [Route("api/forge/callback/oauth")] // see Web.Config FORGE_CALLBACK_URL variable
    public async Task<HttpResponseMessage> OAuthCallback(string code)
    {
      ThreeLeggedApi oauth = new ThreeLeggedApi();
      dynamic bearer = await oauth.GettokenAsync(Code.Config.FORGE_CLIENT_ID, Code.Config.FORGE_CLIENT_SECRET, oAuthConstants.AUTHORIZATION_CODE, code, Code.Config.FORGE_CALLBACK_URL);
      OAuth auth = JsonConvert.DeserializeObject<OAuth>(bearer.ToString());

      // You can store the oauth Access Token and Refresh Token on your own authentication approach
      // The Refresh Token can be used later to get a new Access Token


      // For this basic sample, let's sent a Cooke to the end-user with the Access Token that only give
      // access to his/her own data, so no security breach (assuming a HTTPS connection), but there is a 
      // accountability concern here (as the end-user will use this token to perform operation on the app behalf)
      HttpResponseMessage res = Request.CreateResponse(System.Net.HttpStatusCode.Moved /* rorce redirect */);
      CookieHeaderValue cookie = new CookieHeaderValue(Code.Config.FORGE_OAUTH, auth.AccessToken);
      cookie.Expires = auth.ExpiresAt;
      cookie.Path = "/";
      res.Headers.AddCookies(new CookieHeaderValue[] { cookie });
      res.Headers.Location = new Uri("/", UriKind.Relative); // back to / (root, default)

      return res;
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