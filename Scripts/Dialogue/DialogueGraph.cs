using Godot;

namespace SilentTestimony.Dialogue
{
    [GlobalClass]
    public partial class DialogueGraph : Resource
    {
        [Export] public string GraphId { get; set; } = string.Empty;
        [Export] public string Title { get; set; } = string.Empty;
        [Export] public string StartNodeId { get; set; } = string.Empty;
        [Export] public Godot.Collections.Array<DialogueNode> Nodes { get; set; } = new();

        public DialogueNode FindNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId)) return null;
            foreach (var n in Nodes)
            {
                if (n != null && n.NodeId == nodeId)
                    return n;
            }
            return null;
        }
    }
}

