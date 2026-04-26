using System;
using System.Collections.Generic;
using System.Linq;

namespace GatewayClient {
    public static class RouteHelper {
        private static readonly Dictionary<string, string> OppositeDirections = new Dictionary<string, string>
        {
            {"n", "s"}, {"s", "n"},
            {"e", "w"}, {"w", "e"},
            {"ne", "sw"}, {"sw", "ne"},
            {"nw", "se"}, {"se", "nw"},
            {"u", "d"}, {"d", "u"}
        };

        public static string ReverseRoute(string route) {
            if (string.IsNullOrWhiteSpace(route)) return route;

            string[] steps = route.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> reversedSteps = new List<string>();

            foreach (var step in steps) {
                string[] tokens = step.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string newStep;
                if (tokens.Length == 1) {
                    newStep = ReverseDirection(tokens[0]);
                }
                else {
                    newStep = $"{tokens[0]} {ReverseDirection(tokens[1])}";
                }

                reversedSteps.Insert(0, newStep);
            }

            return string.Join(", ", reversedSteps);
        }

        private static string ReverseDirection(string direction) {
            if (!OppositeDirections.TryGetValue(direction.ToLower(), out var opposite)) {
                throw new InvalidDataException($"Unknown direction '{direction}' cannot be reversed.");
            }
            return opposite;
        }
    }
}