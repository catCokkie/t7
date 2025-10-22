using Godot;
using System.Collections.Generic;
using SilentTestimony.Interfaces;
using SilentTestimony.UI;

namespace SilentTestimony.Player
{
    public partial class PlayerController
    {
        [ExportGroup("Interaction")]
        private float _interactRange = 64.0f;

        [Export(PropertyHint.Range, "8,256,1")]
        private float InteractRange
        {
            get => _interactRange;
            set
            {
                if (Mathf.IsEqualApprox(_interactRange, value))
                    return;

                _interactRange = value;
                ApplyInteractRangeToShape();
            }
        }

        private readonly List<IInteractable> _nearbyInteractables = new();
        private Area2D _interactorArea;
        private CollisionShape2D _interactorShape;
        private InteractionPrompt _interactionPrompt;
        private IInteractable _currentInteractTarget;

        partial void InitializeInteraction()
        {
            _interactionPrompt = GetNodeOrNull<InteractionPrompt>("InteractionPrompt");
            _interactorArea = GetNodeOrNull<Area2D>("Interactor");
            _interactorShape = _interactorArea?.GetNodeOrNull<CollisionShape2D>("CollisionShape2D");

            ApplyInteractRangeToShape();
            RefreshInteractTarget(force: true);
        }

        partial void UpdateInteraction(double delta)
        {
            RefreshInteractTarget();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("interact"))
            {
                var target = GetCurrentInteractTarget();
                bool handled = false;

                if (target != null)
                {
                    target.Interact(this);
                    handled = true;
                }

                RefreshInteractTarget(force: true);

                if (handled)
                {
                    GetTree()?.SetInputAsHandled();
                }
            }
        }

        private void ApplyInteractRangeToShape()
        {
            if (_interactorShape?.Shape is CircleShape2D circle)
            {
                circle.Radius = _interactRange;
            }
        }

        // Signals from Area2D Interactor
        private void OnInteractorBodyEntered(Node2D body)
        {
            if (body is IInteractable interactable)
            {
                if (!_nearbyInteractables.Contains(interactable))
                    _nearbyInteractables.Add(interactable);
                RefreshInteractTarget(force: true);
            }
        }

        private void OnInteractorBodyExited(Node2D body)
        {
            if (body is IInteractable interactable)
            {
                _nearbyInteractables.Remove(interactable);
                RefreshInteractTarget(force: true);
            }
        }

        private void CleanupInteractables()
        {
            for (int i = _nearbyInteractables.Count - 1; i >= 0; i--)
            {
                var interactable = _nearbyInteractables[i];
                if (interactable is not GodotObject go || !GodotObject.IsInstanceValid(go))
                {
                    _nearbyInteractables.RemoveAt(i);
                }
            }
        }

        private IInteractable GetCurrentInteractTarget()
        {
            CleanupInteractables();

            IInteractable closest = null;
            float best = float.MaxValue;
            float rangeSq = _interactRange * _interactRange;

            foreach (var interactable in _nearbyInteractables)
            {
                if (interactable is not Node2D node)
                    continue;

                float distanceSq = node.GlobalPosition.DistanceSquaredTo(GlobalPosition);
                if (distanceSq > rangeSq)
                    continue;

                if (distanceSq < best)
                {
                    best = distanceSq;
                    closest = interactable;
                }
            }

            return closest;
        }

        private void RefreshInteractTarget(bool force = false)
        {
            var target = GetCurrentInteractTarget();
            if (!force && ReferenceEquals(target, _currentInteractTarget))
                return;

            _currentInteractTarget = target;

            if (_interactionPrompt == null)
                return;

            if (_currentInteractTarget != null)
            {
                _interactionPrompt.ShowPrompt(_currentInteractTarget.GetInteractPrompt());
            }
            else
            {
                _interactionPrompt.HidePrompt();
            }
        }
    }
}
