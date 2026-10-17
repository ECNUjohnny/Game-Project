using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;


[System.Serializable]
public class GameSaveData
{
    public int version = 1;              // 以后改结构靠它判断
    public SaveMeta meta = new();
    public PlayerSaveData player = new();
    public float timeOfDay;              // TimeManager 的时间
    public List<string> openDoors = new();
    public List<string> deadNpcs = new();  // 第一版可以留空不实现
}

[System.Serializable]
public class SaveMeta
{
    public string saveTime;   // DateTime.Now.ToString("yyyy-MM-dd HH:mm")，JsonUtility 不认 DateTime，必须存 string
    public string sceneName;
}

[System.Serializable]
public class PlayerSaveData
{
    public Vector3 position;
    public float yRotation;
    public float health;
    public int gold;
    public int[] ammo;
    public int ammoType;
}

public interface ISaveable
{
    void Capture(GameSaveData data);
    void Restore(GameSaveData data);
}

public class SaveSystem: MonoBehaviour
{
    public const int SlotCount = 10;

    public const int CurrentVersion = 1;

    static SaveSystem instance;

    public static SaveSystem Instance
    {
        get
        {
            if (instance == null) new GameObject("Global SaveSystem").AddComponent<SaveSystem>(); 
            return instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { instance = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() { _ = Instance; }

    static string SavesDir => Path.Combine(Application.persistentDataPath, "saves");

    static string SlotPath(int slot) => Path.Combine(SavesDir, $"datas{slot}");

    static IEnumerable<ISaveable> FindSaveables()
        => FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

    public void SaveToSlot(int slot)
    {
        var data = new GameSaveData();
        foreach (var s in FindSaveables()) s.Capture(data);
        data.meta.sceneName = SceneManager.GetActiveScene().name;
        data.meta.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        WriteJson(SlotPath(slot), JsonUtility.ToJson(data, true));
    }

    public void WriteJson(string path, string json)
    {
        Directory.CreateDirectory(SavesDir);
        string tmp = path + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(path)) File.Delete(path);
        File.Move(tmp, path);   
    }

    public static GameSaveData ReadSlot(int slot)
    {
        try
        {
            var data = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(SlotPath(slot)));
            return data != null && data.version == CurrentVersion ? data : null;
        }
        catch { return null; }   // 返回 null = 文件损坏
    }

    IEnumerator LoadfromSlot(int slot)
    {
        GameSaveData data = ReadSlot(slot);
        if (data == null) yield break;

        AsyncOperation op = SceneManager.LoadSceneAsync(data.meta.sceneName);

        while (!op.isDone) yield return null;

        yield return null;

        yield return null;

        foreach (var s in FindSaveables()) s.Restore(data);
    }

    
}
