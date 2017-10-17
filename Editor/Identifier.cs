using UnityEngine;
using UnityEditor;
using Echo;

public class CastableIdentifier : Editor
{
    public class SceneViewExtenderEditorIOHooks : UnityEditor.AssetModificationProcessor
    {
        public static string[] OnWillSaveAssets(string[] paths)
        {
            int uniqueId = 0;
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab");
            foreach (string prefab in prefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(prefab);
                UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object obj in objects)
                {
                    GameObject gameObject = obj as GameObject;
                    if (gameObject != null)
                    {
                        Castable castable = gameObject.GetComponent<Castable>();
                        if (castable != null)
                            castable.SetUnique(uniqueId);
                        Weapon weapon = gameObject.GetComponent<Weapon>();
                        if (weapon != null)
                            weapon.SetUnique(uniqueId);
                    }
                }
                uniqueId++;
            }
            return paths;
        }
    }
}

