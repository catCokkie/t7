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
        private Area2D _interactor;
        private CollisionShape2D _interactorShape;

        private void InitializeInteractor()
        {
            _interactor = GetNodeOrNull<Area2D>("Interactor");
            if (_interactor == null)
            {
                GD.PushWarning("PlayerController: Interactor node not found.");
                return;
            }

            _interactorShape = _interactor.GetNodeOrNull<CollisionShape2D>("InteractorShape")
                ?? _interactor.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

            if (_interactorShape?.Shape is CircleShape2D circleShape)
            {
                circleShape.Radius = _interactRange;
            }
            else
            {
                GD.PushWarning("PlayerController: Interactor CollisionShape2D with CircleShape2D not found.");
            }
        }

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
