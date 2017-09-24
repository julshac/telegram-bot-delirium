using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace pj2
{
    class GameRoom
    {
        private const string str_newgame = "Новая игра";
        private const string str_tip = "Пояснить";
        private const string str_rules = "Правила";
        private const string str_deck = "Колода";
        private const string str_custom_cards = "Свои карты";
        private const string str_back = "Назад";
        private const string str_ingame_menu = "Меню";
        private const string str_main_menu = "Главное меню";

        private ObservableCollection<Card> cards = new ObservableCollection<Card>()
        {
            new Card("Я", 4, "Этот бокал для тебя."),
            new Card("Ты", 4, "Выбери, кто будет пить."),
            new Card("Джентльмены", 2, "Все джентльмены за столом пьют."),
            new Card("Леди", 2, "Все леди за столом пьют."),
            new Card("Тост", 2, "Произносишь тост, все пьют."),
            new Card("Передай налево", 3, "Игрок слева от тебя пьет."),
            new Card("Передай направо", 3, "Игрок справа от тебя пьет."),
            new Card("Вызов", 2, "Выбери игрока и выпей. Он должен выпить не меньше тебя."),
            new Card("Все на пол", 1, "Все игроки должны коснуться пола. Последний пьет."),
            new Card("Прозвище", 4, "Придумай прозвище игроку. Все должны звать его этим прозвищем до конца игры. Тот, кто обратился к нему иначе, пьет."),
            new Card("Твои правила", 3, "Становишься Rulemaster'ом и придумываешь свои правила. Следующий Rulemaster может отменить правила предыдущего."),
            new Card("Секретная служба", 2, "Все должны приложить ладонь к уху, изображая телохранителей. Последний становится президентом и пьет."),
            new Card("Брудершафт", 2, "Выпей на брудершафт с игроком на выбор."),
            new Card("Шах и мат", 3, "Выбери игрока, который будет пить с тобой каждый раз, когда ты ошибаешься."),
            new Card("Повтори за мной", 1, "Просишь игрока повторить за тобой (скороговорку или сложнопроизносимое слово). Если у него не получилось - он пьет. Получилось - пьешь ты."),
            new Card("Неудобные вопросы", 3, "Каждый игрок имеет право задать тебе любой вопрос. Если ты отказываешься на него отвечать - ты пьешь."),
            new Card("Нос", 2, "Все игроки должны коснуться носа. Последний пьет."),
            new Card("Категория", 2, "Вытянувший карту придумывает категорию (марки презервативов, музыкальные группы 90-х годов, модели Mercedes). Остальные игроки называют слова из этой категории. Кто не сможет - пьет."),
            new Card("Я никогда не", 3, "Говоришь то, что ты \"Никогда не делал\" (но на самом деле делал или очень хотел бы). Тот, кто делал это, пьет."),
            new Card("Вопросы", 2, "Игрок задает вопрос игроку слева. Отвечать на него нельзя, нужно быстро задать вопрос следующему соседу. Сбился? Ошибся? Запнулся? Выпей."),
            new Card("Цвет", 2, "Игрок называет цвет, следующий повторяет его и добавляет свой, и так далее. Кто сбился, тот пьет."),
            new Card("Кубок", 4, "Первые три игрока, вытянувшие эту карту, сливают содержимое своих бокалов в кубок. Четвертый это дело выпивает."),
            new Card("Саймон говорит", 1, "Тот, кто вытянул эту карту делает какой-нибудь жест, следующий делает то же самое и добавляет свой. Так продолжается, пока кто-нибудь не собьется."),
            new Card("Товарищ заебал",1,"Игрок, вытянувший эту карту, становится товарищем. Другим игрокам нельзя отвечать на его вопросы.")
        };

        private static readonly string[] buttonTitles = new string[]
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
            "Алкоголь на тебя не действует!",
            "GetRandomButtonTitle()"
        };

        private static readonly string[] confirmations = new string[]
        {
            "Ок.",
            "Выполняю.",
            "Как скажешь.",
            "Да.",
            "Хорошо."
        };

        private Dictionary<string, Action> _actions;

        private Random rnd = new Random(DateTime.Now.Millisecond);

        private ReplyKeyboardMarkup kbMainMenu;
        private ReplyKeyboardMarkup kbGame;
        private ReplyKeyboardMarkup kbGameMenu;
        private InlineKeyboardMarkup kbCardPage;
        private TelegramBotClient bot;
        private long chatId;

        private Stack<int> deck;
        private int cupCount;
        private bool started = false;
        private int lastPlayedId = -1;

        private Stack<IReplyMarkup> kbs = new Stack<IReplyMarkup>();

        private int pageSize = 5;
        private int maxPage = 4;

        public GameRoom(long chatid, TelegramBotClient bot)
        {
            this.bot = bot;
            chatId = chatid;

            cards.CollectionChanged += cardsCollectionChangedHandler;

            InitKeyboardMarkups();
            InitActions();

            kbs.Push(kbMainMenu);
            SendMsg("Добро пожаловать в игру делириум!");
        }

        private void cardsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add
             || e.Action == NotifyCollectionChangedAction.Remove)
            {
                maxPage = (int)Math.Ceiling(cards.Count() / (double)pageSize);
            }
        }

        private void InitActions()
        {
            _actions = new Dictionary<string, Action>()
            {
                {str_newgame, NewGame},
                {str_back, Back},
                {str_main_menu, MainMenu},
                {str_ingame_menu, InGameMenu},
                {str_rules, Rules},
                {"default", NextCard},
                {str_tip, Tip},
                {str_deck, Deck}
            };
        }

        private int callbackType = 0;
        private int callbackDeckCurrentPage = 0;
        private int callbackMessageId = 0;

        private void Deck()
        {
            kbs.Push(CreatePage());
            var task = bot.SendTextMessageAsync(chatId, "Ваша колода:", replyMarkup: kbs.Peek());

            callbackMessageId = task.Result.MessageId;

            callbackType = 1;
        }

        private InlineKeyboardMarkup CreatePage()
        {
            bool last = pageSize * (callbackDeckCurrentPage + 1) >= cards.Count();
            bool first = callbackDeckCurrentPage == 0;

            var cnt = pageSize;
            if (last) cnt = cards.Count() - pageSize * callbackDeckCurrentPage;

            var tmp = new InlineKeyboardButton[cnt + 1][];

            for (int i = 0; i < cnt; i++)
            {
                var k = callbackDeckCurrentPage * pageSize + i;
                var btn = InlineKeyboardButton.WithCallbackData($"{cards[k].Name}: {cards[k].Count}", k.ToString());
                tmp[i] = new InlineKeyboardButton[] { btn };
            }

            if (first) tmp[cnt] = new InlineKeyboardButton[] { "Ок", "->" };
            if (last) tmp[cnt] = new InlineKeyboardButton[] { "Ок", "<-" };
            if (!first && !last) tmp[cnt] = new InlineKeyboardButton[] { "Ок", "<-", "->" };
            if (first && last) tmp[cnt] = new InlineKeyboardButton[] { "Ок" };

            return new InlineKeyboardMarkup(tmp);
        }

        private void Tip()
        {
            if (lastPlayedId == -1) { SendMsg("Всё и так понятно."); return; }
            if (lastPlayedId == -2) { SendMsg("Меню -> Главное меню -> Новая игра."); return; }
            SendMsg(cards[lastPlayedId].Description);
        }

        private void NextCard()
        {
            if (!started) { SendMsg("Игра ещё не начата!"); return; }
            if (deck.Count() == 0) { lastPlayedId = -2; SendMsg("Карты в колоде закончились!"); return; }

            var id = deck.Pop();
            lastPlayedId = id;

            var name = cards[id].Name;

            kbGame.Keyboard[0][0].Text = GetRandomButtonTitle();
            SendMsg(cards[id].Name);

            if (name == "Кубок")
            {
                if (++cupCount == cards[id].Count) SendMsg($"Это {cupCount}-й! Пей до дна!");
                else SendMsg($"Это {cupCount}-й. Только тому, кто вытянет {cards[id].Count}-й, достанется чаша!");
            }
        }

        private void InGameMenu()
        {
            kbs.Push(kbGameMenu);
            SendMsg(Confirmation());
        }

        private void MainMenu()
        {
            started = false;
            lastPlayedId = -1;

            kbs.Clear();
            kbs.Push(kbMainMenu);
            SendMsg(Confirmation());
        }

        private void Back()
        {
            kbs.Pop();
            if (kbs.Count() == 0) kbs.Push(kbMainMenu);
            SendMsg(Confirmation());
        }

        private void InitKeyboardMarkups()
        {
            KeyboardButton[][] tmp = new KeyboardButton[3][];
            tmp[0] = new KeyboardButton[] { str_newgame };
            tmp[1] = new KeyboardButton[] { str_deck, str_custom_cards };
            tmp[2] = new KeyboardButton[] { str_rules };

            kbMainMenu = new ReplyKeyboardMarkup(tmp, true);


            tmp = new KeyboardButton[2][];
            tmp[0] = new KeyboardButton[] { "GetRandomButtonTitle()" };
            tmp[1] = new KeyboardButton[] { str_tip, str_ingame_menu };

            kbGame = new ReplyKeyboardMarkup(tmp, true);


            tmp = new KeyboardButton[3][];
            tmp[0] = new KeyboardButton[] { str_main_menu };
            tmp[1] = new KeyboardButton[] { str_rules };
            tmp[2] = new KeyboardButton[] { str_back };

            kbGameMenu = new ReplyKeyboardMarkup(tmp, true);

            
            var tmp2 = new InlineKeyboardButton[3][];

            tmp2[0] = new InlineKeyboardButton[]{"-","+"};
            tmp2[1] = new InlineKeyboardButton[]{"Описание"};
            tmp2[2] = new InlineKeyboardButton[]{"Назад"};

            kbCardPage = new InlineKeyboardMarkup(tmp2);
        }

        private void NewGame()
        {
            lastPlayedId = -1;
            cupCount = 0;
            CreateDeck();

            started = true;

            kbGame.Keyboard[0][0].Text = GetRandomButtonTitle();
            kbs.Push(kbGame);
            SendMsg("Новая колода готова!");
        }

        private void CreateDeck()
        {
            var tmp = new List<int>();

            for (int i = 0; i < cards.Count(); i++)
            {
                for (int j = 0; j < cards[i].Count; j++)
                {
                    tmp.Add(i);
                }
            }

            int buf, rndPlace;

            for (int i = 0; i < tmp.Count; i++)
            {
                buf = tmp[i];
                rndPlace = rnd.Next(tmp.Count);
                tmp[i] = tmp[rndPlace];
                tmp[rndPlace] = buf;
            }

            deck = new Stack<int>(tmp);
        }


        private void Rules()
        {
            SendMsg("yo, tipo pravila igry");
        }

        private bool hasSentSticker = false;
        public void HandleMessage(Message m)
        {
            if (callbackType > 0)
            {
                bot.DeleteMessageAsync(chatId, m.MessageId);
                return;
            }
            switch (m.Type)
            {
                case MessageType.TextMessage:
                    {
                        if (_actions.ContainsKey(m.Text)) _actions[m.Text]();
                        else _actions["default"]();
                        break;
                    }
                case MessageType.StickerMessage:
                    {
                        if (!hasSentSticker)
                        {
                            hasSentSticker = true;
                            bot.SendTextMessageAsync(chatId, "*winky face*");
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        public void HandleCallbackQuery(CallbackQuery q)
        {
            switch (callbackType)
            {
                case 1:
                    {
                        if (q.Message.MessageId == callbackMessageId) DeckViewer(q);
                        break;
                    }
            }
            bot.AnswerCallbackQueryAsync(q.Id);
        }

        AutoResetEvent resetEvent = new AutoResetEvent(true);
        int callbackCardId;

        private void DeckViewer(CallbackQuery q)
        {
            resetEvent.WaitOne(-1);
            switch (q.Data)
            {
                case "->":
                    {
                        if (callbackDeckCurrentPage < maxPage)
                        {
                            callbackDeckCurrentPage++;

                            kbs.Push(CreatePage());
                            bot.EditMessageReplyMarkupAsync(chatId, callbackMessageId, kbs.Peek());
                        }

                        break;
                    }
                case "<-":
                    {
                        if (callbackDeckCurrentPage > 0)
                        {
                            callbackDeckCurrentPage--;

                            kbs.Pop();
                            bot.EditMessageReplyMarkupAsync(chatId, callbackMessageId, kbs.Peek());
                        }
                        break;
                    }
                case "Ок":
                    {
                        bot.DeleteMessageAsync(chatId, callbackMessageId);
                        
                        callbackType = 0;
                        callbackMessageId = 0;


                        kbs.Clear();
                        kbs.Push(kbMainMenu);
                        SendMsg(Confirmation());

                        break;
                    }
                case "+":
                    {

                        break;
                    }
                case "-":
                    {

                        break;
                    }
                case "Описание":
                    {
                        bot.AnswerCallbackQueryAsync(
                            q.Id,
                            cards[callbackCardId].Description,
                            true
                        );
                        
                        break;
                    }
                case "Назад":
                    {
                        kbs.Pop();
                        kbs.Pop();
                        kbs.Push(CreatePage());

                        bot.EditMessageTextAsync(
                            chatId,
                            callbackMessageId,
                            "Ваша колода:",
                            replyMarkup: kbs.Peek()
                        );
                        break;
                    }
                default:
                    {
                        callbackCardId = Int32.Parse(q.Data);

                        kbs.Push(kbCardPage);

                        bot.EditMessageTextAsync(
                            chatId, 
                            callbackMessageId,
                            $"{cards[callbackCardId].Name}: {cards[callbackCardId].Count}",
                            replyMarkup: kbs.Peek()
                        );   

                        break;
                    }
            }
            resetEvent.Set();
        }

        private void SendMsg(string msg)
        {
            bot.SendTextMessageAsync(chatId, msg, replyMarkup: kbs.Peek());
        }

        int lastval = -1;
        private string GetRandomButtonTitle()
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

        private string Confirmation()
        {
            return confirmations[rnd.Next(0, confirmations.Count())];
        }
    }
}