#region Licence...

//-----------------------------------------------------------------------------
// Date:	25/10/10	Time: 2:33p
// Module:	settings.cs
// Classes:	Settings
//			ExecuteOptions
//
// This module contains the definition of the CSExecutor class. Which implements
// compiling C# code and executing 'Main' method of compiled assembly
//
// Written by Oleg Shilo (oshilo@gmail.com)
//----------------------------------------------
// The MIT License (MIT)
// Copyright (c) 2016 Oleg Shilo
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
// and associated documentation files (the "Software"), to deal in the Software without restriction, 
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial 
// portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT 
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//----------------------------------------------

#endregion Licence...

using System;
using System.Collections.Generic;
using System.IO;
#if net1
using System.Collections;
#else

#endif

using System.Threading;
using System.Xml;
using System.Drawing.Design;

using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace csscript
{
    /// <summary>
    /// Settings is an class that holds CS-Script application settings.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Command to be executed to perform custom cleanup.
        /// If this value is empty automatic cleanup of all
        /// temporary files will occurs after the script execution.
        /// This implies that the script has to be executed in the
        /// separate AppDomain and some performance penalty will be incurred.
        ///
        /// Setting this value to the command for custom cleanup application
        /// (e.g. csc.exe cleanTemp.cs) will force the script engine to execute
        /// script in the 'current' AppDomain what will improve performance.
        /// </summary>
        [Category("CustomCleanup"), Description("Command to be executed to perform custom cleanup.")]
        public string CleanupShellCommand
        {
            get { return cleanupShellCommand; }
            set { cleanupShellCommand = value; }
        }

        /// <summary>
        /// Returns value of the CleanupShellCommand (with expanding environment variables).
        /// </summary>
        /// <returns>shell command string</returns>
        public string ExpandCleanupShellCommand() { return Environment.ExpandEnvironmentVariables(cleanupShellCommand); }

        string cleanupShellCommand = "";

        /// <summary>
        /// This value indicates frequency of the custom cleanup
        /// operation. It has affect only if CleanupShellCommand is not empty.
        /// </summary>
        [Category("CustomCleanup"), Description("This value indicates frequency of the custom cleanup operation.")]
        public uint DoCleanupAfterNumberOfRuns
        {
            get { return doCleanupAfterNumberOfRuns; }
            set { doCleanupAfterNumberOfRuns = value; }
        }

        uint doCleanupAfterNumberOfRuns = 30;

        /// <summary>
        /// Location of alternative code provider assembly. If set it forces script engine to use an alternative code compiler.
        /// </summary>
        [Category("Extensibility"), Description("Location of alternative code provider assembly. If set it forces script engine to use an alternative code compiler.")]
#if !InterfaceAssembly
        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
#endif
        public string UseAlternativeCompiler
        {
            get { return useAlternativeCompiler; }
            set { useAlternativeCompiler = value; }
        }

        /// <summary>
        /// Returns value of the UseAlternativeCompiler (with expanding environment variables).
        /// </summary>
        /// <returns>Path string</returns>
        public string ExpandUseAlternativeCompiler() { return (useAlternativeCompiler == null) ? "" : Environment.ExpandEnvironmentVariables(useAlternativeCompiler); }

        string useAlternativeCompiler = "";

        /// <summary>
        /// Location of PostProcessor assembly. If set it forces script engine to pass compiled script through PostProcessor before the execution.
        /// </summary>
        [Category("Extensibility"), Description("Location of PostProcessor assembly. If set it forces script engine to pass compiled script through PostProcessor before the execution.")]
#if !InterfaceAssembly
        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(UITypeEditor))]
#endif
        public string UsePostProcessor
        {
            get { return usePostProcessor; }
            set { usePostProcessor = value; }
        }

        /// <summary>
        /// Returns value of the UsePostProcessor (with expanding environment variables).
        /// </summary>
        /// <returns>Path string</returns>
        public string ExpandUsePostProcessor() { return Environment.ExpandEnvironmentVariables(usePostProcessor); }

        string usePostProcessor = "";

        /// <summary>
        /// DefaultApartmentState is an ApartmemntState, which will be used
        /// at run-time if none specified in the code with COM threading model attributes.
        /// </summary>
        [Category("RuntimeSettings"), Description("DefaultApartmentState is an ApartmemntState, which will be used at run-time if none specified in the code with COM threading model attributes.")]
        public ApartmentState DefaultApartmentState
        {
            get { return defaultApartmentState; }
            set { defaultApartmentState = value; }
        }

        ApartmentState defaultApartmentState = ApartmentState.STA;

        /// <summary>
        /// Default command-line arguments. For example if "/dbg" is specified all scripts will be compiled in debug mode
        /// regardless if the user specified "/dbg" when a particular script is launched.
        /// </summary>
        [Category("RuntimeSettings"), Description("Default command-line arguments (e.g.-dbg) for all scripts.")]
        public string DefaultArguments
        {
            get { return defaultArguments; }
            set { defaultArguments = value; }
        }

        bool injectScriptAssemblyAttribute = true;

        /// <summary>
        /// Gets or sets a value indicating whether script assembly attribute should be injected. The AssemblyDecription attribute 
        /// contains the original location of the script file the assembly being compiled from./
        /// </summary>
        /// <value>
        /// <c>true</c> if the attribute should be injected; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool InjectScriptAssemblyAttribute
        {
            get { return injectScriptAssemblyAttribute; }
            set { injectScriptAssemblyAttribute = value; }
        }

        bool resolveAutogenFilesRefs = true;

        /// <summary>
        /// Gets or sets a value indicating whether references to the auto-generated files should be resolved.
        /// <para>
        /// If this flag is set the all references in the compile errors text to the path of the derived auto-generated files 
        /// (e.g. errors in the decorated classless scripts) will be replaced with the path of the original file(s) (e.g. classless script itself).
        /// </para> 
        /// </summary>
        /// <value>
        /// <c>true</c> if preferences needs to be resolved; otherwise, <c>false</c>.
        /// </value>
        public bool ResolveAutogenFilesRefs
        {
            get { return resolveAutogenFilesRefs; }
            set { resolveAutogenFilesRefs = value; }
        }

        string defaultArguments = CSSUtils.Args.Join("c", "sconfig", "co:" + CSSUtils.Args.DefaultPrefix + "warn:0");
        //string defaultArguments = CSSUtils.Args.DefaultPrefix + "c " + CSSUtils.Args.DefaultPrefix + "sconfig " + CSSUtils.Args.DefaultPrefix + "co:" + CSSUtils.Args.DefaultPrefix + "warn:0";

        ///// <summary>
        ///// Enables using a surrogate process to host the script engine at runtime. This may be a useful option for fine control over the hosting process
        ///// (e.g. ensuring "CPU type" of the process, CLR version to be loaded).
        ///// </summary>
        //[Category("RuntimeSettings")]
        //internal bool UseSurrogateHostingProcess  //do not expose it to the user just yet
        //{
        //    get { return useSurrogatepHostingProcess; }
        //    set { useSurrogatepHostingProcess = value; }
        //}

        bool useSurrogatepHostingProcess = false;

        bool openEndDirectiveSyntax = true;

        /// <summary>
        /// Enables omitting closing character (";") for CS-Script directives (e.g. "//css_ref System.Xml.dll" instead of "//css_ref System.Xml.dll;").
        /// </summary>
        [Browsable(false)]
        public bool OpenEndDirectiveSyntax
        {
            get { return openEndDirectiveSyntax; }
            set { openEndDirectiveSyntax = value; }
        }


        string consoleEncoding = DefaultEncodingName;
        /// <summary>
        /// Encoding of he Console Output. Applicable for console applications script engine only.
        /// </summary>
        [Category("RuntimeSettings"), Description("Console output encoding. Use 'default' value if you want to use system default encoding.")]
        [TypeConverter(typeof(EncodingConverter))]
        public string ConsoleEncoding
        {
            get { return consoleEncoding; }
            set
            {
                //consider: https://social.msdn.microsoft.com/Forums/vstudio/en-US/e448b241-e250-4dcb-8ecd-361e00920dde/consoleoutputencoding-breaks-batch-files?forum=netfxbcl 
                if (consoleEncoding != value)
                {
                    consoleEncoding = Utils.ProcessNewEncoding(value);
                }
            }
        }

        internal const string DefaultEncodingName = "default";

        /// <summary>
        /// Specifies the .NET Framework version that the script is compiled against. This option can have the following values:
        ///   v2.0
        ///   v3.0
        ///   v3.5
        ///   v4.0
        /// </summary>
        [Browsable(false)]
        public string TargetFramework
        {
            get { return targetFramework; }
            set { targetFramework = value; }
        }

#if net35
        string targetFramework = "v3.5";
#else
        string targetFramework = "v4.0";
#endif

        /// <summary>
        /// Specifies the .NET Framework version that the script is compiled against. This option can have the following values:
        ///   v2.0
        ///   v3.0
        ///   v3.5
        ///   v4.0
        /// </summary>
        [Category("RuntimeSettings")]
        [Description("Specifies the .NET Framework version that the script is compiled against (used by CSharpCodeProvider.CreateCompiler as the 'CompilerVersion' parameter).\nThis option is for the script compilation only.\nFor changing the script execution CLR use //css_host directive from the script.\nYou are discouraged from modifying this value thus if the change is required you need to edit css_config.xml file directly.")]
        public string CompilerFramework
        {
            get { return targetFramework; }
        }

        /// <summary>
        /// List of assembly names to be automatically referenced by the script. The items must be separated by coma or semicolon. Specifying .dll extension (e.g. System.Core.dll) is optional.
        /// Assembly can contain expandable environment variables.
        /// </summary>
        [Category("Extensibility"), Description("List of assembly names to be automatically referenced by the scripts (e.g. System.dll, System.Core.dll). Assembly extension is optional.")]
        public string DefaultRefAssemblies
        {
            get
            {
                if (defaultRefAssemblies == null)
                    defaultRefAssemblies = InitDefaultRefAssemblies();

                return defaultRefAssemblies;
            }
            set { defaultRefAssemblies = value; }
        }

        string defaultRefAssemblies;


        string InitDefaultRefAssemblies()
        {
#if net4
            if (Utils.IsLinux())
            {
                return "System;System.Core;";
            }
            else
            {
                if (Utils.IsNet45Plus())
                    return "System.Core; System.Linq;";
                else
                    return "System.Core;";

            }
#else
            return "";
#endif
        }

        /// <summary>
        /// Returns value of the DefaultRefAssemblies (with expanding environment variables).
        /// </summary>
        /// <returns>List of assembly names</returns>
        public string ExpandDefaultRefAssemblies() { return Environment.ExpandEnvironmentVariables(DefaultRefAssemblies); }

        /// <summary>
        /// List of directories to be used to search (probing) for referenced assemblies and script files.
        /// This setting is similar to the system environment variable PATH.
        /// </summary>
        [Category("Extensibility"), Description("List of directories to be used to search (probing) for referenced assemblies and script files.\nThis setting is similar to the system environment variable PATH.")]
        public string SearchDirs
        {
            get { return searchDirs; }
            set { searchDirs = value; }
        }

        string searchDirs = "%CSSCRIPT_DIR%" + Path.DirectorySeparatorChar + "lib;%CSSCRIPT_INC%;";

        /// <summary>
        /// Add search directory to the search (probing) path Settings.SearchDirs.
        /// For example if Settings.SearchDirs = "c:\scripts" then after call Settings.AddSearchDir("c:\temp") Settings.SearchDirs is "c:\scripts;c:\temp"
        /// </summary>
        /// <param name="dir">Directory path.</param>
        public void AddSearchDir(string dir)
        {
            if (dir != "")
            {
                foreach (string searchDir in searchDirs.Split(';'))
                    if (searchDir != "" && Utils.IsSamePath(Path.GetFullPath(searchDir), Path.GetFullPath(dir)))
                        return; //already there

                searchDirs += ";" + dir;
            }
        }

        /// <summary>
        /// The value, which indicates if auto-generated files (if any) should should be hidden in the temporary directory.
        /// </summary>
        [Category("RuntimeSettings"), Description("The value, which indicates if auto-generated files (if any) should should be hidden in the temporary directory.")]
        public HideOptions HideAutoGeneratedFiles
        {
            get { return hideOptions; }
            set { hideOptions = value; }
        }

        string precompiler = "";

        /// <summary>
        /// Path to the precompiller script/assembly (see documentation for details). You can specify multiple recompiles separating them by semicolon.
        /// </summary>
        [Category("RuntimeSettings"), Description("Path to the precompiller script/assembly (see documentation for details). You can specify multiple recompiles separating them by semicolon.")]
        public string Precompiler
        {
            get { return precompiler; }
            set { precompiler = value; }
        }

        bool customHashing = true;

        /// <summary>
        /// Gets or sets a value indicating whether custom string hashing algorithm should be used.
        /// <para>
        /// String hashing is used by the script engine for allocating temporary and cached paths. 
        /// However default string hashing is platform dependent (x32 vs. x64) what makes impossible 
        /// truly deterministic string hashing. This in turns complicates the integration of the 
        /// CS-Script infrastructure with the third-party applications (e.g. Notepad++ CS-Script plugin).
        /// </para>
        /// <para>
        /// To overcome this problem CS-Script uses custom string hashing algorithm (default setting).
        /// Though the native .NET hashing can be enabled if desired by setting <c>CustomHashing</c>
        /// to <c>false</c>.</para>
        /// </summary>
        /// <value>
        ///   <c>true</c> if custom hashing is in use; otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool CustomHashing
        {
            get { return customHashing; }
            set { customHashing = value; }
        }

        HideOptions hideOptions = HideOptions.HideMostFiles;
        ///// <summary>
        ///// The value, which indicates which version of CLR compiler should be used to compile script.
        ///// For example CLR 2.0 can use the following compiler versions:
        ///// default - .NET 2.0
        ///// 3.5 - .NET 3.5
        ///// Use empty string for default compiler.
        ///// </summary>string compilerVersion = "";
        //[Category("RuntimeSettings")]
        //public string CompilerVersion
        //{
        //    get { return compilerVersion; }
        //    set { compilerVersion = value; }
        //}
        //string compilerVersion = "";

        /// <summary>
        /// Enum for possible hide auto-generated files scenarios
        /// Note: when HideAll is used it is responsibility of the pre/post script to implement actual hiding.
        /// </summary>
        public enum HideOptions
        {
            /// <summary>
            /// Do not hide auto-generated files.
            /// </summary>
            DoNotHide,
            /// <summary>
            /// Hide the most of the auto-generated (cache and "imported") files.
            /// </summary>
            HideMostFiles,
            /// <summary>
            /// Hide all auto-generated files including the files generated by pre/post scripts.
            /// </summary>
            HideAll
        }

        /// <summary>
        /// Boolean flag that indicates how much error details to be reported should error occur.
        /// false - Top level exception will be reported
        /// true - Whole exception stack will be reported
        /// </summary>
        [Category("RuntimeSettings"), Description("Indicates how much error details to be reported should error occur.")]
        public bool ReportDetailedErrorInfo
        {
            get { return reportDetailedErrorInfo; }
            set { reportDetailedErrorInfo = value; }
        }

        bool reportDetailedErrorInfo = true;

        /// <summary>
        /// Gets or sets a value indicating whether Optimistic Concurrency model should be used when executing scripts from the host application.
        /// If set to <c>false</c> the script loading (not the execution) is globally thread-safe. If set to <c>true</c> the script loading is
        /// thread-safe only among loading operations for the same script file.
        /// <para>The default value is <c>true</c>.</para>
        /// </summary>
        /// <value>
        /// 	<c>true</c> if Optimistic Concurrency model otherwise, <c>false</c>.
        /// </value>
        [Browsable(false)]
        public bool OptimisticConcurrencyModel
        {
            get { return optimisticConcurrencyModel; }
            set { optimisticConcurrencyModel = value; }
        }

        bool optimisticConcurrencyModel = true;

        /// <summary>
        /// Boolean flag that indicates if compiler warnings should be included in script compilation output.
        /// false - warnings will be displayed
        /// true - warnings will not be displayed
        /// </summary>
        [Category("RuntimeSettings"), Description("Indicates if compiler warnings should be included in script compilation output.")]
        public bool HideCompilerWarnings
        {
            get { return hideCompilerWarnings; }
            set { hideCompilerWarnings = value; }
        }

        bool hideCompilerWarnings = false;

        /// <summary>
        /// Boolean flag that indicates the script assembly is to be loaded by CLR as an in-memory byte stream instead of the file.
        /// This setting can be useful when you need to prevent script assembly (compiled script) from locking by CLR during the execution.
        /// false - script assembly will be loaded as a file. It is an equivalent of Assembly.LoadFrom(string assemblyFile).
        /// true - script assembly will be loaded as a file. It is an equivalent of Assembly.Load(byte[] rawAssembly)
        /// <para>Note: some undesired side effects can be triggered by having assemblies with <c>Assembly.Location</c> being empty. 
        /// For example <c>Interface Alignment</c> any not work with such assemblies as it relies on CLR compiling services that 
        /// typically require assembly <c>Location</c> member being populated with the valid path.</para>
        /// </summary>
        [Category("RuntimeSettings"), 
         Description("Indicates the script assembly is to be loaded by CLR as an in-memory byte stream instead of the file. "+                      
                      "Note this settings can affect the use cases requiring the loaded assemblies to have non empty Assembly.Location.")]
        public bool InMemoryAssembly
        {
            get { return inMemoryAsm; }
            set { inMemoryAsm = value; }
        }

        bool inMemoryAsm = true;

        /// <summary>
        /// Gets or sets the concurrency control model.
        /// </summary>
        /// <value>The concurrency control.</value>
        [Browsable(false)]
        public ConcurrencyControl ConcurrencyControl
        {
            get
            {
                if (concurrencyControl == ConcurrencyControl.HighResolution && Utils.IsLinux())
                    concurrencyControl = ConcurrencyControl.Standard;
                return concurrencyControl;
            }
            set
            {
                concurrencyControl = value;
            }
        }

        ConcurrencyControl concurrencyControl = ConcurrencyControl.Standard;

        /// <summary>
        /// Saves CS-Script application settings to a file (.dat).
        /// </summary>
        /// <param name="fileName">File name of the .dat file</param>
        public void Save(string fileName)
        {
            //It is very tempting to use XmlSerializer but it adds 200 ms to the
            //application startup time. Whereas current startup delay for cscs.exe is just a 100 ms.
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<CSSConfig/>");
                doc.DocumentElement.AppendChild(doc.CreateElement("defaultArguments")).AppendChild(doc.CreateTextNode(DefaultArguments));
                doc.DocumentElement.AppendChild(doc.CreateElement("defaultApartmentState")).AppendChild(doc.CreateTextNode(DefaultApartmentState.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("reportDetailedErrorInfo")).AppendChild(doc.CreateTextNode(ReportDetailedErrorInfo.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("useAlternativeCompiler")).AppendChild(doc.CreateTextNode(UseAlternativeCompiler));
                doc.DocumentElement.AppendChild(doc.CreateElement("usePostProcessor")).AppendChild(doc.CreateTextNode(UsePostProcessor));
                doc.DocumentElement.AppendChild(doc.CreateElement("searchDirs")).AppendChild(doc.CreateTextNode(SearchDirs));
                doc.DocumentElement.AppendChild(doc.CreateElement("cleanupShellCommand")).AppendChild(doc.CreateTextNode(CleanupShellCommand));
                doc.DocumentElement.AppendChild(doc.CreateElement("doCleanupAfterNumberOfRuns")).AppendChild(doc.CreateTextNode(DoCleanupAfterNumberOfRuns.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("hideOptions")).AppendChild(doc.CreateTextNode(hideOptions.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("hideCompilerWarnings")).AppendChild(doc.CreateTextNode(HideCompilerWarnings.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("inMemoryAsm")).AppendChild(doc.CreateTextNode(InMemoryAssembly.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("ConcurrencyControl")).AppendChild(doc.CreateTextNode(ConcurrencyControl.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("TragetFramework")).AppendChild(doc.CreateTextNode(TargetFramework));
                doc.DocumentElement.AppendChild(doc.CreateElement("ConsoleEncoding")).AppendChild(doc.CreateTextNode(ConsoleEncoding));
                doc.DocumentElement.AppendChild(doc.CreateElement("defaultRefAssemblies")).AppendChild(doc.CreateTextNode(DefaultRefAssemblies));
                doc.DocumentElement.AppendChild(doc.CreateElement("useSurrogatepHostingProcess")).AppendChild(doc.CreateTextNode(useSurrogatepHostingProcess.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("openEndDirectiveSyntax")).AppendChild(doc.CreateTextNode(openEndDirectiveSyntax.ToString()));
                doc.DocumentElement.AppendChild(doc.CreateElement("Precompiler")).AppendChild(doc.CreateTextNode(Precompiler));
                doc.DocumentElement.AppendChild(doc.CreateElement("CustomHashing")).AppendChild(doc.CreateTextNode(CustomHashing.ToString()));

                doc.Save(fileName);
            }
            catch { }
        }

        /// <summary>
        /// Loads CS-Script application settings from a file. Default settings object is returned if it cannot be loaded from the file.
        /// </summary>
        /// <param name="fileName">File name of the XML file</param>
        /// <returns>Setting object deserilized from the XML file</returns>
        public static Settings Load(string fileName)
        {
            return Load(fileName, true);
        }

        /// <summary>
        /// Loads CS-Script application settings from a file.
        /// </summary>
        /// <param name="fileName">File name of the XML file</param>
        /// <param name="createAlways">Create and return default settings object if it cannot be loaded from the file.</param>
        /// <returns>Setting object deserialized from the XML file</returns>
        public static Settings Load(string fileName, bool createAlways)
        {
            //System.Diagnostics.Debug.Assert(false);
            Settings settings = new Settings();
            if (File.Exists(fileName))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    XmlNode data = doc.FirstChild;
                    settings.defaultArguments = data.SelectSingleNode("defaultArguments").InnerText;
                    settings.defaultApartmentState = (ApartmentState) Enum.Parse(typeof(ApartmentState), data.SelectSingleNode("defaultApartmentState").InnerText, false);
                    settings.reportDetailedErrorInfo = data.SelectSingleNode("reportDetailedErrorInfo").InnerText.ToLower() == "true";
                    settings.UseAlternativeCompiler = data.SelectSingleNode("useAlternativeCompiler").InnerText;
                    settings.UsePostProcessor = data.SelectSingleNode("usePostProcessor").InnerText;
                    settings.SearchDirs = data.SelectSingleNode("searchDirs").InnerText;
                    settings.cleanupShellCommand = data.SelectSingleNode("cleanupShellCommand").InnerText;
                    settings.doCleanupAfterNumberOfRuns = uint.Parse(data.SelectSingleNode("doCleanupAfterNumberOfRuns").InnerText);
                    settings.hideOptions = (HideOptions) Enum.Parse(typeof(HideOptions), data.SelectSingleNode("hideOptions").InnerText, true);
                    settings.hideCompilerWarnings = data.SelectSingleNode("hideCompilerWarnings").InnerText.ToLower() == "true";
                    settings.inMemoryAsm = data.SelectSingleNode("inMemoryAsm").InnerText.ToLower() == "true";
                    settings.concurrencyControl = (ConcurrencyControl) Enum.Parse(typeof(ConcurrencyControl), data.SelectSingleNode("ConcurrencyControl").InnerText, false);
                    settings.TargetFramework = data.SelectSingleNode("TragetFramework").InnerText;
                    settings.defaultRefAssemblies = data.SelectSingleNode("defaultRefAssemblies").InnerText;
                    settings.useSurrogatepHostingProcess = data.SelectSingleNode("useSurrogatepHostingProcess").InnerText.ToLower() == "true";
                    settings.OpenEndDirectiveSyntax = data.SelectSingleNode("openEndDirectiveSyntax").InnerText.ToLower() == "true";
                    settings.Precompiler = data.SelectSingleNode("Precompiler").InnerText;
                    settings.CustomHashing = data.SelectSingleNode("CustomHashing").InnerText.ToLower() == "true";
                    settings.ConsoleEncoding = data.SelectSingleNode("ConsoleEncoding").InnerText;
                }
                catch
                {
                    if (!createAlways)
                        settings = null;
                    else
                        settings.Save(fileName);
                }

                CSharpParser.OpenEndDirectiveSyntax = settings.OpenEndDirectiveSyntax;
            }
            return settings;
        }
    }

    internal class EncodingConverter : TypeConverter
    {
        public EncodingConverter()
        {
            encodings.Add("default");
            foreach (EncodingInfo item in Encoding.GetEncodings())
                encodings.Add(item.Name);
        }

        List<string> encodings = new List<string>();

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(encodings);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value;
        }
    }

}