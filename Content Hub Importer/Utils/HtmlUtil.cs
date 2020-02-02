#region

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

#endregion

namespace ContentHub.Importer.Utils
{
    public class HtmlUtil
    {
       

        public static string CleanupString(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            text = Regex.Replace(text, @"\t|\n|\r", string.Empty).Trim();
            text = HttpUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"[^\u0000-\u007F]", string.Empty);
            text = Regex.Replace(text, @"[\d-]", string.Empty);
            while (text.Contains("  ")) text = text.Replace("  ", " ");
            return text;
        }
        
        public static HtmlDocument CleanupDocument(HtmlDocument document)
        {
            var scriptNodes = document.DocumentNode.Descendants("script").ToArray();
            if (scriptNodes != null)
            {
                foreach (var script in scriptNodes)
                    script.Remove(); 
            }

            var styleNodes = document.DocumentNode.Descendants("style").ToArray();
            if (styleNodes != null)
            {
                foreach (var style in styleNodes)
                    style.Remove();  
            }

            var comments = document.DocumentNode.SelectNodes("//comment()");
            if (comments == null) return document;

            foreach (var comment in comments)
            {
                comment.ParentNode.RemoveChild(comment);
            }

            return document;
        }

        public static string GetContentOfHtmlTextNodes(IEnumerable<HtmlNode> htmlContentElements)
        {
            var content = string.Join(" ", htmlContentElements
                .Where(n => !n.HasChildNodes && !string.IsNullOrWhiteSpace(n.InnerText))
                .Select(n => n.InnerText).ToList());
            return content;
        }
    }
}