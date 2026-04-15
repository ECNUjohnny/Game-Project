using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class BatchParentingTool : EditorWindow
{
    [MenuItem("Tools/一键批量归类子物体(匹配_Low)")]
    public static void GroupLODs()
    {
        // 1. 获取场景中所有选中的物体
        GameObject[] allSelected = Selection.gameObjects;
        
        // 分类：存入字典方便快速查找
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
        // 2. 开始匹配并设为子物体
        foreach (var lod1 in lod1Objects)
        {
            // 移除后缀寻找父物体名字：例如 "Church_01_Low" -> "Church_01"
            string parentName = lod1.name.Replace("_Low", "");

            if (mainBuildings.ContainsKey(parentName))
            {
                GameObject parentObj = mainBuildings[parentName];
                
                // 记录撤销操作，防止误操作
                Undo.SetTransformParent(lod1.transform, parentObj.transform, "Batch Parent");
                
                // 坐标归零，确保模型重合
                lod1.transform.localPosition = Vector3.zero;
                lod1.transform.localRotation = Quaternion.identity;
                lod1.transform.localScale = Vector3.one;
                
                count++;
            }
        }

        Debug.Log($"成功将 {count} 个 LOD1 模型设置为对应建筑的子物体！");
    }
}