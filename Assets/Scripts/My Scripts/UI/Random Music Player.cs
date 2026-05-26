using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RandomMusicPlayer : MonoBehaviour
{
    [Header("Music")]

    public AudioClip[] bgms;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (bgms.Length > 0)
        {
            int randIndex = Random.Range(0, bgms.Length);

            audioSource.clip = bgms[randIndex];

            audioSource.Play();

            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
