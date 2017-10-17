using UnityEngine;
using System;
using System.Collections.Generic;

namespace Echo
{
    public static class ShapesHelper
    {
        // A collection of previously generated regular polys.
        private static Dictionary<int, Vector2[]> _storedRegularPolys = new Dictionary<int, Vector2[]>();

        internal static Vector2[] CreateRegularPoly2D(int sides)
        {
            // Returns a quantity of points around a circle at an equal distance from on another.
            // If sides are less than 3, return one point
            if (sides < 3)
                return new Vector2[] { Vector2.zero };

            if (_storedRegularPolys.ContainsKey(sides))
                return _storedRegularPolys[sides];

            // The collection of points to be defined and returned
            Vector2[] points = new Vector2[sides];

            // Find each point evenly around a circle
            float thetaDelta = (2f * Mathf.PI) * (1f / sides);
            for (int i = 0; i < sides; i++)
            {
                float iteration = thetaDelta * i;
                Vector3 point = Vector2.zero;
                point.x = Mathf.Cos(iteration);
                point.y = Mathf.Sin(iteration);

                points[i] = point;
            }
            _storedRegularPolys.Add(sides, points);
            return points;
        }
    }
}