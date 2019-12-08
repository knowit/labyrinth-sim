using UnityEngine;

public static class MessageUtils
{
    public static Quaternion ToQuaternion(this Vec2 vec2)
        => Quaternion.Euler(vec2.X, 0.0f, vec2.Y);

    public static Vec2 ToEulerRotationXZ(this Quaternion quat)
        => new Vec2 { X = quat.eulerAngles.x, Y = quat.eulerAngles.z };

    public static Vec2 ToNormalizedCoordinate(this Vector3 position)
    {
        var normalY = new Plane(Vector3.down, 100).ClosestPointOnPlane(position);
        return new Vec2
        {
            X = Mathf.InverseLerp(-16, 16, normalY.x),
            Y = Mathf.InverseLerp(-16, 16, normalY.z)
        };
    }
}

