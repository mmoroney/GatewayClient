using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GatewayClient {
    public static class AliasManager {
        private class Alias {
            public required Regex Pattern { get; set; }
            public required Action<string[]> Callback { get; set; }
        }

        private static readonly List<Alias> _aliases = new List<Alias>();

        static AliasManager() {
            AddAlias(@"^travel\s+(\S+)\s+(\S+)$", tokens => {
                string source = tokens[0];
                string destination = tokens[1];
                Console.WriteLine($"DoTravel {source} {destination}");
            });
        }

        public static void AddAlias(string pattern, Action<string[]> callback) {
            _aliases.Add(new Alias {
                Pattern = new Regex(pattern, RegexOptions.IgnoreCase),
                Callback = callback
            });
        }

        public static bool ProcessOutgoingLine(string line) {
            foreach (var alias in _aliases) {
                var match = alias.Pattern.Match(line);
                if (match.Success) {
                    string[] tokens = [.. match.Groups.Cast<Group>()
                                          .Skip(1)
                                          .Select(g => g.Value)];

                    alias.Callback(tokens);
                    return true;
                }
            }

            return false;
        }
    }
}