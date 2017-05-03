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
using Newtonsoft.Json.Converters;
using RestSharp;
using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace DataManagementSample.Controllers
{
  public class BIM360Controller : ApiController
  {
    public class Project
    {
      class CustomDateTimeConverter : IsoDateTimeConverter
      {
        public CustomDateTimeConverter()
        {
          base.DateTimeFormat = "yyyy-MM-dd";
        }
      }

      public Project()
      {
        // some default values..
        Currency = "USD";
        //Language = "en";

      }

      [JsonProperty("name")]
      public string Name { get; set; }

      [JsonProperty("project_type")]
      public string ProjectType { get; set; }

      [JsonProperty("value")]
      public string Value { get; set; }

      [JsonProperty("currency")]
      public string Currency { get; set; }

      [JsonProperty("start_date")]
      [JsonConverter(typeof(CustomDateTimeConverter))]
      public DateTime StartDate { get; set; }

      [JsonProperty("end_date")]
      [JsonConverter(typeof(CustomDateTimeConverter))]
      public DateTime EndDate { get; set; }

      [JsonProperty("hubId")]
      public string HubId { get; set; }

    }

    [HttpPost]
    [Route("api/forge/BIM360/project")]
    public async Task<string> CreateBIM360Project([FromBody]Project newProject)
    {
      TwoLeggedApi twoLeggedApi = new TwoLeggedApi();
      dynamic bearer = await twoLeggedApi.AuthenticateAsync(ConfigVariables.FORGE_CLIENT_ID, ConfigVariables.FORGE_CLIENT_SECRET, "client_credentials", new Scope[] { Scope.AccountWrite });

      string body = JsonConvert.SerializeObject(newProject);

      RestClient client = new RestClient("https://developer.api.autodesk.com");
      RestRequest request = new RestRequest("/hq/v1/accounts/{account_id}/projects", Method.POST);
      request.AddParameter("account_id", newProject.HubId, ParameterType.UrlSegment);
      request.AddHeader("Authorization", "Bearer " + bearer.access_token);
      request.AddParameter("application/json", body, ParameterType.RequestBody);
      IRestResponse response = await client.ExecuteTaskAsync(request);

      return response.Content; // ToDo
    }
  }
}
