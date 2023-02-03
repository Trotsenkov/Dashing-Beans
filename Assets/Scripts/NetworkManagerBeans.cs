using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NetworkManagerBeans : NetworkManager
{
    List<Transform> players= new List<Transform>();
    public static new NetworkManagerBeans singleton { get; private set; }
    public static bool isHost;
    public static string localPlayerName;
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Vector3 start = SpawnPoint.Get();
        GameObject player = Instantiate(playerPrefab, start, Quaternion.identity);
        players.Add(player.transform);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
    public override void Awake()
    {
        base.Awake();
        singleton = this;
    }
    public override void Start()
    {
        GetComponent<BeansAuthenticator>().SetPlayername(localPlayerName);
        if (isHost)
            singleton.StartHost();
        else
            singleton.StartClient();

        base.Start();
    }
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        if (conn.authenticationData != null)
            PlayerMovement.playerNames.Remove((string)conn.authenticationData);

        base.OnServerDisconnect(conn);
    }

    [ServerCallback]
    public static void Restart()
    {
        List<string> keys = PlayerMovement.playerNames.Keys.ToList<string>();
        for (int i = 0; i < keys.Count; i++)
            PlayerMovement.playerNames[keys[i]] = 0;

        SpawnPoint.Restart();
        foreach (Transform player in singleton.players)
            player.position = SpawnPoint.Get();
    }
}
