#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ContentHub.Importer.Models;
using HtmlAgilityPack;

#endregion

namespace ContentHub.Importer.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class UrlUtil
    {
        public static bool IsUri(string text)
        {
            Uri uriResult;
            var result = Uri.TryCreate(text, UriKind.Absolute, out uriResult);
            return result;
        }

        public static bool IsImageUrl(string url)
        {
            url = url.ToLower();
            var regex = new Regex("^.*.(jpg|gif|jpeg|png|ico)$");
            var match = regex.Match(url);
            return match.Success;
        }

        /// <summary>
        /// Ensures the absolute URL for partial paths (i.e. relative file references).
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="requestUrl">The request URL.</param>
        /// <returns></returns>
        public static string EnsureAbsoluteUrl(string url, string requestUrl)
            {
            var uri = new Uri(requestUrl);
            if (!string.IsNullOrWhiteSpace(url))
            {
                #region Gravatar Bug Fix

                if (url.StartsWith("."))
                {
                    url = string.Format("{0}://{1}", uri.Scheme, url.TrimStart('.'));
                }

                #endregion

                if (url.StartsWith("//"))
                {
                    url = uri.Scheme + ":" + url;
                }
            }
                if ((IsAbsoluteUrl(url) && (url.ToLower().StartsWith("http") || url.ToLower().StartsWith("https"))) 
                || string.IsNullOrWhiteSpace(url)) return url;

            if (string.IsNullOrWhiteSpace(requestUrl)) return url;
            url = url.ToLower()
                .Replace("http://", string.Empty)
                .Replace("https://", string.Empty);

            
            if (!url.StartsWith("..") && url.StartsWith("/"))
                return uri.Scheme + "://" + (uri.Host.EndsWith("/") ? uri.Host : uri.Host + "/") + url.TrimStart('/');

            var urlParts = uri.LocalPath.Split(new[]{'/'}, StringSplitOptions.RemoveEmptyEntries);
            var levelUpCount = 1;
            if (url.StartsWith(".."))
            {
                levelUpCount = new Regex(Regex.Escape("../")).Matches(url).Count;
                url = url.Replace("../", string.Empty);
            }
            url = string.Join("/", urlParts.Take(urlParts.Count() - levelUpCount)) + "/" + url;
            var absoluteUrl =  uri.Scheme + "://" + (uri.Host.EndsWith("/") ? uri.Host : uri.Host + "/") + url;

            return absoluteUrl;
        }

        public static string EnsureAbsoluteUrlFormat(string url, string requestSchema)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;
            if (!url.ToLower().StartsWith("http") && !url.StartsWith("https"))
                url = string.Format("{0}://{1}", requestSchema, url);
            return url;
        }

        public static bool IsAbsoluteUrl(string url)
        {
            Uri result;
            return Uri.TryCreate(url, UriKind.Absolute, out result);
        }

        public static async Task<bool> UrlExistsAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            try
            {
                var client = new HttpClient();
                var httpRequestMsg = new HttpRequestMessage(HttpMethod.Head, url);
                var response = await client.SendAsync(httpRequestMsg).ConfigureAwait(true);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static string GetUrlContent(string url)
        {
            if (string.IsNullOrWhiteSpace(url) && !UrlExistsAsync(url).GetAwaiter().GetResult())
                return null;
            try
            {
                var webRequest = WebRequest.Create(url);

                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                {
                    if (content == null) return null;
                    using (var reader = new StreamReader(content))
                    {
                        var pageMarkup = reader.ReadToEnd();
                        return pageMarkup;
                    }
                }
            }
            catch (Exception ex)
            {
                // log error
                //recover 
                return null;
            }
        }

        public static IEnumerable<Image> GetMetaImageUrls(HtmlDocument document)
        {
            if (document.DocumentNode.SelectSingleNode("//head") == null) throw new Exception("Cannot find the head element.");
            var head = document.DocumentNode.SelectSingleNode("//head");
            if (head == null) return null;
            var headNodes = head.ChildNodes;
            if (headNodes == null || !headNodes.Any()) return null;
            var images = headNodes
                .Select(n =>
                    new Image
                    {
                        Src = n.GetAttributeValue("href", n.GetAttributeValue("content", null)),
                        Alt = n.GetAttributeValue("property", n.GetAttributeValue("name", "Meta image."))
                    }
                )
                .Where(i => !string.IsNullOrWhiteSpace(i.Src) && IsUri(i.Src) && IsImageUrl(i.Src));

            return images;
        }
        
        public static IEnumerable<string> GetCssFilePaths(HtmlDocument document)
        {
            var rootNode = document.DocumentNode;
            if (rootNode == null) return null;

            var linkNodes = rootNode.SelectNodes("//link");
            if (linkNodes == null || !linkNodes.Any())
                return null;

            var nodes = linkNodes.Where(n =>n!=null &&
                !string.IsNullOrWhiteSpace(n.GetAttributeValue("rel", null)) && 
                (n.GetAttributeValue("rel", null).ToLower() == "stylesheet" || 
                (n.GetAttributeValue("type", null)!=null && n.GetAttributeValue("type", null).ToLower() == "text/css")))
                .Select(n=>n.GetAttributeValue("href", null));
            var cssPaths = nodes.Count(n=>n!=null)==0 ? null : nodes.Where(n => n != null);

            return cssPaths;
        }

        public static IEnumerable<string> GetScriptFilePaths(HtmlDocument document)
        {
            var rootNode = document.DocumentNode;
            if (rootNode == null) return null;

            var scriptNodes = rootNode.SelectNodes("//script");
            if (scriptNodes==null || !scriptNodes.Any())
                return null;

            var nodes = scriptNodes
                .Select(n => n.GetAttributeValue("src", null));
            var scriptPaths = nodes.Count(n => n != null) == 0 ? null : nodes.Where(n => n != null);

            return scriptPaths;
        }
    }
}