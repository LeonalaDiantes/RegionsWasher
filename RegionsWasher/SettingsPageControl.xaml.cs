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

        private void OnSizePreviewTextInput(object sender, TextCompositionEventArgs args)
        {
            var textbox = (TextBox) args.Source;
            var text = textbox.Text.Insert(textbox.CaretIndex, args.Text);

            if (!int.TryParse(text, out int size))
            {
                args.Handled = true;
                return;
            }

            args.Handled = size < 1 || size > 99;
        }
    }
}
