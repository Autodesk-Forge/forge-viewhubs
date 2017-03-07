using System.Web.Configuration;

namespace DataManagementSample.Code
{
  public static class Config
  {
    internal const string FORGE_OAUTH = "ForgeOAuth";

    /// <summary>
    /// The client ID of this app
    /// </summary>
    internal static string FORGE_CLIENT_ID { get { return GetAppSetting("FORGE_CLIENT_ID"); } }

    /// <summary>
    /// The client secret of this app
    /// </summary>
    internal static string FORGE_CLIENT_SECRET { get { return GetAppSetting("FORGE_CLIENT_SECRET"); } }

    /// <summary>
    /// The client secret of this app
    /// </summary>
    internal static string FORGE_CALLBACK_URL { get { return GetAppSetting("FORGE_CALLBACK_URL"); } }

    /// <summary>
    /// Read settings from web.config.
    /// See appSettings section for more details.
    /// </summary>
    /// <param name="settingKey"></param>
    /// <returns></returns>
    private static string GetAppSetting(string settingKey)
    {
      return WebConfigurationManager.AppSettings[settingKey];
    }
  }
}