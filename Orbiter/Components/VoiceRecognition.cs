using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Urho;

namespace Orbiter.Components
{
    public class VoiceRecognition : Component
    {
        private Func<Dictionary<string, Action>, Task<bool>> registerCortanaCommands;

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);

            if (this.Node != this.Scene)
                throw new InvalidOperationException("RocketFactory should be attached to the scene");
        }

        public void SetRegisterCallback(Func<Dictionary<string, Action>, Task<bool>> registerCortanaCommands)
        {
            this.registerCortanaCommands = registerCortanaCommands;
        }

        public async Task<bool> RegisterCommands(Dictionary<string, Action> commands)
        {
            return await this.registerCortanaCommands(commands);
        }
    }
}
