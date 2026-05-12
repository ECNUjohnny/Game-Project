using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform spine;
    
    [Header("旋转设置")]
    public float targetAngle = 45f;   
    public float rotateSpeed = 5f;    

    private float currentOffsetAngle = 0f; 

    void LateUpdate()
    {

        if (Input.GetKey(KeyCode.P))
        {
         
            currentOffsetAngle = Mathf.Lerp(currentOffsetAngle, targetAngle, Time.deltaTime * rotateSpeed);
        }
        else
        {
         
            currentOffsetAngle = Mathf.Lerp(currentOffsetAngle, 0f, Time.deltaTime * rotateSpeed);
        }

        spine.rotation *= Quaternion.Euler(currentOffsetAngle, 0, 0);
    }
}