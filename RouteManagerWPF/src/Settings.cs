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
        private static readonly string DefaultRoutingProviderName = OpenStreetMapProviderName;

        // Cache for settings
        private static string cachedRoutingProviderName;
        private static GMapProvider cachedMapProvider;
        private static double? cachedStartLatitude;
        private static double? cachedStartLongitude;
        private static int? cachedRoadSnapDistance;
        private static string cachedGoogleApiKey;
        private static RoutingProvider cachedRoutingProvider;

        public const string GoogleMapProviderName = "GoogleMapProvider";
        public const string OpenStreetMapProviderName = "OpenStreetMapProvider";

        /// <summary>
        /// Gets a curated list of map providers
        /// </summary>
        /// <returns>List of selected GMapProviders</returns>
        public static List<GMapProvider> GetAllMapProviders()
        {
            return new List<GMapProvider>
            {
                OpenStreetMapProvider.Instance,
                BingMapProvider.Instance,
                BingSatelliteMapProvider.Instance,
                BingHybridMapProvider.Instance,
                BingOSMapProvider.Instance,
                GoogleMapProvider.Instance,
                GoogleSatelliteMapProvider.Instance,
                GoogleHybridMapProvider.Instance,
                GoogleTerrainMapProvider.Instance
            };
        }

        public static string LoadRoutingProviderName()
        {
            if (cachedRoutingProviderName != null)
                return cachedRoutingProviderName;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key?.GetValue(RoutingProviderKey) is string providerName)
                    {
                        System.Diagnostics.Debug.WriteLine($"Loaded routing provider name from registry: {providerName}");
                        cachedRoutingProviderName = providerName;
                        return providerName;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load routing provider name: {ex.Message}");
            }
            cachedRoutingProviderName = DefaultRoutingProviderName;
            return cachedRoutingProviderName;
        }

        public static RoutingProvider LoadRoutingProvider()
        {
            if (cachedRoutingProvider != null)
                return cachedRoutingProvider;

            var routingProviderName = LoadRoutingProviderName();
            if (routingProviderName.Equals(GoogleMapProviderName))
            {
                var apiKey = LoadGoogleApiKey();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    var provider = GoogleMapProvider.Instance as RoutingProvider;
                    if (provider != null)
                    {
                        cachedRoutingProvider = provider;
                        return provider;
                    }
                }
            }
            cachedRoutingProvider = DefaultRoutingProvider;
            return DefaultRoutingProvider;
        }

        public static void SaveRoutingProvider(string routingProviderName)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(RegistryPath))
                {
                    key?.SetValue(RoutingProviderKey, routingProviderName);
                    System.Diagnostics.Debug.WriteLine($"Saved routing provider to registry: {routingProviderName}");
                    cachedRoutingProviderName = routingProviderName;
                    cachedRoutingProvider = null; // Invalidate cache
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
                    cachedMapProvider = provider;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save map provider: {ex.Message}");
            }
        }

        public static GMapProvider LoadMapProvider()
        {
            if (cachedMapProvider != null)
                return cachedMapProvider;

            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(RegistryPath))
                {
                    if (key?.GetValue(MapProviderKey) is string providerName)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found saved provider in registry: {providerName}");
                        var provider = GetAllMapProviders().FirstOrDefault(p => p.Name == providerName);
                        if (provider != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"Loaded provider from registry: {provider.Name}");
                            cachedMapProvider = provider;
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
            cachedMapProvider = DefaultMapProvider;
            return DefaultMapProvider;
        }

        public static void SaveStartLatitude(double latitude)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(StartLatitudeKey, latitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
            cachedStartLatitude = latitude;
        }

        public static void SaveStartLongitude(double longitude)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(StartLongitudeKey, longitude.ToString(System.Globalization.CultureInfo.InvariantCulture));
            cachedStartLongitude = longitude;
        }

        public static double LoadStartLatitude()
        {
            if (cachedStartLatitude.HasValue)
                return cachedStartLatitude.Value;

            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var value = key?.GetValue(StartLatitudeKey)?.ToString();
            var result = value != null && double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsed) 
                ? parsed 
                : 46.538615; // Default latitude
            cachedStartLatitude = result;
            return result;
        }

        public static double LoadStartLongitude()
        {
            if (cachedStartLongitude.HasValue)
                return cachedStartLongitude.Value;

            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var value = key?.GetValue(StartLongitudeKey)?.ToString();
            var result = value != null && double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsed) 
                ? parsed 
                : 10.501385; // Default longitude
            cachedStartLongitude = result;
            return result;
        }

        public static int LoadRoadSnapDistance()
        {
            if (cachedRoadSnapDistance.HasValue)
                return cachedRoadSnapDistance.Value;

            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var value = key?.GetValue(RoadSnapDistanceKey)?.ToString();
            var result = value != null && int.TryParse(value, out int parsed) 
                ? parsed 
                : DefaultRoadSnapDistance;
            cachedRoadSnapDistance = result;
            return result;
        }

        public static void SaveRoadSnapDistance(int distance)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(RoadSnapDistanceKey, distance.ToString());
            cachedRoadSnapDistance = distance;
        }

        public static void SaveGoogleApiKey(string apiKey)
        {
            var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key.SetValue(GoogleApiKeyKey, apiKey);
            cachedGoogleApiKey = apiKey;
            
            // Set the API key for all Google providers
            GoogleMapProvider.Instance.ApiKey = apiKey;
            GoogleSatelliteMapProvider.Instance.ApiKey = apiKey;
            GoogleHybridMapProvider.Instance.ApiKey = apiKey;
            GoogleTerrainMapProvider.Instance.ApiKey = apiKey;
        }

        public static string LoadGoogleApiKey()
        {
            if (cachedGoogleApiKey != null)
                return cachedGoogleApiKey;

            var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            var apiKey = key?.GetValue(GoogleApiKeyKey)?.ToString() ?? string.Empty;
            cachedGoogleApiKey = apiKey;
            
            // Set the API key for all Google providers
            GoogleMapProvider.Instance.ApiKey = apiKey;
            GoogleSatelliteMapProvider.Instance.ApiKey = apiKey;
            GoogleHybridMapProvider.Instance.ApiKey = apiKey;
            GoogleTerrainMapProvider.Instance.ApiKey = apiKey;
            
            return apiKey;
        }
    }
} 