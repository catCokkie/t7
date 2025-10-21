using Godot;

namespace SilentTestimony.UI
{
    public partial class InteractionPrompt : Label
    {
        public void ShowPrompt(string text)
        {
            Text = text;
            Visible = true;
        }

        public void HidePrompt()
        {
            Visible = false;
        }

        public override void _Ready()
        {
            Visible = false;
        }
    }
}

