using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

[AddComponentMenu("")]
    public class BeansAuthenticator : NetworkAuthenticator
    {
        readonly HashSet<NetworkConnection> connectionsPendingDisconnect = new HashSet<NetworkConnection>();

        [Header("Client Username")]
        public string playerName;

        #region Messages

        public struct AuthRequestMessage : NetworkMessage
        {
            public string authUsername;
        }

        public struct AuthResponseMessage : NetworkMessage
        {
            public byte code;
            public string message;
        }

        #endregion

        #region Server

        public override void OnStartServer() => NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);

        public override void OnStopServer() => NetworkServer.UnregisterHandler<AuthRequestMessage>();
        public override void OnServerAuthenticate(NetworkConnectionToClient conn) { /*do nothing...wait for AuthRequestMessage from client*/ }

        public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg)
        {
            Debug.Log($"Authentication Request: {msg.authUsername}");

            if (connectionsPendingDisconnect.Contains(conn)) return;

            if (!PlayerMovement.playerNames.ContainsKey(msg.authUsername))
            {
                PlayerMovement.playerNames.Add(msg.authUsername, 0);

                conn.authenticationData = msg.authUsername;

                AuthResponseMessage authResponseMessage = new AuthResponseMessage
                {
                    code = 100,
                    message = "Success"
                };

                conn.Send(authResponseMessage);

                ServerAccept(conn);
            }
            else
            {
                connectionsPendingDisconnect.Add(conn);

                AuthResponseMessage authResponseMessage = new AuthResponseMessage
                {
                    code = 200,
                    message = "Username already in use...try again"
                };

                conn.Send(authResponseMessage);

                conn.isAuthenticated = false;

                StartCoroutine(DelayedDisconnect(conn, 1f));
            }
        }

        IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            ServerReject(conn);

            yield return null;

            connectionsPendingDisconnect.Remove(conn);
        }

        #endregion

        #region Client

        public void SetPlayername(string username)
        {
            playerName = username;
        }

        public override void OnStartClient()
        {
            NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
        }

        public override void OnStopClient() => NetworkClient.UnregisterHandler<AuthResponseMessage>();

        public override void OnClientAuthenticate()
        {
            AuthRequestMessage authRequestMessage = new AuthRequestMessage
            {
                authUsername = playerName,
            };

            NetworkClient.Send(authRequestMessage);
        }

        public void OnAuthResponseMessage(AuthResponseMessage msg)
        {
            if (msg.code == 100)
            {
                Debug.Log($"Authentication Response: {msg.message}");

                ClientAccept();
            }
            else
            {
                Debug.LogError($"Authentication Response: {msg.message}");

                NetworkManager.singleton.StopHost();

            SceneManager.LoadScene("Menu");
            }
        }

        #endregion
    }