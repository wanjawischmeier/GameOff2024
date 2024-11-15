using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    public Button loadButton;   // TODO: disable load button if no save file is found

    private void Start()
    {
        if (StoryStateManager.saveExists)
        {
            loadButton.interactable = true;
        }
    }

    private void Update()
    {
        
    }

    public void NewGame()
    {
        StoryStateManager.ResetStoryState();
        LoadGame();
    }

    public void LoadGame()
    {
        SceneTransitionFader.TransitionToScene("ChatWindow");
    }

    public void Options()
    {
        SceneManager.LoadScene("Options Menu", LoadSceneMode.Additive);
    }
}
