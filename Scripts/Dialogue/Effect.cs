using Godot;

namespace SilentTestimony.Dialogue
{
    public enum EffectType
    {
        None,
        SetFlagTrue,
        SetFlagFalse,
        AddItem,
        AddEvidence,
        ChangeScene
    }

    [GlobalClass]
    public partial class Effect : Resource
    {
        [Export] public EffectType Type { get; set; } = EffectType.None;
        [Export] public string Key { get; set; } = string.Empty; // flag / item / evidence / scene path
        [Export] public string Extra { get; set; } = string.Empty; // optional (e.g., spawn point)
    }
}

