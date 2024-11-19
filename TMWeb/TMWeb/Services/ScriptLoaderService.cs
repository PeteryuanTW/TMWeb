using DevExpress.Printing.ExportHelpers;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text;
using TMWeb.Scripts.Template;
using Microsoft.JSInterop;
using TMWeb.Data.Script;
using OmniSharp.Models;
using OmniSharp.Options;
using OmniSharp.Models.v1.Completion;
using TMWeb.Omisharp.Service;
using OmniSharp.Models.SignatureHelp;

namespace TMWeb.Services
{
    public class ScriptLoaderService
    {
        RoslynProject completionProject;
        RoslynProject diagnosticProject;

        OmniSharpCompletionService completionService;
        OmniSharpSignatureHelpService signatureService;
        OmniSharpQuickInfoProvider quickInfoProvider;

        public ScriptLoaderService(NavigationManager navigationManager)
        {
            completionProject = new();
            diagnosticProject = new();

            var formattingOptions = new FormattingOptions();

            completionService = new(completionProject.Workspace, formattingOptions);
            quickInfoProvider = new(completionProject.Workspace, formattingOptions);
        }

        Dictionary<string, MetadataReference> MetadataReferenceCache = new Dictionary<string, MetadataReference>();
        private async Task<MetadataReference?> GetAssemblyMetadataReference(Assembly assembly)
        {
            MetadataReference? ret = null;
            var assemblyName = assembly.GetName().Name;
            if (MetadataReferenceCache.TryGetValue(assemblyName, out ret))
            {
                //Console.WriteLine($"O {assemblyName} already exist");
                return ret;
            }

            //var assemblyPath = $"./bin/Debug/net8.0/{assemblyName}.dll";
            //var a = AppDomain.CurrentDomain.GetAssemblies();
            //var assemblyUrl = $"./_framework/{assemblyName}.dll";

            try
            {
                ret = MetadataReference.CreateFromFile(assembly.Location);
                //var tmp = await _httpClient.GetAsync(assemblyUrl);
                //if (tmp.IsSuccessStatusCode)
                //{
                //    var bytes = await tmp.Content.ReadAsByteArrayAsync();
                //    ret = MetadataReference.CreateFromImage(bytes);
                //    //Console.WriteLine($"metadataReference loaded: {assembly} success");
                //Console.WriteLine($"O {assemblyName} found.");
                //}
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"metadataReference not loaded: {assembly} {ex.Message}");
            }
            if (ret == null)
            {
                //throw new Exception($"{assemblyName} ReferenceMetadata not found. If using .Net 8, <WasmEnableWebcil>false</WasmEnableWebcil> must be set in the project .csproj file.");
                //Console.WriteLine($"X {assemblyName} not found.");
            }
            MetadataReferenceCache[assemblyName] = ret;
            return ret;
        }

        public async Task<Assembly> CompileToDLLAssembly(string sourceCode, string assemblyName = "", bool release = true, SourceCodeKind sourceCodeKind = SourceCodeKind.Regular)
        {
            if (string.IsNullOrEmpty(assemblyName)) assemblyName = Path.GetRandomFileName();
            var codeString = SourceText.From(sourceCode);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithKind(sourceCodeKind);
            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            //List<Assembly> appAssemblies = Assembly.GetEntryAssembly()!.GetReferencedAssemblies().Select(o => Assembly.Load(o)).ToList();
            List<Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            //List<Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => Assembly.Load($"{x.GetName()}.dll")).ToList();
            appAssemblies.Add(typeof(object).Assembly);
            //var assemblyPath = Path.GetDirectoryName(typeof(ScriptBaseClass).Assembly.Location);
            //Console.WriteLine(assemblyPath);
            //appAssemblies.Add(typeof(ScriptBaseClass).Assembly);
            var references = new List<MetadataReference>();
            List<string> loaded = new();
            List<string> notloaded = new();
            foreach (var assembly in appAssemblies)
            {
                var metadataReference = await GetAssemblyMetadataReference(assembly);
                if (metadataReference != null)
                {
                    references.Add(metadataReference);
                    loaded.Add(assembly.FullName);
                }
                else
                {
                    notloaded.Add(assembly.FullName);
                }
            }
            CSharpCompilation compilation;
            if (sourceCodeKind == SourceCodeKind.Script)
            {
                compilation = CSharpCompilation.CreateScriptCompilation(
                assemblyName,
                syntaxTree: parsedSyntaxTree,
                references: references,
                options: new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        concurrentBuild: false,
                        optimizationLevel: release ? OptimizationLevel.Release : OptimizationLevel.Debug
                    )
                );
            }
            else
            {
                compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        concurrentBuild: false,
                        optimizationLevel: release ? OptimizationLevel.Release : OptimizationLevel.Debug
                    )
                );
            }
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                if (!result.Success)
                {
                    var errors = new StringBuilder();
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                    foreach (Diagnostic diagnostic in failures)
                    {
                        var startLinePos = diagnostic.Location.GetLineSpan().StartLinePosition;
                        var err = $"({startLinePos.Line+1}, {startLinePos.Character}) {diagnostic.GetMessage()}";
                        errors.AppendLine(err);
                        Console.Error.WriteLine(err);
                    }
                    throw new Exception(errors.ToString());
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    var assembly = Assembly.Load(ms.ToArray());
                    return assembly;
                }
            }
        }


        #region editor

        [JSInvokable]
        public async Task<CompletionResponse> GetCompletionAsync(string code, CompletionRequest completionRequest)
        {
            Solution updatedSolution;

            do
            {
                updatedSolution = completionProject.Workspace.CurrentSolution.WithDocumentText(completionProject.DocumentId, SourceText.From(code));
            } while (!completionProject.Workspace.TryApplyChanges(updatedSolution));

            var document = updatedSolution.GetDocument(completionProject.DocumentId);
            var completionResponse = await completionService.Handle(completionRequest, document);

            return completionResponse;
        }

        [JSInvokable]
        public async Task<CompletionResolveResponse> GetCompletionResolveAsync(CompletionResolveRequest completionResolveRequest)
        {
            var document = completionProject.Workspace.CurrentSolution.GetDocument(completionProject.DocumentId);
            var completionResponse = await completionService.Handle(completionResolveRequest, document);

            return completionResponse;
        }

        [JSInvokable]
        public async Task<SignatureHelpResponse> GetSignatureHelpAsync(string code, SignatureHelpRequest signatureHelpRequest)
        {
            Solution updatedSolution;

            do
            {
                updatedSolution = completionProject.Workspace.CurrentSolution.WithDocumentText(completionProject.DocumentId, SourceText.From(code));
            } while (!completionProject.Workspace.TryApplyChanges(updatedSolution));

            var document = updatedSolution.GetDocument(completionProject.DocumentId);
            var signatureHelpResponse = await signatureService.Handle(signatureHelpRequest, document);

            return signatureHelpResponse;
        }



        [JSInvokable]
        public async Task<QuickInfoResponse> GetQuickInfoAsync(QuickInfoRequest quickInfoRequest)
        {
            var document = diagnosticProject.Workspace.CurrentSolution.GetDocument(diagnosticProject.DocumentId);
            var quickInfoResponse = await quickInfoProvider.Handle(quickInfoRequest, document);

            return quickInfoResponse;
        }


        public async Task<IEnumerable<ScriptDiagnosis>> GetDiagnosticsAsync(string code, string assemblyName = "", bool release = true, SourceCodeKind sourceCodeKind = SourceCodeKind.Regular)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = Path.GetRandomFileName();
            }
            Solution updatedSolution;

            do
            {
                updatedSolution = diagnosticProject.Workspace.CurrentSolution.WithDocumentText(diagnosticProject.DocumentId, SourceText.From(code));
            } while (!diagnosticProject.Workspace.TryApplyChanges(updatedSolution));
            var codeString = SourceText.From(code);
            var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest).WithKind(sourceCodeKind);
            var parsedSyntaxTree = SyntaxFactory.ParseSyntaxTree(codeString, options);

            //List<Assembly> appAssemblies = Assembly.GetEntryAssembly()!.GetReferencedAssemblies().Select(o => Assembly.Load(o)).ToList();
            List<Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            //List<Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => Assembly.Load($"{x.GetName()}.dll")).ToList();
            appAssemblies.Add(typeof(object).Assembly);
            var assemblyPath = Path.GetDirectoryName(typeof(ScriptBaseClass).Assembly.Location);
            //Console.WriteLine(assemblyPath);
            //appAssemblies.Add(typeof(ScriptBaseClass).Assembly);
            var references = new List<MetadataReference>();
            List<string> loaded = new();
            List<string> notloaded = new();
            foreach (var assembly in appAssemblies)
            {
                var metadataReference = await GetAssemblyMetadataReference(assembly);
                if (metadataReference != null)
                {
                    references.Add(metadataReference);
                    loaded.Add(assembly.FullName);
                }
                else
                {
                    notloaded.Add(assembly.FullName);
                }
            }
            CSharpCompilation compilation;
            if (sourceCodeKind == SourceCodeKind.Script)
            {
                compilation = CSharpCompilation.CreateScriptCompilation(
                assemblyName,
                syntaxTree: parsedSyntaxTree,
                references: references,
                options: new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        concurrentBuild: false,
                        optimizationLevel: release ? OptimizationLevel.Release : OptimizationLevel.Debug
                    )
                );
            }
            else
            {
                compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { parsedSyntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                        OutputKind.DynamicallyLinkedLibrary,
                        concurrentBuild: false,
                        optimizationLevel: release ? OptimizationLevel.Release : OptimizationLevel.Debug
                    )
                );
            }
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                IEnumerable<Diagnostic> failures = result.Diagnostics;//.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);
                return failures.Select(x=>new ScriptDiagnosis(x));
            }
        }

        public async Task<List<ScriptDiagnosis>> GetMyDiagnosticsAsync(string code)
        {
            Solution updatedSolution;

            do
            {
                updatedSolution = diagnosticProject.Workspace.CurrentSolution.WithDocumentText(diagnosticProject.DocumentId, SourceText.From(code));
            } while (!diagnosticProject.Workspace.TryApplyChanges(updatedSolution));

            var compilation = await updatedSolution.Projects.First().GetCompilationAsync();
            var dotnetDiagnostics = compilation.GetDiagnostics();

            var diagnostics = dotnetDiagnostics.Select(current =>
            {
                var lineSpan = current.Location.GetLineSpan();

                return new ScriptDiagnosis()
                {
                    Start = lineSpan.StartLinePosition,
                    End = lineSpan.EndLinePosition,
                    Message = current.GetMessage(),
                    Severity = (int)current.Severity
                };
            }).ToList();

            return diagnostics;
        }





        #endregion
    }
}
