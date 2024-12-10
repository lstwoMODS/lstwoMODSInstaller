using System;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace lstwoMODSInstaller
{
    public static class PATManager
    {
        private const string RegistryPath = @"Software\lstwoMODSInstaller";
        private const string RegistryKeyName = "GitHubPAT";

        /// <summary>
        /// Ensures a PAT exists in the registry. If not, retrieves one using GitHub Device Flow and saves it.
        /// </summary>
        public static async Task EnsurePATExistsAsync()
        {
            string pat = GetPAT();
            if (string.IsNullOrEmpty(pat))
            {
                pat = await GitHubDeviceFlowHelper.GetPATAsync();
                SavePAT(pat);
                Console.WriteLine("New PAT saved successfully!");
            }

        }

        /// <summary>
        /// Saves the PAT to the registry.
        /// </summary>
        public static void SavePAT(string pat)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryPath, true);
                if (key == null)
                {
                    throw new Exception("Failed to create or open registry key.");
                }

                key.SetValue(RegistryKeyName, pat, RegistryValueKind.String);
                key.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save PAT to registry: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the PAT from the registry.
        /// </summary>
        /// <returns>The stored PAT, or null if not found.</returns>
        public static string GetPAT()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                if (key == null)
                {
                    return null;
                }

                string pat = key.GetValue(RegistryKeyName)?.ToString();
                key.Close();
                return pat;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve PAT from registry: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Deletes the stored PAT from the registry.
        /// </summary>
        public static void DeletePAT()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
                if (key != null && key.GetValue(RegistryKeyName) != null)
                {
                    key.DeleteValue(RegistryKeyName);
                    Console.WriteLine("PAT deleted from registry.");
                }

                key?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete PAT from registry: {ex.Message}");
            }
        }
    }
}
