using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

namespace GatewayClient.Data {
    internal static class RouteManager {
        private static JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        public static RouteData? Load() {
            var asm = Assembly.GetExecutingAssembly();

            const string defaultResourceName = "GatewayClient.Data.routes.json";
            Stream? stream = asm.GetManifestResourceStream(defaultResourceName) ?? throw new Exception($"Failed to load embedded resource '{defaultResourceName}'. Available resources: {string.Join(", ", asm.GetManifestResourceNames())}");
            using (stream) {
                using StreamReader reader = new(stream);
                string json = reader.ReadToEnd();
                return JsonSerializer.Deserialize<RouteData>(json, options);
            }
        }
    }
}