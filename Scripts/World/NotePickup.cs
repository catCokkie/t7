using Godot;
using SilentTestimony.Interfaces;
using SilentTestimony.UI;
using SilentTestimony.Data;
using SilentTestimony.Systems;

namespace SilentTestimony.World
{
	/// <summary>
	/// 世界中的笔记。交互时打开阅读器 UI。
	/// </summary>
        public partial class NotePickup : StaticBody2D, IInteractable
        {
                [ExportGroup("Tile Placement")]
                [Export] public bool UseTileCoordinates { get; set; } = false;
                [Export] public Vector2I TileCoords { get; set; } = Vector2I.Zero;
                [Export] public Vector2 TileOffset { get; set; } = Vector2.Zero;
                [Export(PropertyHint.Range, "4,256,1")] public float TileCellSize { get; set; } = 16f;

                [Export] public string Title = "未命名笔记";
                [Export(PropertyHint.MultilineText)] public string Content = "";
                [Export] public bool HideOnRead = false;
                [Export] public EvidenceData Evidence;

                public override void _Ready()
                {
                        base._Ready();
                        ApplyTilePlacement();
                }

                private void ApplyTilePlacement()
                {
                        if (!UseTileCoordinates)
                                return;

                        GlobalPosition = RuntimeTilemapBuilderLayers.TileToWorldPosition(TileCoords, TileCellSize, TileOffset);
                }

                public string GetInteractPrompt()
                {
                        return "阅读笔记";
                }

		public void Interact(Node2D interactor)
		{
			var reader = GetNodeOrNull<NoteReaderManager>("/root/NoteReaderManager");
			if (reader == null)
			{
				GD.PushWarning("NotePickup: 未找到 NoteReaderManager（需要设置为 Autoload）。");
				return;
			}

			// 先登记证据（如果有）
			if (Evidence != null)
			{
				var ev = GetNodeOrNull<EvidenceManager>("/root/EvidenceManager");
				ev?.AddEvidence(Evidence);
				// 使用证据内容展示
				reader.ShowNote(string.IsNullOrEmpty(Evidence.Title) ? Title : Evidence.Title,
								string.IsNullOrEmpty(Evidence.Content) ? Content : Evidence.Content);
			}
			else
			{
				reader.ShowNote(Title, Content);
			}
			if (HideOnRead)
			{
				QueueFree();
			}
		}
	}
}
