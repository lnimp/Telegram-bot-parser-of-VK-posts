using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;
using VKPublicPostBot.TelegramBot.Core;
using VKPublicPostBot.TelegramBot.WorkPublic;

namespace VKPublicPostBot.TelegramBot
{
    internal class BotClient : Bot
    {
        private async Task HadleUpdatesAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        {
                            if (update.Message.SuccessfulPayment != null)
                            {
                                await database.UpdateSubscription(update.Message.From.Id, Convert.ToInt32(update.Message.SuccessfulPayment.InvoicePayload));
                                botClient.SendTextMessageAsync(update.Message.From.Id, "Оплата прошла,подписка обновлена.");
                                Console.WriteLine(update.Message.From.Id + "оплатил" + update.Message.SuccessfulPayment.InvoicePayload);
                                return;
                            }
                            switch (update.Message.ReplyToMessage)
                            {
                                case null:
                                    await HandleMessage(botClient, update.Message);
                                    return;
                                default:
                                    await HandleReplyMessage(botClient, update.Message);
                                    return;
                            }
                        }
                    case UpdateType.MyChatMember:
                        {
                            if (update.MyChatMember.NewChatMember.Status != ChatMemberStatus.Kicked)
                            {
                                if (await database.CheckUser(update.MyChatMember.From))
                                {
                                    await database.InsertUser(update.MyChatMember.From);
                                }
                            }
                            break;
                        }
                    case UpdateType.CallbackQuery:
                        await HandleCallbackQuery(botClient, update.CallbackQuery);
                        break;
                }
            }
            catch
            {
                await botClient.SendTextMessageAsync(update.Message.From.Id, "Ошибка!");
            }

        }
        private async Task HandleMessage(ITelegramBotClient botClient, Message message)
        {
            switch (message.Text)
            {
                case "/start":
                    {
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"⚡Добро пожаловать,{message.From.Username}⚡\n" +
                            $"📢Подключение паблика - подписывайтесь на паблики через ссылку\n" +
                            $"📮Мои паблики - паблики с которых вы получаете посты.Отпишитесь, нажав на кпопку\n" +
                            $"📬Другие паблики - паблики на которые подписаны другие пользователи.Подпишитесь, нажав на кпопку\n" +
                            $"⛔ВНИМАНИЕ!Бот не может отправлять видео и аудио⛔",
                            replyMarkup: keyboardStartMenu);
                        return;
                    }
                case "/Menu":
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"🚩Меню", replyMarkup: keyboardStartMenu);
                    return;
                case "📢Подключение паблика":
                    await botClient.SendTextMessageAsync(message.Chat.Id, "📩В ответ пришлите ссылку на публичное сообщество VK", replyMarkup: new ForceReplyMarkup { Selective = true });
                    return;
                case "📮Мои паблики":
                    await GetMePublic(botClient, message);
                    return;
                case "📬Другие паблики":
                    await GetOtherPublic(botClient, message);
                    return;
            }
        }
        private async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            switch (callbackQuery.Message.Text)
            {
                case string pub when pub == "📬Другие паблики" || pub == "📮Мои паблики":
                    await HandlePublic(botClient, callbackQuery);
                    return;
            }
        }
        private async Task HandleReplyMessage(ITelegramBotClient botClient, Message message)
        {
            switch (message.ReplyToMessage.Text)
            {
                case "📩В ответ пришлите ссылку на публичное сообщество VK":
                    {
                        string titlePublic = await CheckLink(message?.Text);
                        switch (titlePublic)
                        {
                            default:
                                if (!stream.FindAll(i => i.idUsers.Contains(message.Chat.Id)).Any(i => i.titlePublic == titlePublic))
                                {
                                    switch (stream.Any(i => i.titlePublic == titlePublic))
                                    {
                                        case true:
                                            stream.FirstOrDefault(i => i.titlePublic == titlePublic).AddUser(botClient, message.Chat.Id);
                                            await database.InsertUserForPublic(message.Text, message.From);
                                            await botClient.SendTextMessageAsync(message.Chat.Id, $"✅Все успешно прошло" +
                                                              $"\n📣Ожидайте уведомление о постах {titlePublic}");
                                            return;
                                        case false:
                                            stream.Add(new StreamPublic(botClient, message, titlePublic));
                                            await database.InsertPublic(titlePublic, message.Text);
                                            await database.InsertUserForPublic(stream.FirstOrDefault(i => i.titlePublic == titlePublic).url, message.From);
                                            await botClient.SendTextMessageAsync(message.Chat.Id, $"✅Все успешно прошло" +
                                                              $"\n📣Ожидайте уведомление о постах {titlePublic}");
                                            return;
                                    }
                                }
                                else await botClient.SendTextMessageAsync(message.Chat.Id, $"❌Вы уже подписаны на {titlePublic}", replyMarkup: keyboardStartMenu);
                                return;
                            case null:
                                await botClient.SendTextMessageAsync(message.Chat.Id, "💢Вы скинули не правильную ссылку", replyMarkup: keyboardStartMenu);
                                return;
                            case "":
                                await botClient.SendTextMessageAsync(message.Chat.Id, "💢Вы скинули не правильную ссылку", replyMarkup: keyboardStartMenu);
                                return;
                        }
                    }
            }
        }
        private async Task HandlePublic(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            switch (stream.FirstOrDefault(i => i.titlePublic == callbackQuery.Data)?.idUsers.Contains(callbackQuery.Message.Chat.Id))
            {
                case true:
                    UnsubscribePublic(botClient, callbackQuery);
                    return;
                case false:
                    SubscriptionPublic(botClient, callbackQuery);
                    return;
                case null:
                    await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, "⚠Ошибка паблик закрыт\n\nПопробуйте через подключение паблика", replyMarkup: keyboardStartMenu);
                    return;
            }
        }
        private static async Task UnsubscribePublic(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            string? titlePublic = callbackQuery.Data;
            switch (stream.FirstOrDefault(i => i.titlePublic == titlePublic)?.idUsers.Count)
            {
                case 1:
                    stream.FirstOrDefault(i => i.titlePublic == titlePublic)?.RemovePublic();
                    stream.Remove(stream.FirstOrDefault((i => i.titlePublic == titlePublic)));
                    await database.DeleteUserForPublic(titlePublic, callbackQuery.From);
                    await database.DeletePublic(titlePublic);
                    break;
                default:
                    stream.FirstOrDefault(i => i.titlePublic == titlePublic)?.RemoveUserToPublic(callbackQuery.Message.Chat.Id);
                    await database.DeleteUserForPublic(titlePublic, callbackQuery.From);
                    break;
            }
            await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"✅Вы больше не будете получать посты от {titlePublic}");
        }
        private async Task GetMePublic(ITelegramBotClient botClient, Message message)
        {
            switch (stream.Any(i => i.idUsers.Contains(message.Chat.Id)))
            {
                case true:
                    {
                        var MeStreams = stream.FindAll(i => i.idUsers.Contains(message.Chat.Id)).Select(i => i.titlePublic).ToList();
                        var keyboardMarkup = new InlineKeyboardMarkup(GetTitlesPublic(MeStreams));
                        await botClient.SendTextMessageAsync(message.Chat.Id, $"📮Мои паблики", replyMarkup: keyboardMarkup);
                        return;
                    }
                default:
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"❌У вас еще нет пабликов", replyMarkup: keyboardStartMenu);
                    return;
            }
        }
        private async Task SubscriptionPublic(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            string? titlePublic = callbackQuery.Data;
            if (stream.Any(i => i.titlePublic == titlePublic))
            {
                stream.FirstOrDefault(i => i.titlePublic == titlePublic).AddUser(botClient, callbackQuery.Message.Chat.Id);
                await botClient.SendTextMessageAsync(callbackQuery.Message.Chat.Id, $"✅Все успешно прошло" +
                                                      $"\n📣Ожидайте уведомление о постах {titlePublic}");
                await database.InsertUserForPublic(stream.FirstOrDefault(i => i.titlePublic == titlePublic).url, callbackQuery.From);
            }
            return;
        }
        private async Task GetOtherPublic(ITelegramBotClient botClient, Message message)
        {
            switch (stream.All(i => i.idUsers.Contains(message.Chat.Id)))
            {
                case true:
                    await botClient.SendTextMessageAsync(message.Chat.Id, $"❌Нет еще пабликов", replyMarkup: keyboardStartMenu);
                    return;
                case false:
                    {
                        var orderPublic = stream.Except(stream.FindAll(i => i.idUsers.Contains(message.Chat.Id)).ToList()).Select(i => i.titlePublic);
                        switch (orderPublic.Count())
                        {
                            case > 0:
                                var keyboardMarkup = new InlineKeyboardMarkup(GetTitlesPublic(orderPublic.ToList()));
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"🌌Паблики", replyMarkup: keyboardMarkup);
                                return;

                            default:
                                await botClient.SendTextMessageAsync(message.Chat.Id, $"🌌Паблики", replyMarkup: new InlineKeyboardMarkup(GetTitlesPublic(stream.Select(i => i.titlePublic).ToList())));
                                return;
                        }
                    }
            }
        } 
        async public Task Start()
        {
            try
            {
                stream = await database.CopyToStreamPublic(botClient);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            botClient.StartReceiving(
             HadleUpdatesAsync,
             HandleErrorAsync,
             ReceiverOptions,
             cancellationToken: cancellationToken.Token);
            Console.ReadKey();
            cancellationToken.Cancel();
        }
        Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Ошибка телеграм АПИ:\n{apiRequestException.ErrorCode}\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}