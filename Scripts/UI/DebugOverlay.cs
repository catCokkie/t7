using Godot;
using SilentTestimony.Systems;
using SilentTestimony.Core;

namespace SilentTestimony.UI
{
    public partial class DebugOverlay : Control
    {
        private Label _fps;
        private Label _time;
        private Label _inv;
        private Label _evi;
        private Label _ai;
        private Button _tsMinus;
        private Button _tsPlus;
        private Label _tsValue;
        private Label _status;
        private Button _save;
        private Button _load;
        private double _statusTimer;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            _fps = GetNodeOrNull<Label>("Panel/VBox/FPS");
            _time = GetNodeOrNull<Label>("Panel/VBox/Time");
            _inv = GetNodeOrNull<Label>("Panel/VBox/Inventory");
            _evi = GetNodeOrNull<Label>("Panel/VBox/Evidence");
            _status = GetNodeOrNull<Label>("Panel/VBox/Status");
            _ai = GetNodeOrNull<Label>("Panel/VBox/AICount");
            _save = GetNodeOrNull<Button>("Panel/VBox/Buttons/SaveButton");
            _load = GetNodeOrNull<Button>("Panel/VBox/Buttons/LoadButton");
            _tsMinus = GetNodeOrNull<Button>("Panel/VBox/TimeScaleRow/Minus");
            _tsPlus = GetNodeOrNull<Button>("Panel/VBox/TimeScaleRow/Plus");
            _tsValue = GetNodeOrNull<Label>("Panel/VBox/TimeScaleRow/Value");

            if (_save != null) _save.Pressed += () => { GetNodeOrNull<SaveManager>("/root/SaveManager")?.SaveGame(); ShowStatus("Saved"); };
            if (_load != null) _load.Pressed += () => { GetNodeOrNull<SaveManager>("/root/SaveManager")?.LoadGame(); ShowStatus("Loaded"); };
            if (_tsMinus != null) _tsMinus.Pressed += () => AdjustTimeScale(-0.1f);
            if (_tsPlus != null) _tsPlus.Pressed += () => AdjustTimeScale(+0.1f);

            Visible = false;
        }

        public override void _Process(double delta)
        {
            if (_fps != null) _fps.Text = $"FPS: {Engine.GetFramesPerSecond()}";

            var tm = GetNodeOrNull<TimeManager>("/root/TimeManager");
            if (tm != null && _time != null) _time.Text = $"Time: {tm.GetDayAsString()} {tm.GetTimeAsString()}";

            var invMan = GetNodeOrNull<InventoryManager>("/root/InventoryManager");
            if (invMan != null && _inv != null) _inv.Text = $"Inventory items: {invMan.GetAllItems().Count}";

            var evMan = GetNodeOrNull<EvidenceManager>("/root/EvidenceManager");
            if (evMan != null && _evi != null) _evi.Text = $"Evidence: {evMan.GetAll().Count}";

            // AI count (via group "AI" if available, else fallback by type name search)
            if (_ai != null)
            {
                int aiCount = GetTree().GetNodesInGroup("AI").Count;
                if (aiCount == 0)
                {
                    // fallback: rough count of EnemyAIController instances in current scene
                    aiCount = CountNodesOfTypeName("EnemyAIController");
                }
                _ai.Text = $"AI: {aiCount}";
            }

            // TimeScale display
            if (_tsValue != null)
            {
                _tsValue.Text = $"x{Engine.TimeScale:F1}";
            }

            if (_status != null && _status.Visible)
            {
                _statusTimer -= delta;
                if (_statusTimer <= 0)
                {
                    _status.Visible = false;
                }
            }
        }

        public void ShowStatus(string text, double seconds = 1.0)
        {
            if (_status == null) return;
            _status.Text = text;
            _status.Visible = true;
            _statusTimer = seconds;
        }

        private void AdjustTimeScale(float delta)
        {
            float ts = (float)Engine.TimeScale + delta;
            ts = Mathf.Clamp(ts, 0.1f, 4.0f);
            Engine.TimeScale = ts;
            ShowStatus($"TimeScale: x{ts:F1}");
        }

        private int CountNodesOfTypeName(string typeName)
        {
            int count = 0;
            var scene = GetTree().CurrentScene;
            if (scene == null) return 0;
            void Walk(Node n)
            {
                if (n.GetType().Name == typeName) count++;
                foreach (Node c in n.GetChildren()) Walk(c);
            }
            Walk(scene);
            return count;
        }
    }
}
