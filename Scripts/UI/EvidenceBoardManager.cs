using Godot;
using SilentTestimony.Systems;

namespace SilentTestimony.UI
{
    /// <summary>
    /// Autoload: 管理 EvidenceBoard 的生命周期与输入切换
    /// </summary>
    public partial class EvidenceBoardManager : Node
    {
        private const string ScenePath = "res://Scenes/UI/EvidenceBoard.tscn";

        private CanvasLayer _layer;
        private EvidenceBoard _board;
        private EvidenceManager _evidence;

        public override void _Ready()
        {
            _layer = new CanvasLayer();
            AddChild(_layer);

            var packed = GD.Load<PackedScene>(ScenePath);
            if (packed != null)
            {
                _board = packed.Instantiate<EvidenceBoard>();
                _layer.AddChild(_board);
            }
            else
            {
                GD.PushError($"EvidenceBoardManager: 无法加载 {ScenePath}");
            }

            _evidence = GetNodeOrNull<EvidenceManager>("/root/EvidenceManager");
            if (_evidence != null)
            {
                _evidence.EvidenceAdded += OnEvidenceAdded;
            }
        }

        public override void _ExitTree()
        {
            if (_evidence != null)
            {
                _evidence.EvidenceAdded -= OnEvidenceAdded;
            }
        }

        private void OnEvidenceAdded(string id)
        {
            if (_board != null && _board.Visible)
            {
                _board.UpdateList(_evidence.GetAll());
            }
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("evidence"))
            {
                Toggle();
                GetViewport()?.SetInputAsHandled();
            }
        }

        private void Toggle()
        {
            if (_board == null || _evidence == null) return;
            if (_board.Visible)
            {
                _board.Visible = false;
            }
            else
            {
                _board.ShowWith(_evidence.GetAll());
            }
        }
    }
}
