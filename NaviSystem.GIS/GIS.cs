using ProjNet.CoordinateSystems.Transformations;
using ProjNet.CoordinateSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NaviSystem.GIS
{
    public interface IGIS
    {
        /// <summary>
        /// Geographic to UTM convertation
        /// </summary>
        /// <param name="g_point">X - latitude, Y - longitude</param>
        /// <returns></returns>
        Point GeoToUTM(Point g_point);

        /// <summary>
        /// UTM to Geographic convertation
        /// </summary>
        /// <param name="utm_point"></param>
        /// <param name="isNorth"></param>
        /// <returns>X - latitude, Y - longitude</returns>
        Point UTMToGeo(Point utm_point, bool isNorth);
    }
    public class GIS : IGIS
    {
        /// <summary>
        /// Geographic to UTM convertation
        /// </summary>
        /// <param name="g_point">X - latitude, Y - longitude</param>
        /// <returns></returns>
        public Point GeoToUTM(Point g_point)
        {
            var data = ConvertToUtm(g_point.X, g_point.Y);
            return new Point(data[0], data[1]);
        }

        public Point UTMToGeo(Point utm_point, bool isNorth)
        {
            var data = UTM_to_GEO(utm_point.X, utm_point.Y, isNorth);
            return new Point(data[0], data[1]);
        }

        //public double lat; //Широта
        //public double lon; //Долгота
        private static int zone = 200;
        private static double[] ConvertToUtm(double latitude, double longitude)
        {
            //lat = latitude;
            //lon = longitude;
            if (latitude < -80 || latitude > 84)
                return null;
            zone = GetZone(latitude, longitude);
            string band = GetBand(latitude);
            //Transform to UTM
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            ICoordinateSystem wgs84geo = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
            ICoordinateSystem utm = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WGS84_UTM(zone, latitude > 0);
            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84geo, utm);
            double[] pUtm = trans.MathTransform.Transform(new double[] { longitude, latitude });
            double[] date = { pUtm[0], pUtm[1] };
            return date;
        }
        public static double[] GeocentricToGEO(double X, double Y, double Z)
        {
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            ICoordinateSystem wgs84geo = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
            ICoordinateSystem wgs84geocentric = ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84;
            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84geocentric, wgs84geo);
            double[] geocentric = trans.MathTransform.Transform(new double[] { X, Y, Z });
            double[] date = { geocentric[0], geocentric[1] };
            return date;
        }
        public static double[] GEOToGeocentric(double latitude, double longitude)
        {
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            ICoordinateSystem wgs84geo = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
            ICoordinateSystem wgs84geocentric = ProjNet.CoordinateSystems.GeocentricCoordinateSystem.WGS84;
            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(wgs84geo, wgs84geocentric);
            double[] geocentric = trans.MathTransform.Transform(new double[] { longitude, latitude });
            //double[] date = { geocentric[0], geocentric[1] };
            return geocentric;
        }
        private static string GetBand(double latitude)
        {
            if (latitude <= 84 && latitude >= 72)
                return "X";
            else if (latitude < 72 && latitude >= 64)
                return "W";
            else if (latitude < 64 && latitude >= 56)
                return "V";
            else if (latitude < 56 && latitude >= 48)
                return "U";
            else if (latitude < 48 && latitude >= 40)
                return "T";
            else if (latitude < 40 && latitude >= 32)
                return "S";
            else if (latitude < 32 && latitude >= 24)
                return "R";
            else if (latitude < 24 && latitude >= 16)
                return "Q";
            else if (latitude < 16 && latitude >= 8)
                return "P";
            else if (latitude < 8 && latitude >= 0)
                return "N";
            else if (latitude < 0 && latitude >= -8)
                return "M";
            else if (latitude < -8 && latitude >= -16)
                return "L";
            else if (latitude < -16 && latitude >= -24)
                return "K";
            else if (latitude < -24 && latitude >= -32)
                return "J";
            else if (latitude < -32 && latitude >= -40)
                return "H";
            else if (latitude < -40 && latitude >= -48)
                return "G";
            else if (latitude < -48 && latitude >= -56)
                return "F";
            else if (latitude < -56 && latitude >= -64)
                return "E";
            else if (latitude < -64 && latitude >= -72)
                return "D";
            else if (latitude < -72 && latitude >= -80)
                return "C";
            else
                return null;
        }
        private static int GetZone(double latitude, double longitude)
        {
            // Norway
            if (latitude >= 56 && latitude < 64 && longitude >= 3 && longitude < 13)
                return 32;

            // Spitsbergen
            if (latitude >= 72 && latitude < 84)
            {
                if (longitude >= 0 && longitude < 9)
                    return 31;
                else if (longitude >= 9 && longitude < 21)
                    return 33;
                if (longitude >= 21 && longitude < 33)
                    return 35;
                if (longitude >= 33 && longitude < 42)
                    return 37;
            }

            return (int)Math.Ceiling((longitude + 180) / 6);
        }
        public static double[] GEO_to_UTM(double latitude, double longitude)
        {
            // output double[x, y] [0] - long, [1] - lat
            return ConvertToUtm(latitude, longitude);
        }
        public static double[] UTM_to_GEO(double easting, double northing, bool isNorth)
        {
            CoordinateTransformationFactory ctfac = new CoordinateTransformationFactory();
            ICoordinateSystem wgs84geo = ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84;
            ICoordinateSystem utm = ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WGS84_UTM(zone, isNorth); //(zone, lat > 0);
            ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(utm, wgs84geo);
            return trans.MathTransform.Transform(new double[] { easting, northing });
        }

        

    }
}
