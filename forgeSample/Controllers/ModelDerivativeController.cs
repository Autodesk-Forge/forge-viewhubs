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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using System.Net.Http;
using System.Net;
using System.IO;

namespace forgeSample.Controllers
{
    [ApiController]
    public class ModelDerivativeController : ControllerBase
    {
        // HttpClient has been designed to be re-used for multiple calls. 
        // Even across multiple threads. 
        // https://stackoverflow.com/a/22561368/4838205
        private static HttpClient _httpClient;

        private const string FORGE_BASE_URL = "https://developer.api.autodesk.com";

        /// <summary>
        /// Start the translation job for a give bucketKey/objectName
        /// </summary>
        /// <param name="objModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/forge/modelderivative/jobs")]
        public async Task<dynamic> TranslateObject([FromBody]ObjectModel objModel)
        {
            Credentials credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            DerivativesApi derivative = new DerivativesApi();
            derivative.Configuration.AccessToken = credentials.TokenInternal;

            var manifest = await derivative.GetManifestAsync(objModel.urn);
            if (manifest.status == "inprogress") return null; // another job in progress

            // prepare the payload
            List<JobPayloadItem> outputs = new List<JobPayloadItem>()
            {
                new JobPayloadItem(JobPayloadItem.TypeEnum.Ifc)
            };

            JobPayload job = new JobPayload(new JobPayloadInput(objModel.urn), new JobPayloadOutput(outputs));

            // start the translation
            dynamic jobPosted = await derivative.TranslateAsync(job, false/* do not force re-translate */);
            return jobPosted;
        }

        [HttpGet]
        [Route("api/forge/modelderivative/{urn}/{outputType}")]
        public async Task<IActionResult> DownloadDerivative(string urn, string outputType)
        {
            Credentials credentials = await Credentials.FromSessionAsync(base.Request.Cookies, Response.Cookies);
            DerivativesApi derivative = new DerivativesApi();
            derivative.Configuration.AccessToken = credentials.TokenInternal;

            var manifest = await derivative.GetManifestAsync(urn);
            foreach (KeyValuePair<string, dynamic> output in new DynamicDictionaryItems(manifest.derivatives))
            {
                if (output.Value.outputType == outputType)
                {
                    // already translated!

                    if (_httpClient == null)
                    {
                        _httpClient = new HttpClient(
                          // this should avoid HttpClient seaching for proxy settings
                          new HttpClientHandler()
                          {
                              UseProxy = false,
                              Proxy = null
                          }, true);
                        _httpClient.BaseAddress = new Uri(FORGE_BASE_URL);
                        ServicePointManager.DefaultConnectionLimit = int.MaxValue;
                    }

                    // request to download file
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}/modelderivative/v2/designdata/{1}/manifest/{2}", FORGE_BASE_URL, urn, output.Value.children[0].urn));
                    request.Headers.Add("Authorization", "Bearer " + credentials.TokenInternal); // add our Access Token
                    HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    Stream file = await response.Content.ReadAsStreamAsync();

                    // stream result to client
                    var cd = new System.Net.Mime.ContentDisposition
                    {
                        FileName = "result.ifc",
                        // always prompt the user for downloading, set to true if you want 
                        // the browser to try to show the file inline
                        Inline = false,
                    };
                    Response.Headers.Add("Content-Disposition", cd.ToString());
                    return File(file, "application/octet-stream");
                }
            }
            return null;
        }
    }

    public class ObjectModel
    {
        /// <summary>
        /// Model for TranslateObject method
        /// </summary>
        public string urn { get; set; }
        public string output { get; set; }
    }
}
