using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class ExtensionMethods
{
    #region Vector Math
    internal static float Towards(this float f, float target, float stepAmount)
    {
        // Make sure we have our step in the appropriate direction.
        stepAmount = Mathf.Abs(stepAmount);
        float difference = Mathf.Abs(f - target);
        if (difference < stepAmount)
            stepAmount = difference;
        if (target < f)
            stepAmount = -stepAmount;
       
        return f + stepAmount;
    }
    /// <summary>
    /// Takes the given value and randomly returns it either as it was, or negative.
    /// </summary>
    /// <param name="x">The value to randomly negate.</param>
    /// <returns>Randomly will return the original value or a negative counterpart to it.</returns>
    internal static float RandomDirection(this float x)
    {
        int direction = UnityEngine.Random.Range(0, 2);
        return (direction == 0) ? -x : x;
    }
    /// <summary>
    /// Takes the given value and randomly returns it either as it was, or negative.
    /// </summary>
    /// <param name="x">The value to randomly negate.</param>
    /// <returns>Randomly will return the original value or a negative counterpart to it.</returns>
    internal static int RandomDirection(this int x)
    {
        int direction = UnityEngine.Random.Range(0, 2);
        return (direction == 0) ? -x : x;
    }
    /// <summary>
    /// Takes the given value and randomly returns it either as it was, or negative.
    /// </summary>
    /// <param name="x">The value to randomly negate.</param>
    /// <returns>Randomly will return the original value or a negative counterpart to it.</returns>
    internal static short RandomDirection(this short x)
    {
        int direction = UnityEngine.Random.Range(0, 2);
        return (short)((direction == 0) ? -x : x);
    }
    /// <summary>
    /// Returns the given vector with specified components randomly flipped negative.
    /// </summary>
    /// <param name="v">The vector to go in random directions.</param>
    /// <param name="x">Describes if we should consider negating the x component.</param>
    /// <param name="y">Describes if we should consider negating the y component.</param>
    /// <param name="z">Describes if we should consider negating the z component.</param>
    /// <returns>Returns the vector with certain components randomly original, or negative.</returns>
    internal static Vector3 RandomDirections(this Vector3 v, bool x=true, bool y=true, bool z=true)
    {
        float vX = x ? v.x.RandomDirection() : v.x;
        float vY = y ? v.y.RandomDirection() : v.y;
        float vZ = z ? v.z.RandomDirection() : v.z;
        return new Vector3(vX, vY, vZ);
    }

    internal static Vector3 AtSlope(this Vector3 direction, Vector3 slopeNormal)
    {
        return direction - Vector3.Dot(slopeNormal, direction) * slopeNormal;
    }

    /// <summary>
    /// Calculates the centroid of an array of Vector3's.
    /// </summary>
    /// <param name="points">An array of Vector3 points.</param>
    /// <returns>A Vector3 at the center of the array argument.</returns>
    internal static Vector3 Centroid(this Vector3[] points)
    {  
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
            centroid += point;
        return centroid /= points.Length;
    }
    /// <summary>
    /// Calculates the centroid of a list of Vector3's.
    /// </summary>
    /// <param name="points">An array of Vector3 points.</param>
    /// <returns>A Vector3 at the center of the array argument.</returns>
    internal static Vector3 Centroid(this List<Vector3> points)
    {
        Vector3 centroid = Vector3.zero;
        foreach (Vector3 point in points)
            centroid += point;
        return centroid /= points.Count;
    }


    internal static Vector3 Average(this Vector3[] points)
    {
        float x = 0;
        float y = 0;
        float z = 0;
        foreach (Vector3 point in points)
        {
            x += point.x;
            y += point.y;
            z += point.z;
        }
        return new Vector3(x / points.Length, y / points.Length, z / points.Length);
    }

    internal static void ResetLocal(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;
    }
    internal static void Reset(this Transform transform)
    {
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;
    }

    internal static Vector3 Divide(this Vector3 point, Vector3 d)
    {
        point.x /= d.x;
        point.y /= d.y;
        point.z /= d.z;
        return point;
    }

    internal static Vector3 Rotate(this Vector3 vector, float angle, Vector3 axis, Space space=Space.Self)
    {
        return Quaternion.Euler(vector).Rotate(angle, axis, space).eulerAngles;
    }
    internal static Quaternion Rotate(this Quaternion quaternion, float angle, Vector3 axis, Space space=Space.Self)
    {
        if(space == Space.Self)
            return (quaternion * Quaternion.AngleAxis(angle, axis));
        else
            return (Quaternion.AngleAxis(angle, axis) * quaternion);
    }

    internal static Vector3 RotateAround(this Vector3 point, Vector3 pivot, Quaternion angle)
    {
        Vector3 direction = point - pivot;
        direction = angle * direction;
        point = direction + pivot;
        return point;
    }
    internal static Vector3[] RotateAround(this Vector3[] points, Vector3 pivot, Quaternion angle)
    {
        Vector3[] transformedPoints = (Vector3[])points.Clone();

        for (int i = 0; i < transformedPoints.Length; i++)
        {
            Vector3 direction = transformedPoints[i] - pivot;
            direction = angle * direction;
            transformedPoints[i] = direction + pivot;
        }
        return transformedPoints;
    }
    internal static List<Vector3> RotateAround(this List<Vector3> points, Vector3 pivot, Quaternion angle)
    {
        // This is likely dumb as fuck
        Vector3[] transformedPointsArray = new Vector3[] { };
        points.CopyTo(transformedPointsArray);
        List<Vector3> transformedPoints = transformedPointsArray.ToList<Vector3>();

        for (int i = 0; i < transformedPoints.Count; i++)
        {
            Vector3 direction = transformedPoints[i] - pivot;
            direction = angle * direction;
            transformedPoints[i] = direction + pivot;
        }
        return transformedPoints;
    }
    #endregion

    #region Game Objects
    internal static void SetVisibility(this MonoBehaviour gameObject, bool visible, bool thisMeshRenderers=true, bool childMeshRenderers=false, bool parentMeshRenderers=false)
    {
        // List of mesh renderers.
        MeshRenderer[] meshRenderers;

        // Set visibility for the game objects mesh components
        if (thisMeshRenderers)
        {
            meshRenderers = gameObject.GetComponents<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.enabled = visible;
        }

        // Set visibility for child mesh renderers
        if (childMeshRenderers)
        {
            meshRenderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.enabled = visible;
        }

        // Set visibility for parent mesh renderers.
        if (parentMeshRenderers)
        {
            meshRenderers = gameObject.GetComponentsInParent<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
                meshRenderer.enabled = visible;
        }
    }

    internal static bool IsVisibleFrom(this Renderer renderer, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }
    #endregion

    #region Primitives/Simple Types
    internal static string ToHexString(this byte[] array)
    {
        string result = "";
        foreach (byte b in array)
            result += b.ToString("X2");
        return result;
    }
    internal static byte[] ToArrayFromHexString(this string hexstring)
    {
        byte[] data = new byte[hexstring.Length / 2];
        for (int i = 0; i < hexstring.Length; i += 2)
            data[i / 2] = Convert.ToByte(hexstring.Substring(i, 2), 16);
        return data;
    }
    internal static bool IsType(this Type type, Type c)
    {
        return c == type || type.IsSubclassOf(c);
    }
    internal static string ToArgbString(this Color color)
    {
        return ((byte)(color.r * 255)).ToString("X2") + ((byte)(color.g * 255)).ToString("X2") + ((byte)(color.b * 255)).ToString("X2") + ((byte)(color.a * 255)).ToString("X2");
    }

    internal static int Append(this int n, int i)
    {
        int c = 1;
        while (c <= i) c *= 10;
        return n * c + i;
    }
    #endregion

    #region Member/Property/Field Info
    internal static System.Object GetValue(this MemberInfo mi, System.Object parentObj)
    {
        if(mi.MemberType == MemberTypes.Property)
            return ((PropertyInfo)mi).GetValue(parentObj, null);
        else if(mi.MemberType == MemberTypes.Field)
            return ((FieldInfo)mi).GetValue(parentObj);
        else
            throw new ArgumentException(string.Format("MemberInfo.GetValue() does not support this type: {0}", mi.MemberType.ToString()));
    }
    internal static void SetValue(this MemberInfo mi, System.Object parentObj, System.Object value)
    {
        if (mi.MemberType == MemberTypes.Property)
             ((PropertyInfo)mi).SetValue(parentObj, value, null);
        else if (mi.MemberType == MemberTypes.Field)
             ((FieldInfo)mi).SetValue(parentObj, value);
        else
            throw new ArgumentException(string.Format("MemberInfo.SetValue() does not support this type: {0}", mi.MemberType.ToString()));
    }
    internal static Type GetUnderlyingValueType(this MemberInfo mi)
    {
        if (mi.MemberType == MemberTypes.Property)
            return ((PropertyInfo)mi).PropertyType;
        else if (mi.MemberType == MemberTypes.Field)
            return ((FieldInfo)mi).FieldType;
        else
            throw new ArgumentException(string.Format("MemberInfo.GetUnderlyingValueType() does not support this type: {0}", mi.MemberType.ToString()));
    }
    #endregion

    internal static float EvaluateByDistance(this AnimationCurve aC, float position, float maxPosition)
    {
        float time = maxPosition == 0 ? 1 : Mathf.Max(position / maxPosition, 0);
        return aC.Evaluate(time);
    }
}