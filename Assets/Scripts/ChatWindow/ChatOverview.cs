using TMPro;
using UnityEngine;

public class ChatOverview : MonoBehaviour
{
    public Transform chatViewport;
    public GameObject nightRecap, contactBanner;
    public TextMeshProUGUI contactName;

    int selectedChat = -1;

    public void ChatSelected(int chat)
    {
        if (selectedChat == -1)
        {
            nightRecap.SetActive(false);
            contactBanner.SetActive(true);
        }
        else
        {
            // hide previous chat
            chatViewport.GetChild(selectedChat).gameObject.SetActive(false);
        }

        selectedChat = chat;
        var selectedChatTransform = chatViewport.GetChild(selectedChat);
        selectedChatTransform.gameObject.SetActive(true);
        contactName.text = selectedChatTransform.name;
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
