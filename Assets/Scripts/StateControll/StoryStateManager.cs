using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class StoryStateManager : MonoBehaviour
{
    [Serializable]
    public struct StoryState
    {
        public int currentMessageIndex, lockNightIndex, unreadMessages;
        public long[] messageTimes;
    }

    struct GameState
    {
        public int nightCount;
        public int[] newCollectedItems, collectedItems;
        public StoryState[] storyStates;
    }

    public ChatManager chatManager;

    public static int nightCount = 0;
    public static List<int> newCollectedItems = null;
    public static List<int> collectedItems = null;
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
        if (chatManager != null)
        {
            storylineParent = (RectTransform)chatManager.chatScrollRect.transform.GetChild(0);
            storylines = FindObjectsByType<Storyline>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Array.Sort(storylines, (item0, item1) => item0.transform.GetSiblingIndex() < item1.transform.GetSiblingIndex() ? -1 : 1);
        }

        if (!LoadStoryState())
        {
            // no story save
            if (chatManager == null)
            {
                // unable to initialize story save in mission scene
                Debug.LogError("Mission loaded without a story save.");
                return;
            }

            SaveStoryState();   // create new empty save
        }
    }

    public static void SaveStoryState()
    {
        if (storylines == null && (collectedItems == null || storyStates == null))
        {
            Debug.LogError("Unable to initialize story save in mission scene");
            return;
        }

        if (collectedItems == null)
        {
            Debug.Log($"Initializing new collected items state at {storySaveFilePath}.");
            newCollectedItems = new List<int>();
            collectedItems = new List<int>();
        }

        if (storyStates == null)
        {
            Debug.Log($"Initializing new story state at {storySaveFilePath}.");
            storyStates = new StoryState[storylines.Length];

            for (int i = 0; i < storylines.Length; i++)
            {
                storyStates[i] = new StoryState()
                {
                    currentMessageIndex = 0,
                    lockNightIndex = -1,
                    unreadMessages = 0,
                    messageTimes = new long[storylines[i].transform.childCount]
                };
            }
        }

        var gameState = new GameState()
        {
            nightCount = nightCount,
            newCollectedItems = newCollectedItems.ToArray(),
            collectedItems = collectedItems.ToArray(),
            storyStates = storyStates
        };
         
        string saveFile = JsonUtility.ToJson(gameState);
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
            return false;
        }

        string saveFile = File.ReadAllText(storySaveFilePath);
        var gameState = JsonUtility.FromJson<GameState>(saveFile);
        if (gameState.newCollectedItems == null)
        {
            Debug.LogWarning($"Deleting corrupt save file at {storySaveFilePath}.");
            ResetStoryState();
            return false;
        }
        nightCount = gameState.nightCount;
        newCollectedItems = new List<int>(gameState.newCollectedItems);
        collectedItems = new List<int>(gameState.collectedItems);
        storyStates = gameState.storyStates;

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

    public static void RemoveNewCollectedItems()
    {
        if (newCollectedItems == null)
        {
            Debug.LogError("Called <RemoveNewCollectedItems> without a valid story save loaded.");
            return;
        }

        newCollectedItems.Clear();
        SaveStoryState();
    }

    /// <summary>
    /// This returns the new items in an appropiate formatting.
    /// Also marks them as not newly collected.
    /// </summary>
    /// <returns></returns>
    public static string RetrieveAndProcessNewItems()
    {
        StringBuilder formattedItems = new StringBuilder();

        for (int i = 0; i < newCollectedItems.Count; i++)
        {
            // Retrieve the item's name
            string itemName = ItemManager.GetItem(newCollectedItems[i]).itemName;

            // Append the formatted string
            formattedItems.AppendLine($"- Item {i} {itemName}");
        }

        // mark as not newly collected
        collectedItems.AddRange(newCollectedItems);
        RemoveNewCollectedItems();

        return formattedItems.ToString();
    }
}
