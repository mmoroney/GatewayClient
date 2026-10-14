namespace GatewayClient {
    public readonly record struct ApothecaryWeight(int Ounces, int Drams, int Scruples, int Grains) {
        // 1 oz = 8 dr = 24 sc = 480 gr | 1 dr = 3 sc = 60 gr | 1 sc = 20 gr
        public int TotalGrains => (Ounces * 480) + (Drams * 60) + (Scruples * 20) + Grains;

        public override string ToString() =>
            $"{Ounces} oz, {Drams} dr, {Scruples} sc, {Grains} gr ({TotalGrains} grains)";
    }
}
