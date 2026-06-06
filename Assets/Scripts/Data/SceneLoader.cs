using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadLobby()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    public void LoadMainGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    public void LoadGacha()
    {
        SceneManager.LoadScene("GachaScene");
    }
}
