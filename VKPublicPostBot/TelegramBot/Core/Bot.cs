using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using VKPublicPostBot.DB;
using VKPublicPostBot.Parser.Core;
using VKPublicPostBot.TelegramBot.WorkPublic;

namespace VKPublicPostBot.TelegramBot.Core
{
    abstract class Bot
    {
        public static TelegramBotClient botClient = new("5519876626:AAEPwjmlxrvgEFl_MuarR8-9-_RXqaeJIJk");
        public static DatabaseBot database = new("VKPublic.db");
        public ReceiverOptions ReceiverOptions { get; set; } = new ReceiverOptions
        {
            AllowedUpdates = { }
        };
        public CancellationTokenSource cancellationToken = new();
        public static List<StreamPublic> stream = new();
        public ReplyKeyboardMarkup keyboardStartMenu = new(new[]
        {
                 new KeyboardButton[] { "📮Мои паблики", "📬Другие паблики", "🔑Подписка" },
                 new KeyboardButton[] { "📢Подключение паблика" }
        })
        {
            OneTimeKeyboard = true,
            ResizeKeyboard = true,
        };
        public static InlineKeyboardButton[][] GetTitlesPublic(List<string> titlesPublic)
        {
            var keyboardInline = new InlineKeyboardButton[titlesPublic.Count][];
            var keyboardButtons = new InlineKeyboardButton[titlesPublic.Count];
            for (var i = 0; i < titlesPublic.Count; i++)
            {

                keyboardButtons[i] = new InlineKeyboardButton(titlesPublic[i])
                {
                    CallbackData = titlesPublic[i],
                };
            }
            for (var j = 1; j <= titlesPublic.Count; j++)
            {
                keyboardInline[j - 1] = keyboardButtons.Take(1).ToArray();
                keyboardButtons = keyboardButtons.Skip(1).ToArray();
            }

            return keyboardInline;
        }
        public static async Task<string?> CheckLink(string url)
        {
            HtmlLoader htmlLoader = new();
            switch (url.IndexOf("https://vk.com/"))
            {
                case 0:
                    var document = await htmlLoader.LoaderDocument(url);
                    if (document != null && document.GetElementsByClassName("pi_text").Length != 0 || document.GetElementsByClassName("thumb_map_img thumb_map_img_as_div").Length != 0)
                    {
                        string titlePublic = await htmlLoader.GetTitlePublic(url);
                        return titlePublic;
                    }
                    break;
            }
            return null;
        }
    }
}
