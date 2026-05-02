using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using System.Collections.Generic;

public class Door : MonoBehaviour
{
    [MenuItem("Tools/自动为门物体创建box collider和添加脚本")]
    public static void DoorEditor()
    {
        GameObject[] selectedobjects = Selection.gameObjects;

        if (selectedobjects.Length == 0) return;

        int successcnt = 0;

        Undo.SetCurrentGroupName("批量安装门设置");
        int undoGroupIndex = Undo.GetCurrentGroup();

        foreach (GameObject obj in selectedobjects)
        {
            if (!obj.name.Contains("Door")) continue;

            obj.isStatic = false;

            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);

            if (!obj.TryGetComponent(out LODGroup lod) || lod.size == 4)
            {
                lod = Undo.AddComponent<LODGroup>(obj);

                LOD[] lods = new LOD[1];

                List<Renderer> lst = new();

                lst.Add(obj.GetComponent<MeshRenderer>());

                lods[0] = new LOD(0.2f, lst.ToArray());

                lod.SetLODs(lods);
            }

            //continue;

            BoxCollider[] boxColliders = obj.GetComponents<BoxCollider>();

            if (boxColliders.Length == 2) continue;

            BoxCollider boxCollider = Undo.AddComponent<BoxCollider>(obj);

            boxCollider.isTrigger = true;

            boxCollider.size = new(1.5f, 2.0f, 4.0f);

            if (!obj.TryGetComponent(out DoorControl doorControl)) Undo.AddComponent<DoorControl>(obj);

            if (!obj.TryGetComponent(out DoorInteraction doorInteraction)) Undo.AddComponent<DoorInteraction>(obj);  

            obj.GetComponent<DoorInteraction>().Script = obj.GetComponent<DoorControl>();

            successcnt++;
        }

        Undo.CollapseUndoOperations(undoGroupIndex);

        Debug.Log($"{successcnt} doors have set the interactive system!");
    }
}
