using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Args;
using Telegram.Bot.Types; 

namespace pj2.Controllers
{
    public class apiController : Controller
    {
        List<Update> list = new List<Update>(); //kinda DB //thread safeness?

        static string sList = "Updates:\n";

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(sList);
        }

        
        [HttpPost("[controller]/p")] //поменять
        public IActionResult Post([FromBody] Telegram.Bot.Types.Update u)
        {
            list.Add(u);
            sList += $"{DateTime.UtcNow.AddHours(3).ToString()}: Chat ID: {u.Message.Chat.Id}, user name: {u.Message.Chat.FirstName},"
                + $"update type: {u.Type}, text: {u.Message.Text}";

            Program.HandleUpdate(u);

            return Ok();
        }
    }
}
