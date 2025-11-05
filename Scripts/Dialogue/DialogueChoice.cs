using Godot;

namespace SilentTestimony.Dialogue
{
    [GlobalClass]
    public partial class DialogueChoice : Resource
    {
        [Export] public string Text { get; set; } = string.Empty;
        [Export] public string TextKey { get; set; } = string.Empty;
        [Export] public Godot.Collections.Array<Condition> Conditions { get; set; } = new();
        [Export] public Godot.Collections.Array<Effect> Effects { get; set; } = new();
        [Export] public string TargetNodeId { get; set; } = string.Empty;
    }
}
