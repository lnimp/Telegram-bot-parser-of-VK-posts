using Microsoft.Data.Sqlite;
using Telegram.Bot;
using Telegram.Bot.Types;
using VKPublicPostBot.DB;

namespace VKPublicPostBot.TelegramBot.WorkPublic
{
    internal class StreamPublic
    {
        public string titlePublic;
        public Task task;
        public string url;
        public List<long> idUsers = new();
        private readonly CancellationTokenSource cancelTokenSource = new();
        private readonly CancellationToken token = new();
        private  PostMessage post = new();
        public StreamPublic(ITelegramBotClient botClient, Message message, string titlePublic)
        {
            this.titlePublic = titlePublic;
            url = message.Text;
            idUsers.Add(message.Chat.Id);
            token = cancelTokenSource.Token;
            task = new Task(() => SendingMessages(botClient, url, titlePublic), token);
            task.Start();
        } // Using user to public 
        public StreamPublic(ITelegramBotClient botClient, string titlePublic, string url)
        {
            this.titlePublic = titlePublic;
            this.url = url;
            token = cancelTokenSource.Token;
            task = new Task(() => SendingMessages(botClient, url, titlePublic), token);
            task.Start();
        } //Create Public db
        private async Task SendingMessages(ITelegramBotClient botClient, string url, string titlePublic)
        {
            do
            {
                bool check = await post.ParserPost(url, titlePublic);
                if (check)
                {
                    foreach (var user in idUsers)
                    {
                        try
                        {
                            Task send = new(() => post.HandlerSendPost(botClient, user));
                            send.Start();
                        }
                        catch (Exception e)
                        {
                            continue;
                            Console.WriteLine($"Ошибка USER отправки{e}");
                        }
                    }
                }
                else
                {
                    Random random = new();
                    Thread.Sleep(random.Next(180000, 600000));
                }
                switch (token.IsCancellationRequested)
                {
                    case true:
                        return;
                }
            }
            while (true);
        }
        public async Task AddUser(ITelegramBotClient botClient, long id)
        {
            idUsers.Add(id);
            if (post != null)
            {
                await post.HandlerSendPost(botClient, id);
            }
        }
        public void RemovePublic()
        {
            cancelTokenSource.Cancel();
            switch (task.IsCompleted)
            {
                case true:
                    Console.WriteLine($"Паблик остановлен:{titlePublic}");
                    break;
            }
        }
        public void RemoveUserToPublic(long idUser)
        {
            idUsers.Remove(idUser);
            Console.WriteLine($"{idUser} отписался из паблика:{titlePublic}");
        }
    }

}

