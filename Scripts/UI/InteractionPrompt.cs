using Godot;

namespace SilentTestimony.UI
{
    public partial class InteractionPrompt : Label
    {
        private Tween _fadeTween;
        private const float FadeOutSeconds = 0.15f;

        public void ShowPrompt(string rawText)
        {
            string formatted = FormatText(rawText);
            if (_fadeTween?.IsRunning() == true)
            {
                _fadeTween.Kill();
            }
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1f);
            Text = formatted;
            Visible = true;
        }

        public void HidePrompt()
        {
            if (!Visible)
                return;
            if (_fadeTween?.IsRunning() == true)
            {
                _fadeTween.Kill();
            }
            _fadeTween = CreateTween();
            _fadeTween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            _fadeTween.TweenProperty(this, "modulate:a", 0.0f, FadeOutSeconds);
            _fadeTween.Finished += () => { Visible = false; };
        }

        public override void _Ready()
        {
            Visible = false;
            Modulate = new Color(Modulate.R, Modulate.G, Modulate.B, 1f);
        }

        private string FormatText(string raw)
        {
            string t = raw ?? "";
            // 统一样式：按 E：{动作}
            if (!string.IsNullOrEmpty(t))
            {
                // 尝试避免重复加前缀
                if (!(t.StartsWith("按 E") || t.StartsWith("E ") || t.StartsWith("E：") || t.StartsWith("E:") ))
                {
                    t = $"按 E：{t}";
                }
                // 超长截断（约 24 字）
                const int max = 24;
                if (t.Length > max)
                {
                    t = t.Substring(0, max) + "…";
                }
            }
            return t;
        }
    }
}

