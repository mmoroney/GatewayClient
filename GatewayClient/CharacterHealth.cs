namespace GatewayClient {
    public readonly record struct CharacterHealth(int CurrentHp, int MaxHp, int CurrentSp, int MaxSp) {
        public double HpPercent => MaxHp > 0 ? (double)CurrentHp / MaxHp * 100 : 0;
        public double SpPercent => MaxSp > 0 ? (double)CurrentSp / MaxSp * 100 : 0;

        public override string ToString() =>
            $"HP: {CurrentHp}/{MaxHp} ({HpPercent:F0}%), SP: {CurrentSp}/{MaxSp} ({SpPercent:F0}%)";
    }
}
