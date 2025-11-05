using Godot;
using SilentTestimony.World;

namespace SilentTestimony.Player
{
    public partial class PlayerController
    {
        [ExportGroup("Grid Movement")]
        [Export] private bool _useGridMovement = false;
        [Export(PropertyHint.Range, "1,30,0.5")] private float _tilesPerSecond = 6f;
        [Export] private NodePath _gridLayerPath;

        private bool _isGridMoving;
        private Vector2 _gridTargetWorld;
        private Tween _gridTween;
        private TileMapLayer _gridLayer;

        private TileMapLayer ResolveGridLayer()
        {
            if (_gridLayer != null && IsInstanceValid(_gridLayer))
                return _gridLayer;

            if (_gridLayerPath != null && !_gridLayerPath.IsEmpty)
            {
                _gridLayer = GetNodeOrNull<TileMapLayer>(_gridLayerPath);
                if (_gridLayer != null)
                    return _gridLayer;
            }

            Node levelRoot = GetTree().CurrentScene ?? GetParent();
            if (levelRoot != null)
            {
                var tileWorld = levelRoot.GetNodeOrNull<Node>("TileWorld");
                if (tileWorld != null)
                {
                    _gridLayer = FindLayerRecursively(tileWorld, "Ground")
                                  ?? FindLayerRecursively(tileWorld, null);
                }
                else
                {
                    _gridLayer = FindLayerRecursively(levelRoot, "Ground")
                                  ?? FindLayerRecursively(levelRoot, null);
                }
            }

            return _gridLayer;
        }

        private TileMapLayer FindLayerRecursively(Node root, string name)
        {
            if (root == null) return null;
            foreach (Node child in root.GetChildren())
            {
                if (child is TileMapLayer layer)
                {
                    if (string.IsNullOrEmpty(name) || layer.Name.Equals(name))
                        return layer;
                }
                var found = FindLayerRecursively(child, name);
                if (found != null) return found;
            }
            return null;
        }

        private Vector2I WorldToCell(Vector2 world)
        {
            var layer = ResolveGridLayer();
            if (layer == null)
            {
                return GridUtility.WorldToTile(world, GridUtility.CellSize);
            }
            return layer.LocalToMap(layer.ToLocal(world));
        }

        private Vector2 CellToWorldCenter(Vector2I cell)
        {
            var layer = ResolveGridLayer();
            if (layer == null)
            {
                return GridUtility.TileToWorldCenter(cell, GridUtility.CellSize);
            }
            return layer.ToGlobal(layer.MapToLocal(cell));
        }

        private bool IsCellWalkable(Vector2I cell)
        {
            Vector2 target = CellToWorldCenter(cell);
            Vector2 motion = target - GlobalPosition;
            if (motion.LengthSquared() < 0.001f) return true;
            return !TestMove(GlobalTransform, motion);
        }

        private void HandleGridMovement(double delta)
        {
            if (!_useGridMovement)
                return;

            if (_isGridMoving)
                return;

            Vector2 input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (input == Vector2.Zero)
            {
                Velocity = Vector2.Zero;
                return;
            }

            Vector2 dir;
            if (Mathf.Abs(input.X) > Mathf.Abs(input.Y))
                dir = new Vector2(Mathf.Sign(input.X), 0);
            else
                dir = new Vector2(0, Mathf.Sign(input.Y));

            if (Input.IsActionPressed("sneak"))
                CurrentState = PlayerState.Sneaking;
            else if (Input.IsActionPressed("run"))
                CurrentState = PlayerState.Running;
            else
                CurrentState = PlayerState.Walking;

            Vector2I currentCell = WorldToCell(GlobalPosition);
            Vector2I targetCell = currentCell + new Vector2I((int)dir.X, (int)dir.Y);

            if (!IsCellWalkable(targetCell))
            {
                Velocity = Vector2.Zero;
                return;
            }

            _isGridMoving = true;
            _gridTargetWorld = CellToWorldCenter(targetCell);

            float speed = _walkSpeed;
            if (CurrentState == PlayerState.Sneaking) speed = _sneakSpeed;
            else if (CurrentState == PlayerState.Running) speed = _runSpeed;
            Velocity = dir * speed;

            float duration = 1.0f / Mathf.Max(0.01f, _tilesPerSecond);

            _gridTween?.Kill();
            _gridTween = CreateTween();
            _gridTween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
            _gridTween.TweenProperty(this, "global_position", _gridTargetWorld, duration);
            _gridTween.Finished += () =>
            {
                GlobalPosition = _gridTargetWorld;
                Velocity = Vector2.Zero;
                _isGridMoving = false;
            };
        }
    }
}

