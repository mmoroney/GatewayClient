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
                TravelManager.DoTravel(source, destination);
            });

            AddAlias(@"^checkvenom$", async _ => {
                try {
                    var weight = await SerpentManager.CheckVenomAsync();
                    Console.WriteLine($"\n[Serpent] Current venom: {weight}");
                }
                catch (Exception ex) {
                    Console.WriteLine($"\n[Serpent] CheckVenom failed: {ex.Message}");
                }
            });

            AddAlias(@"^checkhealth$", async _ => {
                try {
                    var health = await CharacterManager.CheckHealthAsync();
                    Console.WriteLine($"\n[Character] {health}");
                }
                catch (Exception ex) {
                    Console.WriteLine($"\n[Character] CheckHealth failed: {ex.Message}");
                }
            });

            AddAlias(@"^serpentheal$", async _ => {
                try {
                    Console.WriteLine("\n[Serpent] Initiating toxopiate healing...");
                    bool success = await SerpentManager.SerpentHealAsync();
                    if (success) {
                        Console.WriteLine("\n[Serpent] Healing complete: You are at full strength.");
                    }
                    else {
                        Console.WriteLine("\n[Serpent] Healing failed: Insufficient distillate.");
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"\n[Serpent] Heal failed or timed out: {ex.Message}");
                }
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