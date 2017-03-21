using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataManagementSample.Controllers
{
  public static class Utils
  {
    public static string Base64Encode(this string plainText)
    {
      var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
      return System.Convert.ToBase64String(plainTextBytes);
    }
  }
}