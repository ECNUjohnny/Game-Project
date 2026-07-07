using System.Linq;
using Codice.Client.Common.WebApi.Responses;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class BoneSetupCopier : EditorWindow
{
    private Transform sourceRoot;

    private Transform targetRoot;

    [MenuItem("Tools/Bone Setup copier")]
    public static void ShowWindow()
    {
        GetWindow<BoneSetupCopier>("复制人物骨骼设置");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("1. 拖入已经配置完美的 NPC 根骨骼 (如 Hips)", EditorStyles.boldLabel);
        sourceRoot = (Transform)EditorGUILayout.ObjectField("完美源骨骼 (Source)", sourceRoot, typeof(Transform), true);

        GUILayout.Space(10);
        GUILayout.Label("2. 拖入需要被配置的新 NPC 根骨骼", EditorStyles.boldLabel);
        targetRoot = (Transform)EditorGUILayout.ObjectField("待配目标骨骼 (Target)", targetRoot, typeof(Transform), true);

        GUILayout.Space(20);
        if (GUILayout.Button("一键同步 (Colliders / Rigidbodies / Scripts)", GUILayout.Height(40)))
        {
            if (sourceRoot != null && targetRoot != null)
            {
                Undo.IncrementCurrentGroup();
                int undoGroupIndex = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Sync Bone Setup");
                
                CopyBonesRecursively(sourceRoot, targetRoot);

                

                Debug.Log($"<color=green><b>同步完成！</b></color> 已将 {sourceRoot.name} 的配置完美克隆给 {targetRoot.name}。");
            }
            else
            {
                Debug.LogError("请先拖入 Source 和 Target 骨骼！");
            }
        }
    }

    private void CopyBonesRecursively(Transform src, Transform dst)
    {
        if (dst.TryGetComponent<DamageForwarder>(out var damageForwarder))
        {
            Undo.DestroyObjectImmediate(damageForwarder);
        }
        
        CopySpecificComponent<Collider>(src, dst);
        CopySpecificComponent<Rigidbody>(src, dst);
        CopySpecificComponent<DamageForwarder>(src, dst);
        CopySpecificComponent<CharacterJoint>(src, dst);

    
        foreach (Transform srcChild in src)
        {
            Transform dstChild = dst.Find(srcChild.name);
        
            if (dstChild != null)
            {
                CopyBonesRecursively(srcChild, dstChild);
            }
        }
    }

    private void CopySpecificComponent<T>(Transform src, Transform dst) where T: Component
    {
        T[] srcComponents = src.GetComponents<T>();
        T[] dstComponents = dst.GetComponents<T>();

        for (int i = 0; i < srcComponents.Length; i++)
        {
            if (i < dstComponents.Length)
            {
                Undo.RecordObject(dstComponents[i], "Sync Bone Setup");

                EditorUtility.CopySerialized(srcComponents[i], dstComponents[i]);
            }
            else
            {
                T newComp = Undo.AddComponent<T>(dst.gameObject);

                EditorUtility.CopySerialized(srcComponents[i], newComp);
            }
        }

        for (int i = srcComponents.Length; i < dstComponents.Length; i++)
        {
            Undo.DestroyObjectImmediate(dstComponents[i]);
        }
    }

    private void RemapCharacterJoints(Transform srcRoot, Transform dstRoot, Transform currentDstNode)
    {
        CharacterJoint[] joints = currentDstNode.GetComponents<CharacterJoint>();

        foreach (var joint in joints)
        {
            string path = GetRelativePath(srcRoot, joint.connectedBody.transform);

            Transform targetBone = dstRoot.Find(path);
            if (targetBone != null)
            {
                if (targetBone.TryGetComponent<Rigidbody>(out var targetRb))
                {
                    Undo.RecordObject(joint, "Remap Joint");

                    joint.connectedBody = targetRb;
                }
            }
        }

        foreach (Transform child in currentDstNode)
        {
            RemapCharacterJoints(srcRoot, dstRoot, child);
        }
    }

    private string GetRelativePath(Transform root, Transform target)
    {
        if (target == root) return "";

        string path = target.name;

        Transform parent = target.parent;

        for (; parent != null && parent != root; parent = parent.parent)
        {
            path = parent.name + "/" + path;
        }

        return path;
    }

}
