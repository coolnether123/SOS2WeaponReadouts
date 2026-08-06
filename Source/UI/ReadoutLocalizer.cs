using SOS2WeaponReadouts.Domain;
using Verse;

namespace SOS2WeaponReadouts.UI
{
    /// <summary>
    /// Resolves RimWorld translations at the UI boundary so domain formatting
    /// stays deterministic and testable.
    /// </summary>
    public static class ReadoutLocalizer
    {
        public static ReadoutLabels CreateLabels(
            WeaponReadout readout)
        {
            return new ReadoutLabels
            {
                Section = "SOS2WR.Readout.Section".Translate(),
                HeatPerShot =
                    "SOS2WR.Readout.HeatPerShot".Translate(),
                HeatPerShotCompact =
                    "SOS2WR.Readout.HeatPerShotCompact".Translate(
                        ReadoutFormatter.FormatNumber(
                            readout.HeatPerShot)),
                ElectricalDrawPerShot =
                    "SOS2WR.Readout.ElectricalDraw".Translate(),
                ValuesUnavailable =
                    "SOS2WR.Readout.ValuesUnavailable".Translate(),
                ThermalDisconnected =
                    "SOS2WR.Warning.ThermalDisconnected".Translate(),
                BridgeDisconnected =
                    "SOS2WR.Warning.BridgeDisconnected".Translate(),
                CurrentHeat =
                    "SOS2WR.Readout.CurrentHeat".Translate(),
                HeatInsufficient =
                    "SOS2WR.Warning.HeatInsufficient".Translate(),
                ExistingElectricalLine =
                    "SoS.HeatTurretEnergy".Translate(
                        readout.ElectricalDrawPerShot
                            .ToStringDecimalIfSmall()),
                ExistingThermalDisconnected =
                    "SoS.HeatNotConnected".Translate(),
                ExistingBridgeDisconnected =
                    "SoS.TurretNotConnected".Translate(),
                HeatUnits = "HU",
                ElectricalUnits = "Wd"
            };
        }
    }
}
