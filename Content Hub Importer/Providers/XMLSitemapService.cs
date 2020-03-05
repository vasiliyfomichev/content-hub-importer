using ContentHub.Importer.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;

namespace ContentHub.Importer.Providers
{
    public class XMLSitemapService
    {
        public static List<Asset> GetAssets(string path)
        {
            Console.WriteLine($"Starting to parse {path} sitemap.");
            var htmlDoc = new HtmlDocument();
            htmlDoc.OptionReadEncoding = false;
            var request = (HttpWebRequest)WebRequest.Create(path);
            request.Method = "GET";
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    htmlDoc.Load(stream, Encoding.UTF8);
                }
            }
                                 
            var urlNodes = htmlDoc.DocumentNode.SelectNodes("//url");
            Console.WriteLine($"Found {urlNodes.Count} URLs to parse in the sitemap.");

            var assets = new List<Asset>();
            var urls = new List<SitemapUrl>();
            foreach (HtmlNode urlNode in urlNodes)
            {
                urls.Add(new SitemapUrl
                {
                    Url = urlNode.SelectSingleNode("loc").InnerText
                });
            }

            foreach(var url in urls.Take(2))
            {
                assets.AddRange(new UrlService().GetImages(url.Url));
            }

            Console.WriteLine();
            Console.WriteLine($"*************************************************");
            Console.WriteLine($"Found a total of {assets.Count} assets to import.");
            Console.WriteLine($"*************************************************");
            Console.WriteLine();
            return assets;
        }
    }
}