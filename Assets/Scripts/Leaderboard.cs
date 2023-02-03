using TMPro;
using UnityEngine;
using Mirror;
using System.Collections;

public class Leaderboard : NetworkBehaviour
{
    [SyncVar]
    string leaders;
    [SyncVar]
    string winner;
    [SerializeField] TMP_Text Leaders;
    [SerializeField] TMP_Text Winner;
    [SerializeField] int MaxScore = 3;

    static Leaderboard singleton;
    private void Awake()
    {
        singleton = this;
    }
    void Update()
    {
        if (isServer)
        {
            leaders = "";
            foreach (var kvp in PlayerMovement.playerNames)
                leaders += kvp.Key + ": " + kvp.Value + '\n';
        }

        Leaders.text = leaders;
        Winner.text = winner;
    }

    static Coroutine cor;
    [ServerCallback]
    public static void CheckWin(string name)
    {
        if (cor == null)
            if (PlayerMovement.playerNames[name] >= singleton.MaxScore)
                cor = singleton.StartCoroutine(singleton.Restart(6, name));
    }
    IEnumerator Restart(float time, string name)
    {
        while (time > 0)
        {
            winner = name + " won!!\nRestart in " + (time-1);
            time--;
            yield return new WaitForSeconds(1f);
        }
        winner = "";
        NetworkManagerBeans.Restart();
        cor = null;
    }
}