using System.Collections;
using System.Collections.Generic;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEngine;

public class BatchLODSetter : MonoBehaviour
{

    [MenuItem("Tools/一键为物体创建lod组件(_Low)")]
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
            foreach (Transform child in obj.transform)
            {
                if (child.name.EndsWith("_Low") || child.name.Contains("_Low"))
                {
                    MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                    if (childRenderer != null)
                    {
                        lod1Renderers.Add(childRenderer);
                    }
                }
                else if (parentRenderer == null)
                {
                    MeshRenderer cRen = child.GetComponent<MeshRenderer>();
                    if (cRen != null) lod0Renderers.Add(cRen);
                }
            }

            LOD[] lods;
            
            if (obj.transform.childCount > 0)
            {
                lods = new LOD[2];
          
                lods[0] = new LOD(0.25f, lod0Renderers.ToArray());
            
                lods[1] = new LOD(0.1f, lod1Renderers.ToArray());
            }
            else
            {
                lods = new LOD[1];

                lods[0] = new LOD(0.25f, lod0Renderers.ToArray());
            }
            
            lodGroup.SetLODs(lods);
            lodGroup.RecalculateBounds(); 

        
            EditorUtility.SetDirty(obj);
            successcnt++;
        }

        Debug.Log("[{successcnt}] objects have set up the LOD kit");
    }
}
