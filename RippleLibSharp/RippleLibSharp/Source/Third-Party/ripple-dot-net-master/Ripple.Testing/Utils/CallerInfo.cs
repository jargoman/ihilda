using System;
using System.Diagnostics;

namespace Ripple.Testing.Utils
{
    public class CallerInfo
    {
        public string MemberName = "";
        public string SourceFilePath = "";
        // ReSharper disable once RedundantDefaultMemberInitializer
        public int SourceLineNumber = 0;
        public bool IsEmpty = true;

        public CallerInfo()
        {
            
        }

        public CallerInfo(string memberName, string sourceFilePath, int sourceLineNumber)
        {
            MemberName = memberName;
            SourceFilePath = sourceFilePath;
            SourceLineNumber = sourceLineNumber;
            IsEmpty = false;
        }

        public bool IsNull()
        {
            return IsEmpty;
        }
    }
}