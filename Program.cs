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

namespace pj2
{
    public class Program
    {
        public static DateTime STARTTIME = DateTime.UtcNow;
        public static TelegramBotClient bot;
        
        private static Dictionary<long, GameRoom> rooms = new Dictionary<long, GameRoom>();

        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) 
        {
            var confBuilder = new ConfigurationBuilder();
            var conf = confBuilder
                .AddEnvironmentVariables("APPSETTING_")
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseConfiguration(conf)
                .UseAzureAppServices()
                .Build();
        }

        public static void HandleMessage(Message m)
        {
            var id = m.Chat.Id;
            if (!rooms.ContainsKey(id))
            {
                GameRoom room = new GameRoom(id, bot);
                rooms.Add(id, room);
            }
            rooms[id].HandleMessage(m);
        }
    }
}
