using System;
using System.Threading;
using log4net;

namespace Ripple.Testing.Utils
{
    public class MultiThreadedFailures
    {
        public static readonly ILog Log = Logging.ForContext();

        private Exception _unhandled;
        private Thread _testThread;
        private bool _aborted;
        private bool _setup;

        public void SetUp()
        {
            // Guard against multiple bindings of event handlers
            if (!_setup)
            {
                Log.Debug("Setting up unhandled exception handler");
                _unhandled = null;
                _aborted = false;
                _testThread = Thread.CurrentThread;
                AppDomain.CurrentDomain.UnhandledException += UnhandledException;
                _setup = true;
            }
            else
            {
                Log.Debug("Already setup unhandled exception handler");
            }
        }

        public void EnableForBlock(Action codeBlock)
        {
            try
            {
                SetUp();
                codeBlock();
            }
            finally
            {
                TearDown();
                
            }
        }

        public void TearDown()
        {
            if (_setup)
            {
                Log.Debug("Tearing down unhandled exception handler");
                AppDomain.CurrentDomain.UnhandledException -= UnhandledException;
                if (_unhandled != null)
                {
                    throw new NonTestThreadException(_unhandled);
                }
                _setup = false;
            }
            else
            {
                Log.Debug("Already tore down unhandled exception handler");
            }
        }

        private void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (!_aborted)
            {
                _unhandled = e.ExceptionObject as Exception;
                _aborted = true;
                Log.ErrorFormat("Aborting test thread because {0}", e.ExceptionObject);
                _testThread.Abort(e.ExceptionObject);
            }
        }
    }
}

