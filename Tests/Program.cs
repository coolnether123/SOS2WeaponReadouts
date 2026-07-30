using System;
using System.Collections.Generic;
using SOS2WeaponReadouts.Compatibility;
using SOS2WeaponReadouts.Domain;

namespace SOS2WeaponReadouts.Tests
{
    internal static class Program
    {
        private sealed class Module : ICompatibilityModule
        {
            private readonly IList<string> initialized;

            public Module(string id, IList<string> initialized)
            {
                Id = id;
                this.initialized = initialized;
            }

            public string Id { get; }

            public void Initialize()
            {
                initialized.Add(Id);
            }
        }

        private static int Main()
        {
            if ((int)FeatureState.Disabled != 0)
            {
                throw new InvalidOperationException(
                    "Disabled must remain the default feature state.");
            }

            var initialized = new List<string>();
            CompatibilityRegistry.Register(new Module("z-last", initialized));
            CompatibilityRegistry.Register(new Module("a-first", initialized));
            CompatibilityRegistry.InitializeAll();
            if (string.Join(",", initialized) != "a-first,z-last")
            {
                throw new InvalidOperationException(
                    "Compatibility initialization must be deterministic.");
            }

            try
            {
                CompatibilityRegistry.Register(
                    new Module("a-first", initialized));
                throw new InvalidOperationException(
                    "Duplicate compatibility IDs must be rejected.");
            }
            catch (InvalidOperationException exception)
            {
                if (!exception.Message.StartsWith(
                    "Duplicate compatibility module ID:",
                    StringComparison.Ordinal))
                {
                    throw;
                }
            }

            Console.WriteLine("PASS: generated pure contracts");
            return 0;
        }
    }
}
