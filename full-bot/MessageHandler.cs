using VkNet.Model;
using VkNet;
using System.Text.RegularExpressions;

namespace VkBotDemo
{
    /// <summary>
    /// Класс-обработчик входящих сообщений.
    /// Простое добавление новой команды не затрагивает остальной код.
    /// </summary>
    public class MessageHandler
    {
        private readonly VkApi _api;

        // Конструктор получает уже авторизованный api
        public MessageHandler(VkApi api)
        {
            _api = api;
        }

        /// <summary>
        /// Главная точка входа: обработать одно сообщение.
        /// </summary>
        public void Handle(Message msg)
        {
            // Игнорируем пустые сообщения
            if (string.IsNullOrWhiteSpace(msg.Text)) return;

            var text = msg.Text.Trim();
            Console.WriteLine($"[ВХОД] => {text}");

            // 1. Определяем команду (можно легко расширить)
            switch (text.ToLower())
            {
                case "!привет":
                    SendMessage(msg, "Передаю привет студентам СГТУ 🤖");
                    break;

                case "!время":
                    SendMessage(msg, $"Сейчас: {DateTime.Now:HH:mm:ss}");
                    break;

                case "!помощь":
                    var help = "Мои команды:\n!привет\n!время\n!калькулятор 2+2\n!скайнет";
                    SendMessage(msg, help);
                    break;

                case "!скайнет":
                    SendMessage(msg, $"Не переживай, мы не будем захватывать вас, кожаные 😈\nПока что...");
                    break;

                // Продвинутый пример: "!калькулятор 5+3" вычисляет ответ
                case var _ when text.StartsWith("!калькулятор"):
                    HandleCalculator(msg, text);
                    break;

                default:
                    SendMessage(msg, $"Не знаю команду '{text}'. Напиши !помощь");
                    break;
            }
        }

        /// <summary>
        /// Обработчик для калькулятора (простой пример с регуляркой)
        /// </summary>
        private void HandleCalculator(Message msg, string fullText)
        {
            var match = Regex.Match(fullText, @"!калькулятор\s+(\d+)\s*([+\-*/])\s*(\d+)");
            if (!match.Success)
            {
                SendMessage(msg, "Пример использования: !калькулятор 5+3");
                return;
            }

            int a = int.Parse(match.Groups[1].Value);
            string op = match.Groups[2].Value;
            int b = int.Parse(match.Groups[3].Value);
            int result = 0;

            switch (op)
            {
                case "+": result = a + b; break;
                case "-": result = a - b; break;
                case "*": result = a * b; break;
                case "/": result = b != 0 ? a / b : 0; break;
            }

            SendMessage(msg, $"{a} {op} {b} = {result}");
        }

        /// <summary>
        /// Утилита: отправить сообщение в тот же диалог
        /// </summary>
        private void SendMessage(Message originalMsg, string responseText)
        {
            try
            {
                _api.Messages.Send(new MessagesSendParams
                {
                    PeerId = originalMsg.PeerId,
                    Message = responseText,
                    RandomId = new Random().Next()
                });
                Console.WriteLine($"[ОТВЕТ] => {responseText}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ОШИБКА ОТПРАВКИ] {ex.Message}");
            }
        }
    }
}