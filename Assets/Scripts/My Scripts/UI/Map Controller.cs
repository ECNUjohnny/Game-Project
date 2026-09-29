using UnityEngine;

public class MapController : MonoBehaviour
{
    [Header("Location Setting")]
    public Transform player;

    public RectTransform mapBackground;

    [Header("Transform Setting")]

    public float scale = 2.0f;

    public float mapOffset = 500f;

    public static MapController Instance { get; private set; }

    void Awake()
    {
        Instance = this;   
    }

    public Vector3 WorldToMapLocalPosition(Vector3 worldPos)
    {
        return new(
            (worldPos.x + mapOffset) * scale,
            (worldPos.z + mapOffset) * scale,
            0f
        );
    }

    public Quaternion WorldToMapLocalRotation()
    {
        return Quaternion.identity;
    }

    void Update()
    {
        if (player == null || mapBackground == null) return;
        
        mapBackground.localPosition = -WorldToMapLocalPosition(player.position);
    }
}