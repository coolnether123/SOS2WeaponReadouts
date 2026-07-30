namespace SOS2WeaponReadouts.Domain
{
    public sealed class ReadoutPresentation
    {
        public bool ShowElectricalDraw { get; set; } = true;

        public bool ShowNetworkComparison { get; set; } = true;
    }

    public sealed class ReadoutLabels
    {
        public string Section { get; set; }

        public string HeatPerShot { get; set; }

        public string ElectricalDrawPerShot { get; set; }

        public string ValuesUnavailable { get; set; }

        public string ThermalDisconnected { get; set; }

        public string BridgeDisconnected { get; set; }

        public string HeatAfterShot { get; set; }

        public string HeatInsufficient { get; set; }

        public string ExistingElectricalLine { get; set; }

        public string ExistingThermalDisconnected { get; set; }

        public string ExistingBridgeDisconnected { get; set; }

        public string HeatUnits { get; set; } = "HU";

        public string ElectricalUnits { get; set; } = "Wd";
    }
}
