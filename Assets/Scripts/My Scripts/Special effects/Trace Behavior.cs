using UnityEngine;
using System.Collections;

// 挂载到你的 BulletTracer 预制体上
public class TracerBehavior : MonoBehaviour
{
    public float duration = 0.5f; // 连发武器建议把留存时间调短一点，比如0.1~0.2秒
    
    private LineRenderer lr;
    private Color trueStartColor;
    private Color trueEndColor;

    // 外部调用这个方法来激活线段
    public void Init(Vector3 start, Vector3 end)
    {
        lr = GetComponent<LineRenderer>();
        
        // 设置位置
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        
        // 记录初始颜色
        trueStartColor = lr.startColor;
        trueEndColor = lr.endColor;

        // 启动自毁协程
        StartCoroutine(FadeOutAndDestroy());
    }

    IEnumerator FadeOutAndDestroy()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float currentStartAlpha = Mathf.Lerp(trueStartColor.a, 0, t);
            float currentEndAlpha = Mathf.Lerp(trueEndColor.a, 0, t);

            lr.startColor = new Color(trueStartColor.r, trueStartColor.g, trueStartColor.b, currentStartAlpha);
            lr.endColor = new Color(trueEndColor.r, trueEndColor.g, trueEndColor.b, currentEndAlpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 动画播放完毕，直接销毁这个物体！
        Destroy(gameObject); 
    }
}