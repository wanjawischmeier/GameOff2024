using System;
using System.IO;
using UnityEngine;

public class StoryStateManager : MonoBehaviour
{
    [Serializable]
    public struct StoryState
    {
        public int currentMessageIndex, unreadMessages;
        public long[] messageTimes;
    }

    // wrapper struct is neccessary for JsonUtitlity to serialize correctly
    struct StoryStates
    {
        public StoryState[] storyStates;
    }

    public ChatManager chatManager;

    public static StoryState[] storyStates = null;
    static RectTransform storylineParent;
    static Storyline[] storylines;

    static string storySaveFilePath
    {
        get => Path.Combine(Application.persistentDataPath, "StoryState.save");
    }

    public static bool saveExists => File.Exists(storySaveFilePath);

    private void Awake()
    {
        storylineParent = (RectTransform)chatManager.chatScrollRect.transform.GetChild(0);
        storylines = FindObjectsByType<Storyline>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Array.Sort(storylines, (item0, item1) => item0.transform.GetSiblingIndex() < item1.transform.GetSiblingIndex() ? -1 : 1);

        LoadStoryState();
    }

    public static void SaveStoryState()
    {
        if (storyStates == null)
        {
            Debug.Log($"Creating new story save at {storySaveFilePath}.");
            storyStates = new StoryState[storylines.Length];
            for (int i = 0; i < storylines.Length; i++)
            {
                storyStates[i] = new StoryState()
                {
                    currentMessageIndex = 0, unreadMessages = 0,
                    messageTimes = new long[storylines[i].transform.childCount]
                };
            }
        }

        StoryStates states = new StoryStates() { storyStates = storyStates };
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
        storyStates = states.storyStates;

        return true;
    }

    public static bool ResetStoryState()
    {
        if (!File.Exists(storySaveFilePath))
        {
            return false;
        }

        File.Delete(storySaveFilePath);
        storyStates = null;

        return true;
    }
}
