using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;

namespace RegionsWasher
{
    internal class Classifier : IClassifier
    {
        private readonly IClassificationType classificationType;
        private readonly ITextSearchService textSearchService;

        internal Classifier(IClassificationTypeRegistryService registry, ITextSearchService textSearchService)
        {
            this.textSearchService = textSearchService;
            classificationType = registry.GetClassificationType("RegionClassifier");
        }

#pragma warning disable CS0067

        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning disable CS0067

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var findData = new FindData(@"(#region((\s+\S+)+)?)|(#endregion)", span.Snapshot)
            {
                FindOptions = FindOptions.UseRegularExpressions
            };

            textSearchService.FindAll(findData);

            var result = textSearchService.FindAll(findData).Select(sp => new ClassificationSpan(sp, classificationType)).ToList();
            return result;
        }
    }
}
