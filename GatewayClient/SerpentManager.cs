using System;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayClient {
    public static class SerpentManager {
        public const string VenomCheckTriggerName = "Serpent.VenomCheck";

        // Regex matches "Current quantity:        4 ounces, 1 drams, 0 scruples, and 15 grains"
        private const string VenomCheckRegex =
            @"Current quantity:\s+(\d+)\s+ounces?,\s+(\d+)\s+drams?,\s+(\d+)\s+scruples?,\s+and\s+(\d+)\s+grains?";

        static SerpentManager() {
            TriggerManager.RegisterTrigger(new Trigger(VenomCheckTriggerName, VenomCheckRegex));
        }

        public static async Task<ApothecaryWeight> CheckVenomAsync(TimeSpan? timeout = null, CancellationToken ct = default) {
            var tcs = new TaskCompletionSource<ApothecaryWeight>(TaskCreationOptions.RunContinuationsAsynchronously);

            TriggerManager.EnableTrigger(VenomCheckTriggerName, isOneShot: true, tokens => {
                if (tokens.Length >= 4 &&
                    int.TryParse(tokens[0], out int oz) &&
                    int.TryParse(tokens[1], out int dr) &&
                    int.TryParse(tokens[2], out int sc) &&
                    int.TryParse(tokens[3], out int gr)) {
                    tcs.TrySetResult(new ApothecaryWeight(oz, dr, sc, gr));
                }
                else {
                    tcs.TrySetException(new FormatException($"Unexpected venom tokens received: {string.Join(", ", tokens)}"));
                }
            });

            try {
                // Request glands status from MUD
                await Gateway.SendAsync("glands", ct).ConfigureAwait(false);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(5));

                using (cts.Token.Register(() => {
                    if (tcs.TrySetCanceled()) {
                        TriggerManager.DisableTrigger(VenomCheckTriggerName);
                    }
                })) {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            catch {
                TriggerManager.DisableTrigger(VenomCheckTriggerName);
                throw;
            }
        }
    }
}
