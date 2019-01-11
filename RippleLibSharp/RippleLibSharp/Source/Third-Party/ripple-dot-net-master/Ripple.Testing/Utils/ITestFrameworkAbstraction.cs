using System;
using System.Threading.Tasks;

namespace Ripple.Testing.Utils
{
    public interface ITestFrameworkAbstraction
    {
        string CurrentTestName();
        bool DidTestFail();
        void RunAsyncAction(Func<Task> action);
        Type TestAttributeType();
    }
}