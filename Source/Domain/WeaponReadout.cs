using System;

namespace SOS2WeaponReadouts.Domain
{
    /// <summary>
    /// Carries normalized per-shot costs and optional live network state across
    /// compatibility and UI boundaries.
    /// </summary>
    public sealed class WeaponReadout
    {
        public WeaponReadout(
            float heatPerShot,
            float electricalDrawPerShot,
            bool dynamicValuesAvailable,
            NetworkReadout network)
        {
            HeatPerShot = Math.Max(0f, heatPerShot);
            ElectricalDrawPerShot = Math.Max(0f, electricalDrawPerShot);
            DynamicValuesAvailable = dynamicValuesAvailable;
            Network = network;
        }

        public float HeatPerShot { get; }

        public float ElectricalDrawPerShot { get; }

        public bool DynamicValuesAvailable { get; }

        public NetworkReadout Network { get; }
    }

    /// <summary>
    /// Represents the network facts needed for useful placement and capacity
    /// warnings without exposing SOS2 objects.
    /// </summary>
    public sealed class NetworkReadout
    {
        public NetworkReadout(
            bool thermalNetworkConnected,
            bool bridgeConnected,
            float capacity,
            float used)
        {
            ThermalNetworkConnected = thermalNetworkConnected;
            BridgeConnected = bridgeConnected;
            Capacity = Math.Max(0f, capacity);
            Used = Math.Max(0f, used);
        }

        public bool ThermalNetworkConnected { get; }

        public bool BridgeConnected { get; }

        public float Capacity { get; }

        public float Used { get; }

        public float Available => Math.Max(0f, Capacity - Used);
    }
}
