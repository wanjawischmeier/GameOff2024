using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button loadButton;   // TODO: disable load button if no save file is found

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void NewGame()
    {

    }

    public void LoadGame()
    {

    }

    public void Options()
    {
        SceneManager.LoadScene("Options Menu");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
