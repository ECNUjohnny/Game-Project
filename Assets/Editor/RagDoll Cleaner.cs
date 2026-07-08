using System;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class RagDollCleaner : Editor 
{
    [MenuItem("Tools/一键清除骨骼系统")]
    
    public static void CleanRagdoll()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("请先选择需要清理的骨骼");

            return;
        }    

        foreach (GameObject obj in selectedObjects)
        {
            Undo.RegisterFullObjectHierarchyUndo(obj, "Clean Bones");
            
            CleanBonesRecursively(obj.transform);

            Debug.Log($"<color=orange><b>清理完成</b></color> 已清理{obj.name}上的骨骼系统");
        }
    }

    private static void CleanBonesRecursively(Transform current)
    {
        RemoveCompopnents<CharacterJoint>(current);
        RemoveCompopnents<Rigidbody>(current);
        RemoveCompopnents<Collider>(current);
        RemoveCompopnents<DamageForwarder>(current);
    
        foreach (Transform child in current)
        {
            CleanBonesRecursively(child);
        }
    }

    private static void RemoveCompopnents<T>(Transform t) where T : Component
    {
        T[] comps = t.GetComponents<T>();

        foreach (T c in comps)
        {
            Undo.DestroyObjectImmediate(c);
        } 
    }
}
