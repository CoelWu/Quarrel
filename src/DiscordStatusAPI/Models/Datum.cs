﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DiscordStatusAPI.Models
{
    public partial class Datum
    {
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("value")]
        public ushort Value { get; set; }
    }
}
