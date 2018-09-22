using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace RegionsWasher
{
    internal static class ClassifierClassificationDefinition
    {
#pragma warning disable CS0649
#pragma warning disable CS0169
#pragma warning disable IDE0044

        [Export(typeof(ClassificationTypeDefinition))]
        [Name("RegionClassifier")]
        private static ClassificationTypeDefinition typeDefinition;

#pragma warning restore IDE0044
#pragma warning restore CS0169
#pragma warning restore CS0649
    }
}

