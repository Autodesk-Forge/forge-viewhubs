using System.Web.Configuration;

namespace DataManagementSample
{
  public static class ConfigVariables
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
    /// The BIM 360 Account ID provisioned to this client ID & secret
    /// </summary>
    internal static string FORGE_BIM360_ACCOUNT { get { return GetAppSetting("FORGE_BIM360_ACCOUNT_ID"); } }

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