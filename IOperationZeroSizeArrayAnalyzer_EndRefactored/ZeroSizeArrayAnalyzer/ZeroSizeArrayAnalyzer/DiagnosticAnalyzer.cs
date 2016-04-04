using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace ZeroSizeArrayAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.VisualBasic)]
    public class ZeroSizeArrayAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ZeroSizeArrayAnalyzer";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "FX Cop Rule";

        // You can set the "severity" of your analyzer in your rule declaration.
        // Severities can be: Error, Warning, Info
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        // This method is run the first time the analyzer is instantiated.
        // It lets the analyzer declare a callback that will be run in the 
        // future when this node is seen.
        // We are also checking here to only run this analyzer if the project
        // is targeting 4.6+ (otherwise the Fixer with Array.Empty<T> doesn't
        // apply).
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(startActionContext =>
                {
                    if (startActionContext.Compilation.GetTypeByMetadataName("System.Array").GetMembers("Empty").Any())
                    {
                        startActionContext.RegisterOperationAction(AnalyzeOperation, OperationKind.ArrayCreationExpression);
                    }
                }
            );
        }

        // This is the callback. We get the operation for the array creation
        // and from the IOperation API we can see that all arrays have
        // dimension sizes. If the dimension size is 0 we have a zero-length
        // array and we report our diagnostic. 
        private void AnalyzeOperation(OperationAnalysisContext context)
        {
            var arrayCreation = (IArrayCreationExpression)context.Operation;
            var dimensions = arrayCreation.DimensionSizes[0].ConstantValue.Value;
            if (dimensions is int && (int)dimensions == 0)
            {
                var diagnostic = Diagnostic.Create(Rule, arrayCreation.Syntax.GetLocation(), arrayCreation.Syntax.ToString());
                context.ReportDiagnostic(diagnostic);

            }

            
        }
    }
}
