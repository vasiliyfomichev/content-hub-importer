using ContentHub.Importer.Utils;
using ExcelDataReader;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Sdk.Contracts.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Linq;
using ContentHub.Importer.Providers;

namespace ContentHub.Importer
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.WriteLine("Welcome to the Content Hub Importer.");
            Console.WriteLine("Please select the source of the asset import: [E] Excel, [X] XML Sitemap, [U] URL");
            var option = Console.ReadKey();
            var assets = new List<Asset>();

            switch (option.KeyChar.ToString().ToLower())
            {
                case "e":
                    Console.WriteLine("Importing from Excel.");
                    Console.WriteLine("Please enter the full path to file: ");
                    var excelPath = Console.ReadLine();
                    excelPath = string.IsNullOrWhiteSpace(excelPath) ? @"C:\Users\vasfomic\Desktop\Content Hub Importer\Content Hub Importer\data\mock_data.xlsx" : excelPath;
                    assets.AddRange(ExcelService.GetAssets(excelPath).Take(1));
                    break;
                case "x":
                    Console.WriteLine("Importing from XML Sitemap.");
                    Console.WriteLine("Please enter the full URL to the XML Sitemap: ");
                    var sitemapUrl = Console.ReadLine();
                    sitemapUrl = string.IsNullOrWhiteSpace(sitemapUrl) ? @"https://www.cmsbestpractices.com/sitemap-1.xml" : sitemapUrl;
                    assets.AddRange(XMLSitemapService.GetAssets(sitemapUrl).Take(1));
                    break;

                case "u":
                    Console.WriteLine("Importing from Excel.");
                    Console.WriteLine("Please enter the URL to crawl:");
                    var url = Console.ReadLine();
                    url = string.IsNullOrWhiteSpace(url) ? @"https://www.cmsbestpractices.com" : url;
                    assets.AddRange((new UrlService()).GetImages(url).Take(1));
                    break;
                default:
                    Console.WriteLine("Urecognized option. Exiting...");
                    return;
            }

        
            foreach (var asset in assets)
            {
                Console.Write($"Importing asset from {asset.OriginUrl}.");
                Asset.SetupAsset(asset).Wait();
                Console.WriteLine("Asset imported.");
            }


            var link = MConnector.Client.Entities.GetAsync(10878).Result;

            //var user = MConnector.Client.Users.GetUserAsync("TestUser").Result;

            //var relation = user.GetRelationAsync<IChildToManyParentsRelation>("UserGroupToUser").Result;
            //if (!relation.Parents.Contains(5))
            //{
            //    relation.Parents.Add(5);
            //}
            //MConnector.Client.Entities.SaveAsync(user).Wait();


            Console.WriteLine("Import completed.");
        }


    }
}
