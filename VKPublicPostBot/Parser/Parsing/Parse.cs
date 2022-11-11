using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Telegram.Bot.Types;
using VKPublicPostBot.Parser.Core;
using VKPublicPostBot.TelegramBot.WorkPublic;

namespace VKPublicPostBot.Parser.Parsing
{
    public class Parse
    {
        private HtmlLoader htmlLoader = new();
        private Text text = new();
        private Photo media = new();
        private List<InputMediaPhoto> linksPhoto { get; set; }
        internal string? idPost { get; set; }
        private IElement? element { get; set; }
        private IHtmlElement? document { get; set; }
        public Parse(string url)
        {
            document = htmlLoader.LoaderDocument(url).Result;
            element = htmlLoader.LoaderElement(document).Result;
            idPost = htmlLoader.GetIdElemnt(element).Result;
        }
        public Parse(PostMessage postMessage, string url) : this(url)
        {
            if ((idPost != postMessage.IdPost && (idPost != null || idPost != "")) || postMessage == null && (idPost != null || idPost != ""))
            {
                postMessage.postText = text.Parse(element).Result;
                postMessage.IdPost = idPost;
                postMessage.linksPhoto = media.Parse(element).Result;
                if (postMessage.postText != null || postMessage.linksPhoto.Count > 0 || postMessage.linksPhoto.Count != null)
                {
                    postMessage.exists = true;
                }
                else postMessage.exists = false;
            }
            else postMessage.exists = false;
        }
    }
}
