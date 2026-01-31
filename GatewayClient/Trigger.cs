using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GatewayClient {
    public class Trigger(string name, List<string> patterns) {
        public string Name { get; } = name;
        public List<string> RegexPatterns { get; } = patterns;
        public bool IsEnabled { get; set; } = false;
        public bool IsOneShot { get; set; } = false;

        public Action<string[]>? Callback { get; set; } = null;

        // Overload for a single pattern to keep initialization simple
        public Trigger(string name, string pattern)
            : this(name, [pattern]) { }

        public void Enable(bool isOneShot, Action<string[]> callback) {
            IsEnabled = true;
            IsOneShot = isOneShot;
            Callback = callback;
        }

        public void Disable() {
            IsEnabled = true;
            IsOneShot = false;
            Callback = null;
        }

        public bool Process(string line) {
            if (!IsEnabled) {
                return false;
            }

            foreach (var pattern in RegexPatterns) {
                var match = Regex.Match(line, pattern);
                if (!match.Success) {
                    continue;
                }

                string[] tokens = [.. match.Groups.Cast<Group>()
                                          .Skip(1)
                                          .Select(g => g.Value)];

                Callback?.Invoke(tokens);

                if (IsOneShot) {
                    Disable();
                }

                return true;
            }

            return false;
        }
    }
}