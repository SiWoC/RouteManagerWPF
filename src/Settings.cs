using System;
using Microsoft.Win32;
using GMap.NET.MapProviders;

namespace nl.siwoc.RouteManager
{
    public static class Settings
    {
        private const string RegistryPath = @"Software\RouteManager";
        private const string MapProviderKey = "SelectedMapProvider";

        public static void SaveMapProvider(GMapProvider provider)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    var providerName = provider?.Name ?? string.Empty;
                    key?.SetValue(MapProviderKey, providerName);
                    System.Diagnostics.Debug.WriteLine($"Saved provider to registry: {providerName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save map provider: {ex.Message}");
            }
        }

        public static GMapProvider LoadMapProvider()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key?.GetValue(MapProviderKey) is string providerName)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found saved provider in registry: {providerName}");
                        var provider = MapControlWrapper.GetAllMapProviders().FirstOrDefault(p => p.Name == providerName);
                        if (provider != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Loaded provider from registry: {provider.Name}");
                            return provider;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Provider {providerName} not found in available providers");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No saved provider found in registry");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load map provider: {ex.Message}");
            }
            return null;
        }
    }
} 