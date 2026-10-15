using System;
using System.Collections.Generic;

namespace GatewayClient {
    public static class TriggerManager {
        private static readonly Dictionary<string, Trigger> _triggers = new(StringComparer.OrdinalIgnoreCase);
        private static readonly object _lock = new();


        public static void RegisterTrigger(Trigger trigger) {
            lock (_lock) {
                _triggers[trigger.Name] = trigger;
            }
        }

        public static void EnableTrigger(string name, bool isOneShot, Action<string[]> callback) {
            lock (_lock) {
                if (_triggers.TryGetValue(name, out var trigger)) {
                    trigger.Enable(isOneShot, callback);
                }
            }
        }

        public static void DisableTrigger(string name) {
            lock (_lock) {
                if (_triggers.TryGetValue(name, out var trigger)) {
                    trigger.Disable();
                }
            }
        }

        public static void ProcessIncomingLine(string line) {
            Trigger[] snapshot;
            lock (_lock) {
                snapshot = [.. _triggers.Values];
            }

            foreach (var trigger in snapshot) {
                if (trigger.Process(line)) {
                    return;
                }
            }
        }
    }
}

