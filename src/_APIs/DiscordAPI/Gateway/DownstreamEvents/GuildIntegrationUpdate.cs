using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quarrel.Gateway.DownstreamEvents
{
    public class GuildIntegrationUpdate
    {
        [JsonProperty("guild_id")]
        public string GuildId { get; set; }
    }
}