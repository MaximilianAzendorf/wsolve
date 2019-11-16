using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using WSolve.ExtraConditions.StatelessAccess;

namespace WSolve.ExtraConditions
{
    public static class ExtraConditionsCompiler
    {
        private static readonly string CodeEnvPlaceholder = "##C";
        private static readonly string CodeEnvExtraPlaceholder = "##E";
        private static readonly string CodeEnvStatelessPlaceholder = "##S";

        private static readonly string CodeEnvStatelessPostfix =
            nameof(WorkshopAccessorStateless).Substring(nameof(WorkshopAccessor).Length);
        
        private static readonly string CodeEnvClassName = "WSolve.Generated.ExtraConditions";
        private static readonly int CodeEnvPlaceholderLineOffset;

        private static readonly string CodeEnvironment;

        private static readonly string CodeEnvironmentResourceName =
            "WSolve.Resources.ExtraConditionsCodeEnvironment.txt";

        static ExtraConditionsCompiler()
        {
            using (var reader = new StreamReader(
                typeof(ExtraConditionsBase).Assembly.GetManifestResourceStream(
                    CodeEnvironmentResourceName) ?? throw new InvalidOperationException()))
            {
                CodeEnvironment = reader.ReadToEnd();
                CodeEnvPlaceholderLineOffset =
                    CodeEnvironment.Split('\n').ToList().FindIndex(l => l.Contains(CodeEnvPlaceholder));
            }
        }

        public static string GenerateExtraDefinitions(InputData data)
        {
            var extraDefinitions = new StringBuilder();

            (int s, int w, int p) ignored = (0, 0, 0);
            (int s, int w, int p) conflicts = (0, 0, 0);
            int total = 0;

            var usedNames = new HashSet<string>();

            foreach (string s in data.Slots)
            {
                string name = s.Split(' ')[0];
                if (!IsValidIdentifier(name))
                {
                    ignored.s++;
                }
                else if (usedNames.Contains(name))
                {
                    conflicts.s++;
                }
                else
                {
                    extraDefinitions.AppendLine($"private Slot {name} => Slot(\"{s}\");");
                    usedNames.Add(name);
                    total++;
                }
            }

            foreach (string w in data.Workshops.Select(ws => ws.name))
            {
                string name = w.Split(' ')[0];
                if (!IsValidIdentifier(name))
                {
                    ignored.p++;
                }
                else if (usedNames.Contains(name))
                {
                    conflicts.p++;
                }
                else
                {
                    extraDefinitions.AppendLine($"private Workshop {name} => Workshop(\"{w}\");");
                    usedNames.Add(name);
                    total++;
                }
            }

            foreach (string p in data.Participants.Select(p => p.name))
            {
                string name = p.Split(' ')[0];
                if (!IsValidIdentifier(name))
                {
                    ignored.w++;
                }
                else if (usedNames.Contains(name))
                {
                    conflicts.w++;
                }
                else
                {
                    extraDefinitions.AppendLine($"private Participant {name} => Participant(\"{p}\");");
                    usedNames.Add(name);
                    total++;
                }
            }

            if (ignored != (0, 0, 0))
            {
                Status.Warning(
                    $"{ignored.s} slot, {ignored.w} workshop and {ignored.p} participant identifier(s) were ignored.");
            }

            if (conflicts != (0, 0, 0))
            {
                Status.Warning(
                    $"{conflicts.s} slot, {ignored.w} workshop and {ignored.p} participant identifier(s) were omitted due to name conflics.");
            }

            Status.Info($"{total} identifier(s) were generated.");

            return extraDefinitions.ToString();
        }

        public static Func<Chromosome, bool> Compile(string conditionCode, InputData data, bool stateless)
        {
            conditionCode = CodeEnvironment
                .Replace(CodeEnvPlaceholder, conditionCode)
                .Replace(CodeEnvExtraPlaceholder, GenerateExtraDefinitions(data))
                .Replace(CodeEnvStatelessPlaceholder, stateless ? CodeEnvStatelessPostfix : "");

            CSharpCompilation comp = GenerateCode(conditionCode);
            using (var s = new MemoryStream())
            {
                EmitResult compRes = comp.Emit(s);
                if (!compRes.Success)
                {
                    List<Diagnostic> compilationErrors = compRes.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error)
                        .ToList();

                    Diagnostic firstError = compilationErrors.First();
                    string errorDescription = firstError.GetMessage();
                    int errorLine = firstError.Location.GetLineSpan().StartLinePosition.Line -
                                    CodeEnvPlaceholderLineOffset + 1;
                    string firstErrorMessage = $"{errorDescription} (Line {errorLine})";

                    throw new WSolveException("Could not compile extra conditions: " + firstErrorMessage);
                }

                s.Seek(0, SeekOrigin.Begin);
                Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(s);
                Type type = assembly.GetType(CodeEnvClassName);

                return chromosome =>
                    ((ExtraConditionsBase) Activator.CreateInstance(type, chromosome)).DirectResult;
            }
        }

        private static CSharpCompilation GenerateCode(string sourceCode)
        {
            SourceText codeString = SourceText.From(sourceCode);
            CSharpParseOptions options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);

            SyntaxTree parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);
            string dotNetCoreDir = Path.GetDirectoryName(typeof(object).GetTypeInfo().Assembly.Location);

            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(Path.Combine(dotNetCoreDir, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(typeof(Math).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ExtraConditionsBase).Assembly.Location)
            };

            return CSharpCompilation.Create(
                "Generated.dll",
                new[] {parsedSyntaxTree},
                references,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release,
                    assemblyIdentityComparer: DesktopAssemblyIdentityComparer.Default));
        }

        private static bool IsValidIdentifier(string name)
        {
            return name != "Slot" && name != "Workshop" && name != "Participant" && SyntaxFacts.IsValidIdentifier(name);
        }
    }
}