using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Xsl;
using Microsoft;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Outlining;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Threading;
using Microsoft.VisualStudio.Utilities;

namespace RegionsWasher
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Structured)]
    [ContentType("CSharp")]
    internal class RegionsHandler : IWpfTextViewCreationListener
    {
        private IOutliningManager outliningManager;
        private IWpfTextView textView;
        private Settings settings;

#pragma warning disable CS0649

        [Import(typeof(IOutliningManagerService))]
        private IOutliningManagerService outliningManagerService;

#pragma warning disable CS0649

        public void TextViewCreated(IWpfTextView wpfTextView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.textView = wpfTextView;

            settings = new Settings();
            settings.LoadFromStore();

            this.textView.Closed += OnClosed;
            outliningManager = outliningManagerService.GetOutliningManager(this.textView);

            if (settings.BehaviorMode != BehaviorMode.DoNothing)
            {
                outliningManager.RegionsChanged += OutliningManagerOnRegionsChanged;
            }

            if (settings.BehaviorMode == BehaviorMode.PreventCollapse)
            {
                outliningManager.RegionsCollapsed += OnRegionsCollapsed;
            }            
        }

        private void OutliningManagerOnRegionsChanged(object sender, RegionsChangedEventArgs args)
        {
            outliningManager.RegionsChanged -= OutliningManagerOnRegionsChanged;

            var currentSnapshot = this.textView.TextBuffer.CurrentSnapshot;
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
            }

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
            string text = collapsible.Extent.GetText(collapsible.Extent.TextBuffer.CurrentSnapshot);
            return text.TrimStart().ToLowerInvariant().StartsWith("#region");
        }
    }
}
