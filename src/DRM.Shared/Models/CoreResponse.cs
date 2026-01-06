// CoreResponse.cs (C#)
using System;
using System.Collections.Generic;
using Newtonsoft.Json; // Need to add Newtonsoft.Json NuGet package

namespace DRM.Shared.Models
{
    public class CoreResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; } = false;

        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; } = 0;

        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty("data")]
        public string Data { get; set; } = string.Empty;

        [JsonProperty("logs")]
        public List<string> Logs { get; set; } = new List<string>();
    }
}