using Godot;

namespace SilentTestimony.Dialogue
{
    public partial class DialogueUI : Control
    {
        [Export] public Label SpeakerLabel { get; set; }
        [Export] public Label TextLabel { get; set; }
        [Export] public VBoxContainer ChoicesRoot { get; set; }

        public override void _Ready()
        {
            if (SpeakerLabel == null)
                SpeakerLabel = GetNodeOrNull<Label>("Panel/Margin/VBox/Speaker");
            if (TextLabel == null)
                TextLabel = GetNodeOrNull<Label>("Panel/Margin/VBox/Text");
            if (ChoicesRoot == null)
                ChoicesRoot = GetNodeOrNull<VBoxContainer>("Panel/Margin/VBox/Choices");
            Hide();
        }

        public void ClearChoices()
        {
            foreach (Node c in ChoicesRoot.GetChildren())
                c.QueueFree();
        }

        public void RenderNode(DialogueNode node, DialogueManager mgr)
        {
            SpeakerLabel.Text = mgr.ResolveSpeaker(node);
            TextLabel.Text = mgr.ResolveText(node);
            ClearChoices();
            var map = mgr.BuildVisibleChoiceIndexMap(node);
            if (map.Count > 0)
            {
                for (int j = 0; j < map.Count; j++)
                {
                    int idx = map[j];
                    var ch = node.Choices[idx];
                    var btn = new Button { Text = mgr.ResolveChoiceText(ch, idx) };
                    btn.Pressed += () => mgr.Choose(idx);
                    ChoicesRoot.AddChild(btn);
                }
            }
            else
            {
                // 如果无选项，则显示“继续”按钮
                var btn = new Button { Text = "继续" };
                btn.Pressed += () => mgr.EndDialogue();
                ChoicesRoot.AddChild(btn);
            }
        }

        public async void ShowLastAndAwaitClose()
        {
            // 将唯一按钮置为“继续”，点击后关闭
            ClearChoices();
            var btn = new Button { Text = "继续" };
            btn.Pressed += () => Hide();
            ChoicesRoot.AddChild(btn);
            Show();
            await ToSignal(btn, Button.SignalName.Pressed);
        }
    }
}
