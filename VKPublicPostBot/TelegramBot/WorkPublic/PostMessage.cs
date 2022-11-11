using Telegram.Bot;
using Telegram.Bot.Types;
using VKPublicPostBot.Parser.Parsing;

namespace VKPublicPostBot.TelegramBot.WorkPublic
{
    public class PostMessage
    {
        internal bool exists;
        internal string IdPost;
        internal string postText;
        internal List<InputMediaPhoto> linksPhoto;
        public PostMessage()
        {

        }
        public async Task<bool> ParserPost(string url, string titlePublic)
        {
            try
            {
                Parse parse = new(this, url);
                if (exists)
                {
                    postText += "\n\n" + "#"+titlePublic;
                    return exists;
                }
                else return exists;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка  парсера{e}");
                return false;
            }

        }
        public async Task HandlerSendPost(ITelegramBotClient botClient, long idUser)
        {
            try
            {
                if (linksPhoto != null || postText != null || IdPost != null)
                {
                    switch (linksPhoto.Count)
                    {
                        case 0:
                            await botClient.SendTextMessageAsync(idUser, postText);
                            return;
                        case 1:
                            await SendPhoto(botClient, idUser);
                            return;
                        case int count when count > 0:
                            await SendMedia(botClient, idUser);
                            return;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка  отправки{e}");
                return;
            }
        }
        private async Task SendMedia(ITelegramBotClient botClient, long idUser)
        {
            try
            {
                switch (linksPhoto.Count)
                {
                    case 1:
                        await botClient.SendPhotoAsync(idUser, photo: linksPhoto[0].Media.Url, caption: postText);
                        return;
                    default:
                        linksPhoto[0].Caption = postText;
                        switch (linksPhoto.Count)
                        {
                            case > 9:
                                linksPhoto.GetRange(0, 10);
                                await botClient.SendMediaGroupAsync(idUser, media: linksPhoto);
                                return;
                            default:
                                await botClient.SendMediaGroupAsync(idUser, media: linksPhoto);
                                return;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка  отправки ФОТО И ВИДЕО {e}");
                return;
            }
        }
        private async Task SendPhoto(ITelegramBotClient botClient, long idUser)
        {
            try
            {
                switch (linksPhoto.Count)
                {
                    case 1:
                        await botClient.SendPhotoAsync(idUser, photo: linksPhoto[0].Media.Url, caption: postText);
                        return;
                    default:
                        linksPhoto[0].Caption = postText;
                        switch (linksPhoto.Count)
                        {
                            case > 9:
                                linksPhoto.GetRange(0, 10);
                                await botClient.SendMediaGroupAsync(idUser, media: linksPhoto);
                                return;
                            default:
                                await botClient.SendMediaGroupAsync(idUser, media: linksPhoto);
                                return;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Ошибка  отправки фото {e}");
                return;
            }
        }
    }
}
