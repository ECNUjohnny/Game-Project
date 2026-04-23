using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BatchLODSetter : MonoBehaviour
{
    [MenuItem("Tools/一键为物体创建LOD组件(自适应尺寸)")]
    public static void SetLod()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int successcnt = 0;

        foreach (GameObject obj in selectedObjects)
        {
            LODGroup lodGroup = obj.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = obj.AddComponent<LODGroup>();
            }

            List<Renderer> lod0Renderers = new List<Renderer>();
            MeshRenderer parentRenderer = obj.GetComponent<MeshRenderer>();
            if (parentRenderer != null) lod0Renderers.Add(parentRenderer);

            List<Renderer> lod1Renderers = new List<Renderer>();
            
            // 遍历子物体寻找 _Low 或是归入 LOD0
            foreach (Transform child in obj.transform)
            {
                if (child.name.EndsWith("_Low") || child.name.Contains("_Low"))
                {
                    MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                    if (childRenderer != null) lod1Renderers.Add(childRenderer);
                }
                else if (parentRenderer == null)
                {
                    MeshRenderer cRen = child.GetComponent<MeshRenderer>();
                    if (cRen != null) lod0Renderers.Add(cRen);
                }
            }

            // ================= 新增核心逻辑：计算物体的物理尺寸 =================
            float maxDimension = 1.0f; // 默认尺寸
            if (lod0Renderers.Count > 0)
            {
                // 用 Encapsulate 把所有高模的包围盒合并起来，算出总大小
                Bounds combinedBounds = lod0Renderers[0].bounds;
                for (int i = 1; i < lod0Renderers.Count; i++)
                {
                    combinedBounds.Encapsulate(lod0Renderers[i].bounds);
                }
                // 获取长宽高里最大的那个值作为“尺寸参照”
                maxDimension = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
            }

            // ================= 动态计算 LOD 阈值 =================
            // 逻辑：尺寸越大，阈值越大；尺寸越小，阈值越小（意味着需要退得更远才会变低模）
            // 假设房子 10米 -> 10 * 0.02 = 0.20f (和你原先一致)
            // 假设水杯 0.5米 -> 0.5 * 0.02 = 0.01f (极小，必须离得很远才会切换)
            
             // LOD1 彻底消失的阈值，设为 LOD0 的四分之一

            LOD[] lods;
            if (lod1Renderers.Count > 0)
            {
                lods = new LOD[2];
                // 填入动态计算的阈值
                lods[0] = new LOD(0.2f, lod0Renderers.ToArray());
                lods[1] = new LOD(0.07f, lod1Renderers.ToArray());
            }
            else
            {
                lods = new LOD[1];
                lods[0] = new LOD(0.2f, lod0Renderers.ToArray());
            }
            
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds(); 
            
            EditorUtility.SetDirty(obj);
            successcnt++;
        }

        Debug.Log($"[{successcnt}] 个物体已成功配置自适应 LOD 组件！");
    }
}