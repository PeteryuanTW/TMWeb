using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;

namespace TMWeb.Omisharp.Service
{
    public class RoslynProject
    {
        public RoslynProject()
        {
            var host = MefHostServices.Create(MefHostServices.DefaultAssemblies);

            // workspace
            Workspace = new AdhocWorkspace(host);

            // project
            var filePath = typeof(object).Assembly.Location;
            var documentationProvider = XmlDocumentationProvider.CreateFromFile(@"./Resource/System.Runtime.xml");

            List<Assembly> appAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var references = new List<MetadataReference>();

            foreach (var assembly in appAssemblies)
            {
                MetadataReference? ret = null;
                var assemblyName = assembly.GetName().Name;

                try
                {
                    ret = MetadataReference.CreateFromFile(assembly.Location);
                    references.Add(ret);
                }
                catch (Exception ex)
                {

                }
            }


            var projectInfo = ProjectInfo
                .Create(ProjectId.CreateNewId(), VersionStamp.Create(), "OneDas", "OneDas", LanguageNames.CSharp)
                .WithMetadataReferences(references)// new[] { MetadataReference.CreateFromFile(filePath, documentation: documentationProvider) }
                .WithCompilationOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var project = Workspace.AddProject(projectInfo);
            // code
            UseOnlyOnceDocument = Workspace.AddDocument(project.Id, "Code.cs", SourceText.From(string.Empty));
            DocumentId = UseOnlyOnceDocument.Id;
        }

        public AdhocWorkspace Workspace { get; init; }

        public Document UseOnlyOnceDocument { get; init; }

        public DocumentId DocumentId { get; init; }
    }
}
