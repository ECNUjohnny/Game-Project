
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class DamageForwarderCopier : EditorWindow
{

    private Transform sourceRoot;

    private Transform targetRoot;
 
    [MenuItem("Tools/一键复制伤害触发器")]
    public static void ShowWindow()
    {
        GetWindow<DamageForwarderCopier>("复制伤害触发器");
    }
    
    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("拖入已经配置好的带有伤害转发器的父物体", EditorStyles.boldLabel);
        sourceRoot = (Transform)EditorGUILayout.ObjectField("父物体", sourceRoot, typeof(Transform), true);

        GUILayout.Space(10);
        GUILayout.Label("拖入想要复制伤害转发器的父物体", EditorStyles.boldLabel);
        targetRoot = (Transform)EditorGUILayout.ObjectField("父物体", targetRoot, typeof(Transform), true);

        GUILayout.Space(20);
        if (GUILayout.Button("一键复制", GUILayout.Height(40)))
        {
            if (sourceRoot != null && targetRoot != null)
            {
                Undo.IncrementCurrentGroup();
                int undoGroupIndex = Undo.GetCurrentGroup();
                Undo.SetCurrentGroupName("Copy Damage Forwarder");

                CopyDamageForwarder(sourceRoot, targetRoot);

                Undo.CollapseUndoOperations(undoGroupIndex);

                Debug.Log($"<color=green>Done</color> 伤害转换器已复制");
            }
            else
            {
                Debug.LogError("需要两份不同的骨骼");   
            }
        }
    }

    private void CopyDamageForwarder(Transform src, Transform dst)
    {
        Copy<DamageForwarder>(src, dst);

        foreach (Transform srcChild in src)
        {
            Transform dstChild = dst.Find(srcChild.name);

            if (dstChild != null)
            {
                CopyDamageForwarder(srcChild, dstChild);
            }   
        }

    }

    private void Copy<T>(Transform src, Transform dst) where T: Component
    {
        T[] comps = dst.GetComponents<T>();

        foreach (T comp in comps)
        {
            Undo.DestroyObjectImmediate(comp);
        }   

        
        if (!src.TryGetComponent<T>(out var srcComp)) return;

        T newComp = Undo.AddComponent<T>(dst.gameObject);

        EditorUtility.CopySerialized(srcComp, newComp); 

    }

}
