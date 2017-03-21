using Autodesk.Forge;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

     //[JsonProperty("language")]
      //public string Language { get; set; }

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
      request.AddParameter("account_id", ConfigVariables.FORGE_BIM360_ACCOUNT, ParameterType.UrlSegment);
      request.AddHeader("Authorization", "Bearer " + bearer.access_token);
      request.AddParameter("application/json", body , ParameterType.RequestBody);
      IRestResponse response = await client.ExecuteTaskAsync(request);

      return response.Content; // ToDo
    }
  }
}
