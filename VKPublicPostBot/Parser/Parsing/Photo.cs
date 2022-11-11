using AngleSharp.Dom;
using Telegram.Bot.Types;
using VKPublicPostBot.Parser.Core;

namespace VKPublicPostBot.Parser.Parsing
{
    internal class Photo : IParser<List<InputMediaPhoto>>
    {
        public Task<List<InputMediaPhoto>> Parse(IElement element)
        {
            List<InputMediaPhoto> photo = new();
            switch (element.GetElementsByClassName("medias_thumbs medias_thumbs_map")?.Length)
            {
                case 1:
                    if (element.GetElementsByClassName("thumb_map thumb_map_wide thumb_map_l al_photo").Length > 0 || element.GetElementsByClassName("thumb_map thumb_map_s al_photo").Length > 0)
                    {
                        var linkPhoto = element.GetElementsByClassName("thumb_map_img thumb_map_img_as_div");
                        int countLinks = linkPhoto.Length;
                        string[] links = new string[countLinks];
                        switch (countLinks)
                        {
                            case int count when count > 1:
                                for (int i = 0; linkPhoto.Length > i; i++)
                                {
                                    if (linkPhoto[0].Attributes["data-src_big"]?.Value != "" || linkPhoto[0].Attributes["data-src_big"]?.Value != null)
                                    {
                                        links[i] += linkPhoto[i].Attributes["data-src_big"]?.Value;
                                        photo.Add(new InputMediaPhoto(links[i]));
                                    }
                                }
                                return Task.FromResult(photo);
                            case 1:
                                if (linkPhoto[0].Attributes["data-src_big"]?.Value != "" || linkPhoto[0].Attributes["data-src_big"]?.Value != null)
                                {
                                    links[0] = linkPhoto[0].Attributes["data-src_big"]?.Value;
                                    photo.Add(new InputMediaPhoto(links[0]));
                                    return Task.FromResult(photo);
                                }
                                break;
                            case 0:
                                return Task.FromResult(photo);
                        }
                    }
                    break;
                default:
                    return Task.FromResult(photo);
            }
            return Task.FromResult(photo);
        }

    }
}
