using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Semantics;

namespace ZeroSizeArrayAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(ZeroSizeArrayAnalyzerCodeFixProvider)), Shared]
    public class ZeroSizeArrayAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Use Array.Empty<T>";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(ZeroSizeArrayAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var diagnostic = context.Diagnostics[0];
            var span = diagnostic.Location.SourceSpan;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => UseArrayEmptyAsync(c, context.Document, span),
                    equivalenceKey: title),
                diagnostic);

            return Task.FromResult(false);
        }

        private async Task<Document> UseArrayEmptyAsync(CancellationToken cancellationToken, Document document, TextSpan diagnosticSpan)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var arrayCreation = root.FindNode(diagnosticSpan);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var arrayOperation = (IArrayCreationExpression)semanticModel.GetOperation(arrayCreation);

            // C#7 local function
            SyntaxNode GenerateReplacementTree()
            {
                var generator = SyntaxGenerator.GetGenerator(document);
                var arrayTypeExpression = generator.TypeExpression(semanticModel.Compilation.GetTypeByMetadataName("System.Array"));
                var memberName = generator.GenericName("Empty", arrayOperation.ElementType);
                var memberAccess = generator.MemberAccessExpression(arrayTypeExpression, memberName);
                var i = generator.InvocationExpression(memberAccess);
                return i;
            }

            // Construct syntax for code fix
            SyntaxNode invocationExpression = GenerateReplacementTree();

            var newRoot = root.ReplaceNode(arrayCreation, invocationExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }

    }
}