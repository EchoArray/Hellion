using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Echo;

[CustomEditor(typeof(SurfaceSet))]
sealed public class SurfaceSetTool : Editor
{
    private Vector3 _clickPosition;
    private void OnDisable()
    {
        SurfaceSet surfaceSet = (SurfaceSet)target;
        int subMeshCount = surfaceSet.GetSubMeshCount();
        surfaceSet.subMeshIndexPreview = subMeshCount + 1;
    }

    private void OnSceneGUI()
    {
        SurfaceSet surfaceSet = (SurfaceSet)target;
        int subMeshCount = surfaceSet.GetSubMeshCount();
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && !Event.current.alt)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit raycastHit;
            bool hit = Physics.Raycast(ray, out raycastHit);
            if (hit)
            {
                _clickPosition = raycastHit.point;
                Mesh mesh = MeshHelper.GetObjectMesh(surfaceSet.gameObject);

                int[] meshSubMeshTriangleRanges = MeshHelper.GetMeshSubMeshTriangleRanges(mesh);
                int selectionIndex = MeshHelper.GetSubMeshIndexByTriangle(raycastHit.triangleIndex, subMeshCount, meshSubMeshTriangleRanges);
                if (surfaceSet.subMeshIndexPreview == selectionIndex)
                    surfaceSet.subMeshIndexPreview = subMeshCount + 1;
                else
                    surfaceSet.subMeshIndexPreview = selectionIndex;
            }
        }

        if (surfaceSet.subMeshIndexPreview < surfaceSet.surfaceTypes.Length)
        {
            GUIStyle gUIStlye = new GUIStyle();
            gUIStlye.normal.textColor = Color.white;
            Handles.Label(_clickPosition, "Type: " + surfaceSet.surfaceTypes[surfaceSet.subMeshIndexPreview].ToString() + " (Index : " + surfaceSet.subMeshIndexPreview.ToString() + ")", gUIStlye);

            Handles.BeginGUI();
            Vector2 enumPosition = HandleUtility.WorldToGUIPoint(_clickPosition);

            surfaceSet.surfaceTypes[surfaceSet.subMeshIndexPreview] = (SurfaceType)EditorGUI.EnumPopup(new Rect(enumPosition.x, enumPosition.y + 20, 100, 50), surfaceSet.surfaceTypes[surfaceSet.subMeshIndexPreview]);
            Handles.EndGUI();
        }
        surfaceSet.SetSurfaceTypeCount(subMeshCount);

    }
}
