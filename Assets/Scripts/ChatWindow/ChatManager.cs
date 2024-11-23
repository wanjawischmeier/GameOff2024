using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    struct Chat
    {
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

    const float chatScrollDownThreshold = 0.1f;
    const float messageStartDelay = 2;
    const float writeMessageMinTime = 0.5f;
    const float writeMessageMaxTime = 4;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundtrackQueue(AudioManager.Lookup.chatWindowSoundtrack, silenceDuration: 5);
        }

        chatViewport = chatScrollRect.transform.GetChild(0);
        chatScrollRect.verticalNormalizedPosition = 0;

        chats = new Chat[chatViewport.childCount];
        for (int chatIndex = 0; chatIndex < chatViewport.childCount; chatIndex++)
        {
            var chatTransform = chatViewport.GetChild(chatIndex);
            chats[chatIndex] = new Chat()
            {
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
            UpdateMessagCountBubble(chatIndex);
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

    private bool ScrollDown(bool unconditionally = false)
    {
        if (chatScrollRect.verticalNormalizedPosition >= chatScrollDownThreshold && !unconditionally)
        {
            return false;
        }

        // only scroll down further if user is already scrolled down
        UpdateLayout(transform);
        chatScrollRect.verticalNormalizedPosition = 0;

        return true;
    }

    private void UpdateMessagCountBubble(int chatIndex)
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

        chats[chatIndex].transform.gameObject.SetActive(true);
        contactName.text = chats[chatIndex].transform.name;

        var overviewItem = overviewScrollViewContent.GetChild(selectedChatIndex);
        overviewItem.GetComponent<Image>().color = selectedChatColor;

        StoryStateManager.storyStates[chatIndex].unreadMessages = 0;
        UpdateMessagCountBubble(chatIndex);
        StoryStateManager.SaveStoryState();

        ScrollDown(true);
    }

    private void SendNextMessage(int chatIndex, int messageIndex, Transform messageTransform)
    {
        long messageTime = DateTime.Now.ToFileTime();
        StoryStateManager.storyStates[chatIndex].messageTimes[messageIndex] = messageTime;
        StoryStateManager.storyStates[chatIndex].currentMessageIndex++;
        ScrollDown();

        var dateTransform = messageTransform.Find("Message Content").Find("Date and Time (TMP)");
        dateTransform.GetComponent<TextMeshProUGUI>().text = DateTime.FromFileTime(messageTime).ToString("HH:mm");

        if (selectedChatIndex != chatIndex)
        {
            // player hasn't read message yet
            StoryStateManager.storyStates[chatIndex].unreadMessages++;
            UpdateMessagCountBubble(chatIndex);
        }

        StoryStateManager.SaveStoryState();
        Debug.Log($"Wrote message {messageIndex} in chat {chatIndex}");
    }

    public IEnumerator WriteMessage(int chatIndex, int messageIndex, Transform messageTransform)
    {
        messageTransform.gameObject.SetActive(true);
        var disabledObjects = new List<GameObject>();
        TextMeshProUGUI textMesh = null;
        string messageText = "";

        // hide other elements and display ... in text element
        var contentTransform = messageTransform.Find("Message Content");
        for (int i = 0; i < contentTransform.childCount; i++)
        {
            var child = contentTransform.GetChild(i);
            if (child.name == "Message Text (TMP)")
            {
                textMesh = child.GetComponent<TextMeshProUGUI>();
                messageText = textMesh.text;
                textMesh.text = "...";
            }
            else
            {
                child.gameObject.SetActive(false);
                disabledObjects.Add(child.gameObject);
            }
        }
        ScrollDown();

        // wait for write time
        float writeTime = UnityEngine.Random.Range(writeMessageMinTime, writeMessageMaxTime);
        yield return new WaitForSeconds(writeTime);

        // restore original message
        if (textMesh != null)
        {
            textMesh.text = messageText;
        }

        foreach (var disabledObject in disabledObjects)
        {
            disabledObject.SetActive(true);
        }

        SendNextMessage(chatIndex, messageIndex, messageTransform);
    }

    public IEnumerator ProgressStory(int chatIndex)
    {
        var chatTransform = chats[chatIndex].transform;
        var storyState = StoryStateManager.storyStates[chatIndex];
        var storyline = chats[chatIndex].storyline;
        int messageCount = Mathf.Min(chatTransform.childCount, storyline.continuationConditions.Length);

        yield return new WaitForSeconds(messageStartDelay);

        for (int messageIndex = storyState.currentMessageIndex; messageIndex < messageCount; messageIndex++)
        {
            Debug.Log($"{storyline.storylineName}: Currently at message {messageIndex}");

            var messageTransform = chats[chatIndex].transform.GetChild(messageIndex);
            var continuationCondition = storyline.continuationConditions[messageIndex];

            if (selectedChatIndex != chatIndex && messageTransform.name.Contains("Player"))
            {
                // wait for player to select chat and respond
                Debug.Log($"{storyline.storylineName}: waiting for player to select chat and respond...");
                while (selectedChatIndex != chatIndex)
                {
                    yield return new WaitForSeconds(messageStartDelay);
                }
            }

            switch (continuationCondition.condition)
            {
                case Storyline.ConditionType.None:
                    // the next message should just be written
                    Debug.Log($"{storyline.storylineName}: instantly writing next message...");
                    break;
                case Storyline.ConditionType.TimeInSeconds:
                    // the next message should be written after a certain amount of time has passed
                    Debug.Log($"{storyline.storylineName}: waiting for continuation time");
                    var previousMessageTime = DateTime.FromFileTime(storyState.messageTimes[messageIndex - 1]);
                    int requiredTimePassed = int.Parse(continuationCondition.value);    // should already be in seconds
                    var nextMessageTime = previousMessageTime.AddSeconds(requiredTimePassed);
                    var timeDifference = DateTime.Now.Subtract(nextMessageTime);

                    if (timeDifference.Seconds < 0)
                    {
                        Debug.Log($"{storyline.storylineName}: waiting for {timeDifference.Seconds} seconds");
                        yield return new WaitForSeconds(timeDifference.Seconds);
                    }
                    break;
                case Storyline.ConditionType.Item:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }

            yield return WriteMessage(chatIndex, messageIndex, messageTransform);

            // wait a little bit before writing next message
            float writeTime = UnityEngine.Random.Range(writeMessageMinTime, writeMessageMaxTime);
            yield return new WaitForSeconds(writeTime);
        }

        Debug.Log($"{storyline.storylineName}: storyline comleted.");
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
