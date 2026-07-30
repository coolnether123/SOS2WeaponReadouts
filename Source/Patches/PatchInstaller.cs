using System;
using System.Collections.Generic;
using System.Linq;

namespace SOS2WeaponReadouts.Patches
{
    public interface IPatchModule
    {
        string Id { get; }

        void Install();
    }

    public static class PatchInstaller
    {
        private static readonly List<IPatchModule> Modules =
            new List<IPatchModule>();
        private static bool installed;

        public static void Register(IPatchModule module)
        {
            if (installed)
            {
                throw new InvalidOperationException(
                    "Patch modules cannot be registered after installation.");
            }
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }
            if (string.IsNullOrWhiteSpace(module.Id))
            {
                throw new ArgumentException(
                    "Patch module ID is required.",
                    nameof(module));
            }
            if (Modules.Any(existing =>
                string.Equals(
                    existing.Id,
                    module.Id,
                    StringComparison.Ordinal)))
            {
                throw new InvalidOperationException(
                    "Duplicate patch module ID: " + module.Id);
            }
            Modules.Add(module);
        }

        public static void InstallAll()
        {
            if (installed)
            {
                return;
            }
            foreach (var module in Modules.OrderBy(
                item => item.Id,
                StringComparer.Ordinal))
            {
                module.Install();
            }
            installed = true;
        }
    }
}
