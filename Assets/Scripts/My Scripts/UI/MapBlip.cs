using UnityEngine;

public class MapBlip : MonoBehaviour
{
    public Transform target;

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }   

        transform.localPosition = MapController.Instance.WorldToMapLocalPosition(target.position);
    } 
}
