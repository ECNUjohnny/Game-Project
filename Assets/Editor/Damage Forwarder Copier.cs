
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
                
            }
            else
            {
                
            }
        }
    }

    private void CopyDamageForwarder(Transform current)
    {
        
    }

    private void Copy()
    {
        
    }

}
