using System;
using System.Collections.Generic;

namespace GatewayClient {
    public static class TriggerManager {
        private static readonly Trigger[] triggers = {
        new("OnHealthCheck", @".*\[HP: (\d+)\/\((\d+)\)  SP: (\d+)\/\((\d+)\)\]") };

        public static void EnableTrigger(string name, bool isOneShot, Action<string[]> callback) {
            foreach (var trigger in triggers) {
                if (trigger.Name == name) {
                    trigger.Enable(isOneShot, callback);
                    return;
                }
            }
        }

        public static void DisableTrigger(string name) {
            foreach (var trigger in triggers) {
                if (trigger.Name == name) {
                    trigger.Disable();
                    return;
                }
            }
        }

        public static void ProcessIncomingLine(string line) {
            foreach(var trigger in triggers) {
                if (trigger.Process(line)) {
                    return;
                }
            }
        }
    }
}

