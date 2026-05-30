using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void LoadGacha()
    {
        SceneManager.LoadScene("GachaScene");
    }
}
