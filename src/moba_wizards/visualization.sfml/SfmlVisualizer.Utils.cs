using System;
using SFML.System;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public partial class SfmlVisualizer
    {
        private WayPointInfo[][] GenerateWayPoints(Vector2f[] points, int radius)
        {
            int step = 1;
            int count = 2 * (int) (2*Math.PI*radius);
            int preCount = (int)(count * 0.9);
            int postCount = (int)(count * 0.1);
            WayPointInfo[][] result = new WayPointInfo[points.Length][];
            for (int i = 0; i < points.Length; i++)
            {
                result[i] = new WayPointInfo[count];
                int r = radius;
                for (int j = 0; j < preCount; j++)
                {
                    double angle = j * Math.PI * 2 / count;
                    result[i][j] = new WayPointInfo(j == 0 ? null : result[i][j - 1], new Vector2f(points[i].X + (float)(Math.Sin(angle) * r), points[i].Y + (float)(Math.Cos(angle) * r)));
                }
                for (int j = preCount; j < count; j++)
                {
                    result[i][j] = new WayPointInfo(result[i][preCount - 1], new Vector2f(
                        result[i][preCount - 1].Point.X - (result[i][preCount - 1].Point.X - result[i][0].Point.X) * (j - (preCount - 1)) / postCount,
                        result[i][preCount - 1].Point.Y - (result[i][preCount - 1].Point.Y - result[i][0].Point.Y) * (j - (preCount - 1)) / postCount));
                }
                result[i][0] = new WayPointInfo(result[i][count - 1], result[i][0].Point);
                if (rnd.Next(0, 2) == 0)
                {
                    Array.Reverse(result[i]);
                    foreach (var info in result[i])
                    {
                        info.Angle = info.Angle >= 0 ? info.Angle - Math.PI : info.Angle + Math.PI;
                    }
                }
            }
            return result;
        }

        private class WayPointInfo
        {
            public WayPointInfo(WayPointInfo prevPoint, Vector2f point)
            {
                Point = point;
                if (prevPoint != null)
                {
                    float xDiff = point.X - prevPoint.Point.X;
                    float yDiff = point.Y - prevPoint.Point.Y;
                    Angle = Math.Atan2(yDiff, xDiff);//*180.0/Math.PI;
                }
            }

            public Vector2f Point { get; private set; }
            public double Angle { get; set; }
        }
    }
}