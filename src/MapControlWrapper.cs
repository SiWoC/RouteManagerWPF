using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System;

namespace nl.siwoc.RouteManager
{
    public class MapControlWrapper : GMapControl
    {
        public MapControlWrapper()
        {
            // Configure GMap.NET
            GMapProvider.WebProxy = null;
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            // Initialize map settings
            ShowTileGridLines = false;
            MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
            MinZoom = 0;
            MaxZoom = 20;

            System.Diagnostics.Debug.WriteLine($"MapControlWrapper initialized with provider: {base.MapProvider?.Name ?? "null"}");
        }

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
    }
} 