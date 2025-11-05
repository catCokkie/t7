using Godot;
using SilentTestimony.Dialogue;

namespace SilentTestimony.Dialogue
{
    public partial class DialogueManager : Node
    {
        [Export] public NodePath DialogueUIPath { get; set; }

        private DialogueGraph _graph;
        private DialogueNode _current;
        private DialogueUI _ui;
        public DialogueState State { get; } = new();

        public override void _Ready()
        {
            _ui = GetNodeOrNull<DialogueUI>(DialogueUIPath);
            if (_ui == null)
            {
                _ui = GetNodeOrNull<DialogueUI>("DialogueUI");
            }
            if (_ui == null)
            {
                // Autoload 场景下若未在关卡中放置 UI，则动态实例化一个
                var uiScene = GD.Load<PackedScene>("res://Scenes/UI/DialogueUI.tscn");
                if (uiScene != null)
                {
                    _ui = uiScene.Instantiate<DialogueUI>();
                    _ui.Name = "DialogueUI";
                    AddChild(_ui);
                }
            }
            _ui?.Hide();
        }

        public void StartDialogueByPath(string resourcePath)
        {
            var g = GD.Load<DialogueGraph>(resourcePath);
            if (g == null)
            {
                GD.PushError($"DialogueManager: failed to load graph {resourcePath}");
                return;
            }
            StartDialogue(g);
        }

        public void StartDialogue(DialogueGraph graph)
        {
            _graph = graph;
            _current = _graph?.FindNode(_graph?.StartNodeId ?? string.Empty);
            if (_current == null)
            {
                GD.PushWarning("DialogueManager: no start node");
                return;
            }
            ShowCurrent();
        }

        private void ShowCurrent()
        {
            if (_ui == null || _current == null)
                return;
            _ui.RenderNode(_current, this);
            _ui.Show();
        }

        public System.Collections.Generic.List<int> BuildVisibleChoiceIndexMap(DialogueNode node)
        {
            var map = new System.Collections.Generic.List<int>();
            if (node?.Choices == null) return map;
            for (int i = 0; i < node.Choices.Count; i++)
            {
                var ch = node.Choices[i];
                if (ch == null) continue;
                if (EvaluateConditions(ch))
                    map.Add(i);
            }
            return map;
        }

        public string ResolveSpeaker(DialogueNode node)
        {
            if (!string.IsNullOrEmpty(node.SpeakerKey))
                return TranslationServer.Translate(node.SpeakerKey);
            return node.Speaker ?? string.Empty;
        }

        public string ResolveText(DialogueNode node)
        {
            if (!string.IsNullOrEmpty(node.TextKey))
                return TranslationServer.Translate(node.TextKey);
            return node.Text ?? string.Empty;
        }

        public string ResolveChoiceText(DialogueChoice ch, int index)
        {
            if (ch == null) return $"选项 {index + 1}";
            if (!string.IsNullOrEmpty(ch.TextKey))
                return TranslationServer.Translate(ch.TextKey);
            if (!string.IsNullOrEmpty(ch.Text))
                return ch.Text;
            return $"选项 {index + 1}";
        }

        private bool EvaluateConditions(DialogueChoice ch)
        {
            if (ch?.Conditions == null || ch.Conditions.Count == 0) return true;
            foreach (var cond in ch.Conditions)
            {
                if (!Evaluate(cond)) return false;
            }
            return true;
        }

        private bool Evaluate(Condition cond)
        {
            switch (cond?.Type)
            {
                case ConditionType.None:
                    return true;
                case ConditionType.FlagTrue:
                    return State.GetFlag(cond.Key);
                case ConditionType.FlagFalse:
                    return !State.GetFlag(cond.Key);
                case ConditionType.HasItem:
                {
                    var inv = GetNodeOrNull<Node>("/root/InventoryManager");
                    if (inv == null) return false;
                    if (inv.HasMethod("HasItem"))
                        return (bool)inv.Call("HasItem", cond.Key);
                    return false;
                }
                case ConditionType.HasEvidence:
                {
                    var ev = GetNodeOrNull<Node>("/root/EvidenceManager");
                    if (ev == null) return false;
                    if (ev.HasMethod("HasEvidence"))
                        return (bool)ev.Call("HasEvidence", cond.Key);
                    return false;
                }
            }
            return false;
        }

        public void Choose(int index)
        {
            if (_current == null || _current.Choices == null || index < 0 || index >= _current.Choices.Count)
                return;
            var choice = _current.Choices[index];
            if (choice == null || string.IsNullOrEmpty(choice.TargetNodeId))
            {
                EndDialogue();
                return;
            }
            ApplyEffects(choice);
            var next = _graph.FindNode(choice.TargetNodeId);
            if (next == null)
            {
                EndDialogue();
                return;
            }
            _current = next;
            if (_current.Choices == null || _current.Choices.Count == 0)
            {
                // show last line then auto close on click
                _ui.RenderNode(_current, this);
                _ui.ShowLastAndAwaitClose();
            }
            else
            {
                ShowCurrent();
            }
        }

        private void ApplyEffects(DialogueChoice ch)
        {
            if (ch?.Effects == null || ch.Effects.Count == 0) return;
            foreach (var ef in ch.Effects)
            {
                switch (ef.Type)
                {
                    case EffectType.None:
                        break;
                    case EffectType.SetFlagTrue:
                        State.SetFlag(ef.Key, true);
                        break;
                    case EffectType.SetFlagFalse:
                        State.SetFlag(ef.Key, false);
                        break;
                    case EffectType.AddItem:
                    {
                        var inv = GetNodeOrNull<Node>("/root/InventoryManager");
                        if (inv != null)
                        {
                            if (inv.HasMethod("AddItemById"))
                                inv.Call("AddItemById", ef.Key);
                            else if (inv.HasMethod("AddItem"))
                            {
                                var res = GD.Load<Resource>(ef.Key);
                                if (res != null)
                                    inv.Call("AddItem", res);
                            }
                        }
                        break;
                    }
                    case EffectType.AddEvidence:
                    {
                        var ev = GetNodeOrNull<Node>("/root/EvidenceManager");
                        if (ev != null)
                        {
                            if (ev.HasMethod("AddEvidenceById"))
                                ev.Call("AddEvidenceById", ef.Key);
                            else if (ev.HasMethod("AddEvidence"))
                            {
                                var res = GD.Load<Resource>(ef.Key);
                                if (res != null)
                                    ev.Call("AddEvidence", res);
                            }
                        }
                        break;
                    }
                    case EffectType.ChangeScene:
                    {
                        var loader = GetNodeOrNull<Node>("/root/SceneLoader");
                        if (loader != null)
                        {
                            if (!string.IsNullOrEmpty(ef.Extra))
                                loader.Call("ChangeScene", ef.Key, ef.Extra);
                            else
                                loader.Call("ChangeScene", ef.Key);
                        }
                        break;
                    }
                }
            }
        }

        public void EndDialogue()
        {
            _current = null;
            _graph = null;
            _ui?.Hide();
        }
    }
}
