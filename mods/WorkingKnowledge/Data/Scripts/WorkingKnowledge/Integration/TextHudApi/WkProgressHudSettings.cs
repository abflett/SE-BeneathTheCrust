namespace WkKn
{
    internal struct WkProgressHudSettings
    {
        internal bool Enabled;
        internal int RowCount;
        internal string Order;
        internal string Position;
        internal double OffsetX;
        internal double OffsetY;
        internal double FadeSeconds;

        internal static WkProgressHudSettings Default
        {
            get
            {
                return new WkProgressHudSettings
                {
                    Enabled = true,
                    RowCount = 5,
                    Order = "descending",
                    Position = "topRight",
                    OffsetX = 0.0,
                    OffsetY = 0.0,
                    FadeSeconds = 6.0,
                };
            }
        }
    }
}
