namespace Ripple.Testing.Server
{
    public class RippledExecutionConfig
    {
        public readonly string BinaryPath;
        public readonly string ConfPath;

        // optionals
        public string LedgerDump;
        public bool ShowTerminal;
        public bool VerboseFlag;

        public RippledExecutionConfig WithVerbosity(bool verbose = true)
        {
            VerboseFlag = verbose;
            return this;
        }

        public RippledExecutionConfig WithLedgerFile(string ledgerDump)
        {
            LedgerDump = ledgerDump;
            return this;
        }

        public RippledExecutionConfig WithTerminal(bool terminal = true)
        {
            ShowTerminal = terminal;
            return this;
        }

        public RippledExecutionConfig(string binaryPath, string confPath)
        {
            BinaryPath = binaryPath;
            ConfPath = confPath;
        }
    }
}