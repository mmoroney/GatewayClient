using System;
using System.Threading;
using System.Threading.Tasks;

namespace GatewayClient {
    public static class CharacterManager {
        public const string HealthCheckTriggerName = "Character.HealthCheck";

        // Regex matches "[HP: 716/(716)  SP: 290/(290)]" with flexible whitespace
        private const string HealthCheckRegex =
            @"\[HP:\s*(\d+)/\((\d+)\)\s+SP:\s*(\d+)/\((\d+)\)\]";

        static CharacterManager() {
            TriggerManager.RegisterTrigger(new Trigger(HealthCheckTriggerName, HealthCheckRegex));
        }

        public static async Task<CharacterHealth> CheckHealthAsync(TimeSpan? timeout = null, CancellationToken ct = default) {
            var tcs = new TaskCompletionSource<CharacterHealth>(TaskCreationOptions.RunContinuationsAsynchronously);

            TriggerManager.EnableTrigger(HealthCheckTriggerName, isOneShot: true, tokens => {
                if (tokens.Length >= 4 &&
                    int.TryParse(tokens[0], out int curHp) &&
                    int.TryParse(tokens[1], out int maxHp) &&
                    int.TryParse(tokens[2], out int curSp) &&
                    int.TryParse(tokens[3], out int maxSp)) {
                    tcs.TrySetResult(new CharacterHealth(curHp, maxHp, curSp, maxSp));
                }
                else {
                    tcs.TrySetException(new FormatException($"Unexpected health tokens: {string.Join(", ", tokens)}"));
                }
            });

            try {
                // Send "sc" command to the MUD
                await Gateway.SendAsync("sc", ct).ConfigureAwait(false);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(timeout ?? TimeSpan.FromSeconds(5));

                using (cts.Token.Register(() => {
                    if (tcs.TrySetCanceled()) {
                        TriggerManager.DisableTrigger(HealthCheckTriggerName);
                    }
                })) {
                    return await tcs.Task.ConfigureAwait(false);
                }
            }
            catch {
                TriggerManager.DisableTrigger(HealthCheckTriggerName);
                throw;
            }
        }
    }
}
