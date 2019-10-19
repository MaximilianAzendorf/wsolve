﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using NDesk.Options;
namespace WSolve
{
    internal static class Program
    {
        private static void PrintHeader()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Console.Error.WriteLine("{0} [Version {1}]",
                ((AssemblyTitleAttribute) assembly.GetCustomAttributes(
                    typeof(AssemblyTitleAttribute)).SingleOrDefault())?.Title,
                Assembly.GetExecutingAssembly().GetName().Version);
            Console.Error.WriteLine("{0}\n",
                ((AssemblyCopyrightAttribute) assembly.GetCustomAttributes(
                    typeof(AssemblyCopyrightAttribute)).SingleOrDefault())?.Copyright);
#if DEBUG
            Status.Info($"PID: {Process.GetCurrentProcess().Id}");
#endif
        }

        private static void PrintVersion()
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version);
        }

        private static int Main(string[] args)
        {
            CultureInfo ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            
#if DEBUG
            if (args.Length == 1 && args[0] == "--generate")
            {
                return InputGenerator.GenMain();
            }
#endif
            if (args.Length == 1 && args[0] == "--version")
            {
                PrintVersion();
                return Exit.Ok;
            }
            
            PrintHeader();
            
            if (!Options.ParseFromArgs(args))
            {
                return Exit.Ok;
            }

            var input = InputReader.ReadInput();

            if (Options.Seed != null)
            {
                input.Shuffle(Options.Seed.Value);
            }

            TextWriter wr = null;
            if (Options.OutputFile != null)
            {
                File.Delete(Options.OutputFile);
                Console.SetOut(wr = File.CreateText(Options.OutputFile));
            }

            try
            {
                ISolver solver = new GaSolver();

                var output = solver.Solve(input);
                output.Verify();
                OutputWriter.WriteSolution(output);
            }
            catch (WSolveException ex)
            {
                Status.Error(ex.Message);
                return Exit.Error;
            }
            catch (VerifyException ex)
            {
                Status.Error("Solution failed verification: " + ex.Message);
                return Exit.Error;
            }
            
            wr?.Close();

            return Exit.Ok;
        }
    }
}