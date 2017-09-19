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
        List<Message> list = new List<Message>();

        static string sList = "Messages:\n";

        //[Produces("application/text")]
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(sList);
        }

        
        [HttpPost("[controller]/p")] //поменять
        public IActionResult Post([FromBody] Telegram.Bot.Types.Update u)
        {
            list.Add(u.Message);
            sList += u.Message.Chat.FirstName + ": " + u.Message.Text + "\n";

            Program.HandleMessage(u.Message);

            return Ok();
        }
    }
}
