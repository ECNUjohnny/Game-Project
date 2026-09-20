using System.Collections.Generic;
using UnityEngine;

public class Westerntownbountymission : MonoBehaviour
{
    [Header("Settings")]

    public Transform[] spawnPoints; 

    public int epochs;     

    public static int savedEpochs; 

    public GameObject[] Enemies;

    [Header("Spown Settings")]

    public float spawnInterval;

    public Dictionary<int, GameObject> spawnedEnemies;

    public int enemiesCount; 

    [Header("UI Settings")]

    public GameObject dialoguePanel;

    public UIManager ui;
    
    void Start()
    {
        epochs = 1;
    }

    void StartNewEpoch()
    {
                
    }    

    void Update()
    {
        if (enemiesCount == 0)
        {
            StartNewEpoch();                        
        }
    }
}
