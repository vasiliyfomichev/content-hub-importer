using ContentHub.Importer.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContentHub.Importer.Providers
{
    public class XMLSitemapService
    {
        public static List<Asset> GetAssets(string path)
        {
            Console.WriteLine($"Starting to parse {path} sitemap.");
            var webGet = new HtmlWeb();
            var doc = webGet.Load(path);
            var urlNodes = doc.DocumentNode.SelectNodes("url");
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

            foreach(var url in urls)
            {
                assets.AddRange(new UrlService().GetImages(url.Url));
            }
            
            Console.WriteLine($"Found {assets.Count} assets to import.");
            return assets;
        }
    }
}