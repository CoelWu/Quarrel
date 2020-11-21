using Newtonsoft.Json;

namespace DiscordAPI.Models.Messages
{
    /// <summary>
    /// A model for a message reference.
    /// </summary>
    public class MessageReference
    {
        /// <summary>
        /// Gets or sets the id of the message.
        /// </summary>
        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the id of the channel the message originates from.
        /// </summary>
        [JsonProperty("channel_id")]
        public string ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the id of the guild the message originates from.
        /// </summary>
        [JsonProperty("guild_id")]
        public string GuildId { get; set; }
    }
}
