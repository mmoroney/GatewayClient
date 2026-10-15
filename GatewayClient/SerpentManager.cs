using System;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayClient {
    public static class SerpentManager {
        public const string VenomCheckTriggerName = "Serpent.VenomCheck";

        public const string HealSuccessTriggerName = "Serpent.HealSuccess";
        public const string HealFailedTriggerName = "Serpent.HealFailed";

        // Regex matches "Current quantity:        4 ounces, 1 drams, 0 scruples, and 15 grains"
        private const string VenomCheckRegex =
            @"Current quantity:\s+(\d+)\s+ounces?,\s+(\d+)\s+drams?,\s+(\d+)\s+scruples?,\s+and\s+(\d+)\s+grains?";

        private const string HealSuccessRegex = @"You are at full strength\.";
        private const string HealFailedRegex = @"You have insufficient levels of distillate\.";

        static SerpentManager() {
            TriggerManager.RegisterTrigger(new Trigger(VenomCheckTriggerName, VenomCheckRegex));
            TriggerManager.RegisterTrigger(new Trigger(HealSuccessTriggerName, HealSuccessRegex));
            TriggerManager.RegisterTrigger(new Trigger(HealFailedTriggerName, HealFailedRegex));
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

        public static async Task<bool> SerpentHealAsync(TimeSpan? timeout = null, CancellationToken ct = default) {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void Cleanup() {
                TriggerManager.DisableTrigger(HealSuccessTriggerName);
                TriggerManager.DisableTrigger(HealFailedTriggerName);
            }

            TriggerManager.EnableTrigger(HealSuccessTriggerName, isOneShot: true, _ => {
                Cleanup();
                tcs.TrySetResult(true);
            });

            TriggerManager.EnableTrigger(HealFailedTriggerName, isOneShot: true, _ => {
                Cleanup();
                tcs.TrySetResult(false);
            });

            try {
                // Send command to initiate toxopiate secretion
                await Gateway.SendAsync("secrete toxopiate", ct).ConfigureAwait(false);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                // Healing takes time; default to 30 seconds
                cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(30));

                using (cts.Token.Register(() => {
                    if (tcs.TrySetCanceled()) {
                        Cleanup();
                    }
                })) {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            catch {
                Cleanup();
                throw;
            }
        }
    }
}
