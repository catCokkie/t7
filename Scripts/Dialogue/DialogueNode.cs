using Godot;

namespace SilentTestimony.Dialogue
{
    [GlobalClass]
    public partial class DialogueNode : Resource
    {
        [Export] public string NodeId { get; set; } = string.Empty;
        [Export] public string Speaker { get; set; } = string.Empty;
        [Export] public string SpeakerKey { get; set; } = string.Empty;
        [Export(PropertyHint.MultilineText)] public string Text { get; set; } = string.Empty;
        [Export] public string TextKey { get; set; } = string.Empty;
        [Export] public Godot.Collections.Array<DialogueChoice> Choices { get; set; } = new();
    }
}
