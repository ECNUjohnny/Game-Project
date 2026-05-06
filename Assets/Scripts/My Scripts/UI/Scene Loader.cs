using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;              
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [Header("UI Ref")]

    public GameObject loadScreen;

    public Slider progressBar;

    public TextMeshProUGUI progressText;

    [Header("Scene Settings")]

    public int sceneIndexToLoad = 1;

    public void StartGame()
    {
        loadScreen.SetActive(true);

        StartCoroutine(StartSceneLoading(sceneIndexToLoad));
    }

    public void EndGame()
    {
        Application.Quit();

        
    }

    IEnumerator StartSceneLoading(int sceneIndex)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            progressBar.value = progress;

            progressText.text = (progress * 100f).ToString("F0") + "%";

            yield return null;
        }
    }

}
