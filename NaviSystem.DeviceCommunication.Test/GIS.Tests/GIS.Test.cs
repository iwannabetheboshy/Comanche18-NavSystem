using NUnit.Framework;
using System.Windows;

namespace NaviSystem.GIS.Test
{
    //Point PixToGeo(Point p_pix);
    //Point GeoToPix(Point p_geo);
    //double GetDistance(Point p1, Point p2);
    //Point GeoToUTM(Point g_point);
    //Point UTMToGeo(Point utm_point, bool isNorth);

    [TestFixture]
    public class GISTest
    {
        [Test]
        public void GeoToUTMTest()
        {
            IGIS gis = null;

            Point geoPoint = new Point(134.4587, 42.4587);
            Point utmPoint = new Point(455494, 4700849);
            Point result = gis.GeoToUTM(geoPoint);
            result = new Point((int)result.X, (int)result.Y);
            Assert.That(utmPoint == result);
        }

        [Test]
        public void UTMToGeoTest()
        {
            IGIS gis = null;

            Point geoPoint = new Point(134.4586, 42.4586);
            Point utmPoint = new Point(455494, 4700849);
            Point result = gis.UTMToGeo(utmPoint, true);

            result = new Point((int)(result.X*10000), (int)(result.Y*10000));
            geoPoint = new Point((int)(geoPoint.X * 10000), (int)(geoPoint.Y * 10000));
            Assert.That(geoPoint == result);
        }
    }
}
