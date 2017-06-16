using System;
using SFML.System;

namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    internal static class SfmlMath
    {
        public static float GetMagnitude(this Vector2f v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        public static float Dot(this Vector2f v0, Vector2f v1)
        {
            return v0.X * v1.X + v0.Y * v1.Y;
        }

        public static Vector2f GetNormal(this Vector2f v)
        {
            return new Vector2f(-v.Y, v.X);
        }

        public static Vector2f Normalized(this Vector2f v)
        {
            return v / v.GetMagnitude();
        }
    }
}