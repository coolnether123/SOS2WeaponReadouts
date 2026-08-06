using System;

namespace SOS2WeaponReadouts.Domain
{
    /// <summary>
    /// Centralizes SOS2 firing-cost rules so definition and placed readouts
    /// cannot drift apart.
    /// </summary>
    public static class ShotCostCalculator
    {
        public const float Sos2HeatPulseMultiplier = 3f;

        public static WeaponReadout FromDefinition(
            float heatPerPulse,
            float energyToFire)
        {
            return FromPlaced(
                heatPerPulse,
                energyToFire,
                0f,
                true,
                null);
        }

        public static WeaponReadout FromPlaced(
            float heatPerPulse,
            float energyToFire,
            float amplifierDamageBonus,
            bool dynamicValuesAvailable,
            NetworkReadout network)
        {
            var amplifierMultiplier = Math.Max(
                0f,
                1f + amplifierDamageBonus);
            return new WeaponReadout(
                heatPerPulse * Sos2HeatPulseMultiplier *
                    amplifierMultiplier,
                energyToFire * amplifierMultiplier,
                dynamicValuesAvailable,
                network);
        }
    }
}
