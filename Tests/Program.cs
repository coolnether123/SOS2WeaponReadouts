using System;
using System.Linq;
using SOS2WeaponReadouts.Domain;
using SOS2WeaponReadouts.TestFixture.Domain;

namespace SOS2WeaponReadouts.Tests
{
    internal static class Program
    {
        private static readonly ReadoutLabels Labels =
            new ReadoutLabels
            {
                Section = "SOS2 weapon costs",
                HeatPerShot = "Heat generated per shot",
                ElectricalDrawPerShot =
                    "Electrical draw per shot",
                ValuesUnavailable = "Values unavailable",
                ThermalDisconnected =
                    "Not connected to a thermal network.",
                BridgeDisconnected =
                    "Thermal network has no ship bridge.",
                CurrentHeat = "Current network heat",
                HeatInsufficient =
                    "Insufficient thermal capacity for one shot.",
                ExistingElectricalLine =
                    "Energie zum Feuern: 80 Wd",
                ExistingThermalDisconnected =
                    "Nicht mit einem thermischen Netzwerk verbunden.",
                ExistingBridgeDisconnected =
                    "Keine Verbindung zur Schiffsbrücke.",
                HeatUnits = "HU",
                ElectricalUnits = "Wd"
            };

        private static readonly ReadoutPresentation FullPresentation =
            new ReadoutPresentation
            {
                ShowElectricalDraw = true,
                ShowNetworkComparison = true
            };

        private static int Main()
        {
            TestSos2ShotFormula();
            TestAmplifiedShotFormula();
            TestDefinitionReadout();
            TestExistingSos2FieldsAreNotDuplicated();
            TestSeparateNetworkAndEnergyLinesDoNotHideHeat();
            TestExistingSos2ConnectionIsNotDuplicated();
            TestLocalizedSos2FieldsAreNotDuplicated();
            TestDisconnectedWarnings();
            TestCapacityComparison();
            TestInsufficientCapacity();
            TestUnavailableSpinalValues();
            TestPresentationToggles();
            TestNumberFormatting();
            TestSuccessfulFiringProof();
            TestSuppressedFiringProof();
            TestFiringProofRejectsResourceMismatch();
            Console.WriteLine(
                "PASS: 16 SOS2 weapon readout contracts");
            return 0;
        }

        private static void TestSos2ShotFormula()
        {
            var readout = ShotCostCalculator.FromPlaced(
                10f,
                80f,
                0f,
                true,
                null);
            Equal(30f, readout.HeatPerShot, "base heat");
            Equal(80f, readout.ElectricalDrawPerShot, "base draw");
        }

        private static void TestAmplifiedShotFormula()
        {
            var readout = ShotCostCalculator.FromPlaced(
                10f,
                80f,
                0.5f,
                true,
                null);
            Equal(45f, readout.HeatPerShot, "amplified heat");
            Equal(120f, readout.ElectricalDrawPerShot, "amplified draw");
        }

        private static void TestDefinitionReadout()
        {
            var readout = ShotCostCalculator.FromDefinition(4f, 0f);
            var text = ReadoutFormatter.AppendMissing(
                "Weapon description.",
                readout,
                FullPresentation,
                Labels,
                true);
            Contains(text, "SOS2 weapon costs");
            Contains(text, "Heat generated per shot: 12 HU");
            Contains(text, "Electrical draw per shot: 0 Wd");
        }

        private static void TestExistingSos2FieldsAreNotDuplicated()
        {
            const string existing =
                "Energy to fire: 80 Wd\n" +
                "Grid heat stored/capacity: 30 HU / 100 HU\n" +
                "Heat generated per shot: 30 HU";
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(true, true, 100f, 30f));
            var lines = ReadoutFormatter.BuildMissingLines(
                existing,
                readout,
                FullPresentation,
                Labels);
            False(lines.Any(
                line => line.Contains("Electrical draw")),
                "current SOS2 energy line must suppress electrical duplicate");
            False(lines.Any(
                line => line.StartsWith("Heat generated")),
                "future SOS2 heat line must suppress heat duplicate");
            False(lines.Any(
                line => line.Contains("Current network heat")),
                "existing SOS2 grid heat must suppress a current-heat duplicate");
        }

        private static void TestSeparateNetworkAndEnergyLinesDoNotHideHeat()
        {
            const string existing =
                "Grid heat stored/capacity: 0 HU / 200 HU\n" +
                "Energy to fire: 80 Wd";
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(true, true, 200f, 0f));
            var lines = ReadoutFormatter.BuildMissingLines(
                existing,
                readout,
                FullPresentation,
                Labels);
            Contains(
                string.Join("\n", lines),
                "Heat generated per shot: 30 HU");
            False(lines.Any(
                line => line.Contains("Electrical draw")),
                "SOS2 energy line must still suppress electrical duplicate");
        }

        private static void TestDisconnectedWarnings()
        {
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(false, false, 0f, 0f));
            var lines = ReadoutFormatter.BuildMissingLines(
                string.Empty,
                readout,
                FullPresentation,
                Labels);
            Contains(
                string.Join("\n", lines),
                "Not connected to a thermal network.");
        }

        private static void TestExistingSos2ConnectionIsNotDuplicated()
        {
            const string existing =
                "Not connected to the ship bridge via heat network, " +
                "can't fire!";
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(true, false, 100f, 20f));
            var lines = ReadoutFormatter.BuildMissingLines(
                existing,
                readout,
                FullPresentation,
                Labels);
            False(lines.Any(
                line => line.Contains("no ship bridge")),
                "current SOS2 bridge warning must not be duplicated");
        }

        private static void TestLocalizedSos2FieldsAreNotDuplicated()
        {
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(false, false, 0f, 0f));
            var existing =
                Labels.ExistingElectricalLine + "\n" +
                Labels.ExistingThermalDisconnected;
            var lines = ReadoutFormatter.BuildMissingLines(
                existing,
                readout,
                FullPresentation,
                Labels);
            False(lines.Any(
                line => line.Contains("Electrical draw")),
                "localized SOS2 energy line must not be duplicated");
            False(lines.Any(
                line => line.Contains("thermal network")),
                "localized SOS2 network line must not be duplicated");
        }

        private static void TestCapacityComparison()
        {
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(true, true, 100f, 20f));
            var lines = ReadoutFormatter.BuildMissingLines(
                string.Empty,
                readout,
                FullPresentation,
                Labels);
            Contains(
                string.Join("\n", lines),
                "Current network heat: 20 / 100 HU");
        }

        private static void TestInsufficientCapacity()
        {
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(true, true, 100f, 80f));
            var lines = ReadoutFormatter.BuildMissingLines(
                string.Empty,
                readout,
                FullPresentation,
                Labels);
            Contains(
                string.Join("\n", lines),
                "Insufficient thermal capacity for one shot.");
            Contains(
                string.Join("\n", lines),
                "Current network heat: 80 / 100 HU");
        }

        private static void TestUnavailableSpinalValues()
        {
            var readout = new WeaponReadout(
                30f,
                80f,
                false,
                new NetworkReadout(true, true, 100f, 20f));
            var lines = ReadoutFormatter.BuildMissingLines(
                string.Empty,
                readout,
                FullPresentation,
                Labels);
            Equal(1, lines.Count, "unavailable line count");
            Equal(
                "Values unavailable",
                lines[0],
                "unavailable text");
        }

        private static void TestPresentationToggles()
        {
            var readout = new WeaponReadout(
                30f,
                80f,
                true,
                new NetworkReadout(true, true, 100f, 20f));
            var minimal = new ReadoutPresentation
            {
                ShowElectricalDraw = false,
                ShowNetworkComparison = false
            };
            var lines = ReadoutFormatter.BuildMissingLines(
                string.Empty,
                readout,
                minimal,
                Labels);
            Equal(1, lines.Count, "minimal line count");
            Contains(lines[0], "Heat generated per shot");
        }

        private static void TestNumberFormatting()
        {
            Equal("12", ReadoutFormatter.FormatNumber(12f), "integer");
            Equal(
                "12.35",
                ReadoutFormatter.FormatNumber(12.345f),
                "decimal");
        }

        private static void TestSuccessfulFiringProof()
        {
            Equal(
                string.Empty,
                FiringProofEvaluator.ValidateSuccessfulFire(
                    0f,
                    30f,
                    1000f,
                    920f,
                    30f,
                    80f,
                    1,
                    1,
                    1),
                "successful firing proof");
        }

        private static void TestSuppressedFiringProof()
        {
            Equal(
                string.Empty,
                FiringProofEvaluator.ValidateSuppressedFire(
                    30f,
                    30f,
                    79f,
                    79f,
                    0),
                "suppressed firing proof");
        }

        private static void TestFiringProofRejectsResourceMismatch()
        {
            Contains(
                FiringProofEvaluator.ValidateSuccessfulFire(
                    0f,
                    29f,
                    1000f,
                    920f,
                    30f,
                    80f,
                    1,
                    1,
                    1),
                "Heat delta");
        }

        private static void Equal<T>(
            T expected,
            T actual,
            string label)
        {
            if (!Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    label + ": expected " + expected +
                    ", got " + actual);
            }
        }

        private static void Contains(string text, string expected)
        {
            if (text == null ||
                text.IndexOf(
                    expected,
                    StringComparison.Ordinal) < 0)
            {
                throw new InvalidOperationException(
                    "Expected text not found: " + expected +
                    "\nActual: " + text);
            }
        }

        private static void False(bool value, string message)
        {
            if (value)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
