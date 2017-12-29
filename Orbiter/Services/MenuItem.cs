using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbiter.Services
{
    public class MenuItem
    {
        public MenuItem(string title, Action action)
            : this(title, action, "")
        {
        }

        public MenuItem(string title, Action action, string voiceCommand)
        {
            this.Title = title;
            this.Execute = action;
            this.SubItems = new MenuItem[0];
            this.VoiceCommand = voiceCommand;
        }

        public MenuItem(string title, string voiceCommand, IEnumerable<MenuItem> subItems)
        {
            this.Title = title;
            this.Execute = () => { };
            this.SubItems = subItems.ToArray();
            this.VoiceCommand = voiceCommand;
        }
        
        public string Title { get; private set; }

        public string VoiceCommand { get; private set; } = string.Empty;

        public Action Execute { get; private set; }

        public MenuItem[] SubItems { get; private set; }

        public bool HasSubItems => this.SubItems.Length > 0;
    }
}
