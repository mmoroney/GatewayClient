using GatewayClient.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace GatewayClient {
    public static class TravelManager {
        private const int INF = 99999;

        private class RouteEntry(int depth, int next, string route, string callback) {
            public int Depth = depth;
            public int Next = next;
            public string Route = route;
            public string Callback = callback;
        }

        private static readonly Dictionary<string, int> locations = new();
        private static RouteEntry[,]? routeTable;

        public static void Load() {
            var routes = RouteManager.Load();
            HashSet<string> locationSet = new();
            foreach (Route route in routes) {
                if (route.Source != null) {
                    locationSet.Add(route.Source);
                }
                if (route.Destination != null) {
                    locationSet.Add(route.Destination);
                }
            }

            List<string> locationList = new();
            locationList.AddRange(locationSet);

            for (int i = 0; i < locationList.Count; i++)
                locations[locationList[i]] = i;


            routeTable = new RouteEntry[locationList.Count, locationList.Count];

            for(int i = 0; i < locationList.Count; i++) {
                for(int j = 0; j < locationList.Count; j++) {
                    int depth = i == j ? 0 : INF;
                    routeTable[i, j] = new RouteEntry(depth, j, "", "");
                }
            }

            foreach (Route route in routes) {
                AddEdge(route.Source!, route.Destination!, route.Commands!, route.WaitEvent!);
                if (route.IsTwoWay) {
                    AddEdge(route.Destination!, route.Source!, RouteHelper.ReverseRoute(route.Commands!), route.WaitEvent!);
                }
            }

            for (int i = 0; i < locationList.Count; i++) {
                for (int j = 0; j < locationList.Count; j++) {
                    for (int k = 0; k < locationList.Count; k++) {
                        var ij = routeTable[i, j];
                        var ik = routeTable[i, k];
                        var jk = routeTable[k, j];
                        if (ij.Depth > ik.Depth + jk.Depth) {
                            ij = new RouteEntry(ik.Depth + jk.Depth, ik.Next, ik.Route, ik.Callback);
                            routeTable[i, j] = ij;
                        }
                    }
                }
            }
        }

        public static void DoTravel(string source, string dest, Action? callback = null) {
            if (!locations.TryGetValue(source, out int sourceIndex)) {
                Console.WriteLine($"Unknown source location: {source}");
                return;
            }

            if (!locations.TryGetValue(dest, out int destIndex)) {
                Console.WriteLine($"Unknown dest location: {dest}");
                return;
            }

            Travel(sourceIndex, destIndex, callback);
        }

        private static void Travel(int sourceIndex, int destIndex, Action? callback) {
            while (sourceIndex != destIndex) {
                var routeEntry = routeTable![sourceIndex, destIndex];
                var next = routeTable![routeEntry.Next, destIndex];

                if (routeEntry.Route != "") {
                    Console.WriteLine($"Traveling from {sourceIndex} to {routeEntry.Next} via {routeEntry.Route}");
                }
                else {
                    Console.WriteLine($"Traveling from {sourceIndex} to {routeEntry.Next} via callbacl {routeEntry.Callback}");
                }
                sourceIndex = next.Next;
            }

            if (callback != null) {
                callback();
            }
        }

        private static void AddEdge(string source, string dest, string commands, string waitEvent) {
            int sourceIndex = locations[source];
            int destIndex = locations[dest];
            routeTable![sourceIndex, destIndex] = new RouteEntry(1, destIndex, commands, waitEvent);
        }
    }
}
