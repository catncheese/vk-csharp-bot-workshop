using System.Text.Json;
using VkNet;
using VkNet.Enums.StringEnums;
using VkNet.Model;

namespace VkBotDemo
{
    /// <summary>
    /// Точка входа в приложение VK бота
    /// </summary>
    public class StartUp
    {
        /// <summary>
        /// Флаг для корректного завершения работы бота
        /// </summary>
        private static bool _isRunning = true;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Title = "VK Бот";
            Console.WriteLine("Загрузка конфигурации...");

            // 1. Загрузка конфигурации
            var config = JsonSerializer.Deserialize<ConfigModel>(File.ReadAllText("config.json"));

            if (config == null)
            {
                Console.WriteLine("Не найден файл конфигурации!");
                return;
            }

            // 2. Авторизация в VK API
            var api = new VkApi();
            try
            {
                api.Authorize(new ApiAuthParams { AccessToken = config.AccessToken });
                Console.WriteLine("✅ Авторизация успешна!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка авторизации: { ex.Message }");
                Console.ReadKey();
                return;
            }

            // 3. Создаем обработчик сообщений
            var handler = new MessageHandler(api);
            Console.WriteLine("✅ Обработчик сообщений инициализирован");

            // 4. Получаем параметры для Long Poll подключения
            var server = api.Groups.GetLongPollServer(config.GroupId);
            Console.WriteLine($"✅ Long Poll сервер получен. ID группы: {config.GroupId}");

            // 5. Обработка Ctrl+C для корректного завершения
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _isRunning = false;
                Console.WriteLine("\n⏹️ Получен сигнал завершения...");
            };

            Console.WriteLine("\n🤖 Бот запущен и готов к работе!");
            Console.WriteLine("📝 Доступные команды: !помощь, !привет, !время, !калькулятор, !скайнет");
            Console.WriteLine("❌ Нажмите Ctrl+C для выхода.\n");

            // 6. Бесконечный цикл получения сообщений
            while (_isRunning)
            {
                try
                {
                    // Получаем историю обновлений
                    var poll = api.Groups.GetBotsLongPollHistory(
                        new BotsLongPollHistoryParams
                        {
                            Server = server.Server,
                            Ts = server.Ts,
                            Key = server.Key,
                            Wait = 25
                        });

                    // Если нет обновлений - продолжаем
                    if (poll?.Updates == null) continue;

                    // Обрабатываем каждое обновление
                    foreach (var update in poll.Updates)
                    {
                        if (update.Type.Value == GroupUpdateType.MessageNew)
                        {
                            var newMessage = update.Instance as MessageNew;
                            if (newMessage?.Message != null)
                            {
                                handler.Handle(newMessage.Message);
                            }
                        }
                    }

                    // Обновляем Ts для следующего запроса
                    server.Ts = poll.Ts;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Ошибка в цикле опроса: {ex.Message}");
                    Console.WriteLine("Повторное подключение через 5 секунд...");
                    Thread.Sleep(5000);

                    // Обновляем параметры сервера при ошибке
                    try
                    {
                        server = api.Groups.GetLongPollServer(config.GroupId);
                        Console.WriteLine("✅ Переподключение выполнено успешно");
                    }
                    catch (Exception reconnectEx)
                    {
                        Console.WriteLine($"❌ Ошибка переподключения: {reconnectEx.Message}");
                    }
                }
            }

            Console.WriteLine("\n👋 Бот остановлен. До свидания!");
        }
    }
}