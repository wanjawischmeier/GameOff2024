using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class StartMenuManager : MonoBehaviour
{
    public GameObject promptPrefab;
    public RectTransform startMenuTransform, optionsMenuTransform;
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

    public void ShowOptions()
    {
        LeanTween.moveX(startMenuTransform, -startMenuTransform.rect.width, SceneTransitionFader.sceneTransitionOutTime).setOnComplete(() =>
        {
            optionsMenuTransform.Translate(-startMenuTransform.rect.width, 0, 0);
            startMenuTransform.gameObject.SetActive(false);
            optionsMenuTransform.gameObject.SetActive(true);
            LeanTween.moveX(optionsMenuTransform, 0, SceneTransitionFader.sceneTransitionInTime);
        });
    }

    public void HideOptions()
    {
        LeanTween.moveX(optionsMenuTransform, -optionsMenuTransform.rect.width, SceneTransitionFader.sceneTransitionOutTime).setOnComplete(() =>
        {
            startMenuTransform.Translate(-startMenuTransform.rect.width, 0, 0);
            startMenuTransform.gameObject.SetActive(true);
            optionsMenuTransform.gameObject.SetActive(false);
            LeanTween.moveX(startMenuTransform, 0, SceneTransitionFader.sceneTransitionInTime);
        });
    }
}
