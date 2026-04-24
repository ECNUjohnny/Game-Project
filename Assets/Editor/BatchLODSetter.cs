using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BatchLODSetter : MonoBehaviour
{
    [MenuItem("Tools/一键为物体创建LOD组件(自适应尺寸)")]
    public static void SetLod()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0) return;

        // 【Undo核心 1】把这次批量操作打包成一个“撤销组”
        // 这样按一次 Ctrl+Z，就能把几百个物体的修改一次性全部撤销，而不是按几百次
        Undo.SetCurrentGroupName("批量自适应LOD设置");
        int undoGroupIndex = Undo.GetCurrentGroup();

        int successcnt = 0;

        foreach (GameObject obj in selectedObjects)
        {
            // 尝试获取 LODGroup
            if (!obj.TryGetComponent(out LODGroup lodGroup))
            {
                // 【Undo核心 2：添加组件】
                // 如果没有，使用 Undo 系统来安全添加。撤销时，这个组件会被干净地移除。
                lodGroup = Undo.AddComponent<LODGroup>(obj);
            }
            else
            {
                // 【Undo核心 3：记录旧状态】
                // 如果已经有了，在修改它的任何属性【之前】，先给它拍个快照！
                Undo.RecordObject(lodGroup, "覆盖现有 LODGroup");
            }

            List<Renderer> lod0Renderers = new List<Renderer>();
            MeshRenderer parentRenderer = obj.GetComponent<MeshRenderer>();
            if (parentRenderer != null) lod0Renderers.Add(parentRenderer);

            List<Renderer> lod1Renderers = new List<Renderer>();
            
            // 遍历寻找 _Low
            foreach (Transform child in obj.transform)
            {
                if (child.name.EndsWith("_Low") || child.name.Contains("_Low"))
                {
                    if (child.TryGetComponent(out MeshRenderer childRenderer))
                    {
                        lod1Renderers.Add(childRenderer);
                    }
                }
                else if (parentRenderer == null)
                {
                    if (child.TryGetComponent(out MeshRenderer cRen))
                    {
                        lod0Renderers.Add(cRen);
                    }
                }
            }

            // 计算最大尺寸
            float maxDimension = 1.0f;
            if (lod0Renderers.Count > 0)
            {
                Bounds combinedBounds = lod0Renderers[0].bounds;
                for (int i = 1; i < lod0Renderers.Count; i++)
                {
                    combinedBounds.Encapsulate(lod0Renderers[i].bounds);
                }
                maxDimension = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
            }

            float lod0Threshold = 0;
            // 动态计算阈值
            if (maxDimension >= 5.0f) lod0Threshold = 0.4f;

            else if (3.0f <= maxDimension && maxDimension < 5.0f) lod0Threshold = 0.35f;

            else if (1.0f <= maxDimension && maxDimension < 3.0f) lod0Threshold = 0.25f;

            else if (0.5f <= maxDimension && maxDimension < 1.0f) lod0Threshold = 0.2f;

            else lod0Threshold = 0.15f; 

            float culledThreshold = lod0Threshold * 0.35f;

            LOD[] lods;
            if (lod1Renderers.Count > 0)
            {
                lods = new LOD[2];
                lods[0] = new LOD(lod0Threshold, lod0Renderers.ToArray());
                lods[1] = new LOD(culledThreshold, lod1Renderers.ToArray());
            }
            else
            {
                lods = new LOD[1];
                lods[0] = new LOD(culledThreshold, lod0Renderers.ToArray());
            }
            
            // 因为前面已经执行了 Undo.RecordObject，这里的赋值会被安全监控
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds(); 
            
            // 当你使用 Undo.RecordObject 时，Unity 会自动把物体标记为 Dirty（已修改）
            // 所以 EditorUtility.SetDirty(obj) 其实可以省略了，Undo 系统接管了它。
            
            successcnt++;
        }

        // 【Undo核心 4】闭合撤销组
        Undo.CollapseUndoOperations(undoGroupIndex);

        Debug.Log($"<color=green>[成功]</color> {successcnt} 个物体已配置LOD。如果不满意，请按 Ctrl+Z 撤销！");
    }
}