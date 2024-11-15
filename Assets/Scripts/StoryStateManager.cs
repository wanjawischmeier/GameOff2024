using System.IO;
using UnityEngine;

public class StoryStateManager : MonoBehaviour
{
    // wrapper struct is neccessary for JsonUtitlity to serialize correctly
    struct StoryStates
    {
        public int[] indicies;
    }

    public ChatManager chatManager;

    public static int[] storylineStates = null;
    static RectTransform storylineParent;
    static Storyline[] storylines;

    static string storySaveFilePath
    {
        get => Path.Combine(Application.persistentDataPath, "StoryState.save");
    }

    public static bool saveExists => File.Exists(storySaveFilePath);

    private void Start()
    {
        storylineParent = (RectTransform)chatManager.chatScrollRect.transform.GetChild(0);
        storylines = FindObjectsByType<Storyline>(FindObjectsSortMode.None);

        LoadStoryState();
    }

    public static void SaveStoryState()
    {
        StoryStates states = new StoryStates();

        if (storylineStates == null)
        {
            Debug.Log($"Creating new story save at {storySaveFilePath}.");
            states.indicies = new int[storylines.Length];
        }
        else
        {
            states.indicies = storylineStates;
        }

        string saveFile = JsonUtility.ToJson(states);
        File.WriteAllText(storySaveFilePath, saveFile);
    }

    public static bool LoadStoryState()
    {
        if (File.Exists(storySaveFilePath))
        {
            Debug.Log($"Loading existing story save from {storySaveFilePath}.");
        }
        else
        {
            Debug.Log($"No story save found at {storySaveFilePath}.");
            SaveStoryState();   // create new empty save
            return false;
        }

        string saveFile = File.ReadAllText(storySaveFilePath);
        var states = JsonUtility.FromJson<StoryStates>(saveFile);
        storylineStates = states.indicies;

        return true;
    }

    public static bool ResetStoryState()
    {
        if (!File.Exists(storySaveFilePath))
        {
            return false;
        }

        File.Delete(storySaveFilePath);
        storylineStates = null;

        return true;
    }
}
