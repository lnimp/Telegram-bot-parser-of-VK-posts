using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using VKPublicPostBot.TelegramBot;

namespace VKPublicPostBot.Parser.Core
{
    internal class HtmlLoader
    {
        readonly IConfiguration config = Configuration.Default.WithDefaultLoader();
        public async Task<IHtmlElement?> LoaderDocument(string url)
        {
            try
            {
                using IBrowsingContext context = BrowsingContext.New(config);
                using IDocument document = await context.OpenAsync(url);
                return document.Body;
            }
            catch
            {
                return null;
            }
        }
        public async Task<IElement?> LoaderElement(IHtmlElement document)
        {
            try
            {
                var element = document.GetElementsByClassName("wi_body").ElementAt(1);
                return element;
            }
            catch
            {
                return null;
            }
        }
        public async Task<string> GetIdElemnt(IElement element)
        {
            try
            {
                if (element.GetElementsByClassName("pi_signed copyright_label").Length == 0)
                {
                    string id = element.ParentElement.GetElementsByClassName("wi_head").First().PreviousElementSibling.Attributes["name"].Value;
                    return id;
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
        public async Task<string> GetTitlePublic(string url)
        {
            using IBrowsingContext context = BrowsingContext.New(config);
            using IDocument document = await context.OpenAsync(url);
            return document.Title.Replace(" | ВКонтакте", "");
        }
    }
}
