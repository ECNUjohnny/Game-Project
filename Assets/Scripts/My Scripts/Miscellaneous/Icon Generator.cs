using UnityEngine;
using System.IO;

public class IconGenerator : MonoBehaviour
{

    [Header("设置")]

    public Camera iconCamera;

    public int resolution = 512;

    public string savePath = "Assets/UI/Icon";

    [ContextMenu("一键生成图标")]    
    public void TakeScreenShot()
    {
        if (iconCamera == null)
        {
            Debug.LogError("Need a iconCamera!");
            return;
        }

        RenderTexture rt = new RenderTexture(resolution, resolution, 24);
        iconCamera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);        
    
        iconCamera.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        screenShot.Apply(true);

        iconCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log($"Icon is saved at {savePath}.");
    }


}
