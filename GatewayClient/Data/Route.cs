using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GatewayClient.Data {
    public class Route {
        public string? Source { get; set; }
        public string? Destination { get; set; }
        public string? Commands { get; set; }
        public bool IsTwoWay { get; set; } = false;
        public string? WaitEvent { get; set; }
        public bool IsAsync => !string.IsNullOrEmpty(WaitEvent);
    }
}