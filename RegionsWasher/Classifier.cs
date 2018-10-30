using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;

namespace RegionsWasher
{
    internal class Classifier : IClassifier
    {
        public const string Key = "Region";
        //private readonly IClassificationType classificationType;
        private readonly IClassificationTypeRegistryService registryService;
        private readonly ITextSearchService textSearchService;

        internal Classifier(IClassificationTypeRegistryService registryService, ITextSearchService textSearchService)
        {
            this.registryService = registryService;
            this.textSearchService = textSearchService;
            //classificationType = registry.GetClassificationType("RegionClassifier");
        }
#pragma warning disable CS0067

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning disable CS0067

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var findData = new FindData(@"(#region((\s+\S+)+)?)|(#endregion((\s+\S+)+)?)", span.Snapshot)
            {
                FindOptions = FindOptions.UseRegularExpressions
            };

            var snapshotSpans = textSearchService.FindAll(findData).ToArray();

            var classificationForegroundType = registryService.GetClassificationType(ClassifierForegroundFormat.ClassifierKey);
            var classificationBackgroundType = registryService.GetClassificationType(ClassifierBackgroundFormat.ClassifierKey);
            var classificationIsItalicType = registryService.GetClassificationType(ClassifierIsItalicFormat.ClassifierKey);
            var classificationIsBoldType = registryService.GetClassificationType(ClassifierIsBoldFormat.ClassifierKey);
            var classificationFontRenderingType = registryService.GetClassificationType(ClassifierFontRenderingFormat.ClassifierKey);

            var result = snapshotSpans.Select(sp => new ClassificationSpan(sp, classificationForegroundType))
                .Union(snapshotSpans.Select(sp => new ClassificationSpan(sp, classificationBackgroundType)))
                .Union(snapshotSpans.Select(sp => new ClassificationSpan(sp, classificationIsItalicType)))
                .Union(snapshotSpans.Select(sp => new ClassificationSpan(sp, classificationIsBoldType)))
                .Union(snapshotSpans.Select(sp => new ClassificationSpan(sp, classificationIsBoldType)))
                .Union(snapshotSpans.Select(sp => new ClassificationSpan(sp, classificationFontRenderingType)))
                .ToList();

            return result;
        }
    }
}
