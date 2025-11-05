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
            string action = raw ?? string.Empty;
            if (string.IsNullOrEmpty(action)) return string.Empty;

            var fmt = TranslationServer.Translate("ui.prompt_format");
            if (string.IsNullOrEmpty(fmt)) fmt = "[E] {0}";
            string text = string.Format(fmt, action);

            const int max = 24;
            if (text.Length > max)
            {
                text = text.Substring(0, max) + "…";
            }
            return text;
        }
    }
}

