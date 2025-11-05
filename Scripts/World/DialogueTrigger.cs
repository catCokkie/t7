using Godot;
using SilentTestimony.Interfaces;
using SilentTestimony.World;

namespace SilentTestimony.World
{
    public partial class DialogueTrigger : StaticBody2D, IInteractable
    {
        [ExportGroup("Tile Placement")]
        [Export] public bool UseTileCoordinates { get; set; } = false;
        [Export] public Vector2I TileCoords { get; set; } = Vector2I.Zero;
        [Export] public Vector2 TileOffset { get; set; } = Vector2.Zero;

        [Export] public string DialogueResourcePath { get; set; } = "res://Resources/Dialogue/graphs/Intro.tres";

        public override void _Ready()
        {
            if (UseTileCoordinates)
            {
                GlobalPosition = GridUtility.TileToWorldPosition(TileCoords, TileOffset);
            }
        }

        public string GetInteractPrompt()
        {
            return "交谈";
        }

        public void Interact(Node2D interactor)
        {
            var mgr = GetNodeOrNull<Dialogue.DialogueManager>("/root/DialogueManager");
            if (mgr is not null)
            {
                mgr.StartDialogueByPath(DialogueResourcePath);
            }
            else
            {
                GD.PushWarning("DialogueTrigger: 找不到 DialogueManager 节点");
            }
        }
    }
}
