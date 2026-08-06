namespace SOS2WeaponReadouts.Compatibility
{
    /// <summary>
    /// Categorizes whether readout integration can run or why it was disabled.
    /// </summary>
    public enum CompatibilityState
    {
        Supported,
        MissingDependency,
        UnsupportedApi,
        InitializationFailed
    }

    /// <summary>
    /// Carries a compatibility decision and its player-facing explanation as
    /// one result.
    /// </summary>
    public sealed class CompatibilityStatus
    {
        public CompatibilityStatus(
            CompatibilityState state,
            string detail)
        {
            State = state;
            Detail = detail ?? string.Empty;
        }

        public CompatibilityState State { get; }

        public string Detail { get; }

        public bool IsSupported =>
            State == CompatibilityState.Supported;
    }
}
