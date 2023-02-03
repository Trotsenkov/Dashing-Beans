using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Menu : MonoBehaviour
{
    public new TMP_Text name;
    public void StartGame(bool isHost)
    {
        if (name.text == "")
            name.text = "Player";
        
        SceneManager.LoadScene(1);
        NetworkManagerBeans.isHost = isHost;
        NetworkManagerBeans.localPlayerName = name.text;
    }
}