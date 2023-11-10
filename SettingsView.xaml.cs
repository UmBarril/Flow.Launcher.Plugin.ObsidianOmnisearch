using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Flow.Launcher.Plugin.ObsidianOmnisearch.View
{
    /// <summary>
    /// Interaction logic for OmniSettings.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        // https://stackoverflow.com/questions/3886163/wpf-databinding-textbox-to-integer-property-in-another-object
        public SettingsView(Settings settings)
        {
            InitializeComponent();
            DataContext = settings;
        }
    }
}
