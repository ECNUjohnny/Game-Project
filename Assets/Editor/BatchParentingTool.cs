using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class AutoLoadLODTool : EditorWindow
{
    [MenuItem("Tools/从资源库自动抓取并挂载低模(_Low)")]
    public static void LoadAndParentLODs()
    {
        // 1. 定义你要限定搜索的文件夹路径
        // 注意：路径必须以 "Assets/" 开头，且不要以 "/" 结尾
        // 你可以根据实际情况修改这个路径，也可以在数组里写多个路径
        string[] searchFolders = new string[] { "Assets/Models/Western Model" }; 

        GameObject[] selectedObjects = Selection.gameObjects;
        int successCount = 0;

        foreach (GameObject parentObj in selectedObjects)
        {
            string cleanName = Regex.Replace(parentObj.name, @"\s*\(\d+\)$", "");
            string targetLowName = cleanName + "_Low";

            if (parentObj.transform.Find(targetLowName)) continue;
    
            // 2. 将 searchFolders 作为第二个参数传入，限定搜索范围
            string[] guids = AssetDatabase.FindAssets(targetLowName + " t:Model", searchFolders);

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

                    // 建议同时检查 childRenderer 是否也存在，防止低模本身没有 MeshRenderer 报错
                    if (parentRenderer != null && childRenderer != null) 
                    {
                        Undo.RecordObject(childRenderer, "Sync Material");
                        childRenderer.sharedMaterials = parentRenderer.sharedMaterials;
                    }
                    else
                    {
                        Debug.Log($"[{parentObj.name}] 或其低模 does not have MeshRenderer");
                    }

                    successCount++;
                }
            }
            else
            {
                Debug.LogWarning($"在指定文件夹中找不到名为 [{targetLowName}] 的模型，已跳过 [{parentObj.name}]。");
            }
        }

        Debug.Log($"抓取完成！成功为 {successCount} 个建筑自动生成并挂载了低模！");
    }
}