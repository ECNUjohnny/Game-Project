using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 normalLocalPos = new(0, 0.14f, -1.4f);
    public Vector3 aimLocalPos = new(0.42f, 0.45f, -0.575f);
    public float transitionSpeed = 10f;
    public bool bAiming;
    void Start()
    {
        transform.localPosition = normalLocalPos;
    }

    // Update is called once per frame
    void Update()
    {
        bAiming = Input.GetMouseButton(1);

        Vector3 targetPos = bAiming ? aimLocalPos : normalLocalPos;

        if (bAiming)
        {
            float camAngle = transform.eulerAngles.x;

            if (camAngle > 180) camAngle -= 360; 
        
            if (camAngle < 0) targetPos.z += camAngle * 0.02f;
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * transitionSpeed);
    }
}
