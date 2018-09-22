using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;

namespace RegionsWasher
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("code")] 
    internal class ClassifierProvider : IClassifierProvider
    {
#pragma warning disable CS0649

        [Import]
        private IClassificationTypeRegistryService classificationRegistry;

        [Import]
        private ITextSearchService textSearchService;

#pragma warning restore CS0649

        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(() => new Classifier(this.classificationRegistry, this.textSearchService));
        }
    }
}
