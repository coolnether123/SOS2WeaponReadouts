namespace SOS2WeaponReadouts.Domain
{
    /// <summary>
    /// Supplies presentation choices to the pure formatter without coupling it
    /// to persisted mod settings.
    /// </summary>
    public sealed class ReadoutPresentation
    {
        public bool ShowElectricalDraw { get; set; } = true;

        public bool ShowNetworkComparison { get; set; } = true;
    }

    /// <summary>
    /// Supplies localized wording and units while keeping formatting logic
    /// independent of RimWorld translation APIs.
    /// </summary>
    public sealed class ReadoutLabels
    {
        public string Section { get; set; }

        public string HeatPerShot { get; set; }

        public string HeatPerShotCompact { get; set; }

        public string ElectricalDrawPerShot { get; set; }

        public string ValuesUnavailable { get; set; }

        public string ThermalDisconnected { get; set; }

        public string BridgeDisconnected { get; set; }

        public string CurrentHeat { get; set; }

        public string HeatInsufficient { get; set; }

        public string ExistingElectricalLine { get; set; }

        public string ExistingThermalDisconnected { get; set; }

        public string ExistingBridgeDisconnected { get; set; }

        public string HeatUnits { get; set; } = "HU";

        public string ElectricalUnits { get; set; } = "Wd";
    }
}
