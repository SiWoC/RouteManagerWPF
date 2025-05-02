using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nl.siwoc.RouteManager
{
    public class Utils
    {
        public static string ConvertRouteDuration(double seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            if (hours > 0)
            {
                return $"{hours}h {minutes}m";
            }
            else
            {
                return $"{minutes}m";
            }
        }


    }
}
