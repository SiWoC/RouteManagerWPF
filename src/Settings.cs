using System;
using Microsoft.Win32;
using GMap.NET.MapProviders;

namespace nl.siwoc.RouteManager
{
    public static class Settings
    {
        private const string RegistryPath = @"Software\RouteManager";
        private const string MapProviderKey = "MapProvider";
        private const string StartLatitudeKey = "StartLatitude";
        private const string StartLongitudeKey = "StartLongitude";
        private const string RoadSnapDistanceKey = "RoadSnapDistance";
        private const int DefaultRoadSnapDistance = 500; // meters

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

        public static void SaveStartLatitude(double latitude)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(StartLatitudeKey, latitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public static void SaveStartLongitude(double longitude)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(StartLongitudeKey, longitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public static double LoadStartLatitude()
        {
            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var value = key?.GetValue(StartLatitudeKey)?.ToString();
            return value != null && double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result) 
                ? result 
                : 46.538615; // Default latitude
        }

        public static double LoadStartLongitude()
        {
            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var value = key?.GetValue(StartLongitudeKey)?.ToString();
            return value != null && double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double result) 
                ? result 
                : 10.501385; // Default longitude
        }

        public static int LoadRoadSnapDistance()
        {
            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var value = key?.GetValue(RoadSnapDistanceKey)?.ToString();
            return value != null && int.TryParse(value, out int result) 
                ? result 
                : DefaultRoadSnapDistance;
        }

        public static void SaveRoadSnapDistance(int distance)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(RoadSnapDistanceKey, distance.ToString());
        }
    }
} 