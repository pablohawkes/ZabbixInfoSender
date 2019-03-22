namespace ZabbixInfoSender
{
    public class ZabbixMessage
    {
        public string  Host { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public ZabbixMessage()
        {
        }

        public ZabbixMessage(string host, string key, int value)
        {
            Host = host.Trim();
            Key = key.Trim();
            Value = value.ToString();
        }

        public ZabbixMessage(string host, string key, string value)
        {
            Host = host.Trim();
            Key = key.Trim();
            Value = value.Trim();
        }
    }
}
