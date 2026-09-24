using UnityEngine;

public class FootStep : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip[] footstepClips;

    public PlayerMovement movement;

    public Animator animator;    

    public string speedParamName = "Speed";

    public float threshold = 0.4f;


    void Start()
    {
        audioSource.volume = 0.7f;
        audioSource.spatialBlend = 1.0f;
    }

    public void Footstep()
    {
        if (movement != null)
        {
            if (movement.v < threshold)
            {
                return;
            }
        }

        if (animator != null)
        {
            float speed = animator.GetFloat(speedParamName);

            if (speed < threshold)
            {
                return;
            }
        }        
        
        if (footstepClips.Length > 0 && audioSource != null)
        {
            int randomIndex = Random.Range(0, footstepClips.Length);

            audioSource.pitch = Random.Range(0.9f, 1.1f);

            audioSource.PlayOneShot(footstepClips[randomIndex], Random.Range(0.8f, 1.1f));
        }
    }
}
