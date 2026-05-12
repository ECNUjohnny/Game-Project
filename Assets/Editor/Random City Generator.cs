using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RandomCityGenerator : EditorWindow
{
    [Header("生成资产库")]
    public List<GameObject> prefabsToSpawn = new List<GameObject>();

    [Header("区域设置 (世界坐标)")]
    public float minX = -150f;
    public float maxX = 150f;
    public float minZ = -150f;
    public float maxZ = 150f;
    public float spawnY = 8.1f;

    [Header("泊松采样设置")]
    public float minRadius = 15f; 
    public int rejectionSamples = 30; 

    [Header("自然化手术刀 (打破方框感)")]
    public bool useCircularBounds = true; // 是否把方形切成圆形
    public bool usePerlinNoise = true;    // 是否开启云雾状的自然镂空
    public float noiseScale = 0.05f;      // 噪声缩放（越小，聚集的区块越大）
    [Range(0f, 1f)]
    public float noiseThreshold = 0.4f;   // 剔除阈值（越大，空地越多）

    public Transform parentContainer; 

    private SerializedObject serializedObj;
    private SerializedProperty prefabsProperty;

    [MenuItem("Tools/泊松圆盘建筑撒布机")]
    public static void ShowWindow()
    {
        GetWindow<RandomCityGenerator>("泊松圆盘生成");
    }

    private void OnEnable()
    {
        serializedObj = new SerializedObject(this);
        prefabsProperty = serializedObj.FindProperty("prefabsToSpawn");
    }

    void OnGUI()
    {
        GUILayout.Label("1. 资产列表", EditorStyles.boldLabel);
        serializedObj.Update();
        EditorGUILayout.PropertyField(prefabsProperty, new GUIContent("资产列表 (可多选拖拽)"), true);
        serializedObj.ApplyModifiedProperties();
        
        GUILayout.Space(10);
        GUILayout.Label("2. 生成边界", EditorStyles.boldLabel);
        minX = EditorGUILayout.FloatField("Min X", minX);
        maxX = EditorGUILayout.FloatField("Max X", maxX);
        minZ = EditorGUILayout.FloatField("Min Z", minZ);
        maxZ = EditorGUILayout.FloatField("Max Z", maxZ);
        spawnY = EditorGUILayout.FloatField("基准高度 Y", spawnY);

        GUILayout.Space(10);
        GUILayout.Label("3. 算法与自然化参数", EditorStyles.boldLabel);
        minRadius = EditorGUILayout.FloatField("最小间距 (Radius)", minRadius);
        
        // ★ 新增的自然化 UI 面板
        useCircularBounds = EditorGUILayout.Toggle("开启圆形边界", useCircularBounds);
        usePerlinNoise = EditorGUILayout.Toggle("开启柏林噪声镂空", usePerlinNoise);
        if (usePerlinNoise)
        {
            EditorGUI.indentLevel++;
            noiseScale = EditorGUILayout.Slider("噪声区块大小", noiseScale, 0.01f, 0.2f);
            noiseThreshold = EditorGUILayout.Slider("空地比例", noiseThreshold, 0f, 0.8f);
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(10);
        GUILayout.Label("4. 层级管理", EditorStyles.boldLabel);
        parentContainer = (Transform)EditorGUILayout.ObjectField("统一的父物体", parentContainer, typeof(Transform), true);

        GUILayout.Space(20);

        if (GUILayout.Button("执行泊松圆盘撒布！", GUILayout.Height(40)))
        {
            GenerateBuildings();
        }
    }

    void GenerateBuildings()
    {
        if (prefabsToSpawn == null || prefabsToSpawn.Count == 0) return;
        prefabsToSpawn.RemoveAll(item => item == null);
        if (prefabsToSpawn.Count == 0) return;

        float width = maxX - minX;
        float height = maxZ - minZ;
        Vector2 regionSize = new Vector2(width, height);
        
        // 算出一个圆心坐标，用于做圆形裁剪
        Vector2 centerPoint = new Vector2(minX + width / 2, minZ + height / 2);
        float circleRadius = Mathf.Min(width, height) / 2f;

        // 生成一个随机的噪声偏移值，保证每次点"生成"，就算参数一样，地形分布也完全不同
        float randomNoiseOffset = Random.Range(0f, 10000f);

        List<Vector2> points = GeneratePoissonPoints(minRadius, regionSize, rejectionSamples);
        int successCount = 0;

        foreach (Vector2 point in points)
        {
            Vector3 spawnPosition = new Vector3(minX + point.x, spawnY, minZ + point.y);

            // =====================================
            // 手术刀 1：圆形/椭圆边界裁剪
            // =====================================
            if (useCircularBounds)
            {
                Vector2 currentPos2D = new Vector2(spawnPosition.x, spawnPosition.z);
                if (Vector2.Distance(centerPoint, currentPos2D) > circleRadius)
                {
                    continue; // 距离圆心超过半径的点，直接无情剔除！
                }
            }

            // =====================================
            // 手术刀 2：柏林噪声自然镂空
            // =====================================
            if (usePerlinNoise)
            {
                float sampleX = spawnPosition.x * noiseScale + randomNoiseOffset;
                float sampleZ = spawnPosition.z * noiseScale + randomNoiseOffset;
                
                // 获取当前坐标的噪声值 (范围在 0.0 到 1.0 之间)
                float noiseVal = Mathf.PerlinNoise(sampleX, sampleZ);

                // 如果噪声值低于我们设定的阈值，就把它当成“自然空地”留出来
                if (noiseVal < noiseThreshold)
                {
                    continue; // 剔除！
                }
            }

            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            GameObject randomPrefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];
            
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(randomPrefab);
            Undo.RegisterCreatedObjectUndo(instance, "Organic Spawn");
            instance.transform.position = spawnPosition;
            instance.transform.rotation = randomRotation;

            if (parentContainer != null) instance.transform.SetParent(parentContainer);
            successCount++;
        }

        Debug.Log($"撒布完成！经过自然化过滤，最终生成了 {successCount} 个物体。");
    }

    // （以下 Bridson 算法保持完全一致，无需修改）
    private List<Vector2> GeneratePoissonPoints(float radius, Vector2 sampleRegionSize, int numSamplesBeforeRejection)
    {
        float cellSize = radius / Mathf.Sqrt(2);
        int gridWidth = Mathf.CeilToInt(sampleRegionSize.x / cellSize);
        int gridHeight = Mathf.CeilToInt(sampleRegionSize.y / cellSize);
        int[,] grid = new int[gridWidth, gridHeight];
        List<Vector2> points = new List<Vector2>();
        List<Vector2> spawnPoints = new List<Vector2>();

        spawnPoints.Add(sampleRegionSize / 2);

        while (spawnPoints.Count > 0)
        {
            int spawnIndex = Random.Range(0, spawnPoints.Count);
            Vector2 spawnCenter = spawnPoints[spawnIndex];
            bool candidateAccepted = false;

            for (int i = 0; i < numSamplesBeforeRejection; i++)
            {
                float angle = Random.value * Mathf.PI * 2;
                Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                Vector2 candidate = spawnCenter + dir * Random.Range(radius, 2 * radius);

                if (IsValid(candidate, sampleRegionSize, cellSize, radius, points, grid))
                {
                    points.Add(candidate);
                    spawnPoints.Add(candidate);
                    grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
                    candidateAccepted = true;
                    break;
                }
            }
            if (!candidateAccepted) spawnPoints.RemoveAt(spawnIndex);
        }
        return points;
    }

    private bool IsValid(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
    {
        if (candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
        {
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y] - 1;
                    if (pointIndex != -1)
                    {
                        float sqrDst = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDst < radius * radius) return false;
                    }
                }
            }
            return true;
        }
        return false;
    }
}