using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{
    public void GoBack()
    {
        SceneManager.LoadScene("Start Menu");
    }
}
