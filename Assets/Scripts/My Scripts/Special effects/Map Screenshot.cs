using UnityEngine;
using System.IO;

// 确保挂载此脚本的物体上有一个 Camera 组件
[RequireComponent(typeof(Camera))]
public class MapScreenshot : MonoBehaviour
{
    [Header("截图分辨率 (建议与你相机的长宽比保持一致)")]
    public int resWidth = 4096;
    public int resHeight = 4096;

    [Header("保存路径及文件名")]
    [Tooltip("图片会直接保存在项目的 Assets 文件夹根目录下")]
    public string fileName = "Minimap_HighRes.png";

    private Camera cam;

    // [ContextMenu] 魔法：允许你在 Unity 编辑器的 Inspector 面板右键直接运行这个函数
    [ContextMenu("📸 一键拍摄高清地图")]
    public void TakeHighResScreenshot()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("找不到摄像机！请确保此脚本挂载在你的正交拍照摄像机上。");
            return;
        }

        // 1. 创建一张高分辨率的 RenderTexture（渲染纹理），24位深度缓冲
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        
        // 2. 告诉摄像机：不要把画面输出到屏幕上，而是画到这张纹理上
        cam.targetTexture = rt;
        
        // 3. 准备一张空白的 Texture2D，用来接收像素数据
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        
        // 4. 强制摄像机渲染一帧
        cam.Render();
        
        // 5. 激活 RenderTexture，并把里面的像素“拷贝”到 Texture2D 里
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply(); // 确认应用像素更改

        // 6. 打扫战场：恢复摄像机和渲染状态，防止影响编辑器正常显示
        cam.targetTexture = null;
        RenderTexture.active = null; 
        DestroyImmediate(rt); // 立即销毁临时生成的 RenderTexture 以释放内存

        // 7. 将图片数据编码为 PNG 格式并写入电脑硬盘
        byte[] bytes = screenShot.EncodeToPNG();
        string fullPath = Path.Combine(Application.dataPath, fileName);
        File.WriteAllBytes(fullPath, bytes);

        Debug.Log($"<color=green><b>拍摄成功！</b></color> 图片已保存至: {fullPath}\n<color=yellow>请在 Project 窗口中点击一下，或者按 Ctrl+R 刷新资源目录即可看到。</color>");
    }
}