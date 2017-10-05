using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace pj2.Controllers
{
    public class apiController : Controller
    {
        List<Update> list = new List<Update>(); //kinda DB //thread safeness?

        static string header = "Updates:\n";
        static string log;

        public static string EmailUser { private get; set; }
        public static string EmailPass { private get; set; }
        public static string EmailTo { private get; set; }
        private static AutoResetEvent emailTimeout = new AutoResetEvent(true);

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(header + log);
        }


        [HttpPost]
        public IActionResult p([FromBody] Telegram.Bot.Types.Update u)
        {
            list.Add(u);

            switch (u.Type)
            {
                case UpdateType.MessageUpdate:
                    var str = $"{DateTime.UtcNow.AddHours(3)}, {u.Message.Type}. {u.Message.From.Id}, {u.Message.From.FirstName} : {u.Message.Text}\n";
                    log =  str + log;
                    SendEmail(str, u.Message.From.Id);
                    break;
                case UpdateType.CallbackQueryUpdate:
                    str = $"{DateTime.UtcNow.AddHours(3)}, callback. {u.CallbackQuery.From.Id}, {u.CallbackQuery.From.FirstName} : {u.CallbackQuery.Data}\n";
                    log = str + log;
                    SendEmail(str, u.CallbackQuery.From.Id);
                    break;
                case UpdateType.All:
                    log = $"{DateTime.UtcNow.AddHours(3)}, {u.Type}\n" + log;
                    break;
            }

            Program.HandleUpdate(u);

            return Ok();
        }

        private void SendEmail(string msg, long id)
        {
            if (id != 113472905 && emailTimeout.WaitOne(0))
            {
                SmtpClient client = new SmtpClient();
                client.Port = 587;
                client.Host = "smtp.gmail.com";
                client.EnableSsl = true;
                client.Timeout = 10000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(EmailUser, EmailPass);

                MailMessage mm = new MailMessage("donotreply@nowhere.com", EmailTo, "Delirium Bot admin", msg);
                mm.BodyEncoding = UTF8Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

                client.SendAsync(mm, default(object));

                Task.Run(async () => { await Task.Delay(300000); emailTimeout.Set(); });
            }
        }
    }
}
