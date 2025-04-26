using Microsoft.Win32;
using GMap.NET.MapProviders;
using nl.siwoc.RouteManager.ui;
using GMap.NET;

namespace nl.siwoc.RouteManager
{
    public static class Settings
    {
        private const string RegistryPath = @"Software\RouteManager";
        private const string MapProviderKey = "MapProvider";
        private const string StartLatitudeKey = "StartLatitude";
        private const string StartLongitudeKey = "StartLongitude";
        private const string RoadSnapDistanceKey = "RoadSnapDistance";
        private const string GoogleApiKeyKey = "GoogleApiKey";
        private const string RoutingProviderKey = "RoutingProvider";
        private const int DefaultRoadSnapDistance = 500; // meters
        private static readonly GMapProvider DefaultMapProvider = OpenStreetMapProvider.Instance;
        private static readonly RoutingProvider DefaultRoutingProvider = OpenStreetMapProvider.Instance as RoutingProvider;

        public static string LoadRoutingProviderName()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key?.GetValue(RoutingProviderKey) is string providerName)
                    {
                        System.Diagnostics.Debug.WriteLine($"Loaded routing provider name from registry: {providerName}");
                        return providerName;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load routing provider name: {ex.Message}");
            }
            return "OpenStreetMap";
        }

        public static RoutingProvider LoadRoutingProvider()
        {
            var providerName = LoadRoutingProviderName();
            if (providerName == "GoogleMapProvider")
            {
                var apiKey = LoadGoogleApiKey();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var provider = GoogleMapProvider.Instance as RoutingProvider;
                    if (provider != null)
                    {
                        return provider;
                    }
                }
            }
            return DefaultRoutingProvider;
        }

        public static void SaveRoutingProvider(string providerName)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key?.SetValue(RoutingProviderKey, providerName);
                    System.Diagnostics.Debug.WriteLine($"Saved routing provider to registry: {providerName}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save routing provider: {ex.Message}");
            }
        }

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
            return DefaultMapProvider;
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

        public static void SaveGoogleApiKey(string apiKey)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(GoogleApiKeyKey, apiKey);
            
            // Set the API key for all Google providers
            GoogleMapProvider.Instance.ApiKey = apiKey;
            GoogleSatelliteMapProvider.Instance.ApiKey = apiKey;
            GoogleHybridMapProvider.Instance.ApiKey = apiKey;
            GoogleTerrainMapProvider.Instance.ApiKey = apiKey;
        }

        public static string LoadGoogleApiKey()
        {
            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var apiKey = key?.GetValue(GoogleApiKeyKey)?.ToString() ?? string.Empty;
            
            // Set the API key for all Google providers
            GoogleMapProvider.Instance.ApiKey = apiKey;
            GoogleSatelliteMapProvider.Instance.ApiKey = apiKey;
            GoogleHybridMapProvider.Instance.ApiKey = apiKey;
            GoogleTerrainMapProvider.Instance.ApiKey = apiKey;
            
            return apiKey;
        }
    }
} 