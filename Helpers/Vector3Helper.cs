
using UnityEngine;

namespace Echo
{
    internal class Vector3Helper
    {
        internal static Vector3 GetDirection(Direction direction, Transform transform = null)
        {
            switch (direction)
            {
                default:
                    return Vector3.zero;

                // World
                case Direction.WorldUp:
                    return Vector3.up;
                case Direction.WorldDown:
                    return Vector3.down;
                case Direction.WorldForward:
                    return Vector3.forward;
                case Direction.WorldBackward:
                    return Vector3.back;
                case Direction.WorldLeft:
                    return Vector3.left;
                case Direction.WorldRight:
                    return Vector3.right;

                // Local
                case Direction.LocalUp:
                    return transform.up;
                case Direction.LocalDown:
                    return -transform.up;
                case Direction.LocalForward:
                    return transform.forward;
                case Direction.LocalBackward:
                    return -transform.forward;
                case Direction.LocalLeft:
                    return -transform.right;
                case Direction.LocalRight:
                    return transform.right;
            }
        }

        internal static Vector3 GetWorldDirection(WorldDirection direction)
        {
            switch (direction)
            {
                default:
                    return Vector3.zero;

                // World
                case WorldDirection.Up:
                    return Vector3.up;
                case WorldDirection.Down:
                    return Vector3.down;
                case WorldDirection.Forward:
                    return Vector3.forward;
                case WorldDirection.Backward:
                    return Vector3.back;
                case WorldDirection.Left:
                    return Vector3.left;
                case WorldDirection.Right:
                    return Vector3.right;
            }
        }

        internal static Vector3 GetLocalDirection(LocalDirection direction, Transform transform)
        {
            switch (direction)
            {
                default:
                    return Vector3.zero;
                // Local
                case LocalDirection.Up:
                    return transform.up;
                case LocalDirection.Down:
                    return -transform.up;
                case LocalDirection.Forward:
                    return transform.forward;
                case LocalDirection.Backward:
                    return -transform.forward;
                case LocalDirection.Left:
                    return -transform.right;
                case LocalDirection.Right:
                    return transform.right;
            }
        }
    }
}