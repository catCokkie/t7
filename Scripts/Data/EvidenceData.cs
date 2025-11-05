using Godot;

namespace SilentTestimony.Data
{
    [GlobalClass]
    public partial class EvidenceData : Resource
    {
        [Export] public string EvidenceID { get; set; } = string.Empty;
        [Export] public string Title { get; set; } = string.Empty;
        [Export] public string TitleKey { get; set; } = string.Empty;
        [Export(PropertyHint.MultilineText)] public string Content { get; set; } = string.Empty;
        [Export] public string ContentKey { get; set; } = string.Empty;
    }
}
