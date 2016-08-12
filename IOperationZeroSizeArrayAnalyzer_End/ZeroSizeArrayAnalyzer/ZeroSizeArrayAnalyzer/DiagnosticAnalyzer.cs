using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
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
        private const string Category = "FxCop Rule";

        // You can set the "severity" of your analyzer in your rule declaration.
        // Severities can be: Error, Warning, Info
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        // This method is run the first time the analyzer is instantiated.
        // It lets the analyzer declare a callback that will be run in the 
        // future when this node is seen.
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(startContext =>
            {
                var arrayType = startContext.Compilation.GetTypeByMetadataName("System.Array");
                if(arrayType.GetMembers("Empty").Length > 0)
                {
                    startContext.RegisterOperationAction(AnalyzeArray, OperationKind.ArrayCreationExpression);
                }
            });
        }

        private void AnalyzeArray(OperationAnalysisContext context)
        {
            var arrayExpression = (IArrayCreationExpression)context.Operation;

            if (arrayExpression.DimensionSizes.Length == 1 
                && arrayExpression.DimensionSizes[0].ConstantValue.HasValue)
            {
                var dim = arrayExpression.DimensionSizes[0].ConstantValue.Value;

                if (dim is 0)
                {
                    var diagnostic = Diagnostic.Create(Rule, arrayExpression.Syntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}
