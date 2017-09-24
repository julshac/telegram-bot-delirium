using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(header + log);
        }

        
        [HttpPost]
        public IActionResult p([FromBody] Telegram.Bot.Types.Update u)
        {
            list.Add(u);

            Program.HandleUpdate(u);

            return Ok();
        }
    }
}
