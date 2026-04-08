using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    public bool isOpen = false;
    public float OpenSpeed = 1.0f;
    private Coroutine Animation;
    private Quaternion closeRot;
    private Quaternion openRot;
    void Start()
    {
        openRot = transform.rotation;

        closeRot = transform.rotation * Quaternion.Euler(0, -90f, 0);
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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 2f);
        
            yield return null;
        }

        transform.rotation = targetRot;
    }   
}
