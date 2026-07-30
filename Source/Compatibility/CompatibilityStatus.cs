namespace SOS2WeaponReadouts.Compatibility
{
    public enum CompatibilityState
    {
        Supported,
        MissingDependency,
        UnsupportedApi,
        InitializationFailed
    }

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
