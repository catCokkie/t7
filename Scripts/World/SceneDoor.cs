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
        [Export(PropertyHint.Range, "4,256,1")] public float TileCellSize { get; set; } = 16f;

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

            GlobalPosition = RuntimeTilemapBuilderLayers.TileToWorldPosition(TileCoords, TileCellSize, TileOffset);
        }

        public string GetInteractPrompt()
        {
            // 由 InteractionPrompt 统一加“按 E：”前缀
            return string.IsNullOrEmpty(TargetScenePath) ? "未配置传送" : "进入";
        }

        public void Interact(Node2D interactor)
        {
            if (string.IsNullOrEmpty(TargetScenePath))
            {
                GD.PushWarning("SceneDoor: 未配置 TargetScenePath");
                return;
            }

            var loader = GetNodeOrNull<SceneLoader>("/root/SceneLoader");
            if (loader == null)
            {
                GD.PushError("SceneDoor: 未找到 SceneLoader（需要设置为 Autoload）");
                return;
            }

            loader.ChangeScene(TargetScenePath, TargetSpawnPointName);
        }
    }
}

