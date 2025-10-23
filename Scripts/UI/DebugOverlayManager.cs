using Godot;
using SilentTestimony.Systems;

namespace SilentTestimony.UI
{
    /// <summary>
    /// Autoload: F1 显示/隐藏，F5 快速保存，F9 快速读取
    /// </summary>
    public partial class DebugOverlayManager : Node
    {
        private CanvasLayer _layer;
        private DebugOverlay _overlay;

        public override void _Ready()
        {
            _layer = new CanvasLayer();
            AddChild(_layer);

            var packed = GD.Load<PackedScene>("res://Scenes/UI/DebugOverlay.tscn");
            _overlay = packed.Instantiate<DebugOverlay>();
            _layer.AddChild(_overlay);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("debug_toggle"))
            {
                _overlay.Visible = !_overlay.Visible;
                GetViewport()?.SetInputAsHandled();
                return;
            }
            if (@event.IsActionPressed("quick_save"))
            {
                GetNodeOrNull<SaveManager>("/root/SaveManager")?.SaveGame();
                _overlay?.ShowStatus("Saved");
                GetViewport()?.SetInputAsHandled();
                return;
            }
            if (@event.IsActionPressed("quick_load"))
            {
                GetNodeOrNull<SaveManager>("/root/SaveManager")?.LoadGame();
                _overlay?.ShowStatus("Loaded");
                GetViewport()?.SetInputAsHandled();
                return;
            }
        }
    }
}
