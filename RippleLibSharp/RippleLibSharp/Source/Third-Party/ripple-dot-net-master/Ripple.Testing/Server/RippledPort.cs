using System.Collections.Generic;

namespace Ripple.Testing.Server
{
    public class RippledPort
    {
        public readonly bool IsAdmin;
        public readonly int PortValue;
        public readonly string Protocol;

        public RippledPort(int port, bool isAdmin, string protocol)
        {
            PortValue = port;
            IsAdmin = isAdmin;
            Protocol = protocol;
        }

        public string Name {
            get
            {
                var admin = IsAdmin ? "admin_" : "";
                return $"port_{admin}{Protocol}_{PortValue}";
            }
        }

        public string Url => $"{Protocol}://{Ip}:{PortValue}";
        public readonly string Ip = "127.0.0.1";

        public Dictionary<string, object> Dict()
        {
            var d = new Dictionary<string, object>
            {
                ["protocol"] = Protocol,
                ["port"] = PortValue,
                ["ip"] = Ip
            };
            if (IsAdmin)
            {
                d["admin"] = Ip;
            }
            return d;
        }
    }
}