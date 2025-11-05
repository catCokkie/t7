using Godot;

namespace SilentTestimony.Dialogue
{
    public enum ConditionType
    {
        None,
        FlagTrue,
        FlagFalse,
        HasItem,
        HasEvidence
    }

    [GlobalClass]
    public partial class Condition : Resource
    {
        [Export] public ConditionType Type { get; set; } = ConditionType.None;
        [Export] public string Key { get; set; } = string.Empty; // flag key / item id / evidence id
    }
}

