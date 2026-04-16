using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BatchParentingTool : EditorWindow
{
    [MenuItem("Tools/一键批量归类子物体(匹配_Low)")]
    public static void GroupLODs()
    {
        GameObject[] allSelected = Selection.gameObjects;
        
    
        Dictionary<string, GameObject> mainBuildings = new Dictionary<string, GameObject>();
        List<GameObject> lod1Objects = new List<GameObject>();

        foreach (var obj in allSelected)
        {
            if (obj.name.EndsWith("_Low"))
                lod1Objects.Add(obj);
            else
                mainBuildings[obj.name] = obj;
        }

        int count = 0;
    
        foreach (var lod1 in lod1Objects)
        {
          
            string parentName = lod1.name.Replace("_Low", "");

            if (mainBuildings.ContainsKey(parentName))
            {
                GameObject parentObj = mainBuildings[parentName];
                
               
                Undo.SetTransformParent(lod1.transform, parentObj.transform, "Batch Parent");
                
                lod1.transform.localPosition = Vector3.zero;
                lod1.transform.localRotation = Quaternion.identity;
                lod1.transform.localScale = Vector3.one;
                
                count++;
            }
        }

        Debug.Log($"成功将 {count} 个 LOD1 模型设置为对应建筑的子物体！");
    }
}