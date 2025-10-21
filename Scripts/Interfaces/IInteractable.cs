using Godot;

namespace SilentTestimony.Interfaces
{
    public interface IInteractable
    {
        void Interact(Node2D interactor);
        string GetInteractPrompt();
    }
}

