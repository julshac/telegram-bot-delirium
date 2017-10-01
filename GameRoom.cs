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
using Medallion;

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
        private const string str_add_custom = "Добавить";

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

        private static readonly string[] buttonTitles = new[]
        {
            "Хоп, хэй, лалалэй",
            "Наливай!",
            "Гриша одобряэ",
            "Жопа - не лень",
            "Люк, я твой стакан!",
            "Ты сможешь!",
            "Держись!",
            "Пей до дна!",
            "Сперва подлей соседу",
            "За Родину!",
            "Never gonna give you up",
            "За маму и двор",
            "Матрос - это такая вошь",
            "Алкоголь на тебя не действует!",
            "RandomText()"
        };

        private static readonly string[] confirmations = new[]
        {
            "Ок.",
            "Выполняю.",
            "Как скажешь.",
            "Да.",
            "Хорошо."
        };

        private Dictionary<string, Action<Message>> messageActions;
        private Dictionary<string, Action<CallbackQuery>> callbackActions;

        private Random rnd = new Random(DateTime.Now.Millisecond);

        private ReplyKeyboardMarkup kbMainMenu;
        private ReplyKeyboardMarkup kbGame;
        private ReplyKeyboardMarkup kbGameMenu;
        private ReplyKeyboardMarkup kbCustomCards;
        private ReplyKeyboardMarkup kbCancel;
        private InlineKeyboardMarkup kbCardPage;
        private TelegramBotClient bot;
        private long chatId;

        private Stack<int> deck;
        private int cupCount;
        private bool started = false;
        private int lastPlayedCardInd = -1;

        private Stack<IReplyMarkup> kbs = new Stack<IReplyMarkup>();

        private int pageSize = 5;
        private int maxPage = 4;

        private int callbackType = 0;
        private int callbackDeckCurrentPage = 0;
        private int callbackMessageId = 0;

        private AutoResetEvent resetEvent = new AutoResetEvent(true);

        public GameRoom(long chatid, TelegramBotClient bot)
        {
            this.bot = bot;
            chatId = chatid;

            cards.CollectionChanged += cardsCollectionChangedHandler;

            InitKeyboardMarkups();
            InitMessageActions();

            kbs.Push(kbMainMenu);
            SendMsg("Добро пожаловать в игру делириум!");
        }

        public void HandleMessage(Message m)
        {
            if (resetEvent.WaitOne(0))
            {
                if (m.Type == MessageType.TextMessage)
                    if (ValidateAction(m.Text))
                        if (messageActions.ContainsKey(m.Text)) messageActions[m.Text](m);
                        else messageActions["default"](m);
                    else if (waitingForUserInput)
                        CardAdder(m);

                resetEvent.Set();
            }
        }

        private int cardAddingStage;
        private bool waitingForUserInput;

        private string tmpName;

        private void CardAdder(Message m)
        {
            if (m.Type != MessageType.TextMessage) return;
            if (cardAddingStage == 1 && m.Text.Length > 20 || cardAddingStage == 2 && m.Text.Length > 140) return;

            if (m.Text == str_back)
            {
                cardAddingStage = 0;
                waitingForUserInput = false;
                kbs.Pop();
                SendMsg(GetRandomConfirmation());
                return;
            }

            if (cardAddingStage == 1)
            {
                tmpName = m.Text;
                cardAddingStage = 2;
                SendMsg("А теперь описание карты. Не более 140 символов.");
                return;
            }

            if (cardAddingStage == 2)
            {
                cards.Add(new Card(tmpName, 2, m.Text));
                kbs.Pop();
                SendMsg("Новая карта добавлена в количестве двух экземпляров!");
                cardAddingStage = 0;
                waitingForUserInput = false;
            }
        }

        public void HandleCallbackQuery(CallbackQuery q)
        {
            if (resetEvent.WaitOne(0))
            {
                if (q.Message.MessageId == callbackMessageId)
                    if (ValidateAction(q.Data)) DeckViewer(q);

                bot.AnswerCallbackQueryAsync(q.Id);

                resetEvent.Set();
            }
        }

        private void cardsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e) =>
            maxPage = (int)Math.Ceiling(cards.Count() / (double)pageSize);

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


            tmp = new KeyboardButton[2][];
            tmp[0] = new KeyboardButton[] { str_add_custom };
            tmp[1] = new KeyboardButton[] { str_back };

            kbCustomCards = new ReplyKeyboardMarkup(tmp, true);


            kbCancel = new ReplyKeyboardMarkup(new KeyboardButton[] {str_back}, true);


            var tmp2 = new InlineKeyboardButton[3][];

            tmp2[0] = new InlineKeyboardButton[] { "-", "+" };
            tmp2[1] = new InlineKeyboardButton[] { "Описание" };
            tmp2[2] = new InlineKeyboardButton[] { str_back };

            kbCardPage = new InlineKeyboardMarkup(tmp2);
        }

        #region message actions
        private void InitMessageActions()
        {
            messageActions = new Dictionary<string, Action<Message>>()
            {
                {str_newgame, NewGame},
                {str_main_menu, MainMenu},
                {str_ingame_menu, InGameMenu},
                {str_rules, Rules},
                {"default", NextCard},
                {str_tip, Tip},
                {str_deck, Deck},
                {str_custom_cards, CustomCards},
                {str_add_custom, AddCustomCard},
                {str_back, Back}
            };
        }

        private void NewGame(Message m)
        {
            cupCount = 0;
            CreateDeck();

            started = true;

            SetRandomButtonText();
            kbs.Clear();
            kbs.Push(kbGame);
            SendMsg("Новая колода готова!");
        }

        private void MainMenu(Message m)
        {
            started = false;

            kbs.Clear();
            kbs.Push(kbMainMenu);
            SendMsg(GetRandomConfirmation());
        }

        private void InGameMenu(Message m)
        {
            kbs.Push(kbGameMenu);
            SendMsg(GetRandomConfirmation());
        }

        private void Rules(Message m) => SendMsg("yo, tipo pravila igry");

        private void NextCard(Message m)
        {
            if (!started) return;
            if (deck.Count() == 0) { SendMsg("Карты в колоде закончились!"); return; }

            lastPlayedCardInd = deck.Pop();

            var name = cards[lastPlayedCardInd].Name;

            SetRandomButtonText();
            SendMsg(cards[lastPlayedCardInd].Name);

            if (name == "Кубок")
            {
                if (++cupCount == cards[lastPlayedCardInd].Count) SendMsg($"Это {cupCount}-й! Пей до дна!");
                else SendMsg($"Это {cupCount}-й. Только тому, кто вытянет {cards[lastPlayedCardInd].Count}-й, достанется чаша!");
            }
        }

        private void Tip(Message m)
        {
            try
            {
                if (m.ReplyToMessage == null) SendMsg(cards[lastPlayedCardInd].Description);
                else SendMsg(cards.Single(card => card.Name == m.ReplyToMessage.Text).Description);
            }
            catch { }
        }

        private void Deck(Message m)
        {
            kbs.Push(CreatePage());
            var task = SendMsg("Ваша колода:");

            callbackMessageId = task.Result.MessageId;

            callbackType = 1;
        }

        private void CustomCards(Message m)
        {
            kbs.Push(kbCustomCards);
            SendMsg(GetRandomConfirmation());
        }

        private void AddCustomCard(Message m)
        {
            kbs.Push(kbCancel);
            SendMsg("Пришли мне название карты, пожалуйста.");

            waitingForUserInput = true;
            cardAddingStage = 1;
        }

        private void Back(Message m)
        {
            kbs.Pop();
            SendMsg(GetRandomConfirmation());
        }
        #endregion

        private void CreateDeck()
        {
            var tmp = new List<int>();

            for (int i = 0; i < cards.Count(); i++)
                for (int j = 0; j < cards[i].Count; j++)
                    tmp.Add(i);

            tmp.Shuffle(rnd);

            deck = new Stack<int>(tmp);
        }

        private bool ValidateAction(string a)
        {
            var r = false;
            IReplyMarkup markup;

            if (kbs.TryPeek(out markup))
                if (markup is ReplyKeyboardMarkup)
                {
                    ReplyKeyboardMarkup keyboardMarkup = (ReplyKeyboardMarkup)markup;
                    var keyboard = keyboardMarkup.Keyboard;

                    foreach (var row in keyboard)
                        foreach (var button in row)
                            if (button.Text == a)
                            { r = true; break; }
                }
                else if (markup is InlineKeyboardMarkup)
                {
                    InlineKeyboardMarkup keyboardMarkup = (InlineKeyboardMarkup)markup;

                    var keyboard = keyboardMarkup.InlineKeyboard;

                    foreach (var row in keyboard)
                        foreach (var button in row)
                            if (button.Text == a)
                            { r = true; break; }

                    int x = -1;
                    if (int.TryParse(a, out x) && x >= 0 && x < cards.Count()) r = true;
                }

            return r;
        }

        private int callbackCardId;

        private void DeckViewer(CallbackQuery q)
        {
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
                        SendMsg(GetRandomConfirmation());

                        break;
                    }
                case "+":
                    {
                        if (cards[callbackCardId].Count < 5)
                        {
                            bot.EditMessageTextAsync(chatId, callbackMessageId,
                                $"{cards[callbackCardId].Name}: {++cards[callbackCardId].Count}",
                                replyMarkup: kbs.Peek()
                            );
                        }
                        break;
                    }
                case "-":
                    {
                        if (cards[callbackCardId].Count > 0)
                        {
                            bot.EditMessageTextAsync(chatId, callbackMessageId,
                                $"{cards[callbackCardId].Name}: {--cards[callbackCardId].Count}",
                                replyMarkup: kbs.Peek()
                            );
                        }
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
        }

        private Task<Message> SendMsg(string msg) => bot.SendTextMessageAsync(chatId, msg, replyMarkup: kbs.Peek());

        private void SetRandomButtonText() => kbGame.Keyboard[0][0].Text = buttonTitles[rnd.Next(0, buttonTitles.Count())];

        private string GetRandomConfirmation() => confirmations[rnd.Next(0, confirmations.Count())];
    }
}