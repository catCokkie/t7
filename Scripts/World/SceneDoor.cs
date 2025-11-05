using Godot;
using SilentTestimony.Interfaces;
using SilentTestimony.Systems;

namespace SilentTestimony.World
{
    /// <summary>
    /// 交互后切换场景的门（可选出生点名）
    /// </summary>
    public partial class SceneDoor : StaticBody2D, IInteractable
    {
        [ExportGroup("Tile Placement")]
        [Export] public bool UseTileCoordinates { get; set; } = false;
        [Export] public Vector2I TileCoords { get; set; } = Vector2I.Zero;
        [Export] public Vector2 TileOffset { get; set; } = Vector2.Zero;

        [Export] public string TargetScenePath = string.Empty;
        [Export] public string TargetSpawnPointName = string.Empty;

        public override void _Ready()
        {
            base._Ready();
            ApplyTilePlacement();
        }

        private void ApplyTilePlacement()
        {
            if (!UseTileCoordinates)
            {
                return;
            }

            GlobalPosition = GridUtility.TileToWorldPosition(TileCoords, TileOffset);
        }

        public string GetInteractPrompt()
        {
            if (string.IsNullOrEmpty(TargetScenePath))
                return TranslationServer.Translate("ui.missing_target");
            return TranslationServer.Translate("ui.enter");
        }

        public void Interact(Node2D interactor)
        {
            if (string.IsNullOrEmpty(TargetScenePath))
            {
                GD.PushWarning("SceneDoor: Missing TargetScenePath");
                return;
            }

            var loader = GetNodeOrNull<SceneLoader>("/root/SceneLoader");
            if (loader == null)
            {
                GD.PushError("SceneDoor: SceneLoader not found (expected Autoload)");
                return;
            }

            loader.ChangeScene(TargetScenePath, TargetSpawnPointName);
        }
    }
}

