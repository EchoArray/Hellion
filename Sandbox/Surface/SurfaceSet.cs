using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Echo
{
    [DisallowMultipleComponent]
    sealed public class SurfaceSet : MonoBehaviour
    {
        #region Values
        public int subMeshIndexPreview;

        // A collection of the hightest triagle index for each sub mesh.
        private int[] _meshSubMeshTriangleRanges;
        [Header("NOTE: An editor script limits this to the sub mesh count of its associated renderer.")]
        // A collection of surface types relating to each sub mesh.
        [SerializeField]
        public SurfaceType[] surfaceTypes;

        // Defines the mesh of the renderer component associated with the game object of the surface set.
        private UnityEngine.Mesh _mesh;
        #endregion

        #region Unity Functions
        private void Awake()
        {
            Initialize();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
            Gizmos.DrawMesh(meshFilter.sharedMesh, subMeshIndexPreview, this.transform.position, this.transform.rotation);
        }
        #endregion

        #region Functions
        private void Initialize()
        {
            _mesh = MeshHelper.GetObjectMesh(this.gameObject);
            _meshSubMeshTriangleRanges = MeshHelper.GetMeshSubMeshTriangleRanges(_mesh);
        }

        public int GetSubMeshCount()
        {
            Mesh mesh = MeshHelper.GetObjectMesh(this.gameObject);
            if (mesh != null)
                return MeshHelper.GetMeshSubMeshTriangleRanges(mesh).Length;
            return 0;
        }
        public void SetSurfaceTypeCount(int count)
        {
            if (this.surfaceTypes.Length == count)
                return;

            List<SurfaceType> surfaceTypesList = new List<SurfaceType>(surfaceTypes);
            if (surfaceTypesList.Count > count)
            {
                int range = surfaceTypesList.Count - count;
                for (int i = 0; i < range; i++)
                    surfaceTypesList.RemoveAt(surfaceTypesList.Count - 1);
            }
            else
            {
                int range = count - surfaceTypesList.Count;
                surfaceTypesList.AddRange(new SurfaceType[range]);
            }
            surfaceTypes = surfaceTypesList.ToArray();
        }
        internal SurfaceType GetSurfaceTypeByTriangle(int triangleIndex)
        {
            if (_mesh == null)
                return SurfaceType.Generic;

            if (surfaceTypes.Length == 1)
                return surfaceTypes[0];

            //Get the sub mesh index of the supplied triangle index and find the surface type associated with it
            int subMeshIndex = MeshHelper.GetSubMeshIndexByTriangle(triangleIndex, _mesh.subMeshCount, _meshSubMeshTriangleRanges);
            return subMeshIndex >= surfaceTypes.Length ? SurfaceType.Generic : surfaceTypes[subMeshIndex];
        }
        #endregion
    }
}