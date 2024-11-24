using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Storyline;

public class ChatManager : MonoBehaviour
{
    struct Chat
    {
        public string lockText;
        public Sprite lockSprite;
        public Transform transform, messageCountBubble;
        public Storyline storyline;
    }

    public GameObject messagePlayerPrefab, messageGameCharacterPrefab;
    public GameObject nightRecap, contactBanner, messageCreationBox;
    public Transform overviewScrollViewContent, chatScrollViewContent;
    public TextMeshProUGUI contactName;
    public ScrollRect chatScrollRect;
    public Color selectedChatColor, deselectedChatColor;

    Transform chatViewport;
    Chat[] chats;
    int selectedChatIndex = -1;
    List<int> playerLockedChats;

    const float refreshTime = 0.02f;
    const float messageStartDelay = 2;
    const float messageInbetweenDelay = 0.1f;
    const float writeMessageMinTime = 0.5f;
    const float writeMessageMaxTime = 4;
    const float writeCharacterTime = 0.05f;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundtrackQueue(AudioManager.Lookup.chatWindowSoundtrack, silenceDuration: 5);
        }

        if (StoryStateManager.nightCount == 0)
        {
            nightRecap.SetActive(false);
        }
        else
        {
            var nightRecapHeader = nightRecap.transform.Find("Header (TMP)").GetComponent<TextMeshProUGUI>();
            var nightRecapText = nightRecap.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
            nightRecapHeader.text = $"Completed Night {StoryStateManager.nightCount}";
            nightRecapText.text = StoryStateManager.RetrieveAndProcessNewItems();
        }

        chatViewport = chatScrollRect.transform.GetChild(0);
        chatScrollRect.verticalNormalizedPosition = 0;

        playerLockedChats = new List<int>();
        chats = new Chat[chatViewport.childCount];
        for (int chatIndex = 0; chatIndex < chatViewport.childCount; chatIndex++)
        {
            var chatTransform = chatViewport.GetChild(chatIndex);
            chats[chatIndex] = new Chat()
            {
                lockText = "",
                lockSprite = null,
                transform = chatTransform,
                messageCountBubble = overviewScrollViewContent.GetChild(chatIndex).Find("Bubble"),
                storyline = chatTransform.GetComponent<Storyline>()
            };

            var storyState = StoryStateManager.storyStates[chatIndex];
            int currentMessageIndex = storyState.currentMessageIndex;
            for (int messageIndex = 0; messageIndex < chatTransform.childCount; messageIndex++)
            {
                var messageTransform = chatTransform.GetChild(messageIndex);

                if (messageIndex < currentMessageIndex)
                {
                    var dateTransform = messageTransform.Find("Message Content").Find("Date and Time (TMP)");
                    var messageTime = DateTime.FromFileTime(storyState.messageTimes[messageIndex]);
                    dateTransform.GetComponent<TextMeshProUGUI>().text = messageTime.ToString("HH:mm");
                }
                else
                {
                    messageTransform.gameObject.SetActive(false);
                }
            }

            StartCoroutine(ProgressStory(chatIndex));
            UpdateMessageCountBubble(chatIndex);
        }
    }

    // grabbed from: https://stackoverflow.com/a/55148117
    /// <summary>
    /// Forces the layout of a UI GameObject and all of it's children to update
    /// their positions and sizes.
    /// </summary>
    /// <param name="xform">
    /// The parent transform of the UI GameObject to update the layout of.
    /// </param>
    public static void UpdateLayout(Transform xform)
    {
        Canvas.ForceUpdateCanvases();
        UpdateLayout_Internal(xform);
    }

    private static void UpdateLayout_Internal(Transform xform)
    {
        if (xform == null || xform.Equals(null))
        {
            return;
        }

        // Update children first
        for (int x = 0; x < xform.childCount; ++x)
        {
            UpdateLayout_Internal(xform.GetChild(x));
        }

        // Update any components that might resize UI elements
        foreach (var layout in xform.GetComponents<LayoutGroup>())
        {
            layout.CalculateLayoutInputVertical();
            layout.CalculateLayoutInputHorizontal();
        }
        foreach (var fitter in xform.GetComponents<ContentSizeFitter>())
        {
            fitter.SetLayoutVertical();
            fitter.SetLayoutHorizontal();
        }
    }

    private void ScrollDown()
    {
        // only scroll down further if user is already scrolled down
        UpdateLayout(transform);
        chatScrollRect.verticalNormalizedPosition = 0;
    }

    private void UpdateMessageCountBubble(int chatIndex)
    {
        var messageCountBubble = chats[chatIndex].messageCountBubble;
        var textMesh = messageCountBubble.GetChild(0).GetComponent<TextMeshProUGUI>();

        int unreadMessages = StoryStateManager.storyStates[chatIndex].unreadMessages;
        if (unreadMessages == 0)
        {
            messageCountBubble.gameObject.SetActive(false);
        }
        else
        {
            messageCountBubble.gameObject.SetActive(true);
            textMesh.text = unreadMessages.ToString();
        }
    }

    public void SetChat(int chatIndex)
    {
        if (selectedChatIndex == -1)
        {
            var scrollbarHandle = chatScrollRect.transform.Find("Scrollbar Vertical").GetChild(0).GetChild(0);
            scrollbarHandle.GetComponent<Image>().enabled = true;   // messy but eh
            messageCreationBox.SetActive(true);
            contactBanner.SetActive(true);
            nightRecap.SetActive(false);
        }
        else
        {
            // hide previous chat
            chatViewport.GetChild(selectedChatIndex).gameObject.SetActive(false);
            var previousOverviewItem = overviewScrollViewContent.GetChild(selectedChatIndex);
            previousOverviewItem.GetComponent<Image>().color = deselectedChatColor;
        }

        selectedChatIndex = chatIndex;

        var chat = chats[chatIndex];
        var chatTransform = (RectTransform)chat.transform;
        chatScrollRect.GetComponent<ScrollRect>().content = chatTransform;
        chatTransform.gameObject.SetActive(true);
        contactName.text = chat.transform.name;

        var overviewItem = overviewScrollViewContent.GetChild(selectedChatIndex);
        overviewItem.GetComponent<Image>().color = selectedChatColor;

        StoryStateManager.storyStates[chatIndex].unreadMessages = 0;
        UpdateMessageCountBubble(chatIndex);
        StoryStateManager.SaveStoryState();

        if (chat.lockText == "")
        {
            DisableMessageLockState(chatIndex);
        }
        else
        {
            EnableMessageLockState(chatIndex, chat.lockText, chat.lockSprite);
        }

        var creationTransform = messageCreationBox.transform.Find("Message").GetChild(0);
        var creationText = creationTransform.GetComponent<TextMeshProUGUI>();
        if (playerLockedChats.Contains(chatIndex))
        {
            // update message creation text
            int currentMessageIndex = StoryStateManager.storyStates[chatIndex].currentMessageIndex;
            var messageTransform = chats[chatIndex].transform.GetChild(currentMessageIndex);
            var messageTextTransform = messageTransform.Find("Message Content").Find("Message Text (TMP)");
            creationText.text = messageTextTransform.GetComponent<TextMeshProUGUI>().text;
        }
        else
        {
            creationText.text = "";
        }

        ScrollDown();
    }

    private void DisableMessageLockState(int chatIndex)
    {
        messageCreationBox.transform.Find("Locked").gameObject.SetActive(false);
        chats[chatIndex].lockText = "";
    }

    private void EnableMessageLockState(int chatIndex, string lockText, Sprite lockSprite = null)
    {
        var lockedTransform = messageCreationBox.transform.Find("Locked");
        var lockTextTransform = lockedTransform.Find("Text (TMP)");
        lockTextTransform.GetComponent<TextMeshProUGUI>().text = lockText;

        var requiredItemTransform = lockedTransform.Find("Required Item");
        if (lockSprite == null)
        {
            requiredItemTransform.gameObject.SetActive(false);
        }
        else
        {
            requiredItemTransform.GetComponent<Image>().sprite = lockSprite;
            requiredItemTransform.gameObject.SetActive(true);
        }

        lockedTransform.gameObject.SetActive(true);
        chats[chatIndex].lockText = lockText;
        chats[chatIndex].lockSprite = lockSprite;
    }

    private void SendNextMessage(int chatIndex, int messageIndex, Transform messageTransform)
    {
        long messageTime = DateTime.Now.ToFileTime();
        StoryStateManager.storyStates[chatIndex].messageTimes[messageIndex] = messageTime;
        StoryStateManager.storyStates[chatIndex].currentMessageIndex++;

        var dateTransform = messageTransform.Find("Message Content").Find("Date and Time (TMP)");
        dateTransform.GetComponent<TextMeshProUGUI>().text = DateTime.FromFileTime(messageTime).ToString("HH:mm");
        dateTransform.gameObject.SetActive(false);
        ScrollDown();

        if (selectedChatIndex != chatIndex)
        {
            // player hasn't read message yet
            StoryStateManager.storyStates[chatIndex].unreadMessages++;
            UpdateMessageCountBubble(chatIndex);
        }

        StoryStateManager.SaveStoryState();
        Debug.Log($"Wrote message {messageIndex} in chat {chatIndex}");
    }

    public IEnumerator WriteMessage(int chatIndex, int messageIndex, Transform messageTransform)
    {
        var disabledObjects = new List<GameObject>();
        TextMeshProUGUI textMesh = null;
        string messageText = "";

        // hide other elements and display ... in text element
        var contentTransform = messageTransform.Find("Message Content");

        var dateTimeTransform = contentTransform.Find("Date and Time (TMP)");
        dateTimeTransform.gameObject.SetActive(false);
        disabledObjects.Add(dateTimeTransform.gameObject);

        var messageTextTransform = contentTransform.Find("Message Text (TMP)");
        textMesh = messageTextTransform.GetComponent<TextMeshProUGUI>();
        messageText = textMesh.text;

        bool isPlayer = messageTransform.name.Contains("Player");
        ScrollDown();

        if (isPlayer)
        {
            var creationTransform = messageCreationBox.transform.Find("Message").GetChild(0);
            var creationText = creationTransform.GetComponent<TextMeshProUGUI>();

            for (int characterIndex = 1; characterIndex < messageText.Length; characterIndex++)
            {
                creationText.text = messageText[0..characterIndex];

                // try to scroll down every n characters
                if (characterIndex % 20 == 0)
                {
                    ScrollDown();
                }

                yield return new WaitForSeconds(writeCharacterTime);
            }

            while (playerLockedChats.Contains(chatIndex))
            {
                // wait for player to press send
                yield return new WaitForSeconds(refreshTime);
            }

            creationText.text = "";

            // restore original message
            textMesh.text = messageText;
            messageTransform.gameObject.SetActive(true);
        }
        else
        {
            textMesh.text = "...";
            messageTransform.gameObject.SetActive(true);
            ScrollDown();

            // wait for write time
            float writeTime = UnityEngine.Random.Range(writeMessageMinTime, writeMessageMaxTime);
            yield return new WaitForSeconds(writeTime);

            // restore original message
            textMesh.text = messageText;
        }

        foreach (var disabledObject in disabledObjects)
        {
            disabledObject.SetActive(true);
        }

        SendNextMessage(chatIndex, messageIndex, messageTransform);
    }

    private IEnumerator ProgressStory(int chatIndex)
    {
        var chatTransform = chats[chatIndex].transform;
        var storyState = StoryStateManager.storyStates[chatIndex];
        var storyline = chats[chatIndex].storyline;
        int messageCount = Mathf.Min(chatTransform.childCount, storyline.continuationConditions.Length);

        int startingMessageIndex = storyState.currentMessageIndex;
        if (startingMessageIndex == 0)
        {
            yield return new WaitForSeconds(messageStartDelay);
        }

        for (int messageIndex = startingMessageIndex; messageIndex < messageCount; messageIndex++)
        {
            Debug.Log($"{storyline.storylineName}: Currently at message {messageIndex}");

            var messageTransform = chats[chatIndex].transform.GetChild(messageIndex);
            var continuation = storyline.continuationConditions[messageIndex];

            if (selectedChatIndex != chatIndex && messageTransform.name.Contains("Player"))
            {
                // wait for player to select chat and respond
                Debug.Log($"{storyline.storylineName}: waiting for player to select chat and respond...");
                while (selectedChatIndex != chatIndex)
                {
                    yield return new WaitForSeconds(messageStartDelay);
                }
            }
            
            switch (continuation.condition)
            {
                case ConditionType.None:
                    // the next message should just be written
                    Debug.Log($"{storyline.storylineName}: instantly writing next message...");
                    break;
                case ConditionType.WaitForNextNight:
                    int lockNightIndex = StoryStateManager.storyStates[chatIndex].lockNightIndex;
                    if (messageIndex != startingMessageIndex || lockNightIndex == -1)
                    {
                        // previous message was just written, wait for next night
                        Debug.Log($"{storyline.storylineName}: Quit. Waiting for next day...");
                        StoryStateManager.storyStates[chatIndex].lockNightIndex = StoryStateManager.nightCount;
                        StoryStateManager.SaveStoryState();

                        EnableMessageLockState(chatIndex, "Waiting for next day...");
                        yield break;
                    }

                    if (lockNightIndex == StoryStateManager.nightCount)
                    {
                        Debug.Log($"{storyline.storylineName}: Quit. Night still hasn't passed. Waiting for next day...");
                        EnableMessageLockState(chatIndex, "Waiting for next day...");
                        yield break;
                    }

                    // last message was written previous night
                    Debug.Log($"{storyline.storylineName}: Continuing story from previous day...");
                    break;
                case ConditionType.TimeInSeconds:
                    // the next message should be written after a certain amount of time has passed
                    Debug.Log($"{storyline.storylineName}: waiting for continuation time");
                    var previousMessageTime = DateTime.FromFileTime(storyState.messageTimes[messageIndex - 1]);
                    int requiredTimePassed = int.Parse(continuation.value);    // should already be in seconds
                    var nextMessageTime = previousMessageTime.AddSeconds(requiredTimePassed);
                    var timeDifference = DateTime.Now.Subtract(nextMessageTime);

                    if (timeDifference.Seconds < 0)
                    {
                        Debug.Log($"{storyline.storylineName}: waiting for {timeDifference.Seconds} seconds");
                        yield return new WaitForSeconds(timeDifference.Seconds);
                    }
                    break;
                case ConditionType.Item:
                    int itemId = int.Parse(continuation.value);
                    var item = ItemManager.GetItem(itemId);
                    if (!StoryStateManager.collectedItems.Contains(itemId))
                    {
                        // item isn't collected yet
                        Debug.Log($"{storyline.storylineName}: Quit. Waiting for player to collect item...");
                        EnableMessageLockState(chatIndex, "Locked. Required Item:", item.sprite);
                        yield break;
                    }

                    Debug.Log($"{storyline.storylineName}: Player has collected item {item.itemName}. Continuing...");
                    playerLockedChats.Add(chatIndex);
                    break;
                case ConditionType.PressSendButton:
                    Debug.Log($"{storyline.storylineName}: Waiting for player to press send...");
                    playerLockedChats.Add(chatIndex);
                    break;
                default:
                    throw new NotImplementedException();
            }

            yield return WriteMessage(chatIndex, messageIndex, messageTransform);

            // wait a little bit before writing next message
            yield return new WaitForSeconds(messageInbetweenDelay);
        }

        Debug.Log($"{storyline.storylineName}: storyline comleted.");
    }

    public void SendCurrentChat()
    {
        playerLockedChats.Remove(selectedChatIndex);
    }

    public void OnBackToMenu()
    {
        SceneTransitionFader.TransitionToScene("StartMenu");
    }

    public void OnNextNight()
    {
        SceneTransitionFader.TransitionToScene("CampusOutside");
    }
}
