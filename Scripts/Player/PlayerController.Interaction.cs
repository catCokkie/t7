using Godot;
using System.Collections.Generic;
using SilentTestimony.Interfaces;
using SilentTestimony.UI;

namespace SilentTestimony.Player
{
    public partial class PlayerController
    {
        [ExportGroup("Interaction")]
        [Export(PropertyHint.Range, "8,256,1")] private float _interactRange = 64.0f;
        private readonly List<IInteractable> _nearbyInteractables = new();

        public override void _UnhandledInput(InputEvent @event)
        {
            if (Input.IsActionJustPressed("interact"))
            {
                var target = GetCurrentInteractTarget();
                if (target != null)
                {
                    target.Interact(this);
                }
            }
        }

        // Signals from Area2D Interactor
        private void OnInteractorBodyEntered(Node2D body)
        {
            if (body is IInteractable interactable)
            {
                if (!_nearbyInteractables.Contains(interactable))
                    _nearbyInteractables.Add(interactable);
                UpdatePrompt();
            }
        }

        private void OnInteractorBodyExited(Node2D body)
        {
            if (body is IInteractable interactable)
            {
                _nearbyInteractables.Remove(interactable);
                UpdatePrompt();
            }
        }

        private IInteractable GetCurrentInteractTarget()
        {
            IInteractable closest = null;
            float best = float.MaxValue;
            foreach (var it in _nearbyInteractables)
            {
                if (it is Node2D n2)
                {
                    float d = n2.GlobalPosition.DistanceTo(GlobalPosition);
                    if (d < best)
                    {
                        best = d;
                        closest = it;
                    }
                }
            }
            return closest;
        }

        private void UpdatePrompt()
        {
            var prompt = GetNodeOrNull<InteractionPrompt>("InteractionPrompt");
            if (prompt == null)
                return;

            var current = GetCurrentInteractTarget();
            if (current != null)
            {
                prompt.ShowPrompt(current.GetInteractPrompt());
            }
            else
            {
                prompt.HidePrompt();
            }
        }
    }
}
