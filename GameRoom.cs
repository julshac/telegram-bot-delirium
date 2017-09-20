using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace pj2
{
    class GameRoom
    {
        private static Dictionary<int, Card> cards =
            new Dictionary<int, Card>()
            {
                {0, new Card("Я", 4, "Этот бокал для тебя.") },
                {1, new Card("Ты", 4, "Выбери, кто будет пить.") },
                {2, new Card("Джентльмены", 2, "Все джентльмены за столом пьют.") },
                {3, new Card("Леди", 2, "Все леди за столом пьют.") },
                {4, new Card("Тост", 2, "Произносишь тост, все пьют.") },
                {5, new Card("Передай налево", 3, "Игрок слева от тебя пьет.") },
                {6, new Card("Передай направо", 3, "Игрок справа от тебя пьет.") },
                {7, new Card("Вызов", 2, "Выбери игрока и выпей. Он должен выпить не меньше тебя.") },
                {8, new Card("Все на пол", 1, "Все игроки должны коснуться пола. Последний пьет.") },
                {9, new Card("Прозвище", 4, "Придумай прозвище игроку. Все должны звать его этим прозвищем до конца игры. Тот, кто обратился к нему иначе, пьет.") },
                {10, new Card("Твои правила", 3, "Становишься Rulemaster'ом и придумываешь свои правила. Следующий Rulemaster может отменить правила предыдущего.") },
                {11, new Card("Секретная служба", 2, "Все должны приложить ладонь к уху, изображая телохранителей. Последний становится президентом и пьет.") },
                {12, new Card("Брудершафт", 2, "Выпей на брудершафт с игроком на выбор.") },
                {13, new Card("Шах и мат", 3, "Выбери игрока, который будет пить с тобой каждый раз, когда ты ошибаешься.") },
                {14, new Card("Повтори за мной", 1, "Просишь игрока повторить за тобой (скороговорку или сложнопроизносимое слово). Если у него не получилось - он пьет. Получилось - пьешь ты.") },
                {15, new Card("Неудобные вопросы", 3, "Каждый игрок имеет право задать тебе любой вопрос. Если ты отказываешься на него отвечать - ты пьешь.") },
                {16, new Card("Нос", 2, "Все игроки должны коснуться носа. Последний пьет.") },
                {17, new Card("Категория", 2, "Вытянувший карту придумывает категорию (марки презервативов, музыкальные группы 90-х годов, модели Mercedes). Остальные игроки называют слова из этой категории. Кто не сможет - пьет.") },
                {18, new Card("Я никогда не", 3, "Говоришь то, что ты \"Никогда не делал\" (но на самом деле делал или очень хотел бы). Тот, кто делал это, пьет.") },
                {19, new Card("Вопросы", 2, "Игрок задает вопрос игроку слева. Отвечать на него нельзя, нужно быстро задать вопрос следующему соседу. Сбился? Ошибся? Запнулся? Выпей.") },
                {20, new Card("Цвет", 2, "Игрок называет цвет, следующий повторяет его и добавляет свой, и так далее. Кто сбился, тот пьет.") },
                {21, new Card("Кубок", 4, "Первые три игрока, вытянувшие эту карту, сливают содержимое своих бокалов в кубок. Четвертый это дело выпивает.") },
                {22, new Card("Саймон говорит", 1, "Тот, кто вытянул эту карту делает какой-нибудь жест, следующий делает то же самое и добавляет свой. Так продолжается, пока кто-нибудь не собьется.") },
                {23, new Card("Товарищ заебал",1,"Игрок, вытянувший эту карту, становится товарищем. Другим игрокам нельзя отвечать на его вопросы.") }
            };

        private static string[] buttonTitles = new string[]
            {
                "Хоп, хэй, лалалэй",
                "Наливай!",
                "Гриша одобряэ",
                "Жопа - не лень",
                "Люк, я твой стакан!",
                "Ты сможешь!",
                "Держись!",
                "Не жми!",
                "Сперва подлей соседу",
                "За Родину!",
                "Never gonna give you up",
                "42",
                "Матрос - это такая вошь",
                "Алкоголь на тебя не действует!"
            };

        private const string str_newgame = "Новая игра";
        private const string str_wat = "Что делает карта?";
        private const string str_rules = "Правила";
        private const string str_ban = "Бан карты";

        private Random rnd = new Random(DateTime.Now.Millisecond);

        private ReplyKeyboardMarkup menuMarkup;
        private ReplyKeyboardMarkup gameMarkup;

        private TelegramBotClient Client { get; }
        private long ChatID { get; }

        private List<int> deck;
        private int lastCardId;
        private int cupCount;
        private int cupId;
        private bool started;

        public GameRoom(long chatid, TelegramBotClient bot)
        {
            Client = bot;
            ChatID = chatid;

            started = false;

            cupId = cards.Where(x => x.Value.Name == "Кубок").ElementAt(0).Key;

            InitKeyboardMarkups();

            SendMsg("Привет! Ищешь приключений? Сейчас они начнутся! Для начала новой игры в любой момент введи \"/newgame\".", menuMarkup);
        }

        private void InitKeyboardMarkups()
        {
            KeyboardButton[][] notInGame = new KeyboardButton[2][];
            for (int i = 0; i < 2; i++)
            {
                notInGame[i] = new KeyboardButton[1];
            }
            notInGame[0][0] = new KeyboardButton(str_newgame);
            notInGame[1][0] = new KeyboardButton(str_rules);

            menuMarkup = new ReplyKeyboardMarkup(notInGame, true);

            //- - - - - - - - -//

            KeyboardButton[][] inGame = new KeyboardButton[2][];
            for (int i = 0; i < 2; i++)
            {
                inGame[i] = new KeyboardButton[1];
            }
            inGame[0][0] = new KeyboardButton(getRandomButtonTitle());
            inGame[1][0] = new KeyboardButton(str_wat);

            gameMarkup = new ReplyKeyboardMarkup(inGame, true);

            InlineKeyboardButton[][] ikb = new InlineKeyboardButton[8][];

            for (int i = 0; i < 6; i++)
            {
                ikb[i] = new InlineKeyboardButton[3] {
                    "<looooongname>:" + i,
                    "-",
                    "+"
                };
            }
            ikb[6] = new InlineKeyboardButton[2] { ":arrow_left:", "arrow_right" };
            ikb[7] = new InlineKeyboardButton[2] { "Отмена", "Сохранить" };

            test = new InlineKeyboardMarkup(ikb);
        }

        private InlineKeyboardMarkup test;

        private void NewGame()
        {
            lastCardId = -1;
            cupCount = 0;
            BuildShuffleStack();
            gameMarkup.Keyboard[0][0].Text = getRandomButtonTitle();
            SendMsg("Новая колода готова!", test);
        }

        private void BuildShuffleStack()
        {
            deck = new List<int>();
            foreach (var card in cards)
            {
                for (int i = 0; i < card.Value.DefaultCount; i++)
                {
                    deck.Add(card.Key);
                }
            }

            int buf, rndPlace;
            

            for (int i = 0; i < deck.Count; i++)
            {
                buf = deck[i];
                rndPlace = rnd.Next(deck.Count);
                deck[i] = deck[rndPlace];
                deck[rndPlace] = buf;
            }
        }

        public void HandleMessage(Message m)//Telegram.Bot.Args.MessageEventArgs args)
        {
            //if (args.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
            {
                switch (m.Text)
                {
                    case "/start": break;
                    case str_ban: break; //TODO: DO.
                    case "/newgame": 
                    case str_newgame: NewGame(); started = true; break;
                    case str_wat:
                        {
                            if (lastCardId == -1) SendMsg("Ни одной карты не было разыграно.", gameMarkup);
                            else SendMsg(cards.ElementAt(lastCardId).Value.Description, gameMarkup);
                            break;
                        }
                    case str_rules:
                        {
                            SendMsg("Каждый игрок тянет по одной карте и выполняет действия согласно описанию.", menuMarkup);
                            break;
                        }
                    case "/startup":
                    {
                        SendMsg(Program.STARTTIME.ToString(), null);
                        break;
                    }
                    default:
                        {
                            if (started)
                            {
                                int id = deck.First();
                                deck.RemoveAt(0);

                                lastCardId = id;

                                string cupstr = "";
                                if (id == cupId)
                                {
                                    cupstr = " №" + (++cupCount == 4 ? "4! Время его выпить!" : cupCount.ToString());
                                }

                                gameMarkup.Keyboard[0][0].Text = getRandomButtonTitle();
                                SendMsg(cards.ElementAt(id).Value.Name + cupstr, gameMarkup);
                                
                                if (deck.Count == 0)
                                {
                                    Thread.Sleep(400);
                                    SendMsg("Карты в колоде закончились! Для начала новой игры введи \"/newgame\".", menuMarkup);
                                    started = false;
                                }
                            }
                            break;
                        }
                }
            }
        }

        private void SendMsg(string msg, IReplyMarkup markup)
        {
            Client.SendTextMessageAsync(ChatID, msg, replyMarkup: markup);
        }

        int lastval = -1;
        private string getRandomButtonTitle()
        {
            int newval;
            if (lastval == -1)
            {
                newval = rnd.Next(buttonTitles.Count());
            }
            else
            {
                while (true)
                {
                    newval = rnd.Next(buttonTitles.Count());
                    if (newval != lastval) break;
                }
            }
            lastval = newval;
            return buttonTitles[newval];
        }
    }
}