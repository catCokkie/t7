using Godot;

namespace SilentTestimony.UI
{
    /// <summary>
    /// Autoload 管理器：负责实例化并显示/隐藏笔记阅读窗口
    /// </summary>
    public partial class NoteReaderManager : Node
    {
        private const string NoteReaderScenePath = "res://Scenes/UI/NoteReader.tscn";

        private CanvasLayer _layer;
        private NoteReader _noteReader;

        public override void _Ready()
        {
            _layer = new CanvasLayer();
            AddChild(_layer);

            var packed = GD.Load<PackedScene>(NoteReaderScenePath);
            if (packed != null)
            {
                _noteReader = packed.Instantiate<NoteReader>();
                _layer.AddChild(_noteReader);
            }
            else
            {
                GD.PushError($"NoteReaderManager: 无法加载场景 {NoteReaderScenePath}");
            }
        }

        public void ShowNote(string title, string content)
        {
            _noteReader?.ShowNote(title, content);
        }

        public void HideNote()
        {
            _noteReader?.HideNote();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && _noteReader != null && _noteReader.Visible)
            {
                _noteReader.HideNote();
                GetViewport()?.SetInputAsHandled();
            }
        }
    }
}
