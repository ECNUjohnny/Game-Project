using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 normalLocalPos = new(0, 0.14f, -1.4f);

    public Vector3 aimLocalPos = new(0.42f, 0.45f, -0.75f);

    public Vector3 aimPreciseLocalPos = new(0.42f, 0.45f, -0.15f);

    public float transitionSpeed = 10f;

    public bool bAiming = false;

    public bool bAimingPrecise;

    private Vector3 targetPos;

    void Start()
    {
        transform.localPosition = normalLocalPos;

    }

    // Update is called once per frame
    void Update()
    {
        bAiming = Input.GetMouseButton(1);

        if (bAiming && bAimingPrecise) targetPos = aimPreciseLocalPos;
        
        else if (bAiming) targetPos = aimLocalPos;

        else targetPos = normalLocalPos;       

        if (bAiming)
        {
            float camAngle = transform.eulerAngles.x;

            if (camAngle > 180) camAngle -= 360; 
        
            if (camAngle < 0) targetPos.z += camAngle * 0.02f;

            if (Input.GetAxis("Mouse ScrollWheel") > 0) bAimingPrecise = true;

            if (Input.GetAxis("Mouse ScrollWheel") < 0) bAimingPrecise = false;
        }

        //Debug.Log(transform.localPosition.z);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * transitionSpeed);
    }
}
