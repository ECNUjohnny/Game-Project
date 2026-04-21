using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class AutoLoadLODTool : EditorWindow
{
    [MenuItem("Tools/从资源库自动抓取并挂载低模(_Low)")]
    public static void LoadAndParentLODs()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int successCount = 0;

        foreach (GameObject parentObj in selectedObjects)
        {
            string cleanName = Regex.Replace(parentObj.name, @"\s*\(\d+\)$", "");
            
    
            string targetLowName = cleanName + "_Low";

            if (parentObj.transform.Find(targetLowName)) continue;
    
            string[] guids = AssetDatabase.FindAssets(targetLowName + " t:Model");

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
              
                GameObject lowModelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (lowModelAsset != null)
                {
            
                    GameObject lowInstance = (GameObject)PrefabUtility.InstantiatePrefab(lowModelAsset);
                    
            
                    Undo.RegisterCreatedObjectUndo(lowInstance, "Auto Load LOD");
                    
                
                    Undo.SetTransformParent(lowInstance.transform, parentObj.transform, "Auto Load LOD");
                    lowInstance.transform.localPosition = Vector3.zero;
                    lowInstance.transform.localRotation = Quaternion.identity;
                    

                    MeshRenderer parentRenderer = parentObj.GetComponent<MeshRenderer>();
                    MeshRenderer childRenderer = lowInstance.GetComponent<MeshRenderer>();

                    if (parentRenderer != null)
                    {
                        Undo.RecordObject(childRenderer, "Sync Material");

                        childRenderer.sharedMaterials = parentRenderer.sharedMaterials;
                    }
                    else
                    {
                        Debug.Log("[{parentObj.name}] does not have MeshRenderer");
                    }

                    successCount++;
                }
            }
            else
            {
                Debug.LogWarning($"在资源库找不到名为 [{targetLowName}] 的模型，已跳过 [{parentObj.name}]。");
            }
        }

        Debug.Log($"抓取完成！成功为 {successCount} 个建筑自动生成并挂载了低模！");
    }
}