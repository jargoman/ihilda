using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Ripple.Testing.Server
{
    public class RippledConfig
    {
        public readonly string BasePath;
        protected List<RippledPort> Ports = new List<RippledPort>();
        protected List<string> Features = new List<string>();

        public RippledConfig(string basePath)
        {
            BasePath = basePath;
        }

        public RippledConfig AddDefaultPorts()
        {
            AddPort("http", 5005);
            AddPort("ws", 5006);
            return this;
        }

        public string AdminWebSocketUrl()
        {
            var port = Ports.Find(p => p.IsAdmin && p.Protocol.StartsWith("ws"));
            if (port == null)
            {
                throw new InvalidOperationException(
                    "Can't find an admin/ws port");
            }
            return port.Url;
        }

        public string ConfigText()
        {
            var conf = new ConfigWriter();
            conf.WriteList("server", Ports.Select(p => p.Name).ToList());
            Ports.ForEach(p => conf.WriteDict(p.Name, p.Dict()));
            conf.WriteList("node_db", "type=memory", "path=integration");
            conf.WriteList("features", Features);
            // We add the new line as rippled seems to bork without it
            // Don't be a smartarse an remove it.
            return Environment.NewLine + conf;
        }

        public RippledConfig PreparePathAndWriteConf()
        {
            if (Directory.Exists(BasePath))
            {
                Directory.Delete(BasePath, recursive: true);
            }
            Directory.CreateDirectory(BasePath);
            File.WriteAllText(ConfigPath(), ConfigText(), Encoding.UTF8);
            return this;
        }

        public string ConfigPath()
        {
            return Path.Combine(BasePath, "rippled.cfg");
        }

        public RippledConfig AddPort(string protocol, int port, bool isAdmin = true)
        {
            Ports.Add(new RippledPort(port, isAdmin, protocol));
            return this;
        }

        public RippledExecutionConfig ExecutionConfig(string binaryPath)
        {
            return new RippledExecutionConfig(binaryPath, ConfigPath());
        }

        public RippledConfig AddFeature(string feature)
        {
            Features.Add(feature);
            return this;
        }

        public static RippledConfig PrepareWithDefault(string basePath)
        {
            return new RippledConfig(basePath)
                .AddDefaultPorts()
                .AddFeature("multisign")
                .PreparePathAndWriteConf();
        }
    }
}