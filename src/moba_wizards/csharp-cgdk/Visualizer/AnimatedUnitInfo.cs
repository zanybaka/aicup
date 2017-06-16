namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public class AnimatedUnitInfo
    {
        public long TickIndex;
        public double X;
        public double Y;

        public AnimatedUnitInfo(double x, double y, int tickIndex)
        {
            X = x;
            Y = y;
            TickIndex = tickIndex;
        }
    }
}