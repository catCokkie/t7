using Godot;

namespace SilentTestimony.UI
{
    public partial class NoteReader : Control
    {
        private Label _title;
        private RichTextLabel _body;
        private Button _closeButton;

        public override void _Ready()
        {
            _title = GetNodeOrNull<Label>("CenterContainer/Panel/VBox/Title");
            _body = GetNodeOrNull<RichTextLabel>("CenterContainer/Panel/VBox/Body");
            _closeButton = GetNodeOrNull<Button>("CenterContainer/Panel/VBox/CloseButton");

            if (_closeButton != null)
            {
                _closeButton.Pressed += HideNote;
            }

            Visible = false;
        }

        public void ShowNote(string title, string content)
        {
            if (_title != null) _title.Text = title ?? string.Empty;
            if (_body != null) { _body.Text = content ?? string.Empty; _body.ScrollToLine(0); }
            Visible = true;
        }

        public void HideNote()
        {
            Visible = false;
        }
    }
}

