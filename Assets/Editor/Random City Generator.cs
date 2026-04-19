using UnityEngine;
using UnityEditor;

public class RandomCityGenerator : EditorWindow
{
    public GameObject groundCube;
    public string folderPath = "Assets/Models/Western Model"; 
    public int spawnCount = 20;
    public Transform parentContainer; 

    [MenuItem("Tools/西部世界建筑撒布机")]
    public static void ShowWindow()
    {
        GetWindow<RandomCityGenerator>("随机生成建筑");
    }


    void OnGUI()
    {
        GUILayout.Label("1. 指定目标区域", EditorStyles.boldLabel);
        groundCube = (GameObject)EditorGUILayout.ObjectField("地面 (Cube)", groundCube, typeof(GameObject), true);
        
        GUILayout.Space(10);
        GUILayout.Label("2. 资源与生成设置", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("建筑文件夹路径", folderPath);
        spawnCount = EditorGUILayout.IntSlider("生成数量", spawnCount, 1, 500);
        
        GUILayout.Space(10);
        GUILayout.Label("3. 层级管理 (可选)", EditorStyles.boldLabel);
        parentContainer = (Transform)EditorGUILayout.ObjectField("统一的父物体", parentContainer, typeof(Transform), true);

        GUILayout.Space(20);

        if (GUILayout.Button("开始随机生成！", GUILayout.Height(40)))
        {
            GenerateBuildings();
        }
    }

    void GenerateBuildings()
    {
        if (groundCube == null)
        {
            Debug.LogError("请先把作为地面的 Cube 拖到面板的槽位里！");
            return;
        }

        Collider groundCollider = groundCube.GetComponent<Collider>();
        if (groundCollider == null)
        {
            Debug.LogError("地面必须有 Collider 组件(比如 BoxCollider),否则算不出边界大小!");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { folderPath });
        if (guids.Length == 0)
        {
            Debug.LogError($"在路径 [{folderPath}] 下没有找到任何模型！请检查路径是否拼写正确（不需要带前后斜杠）。");
            return;
        }

        // 获取地面的边界盒子大小
        Bounds bounds = groundCollider.bounds;
        int successCount = 0;

        for (int i = 0; i < spawnCount; i++)
        {

            string randomGuid = guids[Random.Range(0, guids.Length)];
            string assetPath = AssetDatabase.GUIDToAssetPath(randomGuid);
            GameObject assetPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

            if (assetPrefab != null)
            {

                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomZ = Random.Range(bounds.min.z, bounds.max.z);
                
                float spawnY = bounds.max.y; 

                Vector3 spawnPosition = new Vector3(randomX, spawnY, randomZ);

                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(assetPrefab);
             
                Undo.RegisterCreatedObjectUndo(instance, "Random Spawn");

                instance.transform.position = spawnPosition;
                instance.transform.rotation = randomRotation;

                if (parentContainer != null)
                {
                    instance.transform.SetParent(parentContainer);
                }

                successCount++;
            }
        }

        Debug.Log($"撒布完成！成功在地面上生成了 {successCount} 栋建筑。");
    }
}