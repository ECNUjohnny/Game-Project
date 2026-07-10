using System.Collections;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    
    public bool isOpen = false;
    
    public float OpenSpeed = 7.5f;
    
    private Coroutine Animation;
    
    private Quaternion closeRot;
    
    private Quaternion openRot;

    private string Name;


    void Start()
    {
        openRot = transform.rotation;

        Name = gameObject.name; 

        closeRot = ((Name[^1] - '0') & 1) == 1
            ? transform.rotation * Quaternion.Euler(0, -90f, 0)
            : transform.rotation * Quaternion.Euler(0, 90f, 0);

        if (Name.Contains("Window")) closeRot = openRot;
    }

    public void OpenTheDoor()
    {
        isOpen = !isOpen;

        if (Animation != null)
        {
            StopCoroutine(Animation);
        }

        Animation = StartCoroutine(AnimateDoor(isOpen ? closeRot : openRot));
    }

    private IEnumerator AnimateDoor(Quaternion targetRot)
    {
        while (Quaternion.Angle(transform.rotation, targetRot) > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * OpenSpeed);
        
            yield return null;
        }

        transform.rotation = targetRot;
    }   
}
