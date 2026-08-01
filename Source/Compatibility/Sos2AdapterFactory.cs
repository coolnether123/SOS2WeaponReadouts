using System;
using System.Linq;
using Verse;

namespace SOS2WeaponReadouts.Compatibility
{
    public static class Sos2AdapterFactory
    {
        public const string PackageId = "kentington.saveourship2";

        public static ISos2WeaponAdapter Create()
        {
            if (!ModsConfig.IsActive(PackageId))
            {
                return new UnavailableSos2Adapter(
                    new CompatibilityStatus(
                        CompatibilityState.MissingDependency,
                        "Save Our Ship 2 is not active."));
            }

            try
            {
                return new Sos2V16Adapter();
            }
            catch (MissingMemberException exception)
            {
                return new UnavailableSos2Adapter(
                    new CompatibilityStatus(
                        CompatibilityState.UnsupportedApi,
                        "The installed SOS2 API is not compatible: " +
                        exception.Message));
            }
            catch (TypeLoadException exception)
            {
                return new UnavailableSos2Adapter(
                    new CompatibilityStatus(
                        CompatibilityState.UnsupportedApi,
                        "The installed SOS2 types could not be loaded: " +
                        exception.Message));
            }
            catch (Exception exception)
            {
                return new UnavailableSos2Adapter(
                    new CompatibilityStatus(
                        CompatibilityState.InitializationFailed,
                        "SOS2 adapter initialization failed: " +
                        exception.GetType().Name + ": " +
                        exception.Message));
            }
        }

        internal static Type FindRequiredType(string fullName)
        {
            var type = FindOptionalType(fullName);
            if (type == null)
            {
                throw new TypeLoadException(fullName);
            }

            return type;
        }

        internal static Type FindOptionalType(string fullName)
        {
            return AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(assembly => assembly.GetType(
                    fullName,
                    false,
                    false))
                .FirstOrDefault(candidate => candidate != null);
        }
    }
}
