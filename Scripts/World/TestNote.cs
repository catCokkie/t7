using Godot;
using SilentTestimony.Interfaces;

namespace SilentTestimony.World
{
    public partial class TestNote : StaticBody2D, IInteractable
    {
        public string GetInteractPrompt()
        {
            return "[E] 阅读笔记";
        }

        public void Interact(Node2D interactor)
        {
            GD.Print($"{Name}: 玩家触发交互 -> 阅读笔记");
        }
    }
}
