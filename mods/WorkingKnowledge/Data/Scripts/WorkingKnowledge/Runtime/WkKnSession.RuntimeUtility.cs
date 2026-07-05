namespace WkKn
{
    public partial class WkKnSession
    {
        private static double Clamp01(double value)
        {
            return RatioMath.Clamp01(value);
        }

        private static double Clamp(double value, double min, double max)
        {
            return RatioMath.Clamp(value, min, max);
        }
    }
}
