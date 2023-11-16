using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.ObsidianOmnisearch
{
    public class Settings : BaseModel
    {
        public int Port { get; set; } = 51361;
        public bool IsCustomPreviewActive { get; set; } = true;
        public int PreviewFontSize { get; set; } = 14;
        public bool IsFirstTimeUser { get; set; } = true;
    }
}
