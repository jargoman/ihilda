﻿using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        Load(); return;
        testVb(); return;

        string baseDir = @"E:\Galos\Projects\CS-Script.Npp\CSScript.Npp\src\CSScriptNpp\CSScriptNpp\Roslyn\";
        //baseDir = @"E:\Galos\Projects\CS-Script.Npp\gittest\csscriptnpp\bin\Plugins\CSScriptNpp\Roslyn\";
        //baseDir = @"E:\cs-script\lib\Bin\Roslyn\";
        baseDir = @"E:\Galos\Projects\CS-Script\Src\CSSCodeProvider.v4.6\bin\roslyn\"; //CS-S source v1.1.0
        baseDir = @"E:\Galos\Projects\CS-Script.Npp\CSScript.Npp\src\CSScriptNpp\CSScriptNpp\Roslyn\";

        //CSSCodeProvider.CompilerPath = @"E:\Galos\Projects\CS-Script\Src\CSSCodeProvider.v4.6\bin\roslyn\csc.exe";
        //CSSCodeProvider.ProviderPath = @"E:\Galos\Projects\CS-Script\Src\CSSCodeProvider.v4.6\bin\roslyn\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll";

        baseDir = @"C:\Program Files (x86)\Notepad++\plugins\CSScriptNpp\Roslyn\";
        CSSCodeProvider.CompilerPath = baseDir + "csc.exe";
        CSSCodeProvider.ProviderPath = baseDir + "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll";


        CSSCodeProvider.CompilerServerTimeToLive = 60;
        Environment.SetEnvironmentVariable("CSS_CompilerDefaultSyntax", "vb");

        ICodeCompiler compiler = CSSCodeProvider.CreateCompiler("");

        var sw = new Stopwatch();
        sw.Start();

        var compilerParams = new CompilerParameters();

        var file = @"E:\Galos\Projects\CS-Script\Src\CSSCodeProvider.v4.6\CSSCodeProvider.Client\Script.cs";
        file = @"E:\cs-script\samples\Hello.vb";

        compilerParams.GenerateExecutable = false;
        compilerParams.GenerateInMemory = false;

        var result = compiler.CompileAssemblyFromFile(compilerParams, file);
        var failed = result.Errors.Count > 0;
        sw.Stop();
        Console.WriteLine((failed ? "failed - " : "OK - ") + sw.ElapsedMilliseconds);
    }

    static void testVb()
    {
        try
        {
            var baseDir = @"C:\Program Files (x86)\Notepad++\plugins\CSScriptNpp\Roslyn\";
            //baseDir = @"E:\Galos\Projects\CS-Script.Npp\CSScript.Npp\src\CSScriptNpp\CSScriptNpp\Roslyn\";
            CSSCodeProvider.CompilerPath = baseDir + "vbc.exe";
            CSSCodeProvider.ProviderPath = baseDir + "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll";
            CSSCodeProvider.CompilerServerTimeToLive = 600;
            CSSCodeProvider.CompilerServerTimeToLive = 6;


            ICodeCompiler compiler = CSSCodeProvider.CreateCompiler("code.vb");
            var compilerParams = new CompilerParameters();
            //var file = @"E:\cs-script\samples\Hello.vb";
            //compilerParams.ReferencedAssemblies.Add(@"System.Net.Http.Formatting.dll");
            //compilerParams.ReferencedAssemblies.Add(@"System.dll");
            //var result = compiler.CompileAssemblyFromFile(compilerParams, file);


            var result = compiler.CompileAssemblyFromSource(compilerParams, @"
Imports System
Imports System.Windows.Forms

Module Module1
    Sub Main()
        Console.WriteLine(""Hello World!(VB)"")
    End Sub
End Module");
            bool success = !result.Errors.HasErrors;
        }
        catch { }
        Console.WriteLine("done");
    }
    static void Load()
    {
        try
        {
            var baseDir = @"C:\Program Files (x86)\Notepad++\plugins\CSScriptNpp\Roslyn\";
            //baseDir = @"E:\Galos\Projects\CS-Script.Npp\CSScript.Npp\src\CSScriptNpp\CSScriptNpp\Roslyn\";
            CSSCodeProvider.CompilerPath = baseDir + "csc.exe";
            CSSCodeProvider.ProviderPath = baseDir + "Microsoft.CodeDom.Providers.DotNetCompilerPlatform.dll";
            CSSCodeProvider.CompilerServerTimeToLive = 600;
            CSSCodeProvider.CompilerServerTimeToLive = 6;

            ICodeCompiler compiler = CSSCodeProvider.CreateCompiler("");
            var compilerParams = new CompilerParameters();
            var result = compiler.CompileAssemblyFromFile(compilerParams, @"C:\Users\osh\Documents\C# Scripts\New Script64.cs");
        }
        catch { }
        Console.WriteLine("done");
    }

}
