using UnityEngine;
namespace Echo
{
    public static class MeshHelper
    {
        public static int[] GetMeshSubMeshTriangleRanges(UnityEngine.Mesh mesh)
        {
            if (mesh == null)
                return new int[0];

            // Get the highest triangle of each submesh
            int[] subMeshRanges = new int[mesh.subMeshCount];

            for (int i = 0; i < mesh.subMeshCount; i++)
                subMeshRanges[i] = (mesh.GetTriangles(i).Length / 3) + (i == 0 ? 0 : subMeshRanges[i - 1]);

            return subMeshRanges;
        }

        public static int GetSubMeshIndexByTriangle(int index, int subMeshCount, int[] subMeshHighestTriangles)
        {
            // Determine submesh by index ranges
            for (int i = 0; i < subMeshCount; i++)
                if (index <= subMeshHighestTriangles[i] - 1)
                    return i;
            return 0;
        }
        public static void GetSubMeshRange(int[] ranges, int index, out int start, out int end)
        {
            start = (index == 0 ? 0 : ranges[index - 1]) * 3;
            end = ranges[index] * 3;
        }

        public static UnityEngine.Mesh GetObjectMesh(GameObject gameObject)
        {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();

            if (meshFilter != null)
                return meshFilter.sharedMesh;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform transform = gameObject.transform.GetChild(i);

                SkinnedMeshRenderer skinnedMeshRenderer = transform.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                    return skinnedMeshRenderer.sharedMesh;
                else
                {
                    MeshFilter childObjectMeshFilter = transform.GetComponent<MeshFilter>();
                    if (childObjectMeshFilter != null)
                        return childObjectMeshFilter.sharedMesh;
                }
            }
            return null;
        }
        public static object GetObjectMeshRenderer(GameObject gameObject)
        {
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (meshRenderer != null)
                return meshRenderer;

            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                Transform transform = gameObject.transform.GetChild(i);

                SkinnedMeshRenderer skinnedMeshRenderer = transform.GetComponent<SkinnedMeshRenderer>();
                if (skinnedMeshRenderer != null)
                    return skinnedMeshRenderer;
                else
                {
                    MeshRenderer childObjectMeshRenderer = transform.GetComponent<MeshRenderer>();
                    if (childObjectMeshRenderer != null)
                        return childObjectMeshRenderer;
                }
            }
            return null;
        }

    }
}