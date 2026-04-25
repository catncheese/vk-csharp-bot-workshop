using VkNet;
using VkNet.Model;
using VkNet.Enums.StringEnums;

namespace VkEchoBot
{
    public class StartUp
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "VK Эхо-Бот";
            Console.WriteLine("Загрузка конфигурации...");

            // 1. Авторизуемся
            var api = new VkApi();
            api.Authorize(new ApiAuthParams { AccessToken = "TOKEN" });

            ulong groupId = 12345678; // ID группы

            Console.WriteLine("Авторизация успешна!");

            // 2. Получаем параметры Long Poll сервера
            var server = api.Groups.GetLongPollServer(groupId);
            Console.WriteLine($"Бот запущен. ID группы: {groupId}");
            Console.WriteLine("Нажмите Ctrl+C для выхода.\n");

            // 3. Основной цикл
            while (true)
            {
                // Получаем новые события
                var poll = api.Groups.GetBotsLongPollHistory(
                    new BotsLongPollHistoryParams
                    {
                        Server = server.Server,
                        Ts = server.Ts,
                        Key = server.Key,
                        Wait = 25
                    });

                if (poll?.Updates == null) continue;

                // Обрабатываем каждое событие
                foreach (var update in poll.Updates)
                {
                    // Проверяем, что это новое сообщение
                    if (update.Type.Value == GroupUpdateType.MessageNew)
                    {
                        var newMessage = update.Instance as MessageNew;
                        if (newMessage?.Message != null)
                        {
                            var msg = newMessage.Message;

                            // Пропускаем пустые сообщения
                            if (string.IsNullOrWhiteSpace(msg.Text))
                                continue;

                            // Выводим в консоль
                            Console.WriteLine($"[ВХОД] {msg.Text}");

                            // Эхо-ответ: отправляем обратно то же самое сообщение
                            api.Messages.Send(new MessagesSendParams
                            {
                                PeerId = msg.PeerId,
                                Message = msg.Text,  // Отправляем тот же текст
                                RandomId = new Random().Next()
                            });

                            Console.WriteLine($"[ОТВЕТ] {msg.Text}");
                        }
                    }
                }

                // Обновляем Ts для следующего запроса
                server.Ts = poll.Ts;
            }
        }
    }
}