using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace GatewayClient.Data {
    internal static class RouteManager {
        private static JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        public static List<Route> Load() {
            var asm = Assembly.GetExecutingAssembly();

            const string defaultResourceName = "GatewayClient.Data.routes.json";
            Stream stream = asm.GetManifestResourceStream(defaultResourceName)
                ?? throw new Exception($"Failed to load embedded resource '{defaultResourceName}'. Available resources: {string.Join(", ", asm.GetManifestResourceNames())}");
            using (stream) {
                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();
                var data = JsonSerializer.Deserialize<RouteData>(json, options)
                    ?? throw new Exception("Failed to deserialize routes.json into RouteData.");
                return data.Routes ?? throw new Exception("Deserialized RouteData.Routes is null.");
            }
        }
    }
}