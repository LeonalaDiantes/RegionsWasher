using System.Windows.Controls;
using System.Windows.Input;

namespace RegionsWasher
{
    internal partial class SettingsPageControl : UserControl
    {
        public SettingsPageControl(Settings settings)
        {
            DataContext = settings;

            InitializeComponent();
        }
    }
}
