using Godot;

namespace SilentTestimony.UI
{
    /// <summary>
    /// Autoload: 管理暂停菜单显示与输入（Esc）
    /// </summary>
    public partial class PauseMenuManager : Node
    {
        private CanvasLayer _layer;
        private PauseMenu _menu;

        public override void _Ready()
        {
            _layer = new CanvasLayer();
            AddChild(_layer);

            var packed = GD.Load<PackedScene>("res://Scenes/UI/PauseMenu.tscn");
            _menu = packed.Instantiate<PauseMenu>();
            _layer.AddChild(_menu);
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("pause"))
            {
                Toggle();
                GetTree().SetInputAsHandled();
            }
        }

        private void Toggle()
        {
            if (_menu == null) return;
            if (_menu.Visible)
            {
                GetTree().Paused = false;
                _menu.Visible = false;
            }
            else
            {
                GetTree().Paused = true;
                _menu.Visible = true;
            }
        }
    }
}

