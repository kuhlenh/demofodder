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

        // Our analyzer passes a context to the code fix provider. 
        public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            // Get the first diagnostic reported and its location
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => UseArrayEmptyAsync(context.Document, diagnosticSpan, c),
                    equivalenceKey: title),
                diagnostic);

            return Task.FromResult(false);
        }

        // This is our callback for our fix. We use the Syntax Generator
        // to language agnostically construct new nodes. We then swap
        // our new node with our old one and return a new document
        // with the fix. 
        private async Task<Document> UseArrayEmptyAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            
            // Find the array creation expression node
            var arrayCreation = root.FindToken(diagnosticSpan.Start).Parent;

            // Get the operation for the node
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var arrayOperation = (IArrayCreationExpression)semanticModel.GetOperation(arrayCreation);

            // Construct InvocationExpression for Array.Empty
            var generator = SyntaxGenerator.GetGenerator(document);
            var arrayTypeExpression = generator.TypeExpression(semanticModel.Compilation.GetTypeByMetadataName("System.Array"));
            var memberAccess = generator.MemberAccessExpression(arrayTypeExpression, generator.GenericName("Empty", arrayOperation.ElementType));
            var invocationExpression = generator.InvocationExpression(memberAccess);
            
            var newRoot = root.ReplaceNode(arrayCreation, invocationExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);

            return newDocument;
        }
    }
}