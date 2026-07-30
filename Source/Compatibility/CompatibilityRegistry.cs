using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS2WeaponReadouts.Compatibility
{
    public interface ICompatibilityModule
    {
        string Id { get; }

        void Initialize();
    }

    public static class CompatibilityRegistry
    {
        private static readonly List<ICompatibilityModule> Modules =
            new List<ICompatibilityModule>();

        public static IReadOnlyList<ICompatibilityModule> RegisteredModules
        {
            get { return Modules.AsReadOnly(); }
        }

        public static void Register(ICompatibilityModule module)
        {
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }
            if (string.IsNullOrWhiteSpace(module.Id))
            {
                throw new ArgumentException(
                    "Compatibility module ID is required.",
                    nameof(module));
            }
            if (Modules.Any(existing =>
                string.Equals(
                    existing.Id,
                    module.Id,
                    StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Duplicate compatibility module ID: " + module.Id);
            }
            Modules.Add(module);
        }

        public static void InitializeAll()
        {
            foreach (var module in Modules.OrderBy(
                item => item.Id,
                StringComparer.Ordinal))
            {
                module.Initialize();
            }
        }
    }
}
