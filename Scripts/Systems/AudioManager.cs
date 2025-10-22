using Godot;

namespace SilentTestimony.Systems
{
    /// <summary>
    /// 全局音频管理（Autoload）
    /// - PlaySFX: 在世界坐标播放一次性音效
    /// - PlayHeartbeat: 根据强度调整心跳循环音量/音调
    /// </summary>
    public partial class AudioManager : Node
    {
        [Export] public string SfxBusName { get; set; } = "SFX";

        private AudioStreamPlayer2D _heartbeat;
        private AudioStream _heartbeatStream;

        public override void _Ready()
        {
            // 心跳播放器（可选，无流时不发声）
            _heartbeat = new AudioStreamPlayer2D
            {
                Bus = SfxBusName,
                Autoplay = false,
                VolumeDb = -40f,
                PitchScale = 1.0f
            };
            AddChild(_heartbeat);
        }

        public void SetHeartbeatStream(AudioStream stream)
        {
            _heartbeatStream = stream;
            _heartbeat.Stream = stream;
        }

        public void PlaySFX(AudioStream stream, Vector2 worldPos, float volumeDb = 0.0f, float pitchScale = 1.0f)
        {
            if (stream == null)
            {
                GD.PushWarning("AudioManager.PlaySFX: stream is null");
                return;
            }

            var p = new AudioStreamPlayer2D
            {
                Stream = stream,
                GlobalPosition = worldPos,
                Bus = SfxBusName,
                VolumeDb = volumeDb,
                PitchScale = pitchScale,
                Autoplay = true
            };

            AddChild(p);
            p.Finished += () => p.QueueFree();
        }

        /// <summary>
        /// 设置心跳强度：0=静音，1=最大（也可调节音调以增强紧迫感）
        /// </summary>
        public void PlayHeartbeat(float intensity)
        {
            intensity = Mathf.Clamp(intensity, 0f, 1f);

            if (_heartbeatStream == null)
            {
                // 未设置流：静默退出
                return;
            }

            // 音量从 -30dB 到 -3dB 映射
            float db = Mathf.Lerp(-30f, -3f, intensity);
            _heartbeat.VolumeDb = db;
            _heartbeat.PitchScale = Mathf.Lerp(0.9f, 1.15f, intensity);

            if (!_heartbeat.Playing)
            {
                _heartbeat.Play();
            }

            // 当强度足够低时停止
            if (intensity <= 0.01f && _heartbeat.Playing)
            {
                _heartbeat.Stop();
            }
        }
    }
}

