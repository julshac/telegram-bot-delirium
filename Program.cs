using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace pj2
{
    public class Program
    {
        public static DateTime STARTTIME = DateTime.UtcNow.AddHours(3);

        private static TelegramBotClient bot;

        private static string webhookAddress;
        public static string WebhookAddress
        {
            get { return webhookAddress; }
            set
            {
                if (bot != null)
                    webhookAddress = value;
                bot.SetWebhookAsync($"https://{webhookAddress}/api/p/{Token}/");
            }
        }

        private static string token;
        public static string Token
        {
            private get { return token; }
            set
            {
                if (bot == null)
                {
                    token = value;
                    bot = new TelegramBotClient(token);
                }
            }
        }

        private static Dictionary<long, GameRoom> rooms = new Dictionary<long, GameRoom>();

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseAzureAppServices()
                .Build();

        public static void HandleUpdate(Update u)
        {
            switch (u.Type)
            {
                case UpdateType.MessageUpdate:
                    {
                        var id = u.Message.Chat.Id;
                        if (!rooms.ContainsKey(id))
                        {
                            GameRoom room = new GameRoom(id, bot);
                            rooms.Add(id, room);
                        }
                        else
                        { rooms[id].HandleMessage(u.Message); }
                        break;
                    }
                case UpdateType.CallbackQueryUpdate:
                    {
                        var id = u.CallbackQuery.Message.Chat.Id;
                        if (rooms.ContainsKey(id))
                            rooms[id].HandleCallbackQuery(u.CallbackQuery);
                        else
                            bot.AnswerCallbackQueryAsync(u.CallbackQuery.Id, "Начните с сообщения.");
                        break;
                    }
                default:
                    break;
            }

        }
    }
}
