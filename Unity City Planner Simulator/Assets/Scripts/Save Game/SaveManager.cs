using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-100)]
public class SaveManager : MonoBehaviour
{
    private string _filePath;
    public List<ISaveable> _saveableInstances = new List<ISaveable>();

    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        _filePath = Path.Combine(Application.persistentDataPath, "savegame.json");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
        LoadGame();
    }

    private void OnEnable()
    {
        
    }
    public void SaveGame()
    {
        var data = new SaveData();
        foreach (var item in _saveableInstances)
        {
            item.Save(data);
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(_filePath, json);

        Debug.Log($"Game saved to {_filePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(_filePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string json = File.ReadAllText(_filePath);
        var data = JsonUtility.FromJson<SaveData>(json);

        foreach (var item in _saveableInstances)
        {
            item.Load(data);
            Debug.Log("item");
        }

        Debug.Log("Game loaded.");
    }

    public void Register(ISaveable instance) => _saveableInstances.Add(instance);
    public void Unregister(ISaveable instance) => _saveableInstances.Remove(instance);

    private void OnApplicationQuit()
    {
        SaveGame();
    }

}
