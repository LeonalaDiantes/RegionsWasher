using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RegionsWasher
{
    // ToDo simplify these multiple declarations

    internal static class ClassifierClassificationDefinition
    {
#pragma warning disable CS0649, CS0169, IDE0044

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassifierForegroundFormat.ClassifierKey)]
        private static ClassificationTypeDefinition typeDefinitionForeground;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassifierBackgroundFormat.ClassifierKey)]
        private static ClassificationTypeDefinition typeDefinitionBackground;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassifierIsItalicFormat.ClassifierKey)]
        private static ClassificationTypeDefinition typeDefinitionIsItalic;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassifierIsBoldFormat.ClassifierKey)]
        private static ClassificationTypeDefinition typeDefinitionIsBold;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(ClassifierFontRenderingFormat.ClassifierKey)]
        private static ClassificationTypeDefinition typeDefinitionFontRendering;

#pragma warning restore CS0649, CS0169, IDE0044       
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierKey)]
    [Name(ClassifierKey)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class ClassifierForegroundFormat : ClassificationFormatDefinition
    {
        public const string ClassifierKey = "ClassifierForegroundFormat";

        public ClassifierForegroundFormat()
        {
            DisplayName = ClassifierKey;

            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = new Settings();
            settings.LoadFromStore();

            ForegroundColor = settings.Foreground;
        }

        public static void UpdateTextProperties(IClassificationTypeRegistryService registryService, IClassificationFormatMap formatMap, Settings settings)
        {
            var classificationType = registryService.GetClassificationType(ClassifierKey);
            var textProperties = formatMap.GetTextProperties(classificationType);

            if (settings.Foreground.HasValue)
            {
                formatMap.SetTextProperties(classificationType, textProperties.SetForeground(settings.Foreground.Value));                
            }
            else
            {
                formatMap.SetTextProperties(classificationType, textProperties.ClearForegroundBrush());
            }
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierKey)]
    [Name(ClassifierKey)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class ClassifierBackgroundFormat : ClassificationFormatDefinition
    {
        public const string ClassifierKey = "ClassifierBackgroundFormat";

        public ClassifierBackgroundFormat()
        {
            DisplayName = ClassifierKey;

            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = new Settings();
            settings.LoadFromStore();

            BackgroundColor = settings.Background;
        }

        public static void UpdateTextProperties(IClassificationTypeRegistryService registryService, IClassificationFormatMap formatMap, Settings settings)
        {
            var classificationType = registryService.GetClassificationType(ClassifierKey);
            var textProperties = formatMap.GetTextProperties(classificationType);
            if (settings.Background.HasValue)
            {
                formatMap.SetTextProperties(classificationType, textProperties.SetBackground(settings.Background.Value));
            }
            else
            {
                formatMap.SetTextProperties(classificationType, textProperties.ClearBackgroundBrush());
            }
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierKey)]
    [Name(ClassifierKey)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class ClassifierIsItalicFormat : ClassificationFormatDefinition
    {
        public const string ClassifierKey = "ClassifierIsItalicFormat";

        public ClassifierIsItalicFormat()
        {
            DisplayName = ClassifierKey;

            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = new Settings();
            settings.LoadFromStore();

            IsItalic = settings.IsItalic;
        }

        public static void UpdateTextProperties(IClassificationTypeRegistryService registryService, IClassificationFormatMap formatMap, Settings settings)
        {
            var classificationType = registryService.GetClassificationType(ClassifierKey);
            var textProperties = formatMap.GetTextProperties(classificationType);
            formatMap.SetTextProperties(classificationType, textProperties.SetItalic(settings.IsItalic));
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierKey)]
    [Name(ClassifierKey)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class ClassifierIsBoldFormat : ClassificationFormatDefinition
    {
        public const string ClassifierKey = "ClassifierIsBoldFormat";

        public ClassifierIsBoldFormat()
        {
            DisplayName = ClassifierKey;

            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = new Settings();
            settings.LoadFromStore();

            IsBold = settings.IsBold;
        }

        public static void UpdateTextProperties(IClassificationTypeRegistryService registryService, IClassificationFormatMap formatMap, Settings settings)
        {
            var classificationType = registryService.GetClassificationType(ClassifierKey);
            var textProperties = formatMap.GetTextProperties(classificationType);
            formatMap.SetTextProperties(classificationType, textProperties.SetBold(settings.IsBold));
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierKey)]
    [Name(ClassifierKey)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class ClassifierFontRenderingFormat : ClassificationFormatDefinition
    {
        public const string ClassifierKey = "ClassifierFontRenderingFormat";

        public ClassifierFontRenderingFormat()
        {
            DisplayName = ClassifierKey;

            ThreadHelper.ThrowIfNotOnUIThread();

            var settings = new Settings();
            settings.LoadFromStore();
            if (settings.Size.HasValue && !double.IsNaN(settings.Size.Value))
            {
                FontRenderingSize = Math.Min(99.0, Math.Max(1.0, settings.Size.Value));
            }
        }

        public static void UpdateTextProperties(IClassificationTypeRegistryService registryService, IClassificationFormatMap formatMap, Settings settings)
        {
            var classificationType = registryService.GetClassificationType(ClassifierKey);
            var textProperties = formatMap.GetTextProperties(classificationType);

            if (settings.Size.HasValue)
            {
                formatMap.SetTextProperties(classificationType, textProperties.SetFontRenderingEmSize(settings.Size.Value));
            }
            else
            {
                formatMap.SetTextProperties(classificationType, textProperties.ClearFontRenderingEmSize());
            }
        }
    }
}
