using UnityEngine;

public class MinimapController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Player")]
    public Transform player; 
    
    [Tooltip("Map")]
    public RectTransform mapBackground; 

    [Header("Map Setting")]
    [Tooltip("Ratio")]
    public float mapScale = 4.0f;

    void Update()
    {
        if (player != null && mapBackground != null)
        {
            // 1. 获取玩家在 3D 世界中的水平坐标 (X 和 Z 轴)
            float player3D_X = player.position.x;
            float player3D_Z = player.position.z;

            // 2. 将 3D 坐标转换为 UI 像素坐标
            // 为什么要加负号（-）？
            // 因为玩家往右走时，玩家图标（红点）在屏幕上是固定不动的，
            // 所以我们需要让大地图底板向【左】反向滑动，才能产生“玩家在往前走”的视觉效果！
            float uiX = -player3D_X * mapScale;
            float uiY = -player3D_Z * mapScale; // 3D 的 Z 轴对应 UI 的 Y 轴

            // 3. 更新 UI 地图的局部位置 (localPosition)
            mapBackground.localPosition = new Vector3(uiX, uiY, 0f);
        }
    }
}