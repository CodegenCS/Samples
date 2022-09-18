using CodegenCS;
using CodegenCS.DbSchema;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SampleSourceGenerator
{
    [Generator]
    public class MyGenerator : ISourceGenerator
    {
        #region Errors/Warnings
        private static readonly DiagnosticDescriptor InvalidJsonSchemaWarning =
            new DiagnosticDescriptor(id: "MYSOURCEGEN001",
                                    title: "Couldn't parse JSON schema file",
                                    messageFormat: "Couldn't parse JSON schema file '{0}'",
                                    category: "MyXmlGenerator",
                                    DiagnosticSeverity.Warning,
                                    isEnabledByDefault: true);
        #endregion

        public void Initialize(GeneratorInitializationContext context)
        {
            // In Roslyn Analyzers or Source Generators you usually do this (but for this Sample you don't need it)
            // Registers a syntax receiver that will "visit" all nodes in the compilation,
            // and can "decide" which classes/methods/etc will be used (augmented) with code generation
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debug.WriteLine("MyGenerator: Execute");
            MySyntaxReceiver syntaxReceiver = context.SyntaxReceiver as MySyntaxReceiver;
            if (syntaxReceiver == null)
                return;

            // Add auto-generated sources to the compilation output (adds in-memory, doesn't save to disk!)
            GeneratePOCOs(context);
        }

        #region GeneratePOCOs
        internal void GeneratePOCOs(GeneratorExecutionContext context)
        {
            IEnumerable<AdditionalText> schemaFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith("Schema.json", StringComparison.OrdinalIgnoreCase));
            foreach (AdditionalText schemaFile in schemaFiles)
            {
                var ctx = new CodegenContext(defaultOutputFileName: Path.GetFileNameWithoutExtension(schemaFile.Path) + ".generated.cs");

                var options = new SimplePOCOGenerator.SimplePOCOGeneratorOptions()
                { 
                    Namespace = "MyProject.POCOs",
                    SingleFile = true
                };
                var generator = new SimplePOCOGenerator(options);

                DatabaseSchema model;
                try
                {
                    model = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText(schemaFile.Path));
                }
                catch (JsonSerializationException)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidJsonSchemaWarning, Location.None, schemaFile.Path));
                    continue;
                }

                // This runs in-memory...
                generator.Render(ctx, model);

                // Source Generators were made to add sources to the compilation output directly in-memory:
                foreach (var file in ctx.OutputFiles)
                    context.AddSource($"{Path.GetFileName(file.RelativePath)}", SourceText.From(file.GetContents(), Encoding.UTF8));

                // What if I want the Source Generator to save files to disk?
                // - If you save to the same project this triggers a loop in the analyzer because new file triggers analyzer which adds new file, etc.
                // - If you save into a different folder (out of the project) then it should be fine - use "ctx.SaveFiles(someOtherFolder)"
            }
        }
        #endregion


    }
}
