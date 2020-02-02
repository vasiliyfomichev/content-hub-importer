#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ContentHub.Importer.Models;
using HtmlAgilityPack;


#endregion

namespace ContentHub.Importer.Utils
{
    public class ImageUtil
    {

        private static List<string> allowedExtensions = new List<string> { "png", "gif", "jpg", "ico", "jpeg" };

        #region Methods

        public static IEnumerable<Image> GetImagesFromImageTags(HtmlDocument document, string url)
        {
            var images = document.DocumentNode.Descendants("img")
                    .Select(e =>
                        new Image
                        {
                            Src = UrlUtil.EnsureAbsoluteUrl(e.GetAttributeValue("src", null), url),
                            Alt = e.GetAttributeValue("alt", null)
                        })
                    .Where(s => !string.IsNullOrEmpty(s.Src))
                    .ToList();
            return images;
        }

        public static IEnumerable<Image> GetImagesFromReferencedCss(HtmlDocument document, string url, string requestSchema)
        {
            var cssPaths = UrlUtil.GetCssFilePaths(document);
            var images = new List<Image>();
            if (cssPaths == null || !cssPaths.Any()) return images;
            foreach (var path in cssPaths)
            {
                var imageReferences = GetImagesFromCssFile(path, requestSchema);
                if (imageReferences == null || !imageReferences.Any())
                    continue;
                images.AddRange(imageReferences.Select(i => new Image
                {
                    Src = UrlUtil.EnsureAbsoluteUrl(i.Src, path),
                    Alt = i.Alt
                }));
            }
            return images;
        }

        public static IEnumerable<Image> GetImagesFromInlineCss(HtmlDocument document, string url)
        {
            var documentRoot = document.DocumentNode;
            var documentText = documentRoot.InnerHtml;

            var images = new List<Image>();
            var regex = "url(['\"]?(?<url>[^)]+?)['\"]?)";

              var imageReferences = GetImagesFromText(documentText, regex);
            if (imageReferences == null || !imageReferences.Any())
                return images;
            images.AddRange(imageReferences.Select(i => new Image
            {
                Src = UrlUtil.EnsureAbsoluteUrl(i.Src, url),
                Alt = i.Alt
            }));
                GetCurrentMethod();
            return images;
        }

        public static IEnumerable<Image> GetImagesFromReferencedJs(HtmlDocument document, string url, string schema)
        {
            var images = new List<Image>();
            var scriptPaths = UrlUtil.GetScriptFilePaths(document);
            if (scriptPaths == null || !scriptPaths.Any()) return images;

            Parallel.ForEach(scriptPaths, (scriptPath) =>
            {
                var imageReferences = GetImagesFromScriptFile(scriptPath, schema);
                if (imageReferences == null || !imageReferences.Any()) return;
                images.AddRange(imageReferences.Select(i => new Image
                {
                    Src = UrlUtil.EnsureAbsoluteUrl(i.Src, scriptPath),
                    Alt = i.Alt
                }));
            });

            return images;
        }

        public static IEnumerable<Image> GetImagesFromInlineJs(HtmlDocument document, string url)
        {
            var images = new List<Image>();
            var inlineScripts = document.DocumentNode.SelectNodes("//script");
            if (inlineScripts == null || !inlineScripts.Any()) return images;
            Parallel.ForEach(inlineScripts, (inlineScript) =>
            {
                var styleContent = inlineScript.InnerText;
                var regex = "['\"]?(?<url>[^)]+?)['\"]";


                      var imageReferences = GetImagesFromText(styleContent, regex);
                if (imageReferences == null || !imageReferences.Any())
                    return;
                images.AddRange(imageReferences.Select(i => new Image
                {
                    Src = UrlUtil.EnsureAbsoluteUrl(i.Src, url),
                    Alt = i.Alt
                }));
            });

            return images;
        }

        public static IEnumerable<Image> GetImagesFromCssFile(string filePath, string schema)
        {
            if (String.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(schema)) return null;

            filePath = UrlUtil.EnsureAbsoluteUrlFormat(filePath, schema);

            var regex = "url(['\"]?(?<url>[^)]+?)['\"]?)";
            const RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
            var content = UrlUtil.GetUrlContent(filePath);
            if (string.IsNullOrWhiteSpace(content)) return null;

            var matches = Regex.Matches(content, regex, options)
                .Cast<Match>().Select(m => m.Groups["url"]);
            var imageUrls = matches.Select(m => m.Value);

            
            var allowedImageUrls = new List<string>();
            foreach (var imageUrl in allowedExtensions)
            {
                var url = imageUrl;
                allowedImageUrls.AddRange(imageUrls.Where(v => v.EndsWith("." + url, StringComparison.OrdinalIgnoreCase)));
            }
            if (!allowedExtensions.Any()) return null;
            var images = allowedImageUrls.Select(r => new Image
                {
                    Src = r,
                    Alt = "Styling image"
                });
            return images;
        }

        public static IEnumerable<Image> GetImagesFromScriptFile(string filePath, string requestSchema)
        {

            if (String.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(requestSchema)) return null;

            filePath = UrlUtil.EnsureAbsoluteUrlFormat(filePath, requestSchema);

            var regex = "['\"]?(?<url>[^)]+?)['\"]";
            const RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
            var content = UrlUtil.GetUrlContent(filePath);
            if (string.IsNullOrWhiteSpace(content)) return null;

            var matches = Regex.Matches(content, regex, options)
                .Cast<Match>().Select(m => m.Groups["url"]);
            var imageUrls =  matches.Select(m => m.Value);

            var allowedImageUrls = new List<string>();
            foreach (var imageUrl in allowedExtensions)
            {
                var url = imageUrl;
                allowedImageUrls.AddRange(imageUrls.Where(v => v.EndsWith("." + url, StringComparison.OrdinalIgnoreCase)));
            }
               
            if (!allowedExtensions.Any()) return null;
            var images = allowedImageUrls.Select(r => new Image
                {
                    Src = r,
                    Alt = "Script image"
            });

            return images;
        }

        public static IEnumerable<Image> GetImagesFromText(string text, string imageRegex)
        {
            if (String.IsNullOrWhiteSpace(text)) return null;
            const RegexOptions options = ((RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline) | RegexOptions.IgnoreCase);
            if (String.IsNullOrWhiteSpace(text)) return null;

            var matches = Regex.Matches(text, imageRegex, options)
                .Cast<Match>().Select(m => m.Groups["url"]);
            var imageUrls = matches.Select(m => m.Value);
            if (imageUrls.Any())
                imageUrls = imageUrls.Where(v => !v.Trim().StartsWith("."));


            var allowedImageUrls = new List<string>();
            foreach (var imageUrl in allowedExtensions)
            {
                var url = imageUrl;
                allowedImageUrls.AddRange(imageUrls.Where(v => v.EndsWith("." + url, StringComparison.OrdinalIgnoreCase)));
            }
            if (!allowedExtensions.Any()) return null;
            var images = allowedImageUrls
                .Select(r => new Image
                {
                    Src = r,
                    Alt = "Inline image"
                });
            return images;
        }

        #endregion

        #region Helpers

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetCurrentMethod()
        {
            var stack = new StackTrace();
            var frame = stack.GetFrame(1);

            return frame.GetMethod().Name;
        }

        #endregion
    }
}