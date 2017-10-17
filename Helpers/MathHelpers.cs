using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public abstract class MathHelpers
{
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle > 180)
            angle = -(360 - angle);

        return Mathf.Clamp(angle, min, max);
    }

    public static float AngleDifference(float angle1, float angle2)
    {
        float diff = (angle2 - angle1 + 180) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    public static Vector3 Centroid(Vector3 start, Vector3 end)
    {
        return (start + end) / 2;
    }
    public static Vector3 Centroid(Vector3[] points)
    {
        Vector3 result = Vector3.zero;
        foreach (Vector3 point in points)
            result += point;
        result /= points.Length;
        return result;
    }

    public static Vector3 IntersectPosition(Vector3 intersectingPosition, Vector3 intersectingDirection, Vector3 linePosition, Vector3 lineDirection)
    {
        float a = Vector3.Dot(intersectingDirection, intersectingDirection);
        float b = Vector3.Dot(intersectingDirection, lineDirection);
        float e = Vector3.Dot(lineDirection, lineDirection);

        float d = a * e - b * b;

        Vector3 r = intersectingPosition - linePosition;
        float c = Vector3.Dot(intersectingDirection, r);
        float f = Vector3.Dot(lineDirection, r);

        float s = (b * f - c * e) / d;
        float t = (a * f - c * b) / d;

        return linePosition + lineDirection * t;
    }
}