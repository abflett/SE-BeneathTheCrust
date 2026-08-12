namespace WkKn
{
    public class CommandNetworkMessage
    {
        public string Kind;
        public string CommandText;
        public WkPlayerConfigRecord PlayerConfig;
        public WkConfig WorldConfig;
        public bool CanEditWorldConfig;
        public bool Silent;
    }
}
