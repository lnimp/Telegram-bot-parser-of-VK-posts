using AngleSharp.Dom;
using VKPublicPostBot.Parser.Core;

namespace VKPublicPostBot.Parser.Parsing
{
    internal class Text : IParser<string>
    {
        public Task<string> Parse(IElement element)
        {
            switch (element.GetElementsByClassName("pi_text").Length)
            {
                case int length when length > 0:
                    if (element.GetElementsByClassName("pi_text").First().TextContent != null)
                    {
                        string text = element.GetElementsByClassName("pi_text").ElementAt(0).OuterHtml.ToString();
                        string[] classHtml = new[] { "<a href=", "<img class=", "<span style", "<div class=" };
                        for (int count = 0; count < classHtml.Length;)
                        {
                            switch (text.Contains(classHtml[count], StringComparison.CurrentCulture))
                            {
                                case true:
                                    text = text.Remove(text.IndexOf(classHtml[count]), text[text.IndexOf(classHtml[count])..].IndexOf(">") + 1);
                                    break;
                                case false:
                                    count += 1;
                                    break;
                            }
                        }
                        text = text.Replace("<br>", "\n").Replace("</a>", "").Replace("/", "").
                                Replace("&amp;", "&").Replace("<div>", "").Replace("<span>", "").Replace("Показать полностью...", "");
                        return Task.FromResult(text);
                    }
                    break;
                case -1:
                    return Task.FromResult("");
            }
            return Task.FromResult("");
        }
    }
}


