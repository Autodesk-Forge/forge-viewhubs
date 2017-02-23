using System.Web.Http;

namespace DataManagementSample.Config
{
  public class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {
      config.MapHttpAttributeRoutes();
    }
  }
}