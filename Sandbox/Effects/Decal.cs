using Echo.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// TODO: Add sub mesh based boolean
namespace Echo
{
    sealed public class Decal : Livable
    {
        #region Values
        // Defines the life span range of the decal.
        [SerializeField] private Vector2 _lifespanOverideRange;
        // Defines the color range of the decal.
        [Space(10)]
        [SerializeField] private Gradient _colorRange;

        // Defines the scale range of the decal.
        [SerializeField] private Vector2 _scaleRange;
        private float _scale;


        // Determines if the decal will only process the sub mesh it hit.
        [Space(10)]
        [SerializeField] private bool _onlyBooleanHitSubMesh = true;
        // Determines if the decal will project on an existing decal.
        [SerializeField] private bool _projectInBoundsOfExisitingDecals = true;
        // Defines the distance in-which the decal will be offset from the intersecting surface.
        [SerializeField] private float _surfaceOffset = 0.0005f;
        // Defines the max angle that the decal will project onto the intersecting surface.
        [SerializeField] private float _maxAngle = 60;


        public enum UVDivisionType
        {
            X1,
            X4
        }
        // Determines how the texture used is to be spliced when generating UVs.
        [Space(10)]
        [SerializeField]
        private UVDivisionType _uVDivision;

        // Defines the decals base material.
        [Space(5)]
        [SerializeField]
        private Material _material;
        // Defines the instantiated material of the renderer.
        private Material _materialInstance;
        // Defines the property name of the decals material that is to be set after determining color.
        [SerializeField] private string _materialColorProperty = "_TintColor";
        // Defines the property name of the decals material that is to be set after determining surface color.
        [SerializeField] private string _materialLightmapColorProperty = "_LightmapColor";
        // Defines the property name of the decals material fade start time.
        [SerializeField] private string _materialFadeStartProperty = "StartFadeTime";
        // Defines the property name of the decals material fade end time.
        [SerializeField] private string _materialFadeEndProperty = "_EndFadeTime";


        // Defines the mesh filter component of the game object.
        private MeshFilter _meshFilter;
        // Defines the mesh renderer component of the game object.
        private MeshRenderer _meshRenderer;


        // Contain each mesh aspect while building
        private List<Vector3> _bufferVertices = new List<Vector3>();
        private List<Vector3> _bufferNormals = new List<Vector3>();
        private List<Vector2> _bufferTexCoords = new List<Vector2>();
        private List<int> _bufferIndices = new List<int>();
        #endregion

        #region Unity Functions
        protected override void Awake()
        {
            if (!DecalManager.singleton.AllowDecals)
            {
                Destroy(this.gameObject);
                return;
            }

            Globals.singleton.Contain(this);
            Cast();
        }

        private void OnDestroy()
        {
            Destroy(_materialInstance);
        }
        private void OnDrawGizmosSelected()
        {
            // Show the decals size, position and direction
            Matrix4x4 matrixBackup = Gizmos.matrix;

            Gizmos.matrix = transform.localToWorldMatrix;
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

                Gizmos.color = new Color(0, 1, 1, 0.25f);
                Gizmos.DrawCube(Vector3.zero, Vector3.one);

                Gizmos.color = Color.cyan;
                if (_meshFilter.sharedMesh != null)
                {

                    Gizmos.DrawMesh(_meshFilter.sharedMesh);

                    Vector3[] vertices = _meshFilter.sharedMesh.vertices;
                    int[] triangles = _meshFilter.sharedMesh.triangles;

                    for (int i = 0; i < _meshFilter.sharedMesh.triangles.Length; i += 3)
                    {
                        // Define indices
                        int indice0 = triangles[i];
                        int indice1 = triangles[i + 1];
                        int indice2 = triangles[i + 2];

                        // Define and offset verts to world space
                        Vector3 vertice0 = vertices[indice0];
                        Vector3 vertice1 = vertices[indice1];
                        Vector3 vertice2 = vertices[indice2];

                        Gizmos.DrawLine(vertice0, vertice1);
                        Gizmos.DrawLine(vertice1, vertice2);
                        Gizmos.DrawLine(vertice2, vertice0);
                    }
                }

            }
            Gizmos.matrix = matrixBackup;

            Gizmos.color = Color.cyan;
            float offsetExtent = this.transform.lossyScale.z / 2;
            Gizmos.DrawLine(this.transform.position - (offsetExtent * this.transform.forward), this.transform.position);
            Gizmos.DrawWireSphere(this.transform.position, 0.1f);

        }
        #endregion

        #region Functions
        internal override void Cast()
        {
            _scale = Random.Range(_scaleRange.x, _scaleRange.y);
            if (!_projectInBoundsOfExisitingDecals)
            {
                if (CheckOverlap())
                {
                    Destroy(this.gameObject);
                    return;
                }
            }

            DecalManager.singleton.AddToQueue(this);
        }
        
        private void Initialize()
        {
            if(_lifespanOverideRange.magnitude !=0)
                _lifespan = Random.Range(_lifespanOverideRange.x, _lifespanOverideRange.y);

            // Define the mesh renderer and its defaults
            _meshRenderer = this.gameObject.AddComponent<MeshRenderer>();
            _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Define the material and its defaults
            Color colorInRange = _colorRange.Evaluate(UnityEngine.Random.Range(0, 1f));

            _materialInstance = new Material(_material);
            _materialInstance.SetColor(_materialColorProperty, colorInRange);
            _materialInstance.SetFloat(_materialFadeStartProperty, Time.timeSinceLevelLoad);
            _materialInstance.SetFloat(_materialFadeEndProperty, Time.timeSinceLevelLoad + _lifespan);
            _meshRenderer.material = _materialInstance;

            // _materialInstance.SetColor(MaterialLightmapColorProperty, lightmapColor);

            _meshFilter = this.gameObject.AddComponent<MeshFilter>();

            // Define the life span of the decal
            SoftStartLifespan();

        }

        internal void Project()
        {
            Initialize();

            if (_meshFilter.sharedMesh != null)
                return;

            this.transform.localScale = Vector3.one * _scale;
            // Randomly rotate the decal before projection
            this.transform.eulerAngles += new Vector3(180, 0, Random.Range(0f, 360f));
            
            RaycastHit raycastHit;
            bool hit = Physics.Raycast(this.transform.position, this.transform.forward, out raycastHit, _scale);

            if (!hit)
            {
                Destroy(this.gameObject);
                return;
            }
            else if (raycastHit.collider.gameObject.GetComponent<Rigidbody>() != null)
            {
                Destroy(this.gameObject);
                return;
            }
            else if (raycastHit.collider.gameObject.layer != Globals.STRUCTURE_LAYER)
            {
                Destroy(this.gameObject);
                return;
            }
            UnityEngine.Mesh surfaceMesh = MeshHelper.GetObjectMesh(raycastHit.collider.gameObject);
            if (surfaceMesh == null)
            {
                Destroy(this.gameObject);
                return;
            }
            BooleanMesh(surfaceMesh, raycastHit);
        }
        private bool CheckOverlap()
        {
            Decal[] decals = FindObjectsOfType<Decal>();
            foreach (Decal decal in decals)
            {
                if (decal._meshFilter == null || decal._meshFilter.sharedMesh == null)
                    continue;

                Bounds bounds = new Bounds(this.transform.position, (Vector3.one * _scale) / 2);
                Bounds bounds2 = new Bounds(decal.transform.position, decal._meshFilter.sharedMesh.bounds.extents / 2);
                bool intersects = bounds.Intersects(bounds2);
                if (intersects)
                    return decal._uniqueId == _uniqueId;
            }
            return false;
        }

        private void FinalizeMesh()
        {
            Vector2 spritePosition = new Vector2(Random.Range(0, 2), Random.Range(0, 2));

            for (int i = 0; i < _bufferVertices.Count; i++)
            {
                OffsetVertice(i);
                MapTexCoord(i, spritePosition);
            }
        }
        private void CompositeMesh()
        {
            if (_bufferIndices.Count == 0)
                return;

            UnityEngine.Mesh mesh = new UnityEngine.Mesh();

            mesh.name = this.gameObject.name;

            mesh.vertices = _bufferVertices.ToArray();
            mesh.normals = _bufferNormals.ToArray();
            mesh.uv = _bufferTexCoords.ToArray();
            mesh.uv2 = _bufferTexCoords.ToArray();
            mesh.triangles = _bufferIndices.ToArray();

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            _bufferVertices.Clear();
            _bufferNormals.Clear();
            _bufferTexCoords.Clear();
            _bufferIndices.Clear();

            _meshFilter.mesh = mesh;
        }
        private void BooleanMesh(UnityEngine.Mesh surfaceMesh, RaycastHit raycastHit)
        {
            // Define the surfaces vertices and triangles
            Vector3[] vertices = surfaceMesh.vertices;
            int[] triangles = surfaceMesh.triangles;

            int startTriangle = 0;
            int endTriangle = triangles.Length;
            if (_onlyBooleanHitSubMesh)
            {
                int[] subMeshRanges = MeshHelper.GetMeshSubMeshTriangleRanges(surfaceMesh);
                int subMeshIndex = MeshHelper.GetSubMeshIndexByTriangle(raycastHit.triangleIndex, surfaceMesh.subMeshCount, subMeshRanges);
                MeshHelper.GetSubMeshRange(subMeshRanges, subMeshIndex, out startTriangle, out endTriangle);
            }

            // Define clipping planes
            Plane right = new Plane(Vector3.right, Vector3.right / 2f);
            Plane left = new Plane(-Vector3.right, -Vector3.right / 2f);
            Plane top = new Plane(Vector3.up, Vector3.up / 2f);
            Plane bottom = new Plane(-Vector3.up, -Vector3.up / 2f);
            Plane front = new Plane(Vector3.forward, Vector3.forward / 2f);
            Plane back = new Plane(-Vector3.forward, -Vector3.forward / 2f);

            // Define vertex matrix
            Matrix4x4 matrix = this.transform.worldToLocalMatrix * raycastHit.collider.transform.localToWorldMatrix;

            for (int i = startTriangle; i < endTriangle; i += 3)
            {
                // Define indices
                int indice0 = triangles[i];
                int indice1 = triangles[i + 1];
                int indice2 = triangles[i + 2];

                // Define and offset verts to world space
                Vector3 vertice0 = matrix.MultiplyPoint(vertices[indice0]);
                Vector3 vertice1 = matrix.MultiplyPoint(vertices[indice1]);
                Vector3 vertice2 = matrix.MultiplyPoint(vertices[indice2]);

                Vector3 normal = GetTriangleNormal(vertice0, vertice1, vertice2);

                if (Vector3.Angle(-Vector3.forward, normal) >= _maxAngle)
                    continue;

                List<Vector3> poly = new List<Vector3> { vertice0, vertice1, vertice2 };

                ClipPoly(ref poly, right);
                if (poly.Count == 0)
                    continue;
                ClipPoly(ref poly, left);
                if (poly.Count == 0)
                    continue;

                ClipPoly(ref poly, top);
                if (poly.Count == 0)
                    continue;
                ClipPoly(ref poly, bottom);
                if (poly.Count == 0)
                    continue;

                ClipPoly(ref poly, front);
                if (poly.Count == 0)
                    continue;
                ClipPoly(ref poly, back);
                if (poly.Count == 0)
                    continue;

                AddPoly(poly, normal);
            }

            FinalizeMesh();
            CompositeMesh();
        }

        private Vector3 GetTriangleNormal(Vector3 vertice0, Vector3 vertice1, Vector3 vertice2)
        {
            // Define triangle normal
            Vector3 side1 = vertice1 - vertice0;
            Vector3 side2 = vertice2 - vertice0;
            return Vector3.Cross(side1, side2).normalized;
        }

        private void AddPoly(List<Vector3> vertices, Vector3 normal)
        {
            int indice0 = AddVertex(vertices[0], normal);
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                int indice1 = AddVertex(vertices[i], normal);
                int indice2 = AddVertex(vertices[i + 1], normal);

                _bufferIndices.Add(indice0);
                _bufferIndices.Add(indice1);
                _bufferIndices.Add(indice2);
            }
        }
        private void ClipPoly(ref List<Vector3> vertices, Plane plane)
        {
            bool[] positive = new bool[9];
            int positiveCount = 0;

            for (int i = 0; i < vertices.Count; i++)
            {
                positive[i] = !plane.GetSide(vertices[i]);
                if (positive[i])
                    positiveCount++;
            }

            if (positiveCount == 0)
            {
                vertices = new List<Vector3>();
                return;
            }
            if (positiveCount == vertices.Count)
                return;

            List<Vector3> tempVertices = new List<Vector3>();

            for (int i = 0; i < vertices.Count; i++)
            {
                int next = i + 1;
                next %= vertices.Count;

                if (positive[i])
                    tempVertices.Add(vertices[i]);

                if (positive[i] != positive[next])
                {
                    Vector3 v1 = vertices[next];
                    Vector3 v2 = vertices[i];

                    Vector3 v = LineCast(plane, v1, v2);
                    tempVertices.Add(v);
                }
            }
            vertices = tempVertices;
        }

        private int FindVertex(Vector3 vertex)
        {
            for (int i = 0; i < _bufferVertices.Count; i++)
            {
                if ((_bufferVertices[i] - vertex).sqrMagnitude < 0.001f)
                    return i;
            }
            return -1;
        }
        private int AddVertex(Vector3 vertex, Vector3 normal)
        {
            int index = FindVertex(vertex);
            if (index == -1)
            {
                _bufferVertices.Add(vertex);
                _bufferNormals.Add(normal);
                index = _bufferVertices.Count - 1;
            }
            else
            {
                Vector3 t = _bufferNormals[index] + normal;
                _bufferNormals[index] = t.normalized;
            }
            return index;
        }

        private Vector3 LineCast(Plane plane, Vector3 a, Vector3 b)
        {
            float distance;
            Ray ray = new Ray(a, b - a);
            plane.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }

        private void OffsetVertice(int verticeIndex)
        {
            _bufferVertices[verticeIndex] += _bufferNormals[verticeIndex] * _surfaceOffset;
        }
        private void MapTexCoord(int verticeIndex, Vector2 spritePosition)
        {
            Vector2 uv = new Vector2(_bufferVertices[verticeIndex].x + 0.5f, _bufferVertices[verticeIndex].y + 0.5f);

            if (_uVDivision == UVDivisionType.X1)
                _bufferTexCoords.Add(uv);
            else
            {
                uv.x = Mathf.Lerp(0.5f, spritePosition.x, uv.x);
                uv.y = Mathf.Lerp(0.5f, spritePosition.y, uv.y);
                _bufferTexCoords.Add(uv);
            }
        }
        #endregion
    }
}