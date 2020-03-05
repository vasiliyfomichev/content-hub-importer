#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ContentHub.Importer.Models;
using ContentHub.Importer.Utils;
using HtmlAgilityPack;
using ContentHub.Importer.Extensions;

#endregion

namespace ContentHub.Importer.Providers
{
    public class UrlService
    {

        #region IUrlService Members


        public IEnumerable<Asset> GetImages(string url)
        {
            Console.WriteLine();
            Console.WriteLine($"Looking for assets in {url}.");
            var images = GetAllImageReferences(url);
            return images;
        }

        #endregion

        #region Private Methods

        private static IEnumerable<Asset> GetAllImageReferences(string url)
        {
            var assets = new List<Asset>();
            var document = new HtmlWeb().Load(url);
            var imageUrls = new List<Image>();
            var uri = new Uri(url);
            Task.WaitAll(new[]
            {
                Task.Run(() => imageUrls.AddRange(UrlUtil.GetMetaImageUrls(document))),
                Task.Run(() => imageUrls.AddRange(ImageUtil.GetImagesFromImageTags(document, url))),
                Task.Run(() => imageUrls.AddRange(ImageUtil.GetImagesFromReferencedCss(document, url, uri.Scheme))),
                Task.Run(() => imageUrls.AddRange(ImageUtil.GetImagesFromInlineCss(document, url))),
                Task.Run(() => imageUrls.AddRange(ImageUtil.GetImagesFromReferencedJs(document, url, uri.Scheme))),
                Task.Run(() => imageUrls.AddRange(ImageUtil.GetImagesFromInlineJs(document, url)))
            });
            imageUrls = imageUrls.DistinctBy(i=>i.Src.ToLower()).ToList();



            foreach (var imageUrl in imageUrls)
            {
                assets.Add(new Asset
                {
                    OriginUrl = imageUrl.Src,
                    Description = string.IsNullOrWhiteSpace(imageUrl.Alt) ? "Imported from URL" : imageUrl.Alt,
                    MarketingDescription = string.IsNullOrWhiteSpace(imageUrl.Alt) ? "Imported from URL" : imageUrl.Alt,
                    LifecycleStatus = "UnderReview",
                    AssetType = "SocialMediaAsset"
                });
            }
            Console.WriteLine($"Found {assets.Count} assets to import.");
            return assets;
        }


        #endregion
    }
}



