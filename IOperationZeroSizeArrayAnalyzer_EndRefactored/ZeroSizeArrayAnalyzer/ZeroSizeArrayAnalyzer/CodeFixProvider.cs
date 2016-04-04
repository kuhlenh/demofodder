using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Semantics;
#if DEBUG
namespace System.Runtime.CompilerServices
{
    class AsyncStateMachineAttribute : StateMachineAttribute { public AsyncStateMachineAttribute(Type type) : base(type) { } }
    class IteratorStateMachineAttribute : StateMachineAttribute { public IteratorStateMachineAttribute(Type type) : base(type) { } }
}
#endif
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
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Get the first diagnostic reported and its location
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the array creation expression node
            var arrayCreation = root.FindToken(diagnosticSpan.Start).Parent;
            
            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => UseArrayEmpty(context.Document, arrayCreation, c),
                    equivalenceKey: title),
                diagnostic);
        }

        // This is our callback for our fix. We use the Syntax Generator
        // to language agnostically construct new nodes. We then swap
        // our new node with our old one and return a new document
        // with the fix. 
        private async Task<Document> UseArrayEmpty(Document document, SyntaxNode arrayCreation, CancellationToken cancellationToken)
        {
            var semanticModel = await document.GetSemanticModelAsync();
            var operation = semanticModel.GetOperation(arrayCreation);
            var arrayOperation = operation as IArrayCreationExpression;
            if (arrayOperation != null)
            {
                var generator = SyntaxGenerator.GetGenerator(document);
                var arrayTypeExpression = generator.TypeExpression(semanticModel.Compilation.GetTypeByMetadataName("System.Array"));
                var memberAccess = generator.MemberAccessExpression(arrayTypeExpression, generator.GenericName("Empty", arrayOperation.ElementType));
                var invocationExpression = generator.InvocationExpression(memberAccess);
                
                var root = await document.GetSyntaxRootAsync();
                var newRoot = root.ReplaceNode(arrayCreation, invocationExpression);
                var newDocument = document.WithSyntaxRoot(newRoot);

                return newDocument;
            }

            return document;
        }
    }
}