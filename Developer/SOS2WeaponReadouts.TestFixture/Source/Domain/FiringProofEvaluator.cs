using System;

namespace SOS2WeaponReadouts.TestFixture.Domain
{
    public static class FiringProofEvaluator
    {
        private const float Tolerance = 0.01f;

        public static string ValidateSuccessfulFire(
            float heatBefore,
            float heatAfter,
            float powerBefore,
            float powerAfter,
            float expectedHeat,
            float expectedPower,
            int beginBurstCount,
            int castShotCount,
            int projectileLaunchCount)
        {
            if (beginBurstCount < 1)
            {
                return "BeginBurst was not observed.";
            }

            if (castShotCount < 1)
            {
                return "Verb_LaunchProjectileShip.TryCastShot was not observed.";
            }

            if (projectileLaunchCount < 1)
            {
                return "Projectile.Launch was not observed.";
            }

            if (!NearlyEqual(heatAfter - heatBefore, expectedHeat))
            {
                return "Heat delta did not match the weapon's HeatToFire.";
            }

            if (!NearlyEqual(powerBefore - powerAfter, expectedPower))
            {
                return "Power delta did not match the weapon's EnergyToFire.";
            }

            return string.Empty;
        }

        public static string ValidateSuppressedFire(
            float heatBefore,
            float heatAfter,
            float powerBefore,
            float powerAfter,
            int castShotCount)
        {
            if (castShotCount != 0)
            {
                return "A projectile cast occurred for a suppressed firing branch.";
            }

            if (heatAfter > heatBefore + Tolerance)
            {
                return "A suppressed firing branch added heat.";
            }

            if (powerBefore - powerAfter > 1f)
            {
                return "A suppressed firing branch drew weapon-scale power.";
            }

            return string.Empty;
        }

        private static bool NearlyEqual(float left, float right)
        {
            return Math.Abs(left - right) <= Tolerance;
        }
    }
}
