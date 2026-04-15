using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BatchLODSetter : MonoBehaviour
{
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
        }
    }
}
