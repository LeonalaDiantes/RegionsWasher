using System;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Utilities;

namespace RegionsWasher
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Structured)]
    [ContentType("CSharp")]
    internal class RegionsHandler : IWpfTextViewCreationListener, ITinySubscriber<Settings>
    {
        private IOutliningManager outliningManager;
        private Settings settings;
        private IWpfTextView textView;
#pragma warning disable CS0649

        [Import(typeof(IOutliningManagerService))]
        private IOutliningManagerService outliningManagerService;

        [Import] private IClassificationFormatMapService classificationFormatMapService;

        [Import] private IClassificationTypeRegistryService classificationTypeRegistryService;

#pragma warning disable CS0649

        public void TextViewCreated(IWpfTextView wpfTextView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            textView = wpfTextView;

            settings = new Settings();
            settings.LoadFromStore();

            textView.Closed += OnClosed;
            outliningManager = outliningManagerService.GetOutliningManager(textView);

            if (settings.BehaviorMode != BehaviorMode.DoNothing)
            {
                outliningManager.RegionsChanged += OutliningManagerOnRegionsChanged;
            }

            if (settings.BehaviorMode == BehaviorMode.PreventCollapse)
            {
                outliningManager.RegionsCollapsed += OnRegionsCollapsed;
            }

            TinyMessageBroker.Instance.Subscribe(this);
        }

        private void OutliningManagerOnRegionsChanged(object sender, RegionsChangedEventArgs args)
        {
            outliningManager.RegionsChanged -= OutliningManagerOnRegionsChanged;

            var currentSnapshot = textView.TextBuffer.CurrentSnapshot;
            var span = new SnapshotSpan(currentSnapshot, 0, currentSnapshot.Length);

            if (settings.BehaviorMode == BehaviorMode.ExpandAll || settings.BehaviorMode == BehaviorMode.PreventCollapse)
            {
                outliningManager.ExpandAll(span, c => c.IsCollapsed && IsRegion(c));
            }
            else if (settings.BehaviorMode == BehaviorMode.CollapseAll)
            {
                outliningManager.CollapseAll(span, c => !c.IsCollapsed && IsRegion(c));
            }
        }

        private void OnClosed(object sender, EventArgs args)
        {
            if (outliningManager != null)
            {
                outliningManager.RegionsCollapsed -= OnRegionsCollapsed;
                outliningManager.RegionsChanged -= OutliningManagerOnRegionsChanged;
            }

            TinyMessageBroker.Instance.Unsubscribe(this);

            textView.Closed -= OnClosed;
        }

        private void OnRegionsCollapsed(object sender, RegionsCollapsedEventArgs args)
        {
            foreach (var collapsedRegion in args.CollapsedRegions.Where(IsRegion))
            {
                outliningManager.Expand(collapsedRegion);
            }
        }

        private static bool IsRegion(ICollapsible collapsible)
        {
            var text = collapsible.Extent.GetText(collapsible.Extent.TextBuffer.CurrentSnapshot);
            return text.TrimStart().ToLowerInvariant().StartsWith("#region");
        }

        public void Receive(Settings settings)
        {
            if (textView != null && !textView.IsClosed)
            {
                var classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap(textView);

                classificationFormatMap.BeginBatchUpdate();

                ClassifierForegroundFormat.UpdateTextProperties(classificationTypeRegistryService, classificationFormatMap, settings);
                ClassifierBackgroundFormat.UpdateTextProperties(classificationTypeRegistryService, classificationFormatMap, settings);
                ClassifierIsItalicFormat.UpdateTextProperties(classificationTypeRegistryService, classificationFormatMap, settings);
                ClassifierIsBoldFormat.UpdateTextProperties(classificationTypeRegistryService, classificationFormatMap, settings);
                ClassifierFontRenderingFormat.UpdateTextProperties(classificationTypeRegistryService, classificationFormatMap, settings);

                classificationFormatMap.EndBatchUpdate();
            }
        }
    }
}
