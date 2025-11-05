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

                        GlobalPosition = GridUtility.TileToWorldPosition(TileCoords, TileOffset);
                }

                public string GetInteractPrompt()
                {
                        return TranslationServer.Translate("ui.read_note");
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
                                // 使用证据本地化内容展示（优先 Key）
                                string title = Evidence.Title;
                                if (!string.IsNullOrEmpty(Evidence.TitleKey))
                                {
                                        var t = TranslationServer.Translate(Evidence.TitleKey);
                                        if (!string.IsNullOrEmpty(t)) title = t;
                                }
                                if (string.IsNullOrEmpty(title)) title = Title;

                                string content = Evidence.Content;
                                if (!string.IsNullOrEmpty(Evidence.ContentKey))
                                {
                                        var t2 = TranslationServer.Translate(Evidence.ContentKey);
                                        if (!string.IsNullOrEmpty(t2)) content = t2;
                                }
                                if (string.IsNullOrEmpty(content)) content = Content;

                                reader.ShowNote(title, content);
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
