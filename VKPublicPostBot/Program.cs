using VKPublicPostBot.DB;
using VKPublicPostBot.TelegramBot;
BotClient botClient = new();
Console.WriteLine("Запуск");
await botClient.Start();
Console.ReadKey();
