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
        public IClassifier GetClassifier(ITextBuffer buffer)
        {
            return buffer.Properties.GetOrCreateSingletonProperty(Classifier.Key, () => new Classifier(classificationRegistry, textSearchService));
        }
#pragma warning disable CS0649

        [Import] private IClassificationTypeRegistryService classificationRegistry;

        [Import] private ITextSearchService textSearchService;

#pragma warning restore CS0649
    }
}
