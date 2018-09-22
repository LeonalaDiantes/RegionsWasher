using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RegionsWasher
{
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "RegionClassifier")]
    [Name("RegionClassifier")]
    [UserVisible(true)]
    [Order(After = Priority.High)] 
    internal sealed class ClassifierFormat : ClassificationFormatDefinition
    {
        public ClassifierFormat()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DisplayName = "Region";

            var settings = new Settings();
            settings.LoadFromStore();

            this.ForegroundColor = settings.Foreground;
            this.ForegroundOpacity = 1;
            this.BackgroundColor = settings.Background;
            this.BackgroundOpacity = 1;
            this.IsItalic = settings.IsItalic;
            this.IsBold = settings.IsBold;
            this.FontRenderingSize = settings.Size;
        }
    }
}
