using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    public GameObject promptPrefab;
    public Button loadButton;

    GameObject promptObj;

    private void Start()
    {
        if (StoryStateManager.saveExists)
        {
            loadButton.interactable = true;
        }
    }

    private void OnPromptCanceled()
    {
        Destroy(promptObj);
    }

    private void OnPromptConfirmed()
    {
        Debug.Log("Confirmed prompt to overwrite existing save.");
        NewGame(true);
    }

    public void NewGame(bool forceOverwrite = false)
    {
        if (StoryStateManager.saveExists && !forceOverwrite)
        {
            promptObj = Instantiate(promptPrefab, transform.Find("Canvas"));
            var prompt = promptObj.GetComponent<Prompt>();
            prompt.onCanceled += OnPromptCanceled;
            prompt.onConfirmed += OnPromptConfirmed;
            return;
        }

        StoryStateManager.ResetStoryState();
        LoadGame();
    }

    public void LoadGame()
    {
        SceneTransitionFader.TransitionToScene("ChatWindow");
    }

    public void Options()
    {
        SceneManager.LoadScene("OptionsMenu", LoadSceneMode.Additive);
    }
}
