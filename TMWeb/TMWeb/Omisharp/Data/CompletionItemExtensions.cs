using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Text;
using OmniSharp.Models.v1.Completion;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CompletionItem = Microsoft.CodeAnalysis.Completion.CompletionItem;

namespace TMWeb.Omisharp.Data
{
    internal static class CompletionItemExtensions
    {
        private const string GetSymbolsAsync = nameof(GetSymbolsAsync);
        private const string InsertionText = nameof(InsertionText);
        private const string ObjectCreationCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.ObjectCreationCompletionProvider";
        private const string NamedParameterCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.NamedParameterCompletionProvider";
        internal const string OverrideCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.OverrideCompletionProvider";
        internal const string PartialMethodCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.PartialMethodCompletionProvider";
        internal const string InternalsVisibleToCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.InternalsVisibleToCompletionProvider";
        internal const string XmlDocCommentCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.XmlDocCommentCompletionProvider";
        internal const string TypeImportCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.TypeImportCompletionProvider";
        internal const string ExtensionMethodImportCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.ExtensionMethodImportCompletionProvider";
        internal const string EmeddedLanguageCompletionProvider = "Microsoft.CodeAnalysis.CSharp.Completion.Providers.EmbeddedLanguageCompletionProvider";
        private const string ProviderName = nameof(ProviderName);
        private const string SymbolCompletionItem = "Microsoft.CodeAnalysis.Completion.Providers.SymbolCompletionItem";
        private const string SymbolKind = nameof(SymbolKind);
        private const string SymbolName = nameof(SymbolName);
        private const string Symbols = nameof(Symbols);
        private static readonly Type _symbolCompletionItemType;
        private static readonly MethodInfo _getSymbolsAsync;
        private static readonly PropertyInfo _getProviderName;
        private static readonly MethodInfo _getCompletionsInternalAsync;
        private static readonly MethodInfo _getChangeAsync;
        internal static readonly PerLanguageOption<bool?> ShowItemsFromUnimportedNamespaces = new PerLanguageOption<bool?>("CompletionOptions", "ShowItemsFromUnimportedNamespaces", defaultValue: null);

        static CompletionItemExtensions()
        {
            _symbolCompletionItemType = typeof(CompletionItem).GetTypeInfo().Assembly.GetType(SymbolCompletionItem);
            _getSymbolsAsync = _symbolCompletionItemType.GetMethod(GetSymbolsAsync, BindingFlags.Public | BindingFlags.Static);

            _getProviderName = typeof(CompletionItem).GetProperty(ProviderName, BindingFlags.NonPublic | BindingFlags.Instance);

            _getCompletionsInternalAsync = typeof(CompletionService).GetMethod(nameof(GetCompletionsInternalAsync), BindingFlags.NonPublic | BindingFlags.Instance);
            _getChangeAsync = typeof(CompletionService).GetMethod(nameof(GetChangeAsync), BindingFlags.NonPublic | BindingFlags.Instance);
        }

        internal static string GetProviderName(this CompletionItem item) => (string)_getProviderName.GetValue(item);

        public static bool IsObjectCreationCompletionItem(this CompletionItem item) => item.GetProviderName() == ObjectCreationCompletionProvider;

        public static Task<(CompletionList completionList, bool expandItemsAvailable)> GetCompletionsInternalAsync(
            this CompletionService completionService,
            Document document,
            int caretPosition,
            CompletionTrigger trigger = default,
            ImmutableHashSet<string> roles = null,
            OptionSet options = null,
            CancellationToken cancellationToken = default)
            => (Task<(CompletionList completionList, bool expandItemsAvailable)>)_getCompletionsInternalAsync.Invoke(completionService, new object[] { document, caretPosition, trigger, roles, options, cancellationToken });

        internal static Task<CompletionChange> GetChangeAsync(
            this CompletionService completionService,
            Document document,
            CompletionItem item,
            TextSpan completionListSpan,
            char? commitCharacter = null,
            bool disallowAddingImports = false,
            CancellationToken cancellationToken = default)
            => (Task<CompletionChange>)_getChangeAsync.Invoke(completionService, new object[] { document, item, completionListSpan, commitCharacter, disallowAddingImports, cancellationToken });

        public static bool TryGetInsertionText(this CompletionItem completionItem, out string insertionText) => completionItem.Properties.TryGetValue(InsertionText, out insertionText);
    }
}
