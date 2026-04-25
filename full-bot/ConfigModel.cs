namespace VkBotDemo
{
    /// <summary>
    /// Модель конфигурации бота, загружаемая из config.json
    /// </summary>
    public class ConfigModel
    {
        /// <summary>
        /// Access токен сообщества VK
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        
        /// <summary>
        /// ID группы (сообщества) VK
        /// </summary>
        public ulong GroupId { get; set; }
    }
}