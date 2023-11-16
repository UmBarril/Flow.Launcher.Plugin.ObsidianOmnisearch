using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace Flow.Launcher.Plugin.ObsidianOmnisearch
{
    /// <summary>
    /// Interaction logic for PreviewPanel.xaml
    /// </summary>
    public partial class PreviewPanel : UserControl
    {
        public PreviewPanel(OmniResult result, int fontSize)
        {
            InitializeComponent();

            textBlock.Text = "";
            textBlock.FontSize = fontSize;

            int last = 0;
            if (result.Matches.Count > 0)
            {
                var reg = new Regex(result.Matches[0]?.Match);
                foreach (Match item in reg.Matches(result.Excerpt).Cast<Match>())
               {
                    int offset = item.Index;

                    // normal text
                    if (offset > last)
                    {
                        string normal = result.Excerpt[last..offset];
                        
                        textBlock.Inlines.Add(new Run(FormatString(normal)));
                    }

                    // highlighted text
                    {
                        string highlighted_substr = result.Excerpt[offset..(offset + item.Value.Length)];
                        var highlight = new Run(FormatString(highlighted_substr));
                        highlight.SetResourceReference(ForegroundProperty, "InlineHighlight");

                        textBlock.Inlines.Add(highlight);
                    }
                    last = offset + item.Value.Length;
                }
                // last normal text
                string text = result.Excerpt[last..];
                textBlock.Inlines.Add(new Run(FormatString(text)));
            } 
            else
            {
                textBlock.Inlines.Add(new Run(FormatString(result.Excerpt)));
            }
        }

        private static string FormatString(string str)
        {
            return HttpUtility.HtmlDecode(
                str.Replace("<br>", "\n")
                   .Replace("\t", "    ") // 1 tab = 4 spaces
                ); 
        }
    }
}
