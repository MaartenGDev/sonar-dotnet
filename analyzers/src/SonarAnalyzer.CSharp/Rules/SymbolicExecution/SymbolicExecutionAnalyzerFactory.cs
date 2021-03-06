﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Rules.SymbolicExecution
{
    [Rule(EmptyNullableValueAccess.DiagnosticId, LanguageNames.CSharp)]
    [Rule(ObjectsShouldNotBeDisposedMoreThanOnce.DiagnosticId, LanguageNames.CSharp)]
    [Rule(PublicMethodArgumentsShouldBeCheckedForNull.DiagnosticId, LanguageNames.CSharp)]
    [Rule(EmptyCollectionsShouldNotBeEnumerated.DiagnosticId, LanguageNames.CSharp)]
    [Rule(ConditionEvaluatesToConstant.S2583DiagnosticId, LanguageNames.CSharp)]
    [Rule(ConditionEvaluatesToConstant.S2589DiagnosticId, LanguageNames.CSharp)]
    [Rule(InvalidCastToInterfaceRuleConstants.DiagnosticId, LanguageNames.CSharp)]
    [Rule(NullPointerDereference.DiagnosticId, LanguageNames.CSharp)]
    [Rule(RestrictDeserializedTypes.DiagnosticId, LanguageNames.CSharp)]
    [Rule(InitializationVectorShouldBeRandom.DiagnosticId, LanguageNames.CSharp)]
    [Rule(HashesShouldHaveUnpredictableSalt.DiagnosticId, LanguageNames.CSharp)]
    internal sealed class SymbolicExecutionAnalyzerFactory : IRuleFactory
    {
        private readonly ImmutableArray<ISymbolicExecutionAnalyzer> analyzers;

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

        public SymbolicExecutionAnalyzerFactory()
            : this(ImmutableArray.Create<ISymbolicExecutionAnalyzer>(
                new EmptyNullableValueAccess(),
                new ObjectsShouldNotBeDisposedMoreThanOnce(),
                new PublicMethodArgumentsShouldBeCheckedForNull(),
                new EmptyCollectionsShouldNotBeEnumerated(),
                new ConditionEvaluatesToConstant(),
                new InvalidCastToInterfaceSymbolicExecution(),
                new NullPointerDereference(),
                new RestrictDeserializedTypes(),
                new InitializationVectorShouldBeRandom(),
                new HashesShouldHaveUnpredictableSalt()))
        {
        }

        internal SymbolicExecutionAnalyzerFactory(ImmutableArray<ISymbolicExecutionAnalyzer> analyzers)
        {
            // Symbolic execution analyzers and supported diagnostics will not change at runtime so they can be safely cached.
            this.analyzers = analyzers;

            SupportedDiagnostics = analyzers.SelectMany(analyzer => analyzer.SupportedDiagnostics).ToImmutableArray();
        }

        public IEnumerable<ISymbolicExecutionAnalyzer> GetEnabledAnalyzers(SyntaxNodeAnalysisContext context) =>
            // Enabled analyzers can be changed at runtime in the IDE or by using connected mode in SonarLint and because of this
            // they cannot be cached.
            analyzers.Where(analyzer => HasEnabledDiagnostics(analyzer, context.Compilation.Options));

        private static bool HasEnabledDiagnostics(ISymbolicExecutionAnalyzer analyzer, CompilationOptions options) =>
            analyzer.SupportedDiagnostics.Any(diagnosticDescriptor => IsEnabled(options, diagnosticDescriptor));

        private static bool IsEnabled(CompilationOptions options, DiagnosticDescriptor diagnosticDescriptor) =>
            diagnosticDescriptor.GetEffectiveSeverity(options) != ReportDiagnostic.Suppress;
    }
}
