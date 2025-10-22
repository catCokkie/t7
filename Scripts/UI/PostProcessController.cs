using Godot;
using SilentTestimony.Core;
using SilentTestimony.Systems;

namespace SilentTestimony.UI
{
    /// <summary>
    /// 后处理控制（Autoload）：实例化覆盖层，按理智动态调整参数
    /// </summary>
    public partial class PostProcessController : Node
    {
        private const string OverlayScenePath = "res://Scenes/UI/PostProcessOverlay.tscn";

        private CanvasLayer _layer;
        private ColorRect _vignette;
        private ColorRect _noise;
        private PlayerStats _stats;
        private AudioManager _audio;

        [Export] public AudioStream HeartbeatStream { get; set; }

        public override void _Ready()
        {
            _layer = new CanvasLayer();
            AddChild(_layer);

            var packed = GD.Load<PackedScene>(OverlayScenePath);
            if (packed != null)
            {
                var root = packed.Instantiate<CanvasLayer>();
                _layer.AddChild(root);
                _vignette = root.GetNodeOrNull<ColorRect>("VignetteRect");
                _noise = root.GetNodeOrNull<ColorRect>("NoiseRect");
            }
            else
            {
                GD.PushError($"PostProcessController: 无法加载 {OverlayScenePath}");
            }

            _stats = GetNodeOrNull<PlayerStats>("/root/PlayerStats");
            if (_stats != null)
            {
                _stats.SanityChanged += OnSanityChanged;
                // 初始同步
                UpdatePostFX(_stats.Sanity, _stats.MaxSanity);
            }

            _audio = GetNodeOrNull<AudioManager>("/root/AudioManager");
            if (_audio != null && HeartbeatStream != null)
            {
                _audio.SetHeartbeatStream(HeartbeatStream);
            }
        }

        public override void _ExitTree()
        {
            if (_stats != null)
            {
                _stats.SanityChanged -= OnSanityChanged;
            }
        }

        private void OnSanityChanged(int newSanity, int delta)
        {
            UpdatePostFX(newSanity, _stats?.MaxSanity ?? 100);
        }

        private void UpdatePostFX(int sanity, int maxSanity)
        {
            float t = 1.0f - Mathf.Clamp((float)sanity / Mathf.Max(1, maxSanity), 0f, 1f);
            // 轻度非线性增强
            float vignetteIntensity = Mathf.Pow(t, 1.2f) * 0.85f; // 0..~0.85
            float noiseAmount = Mathf.Pow(t, 1.5f) * 0.35f;        // 0..~0.35

            if (_vignette?.Material is ShaderMaterial vm)
            {
                vm.SetShaderParameter("intensity", vignetteIntensity);
            }
            if (_noise?.Material is ShaderMaterial nm)
            {
                nm.SetShaderParameter("amount", noiseAmount);
            }

            // 关联心跳强度：Sanity 越低心跳越强
            if (_audio != null)
            {
                _audio.PlayHeartbeat(t);
            }
        }
    }
}
