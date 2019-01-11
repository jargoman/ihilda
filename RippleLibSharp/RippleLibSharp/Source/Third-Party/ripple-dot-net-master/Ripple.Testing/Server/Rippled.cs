using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using Microsoft.Build.Utilities;
using Ripple.Testing.Utils;

namespace Ripple.Testing.Server
{
    public class Rippled
    {
        public static readonly ILog Log = Logging.ForContext();
        public Process Proc;
        public RippledExecutionConfig Conf;
        public event Action<Rippled> AtExit;
        public event Action<string> OnStandardError, OnStandardOut;

        public Rippled(RippledExecutionConfig config)
        {
            Debug.Assert(File.Exists(config.BinaryPath), 
                $"{nameof(config.BinaryPath)} " +
                $"`{config.BinaryPath}` does not exist");

            Debug.Assert(File.Exists(config.ConfPath),
                $"{nameof(config.ConfPath)} " +
                $"`{config.ConfPath}` does not exist");

            Conf = config;
        }

        public Rippled Start()
        {
            // ReSharper disable once RedundantArgumentName
            Proc = new Process
            {
                StartInfo = StartInfo(redirectStandardIo: !Conf.ShowTerminal),
                // This allows the Exited event to fire
                EnableRaisingEvents = true
            };

            Proc.Start();
            BindEventHandlers();
            return this;
        }

        /// <summary>
        /// This must be called AFTER <see cref="Start"/>
        /// </summary>
        private void BindEventHandlers()
        {
            if (!Conf.ShowTerminal)
            {
                Proc.BeginOutputReadLine();
                Proc.BeginErrorReadLine();
                Proc.ErrorDataReceived += (sender, args) =>
                {
                    OnStandardError?.Invoke(args.Data);
                    Log.DebugFormat("stderr: {0}", args.Data);
                };
                Proc.OutputDataReceived += (sender, args) =>
                {
                    OnStandardOut?.Invoke(args.Data);
                    Log.DebugFormat("stdout: {0}", args.Data);
                };
            }

            Proc.Exited += delegate
            {
                Log.Info("exited: " + Proc.ExitCode);
                AtExit?.Invoke(this);
            };
        }

        private ProcessStartInfo StartInfo(bool redirectStandardIo)
        {
            var info = new ProcessStartInfo
            {
                FileName = Conf.BinaryPath,
                Arguments = CreateArgsLine(),
                // Execute this directly
                UseShellExecute = false,
                // We don't want to spawn a cmd window
                CreateNoWindow = !Conf.ShowTerminal,
                RedirectStandardOutput = redirectStandardIo,
                RedirectStandardError = redirectStandardIo,
            };

            if (redirectStandardIo)
            {
                info.StandardOutputEncoding = Encoding.UTF8;
                info.StandardErrorEncoding = Encoding.UTF8;
            }
            return info;
        }

        private string CreateArgsLine()
        {

            var builder = new CommandLineBuilder();

            builder.AppendSwitch("--standalone");
            builder.AppendSwitchIfNotNull("--ledgerfile=", Conf.LedgerDump);
            builder.AppendSwitchIfNotNull("--conf=", Conf.ConfPath);
            if (Conf.VerboseFlag)
            {
                builder.AppendSwitch("--verbose");
            }
            var argsLine = builder.ToString();
            Log.DebugFormat("Argline used: {0}", argsLine);
            return argsLine;
        }
    }
}