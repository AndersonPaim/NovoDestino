using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Mirror.NetworkBehaviour;

namespace Mirror.Examples.Pong
{

    [AddComponentMenu("")]
    public class GameSystem : NetworkManager
    {
        NetworkManager manager;
        public static event Action OnConnectPlayer;
        public static event Action OnDisconnectPlayer;

        [SerializeField] private List<Player> _players = new List<Player>();
        [SerializeField] private ConnectionMenu _connectionMenu;

        public Player LocalPlayer { get; set; }
        public List<Player> Players => _players;

        public override void Start()
        {
            SetupEvents();
            manager = GetComponent<NetworkManager>();
        }

        private void SetupEvents()
        {
            _connectionMenu.OnStartHost += StartHostEvent;
            _connectionMenu.OnStartClient += StartClientEvent;
            _connectionMenu.OnSetNetworkAddress += SetNetworkAddress;
            PlayerConnection.OnLocalPlayer += SetLocalPlayer;
        }

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            GameObject player = Instantiate(playerPrefab);
            Player p = player.GetComponent<Player>();
            _players.Add(p);

            Debug.Log("NUM PLAYERS: " + numPlayers);
            p.SetupPlayer(numPlayers);
            NetworkServer.AddPlayerForConnection(conn, player);
            OnConnectPlayer?.Invoke();
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            // call base functionality (actually destroys the player)
            base.OnServerDisconnect(conn);
            OnDisconnectPlayer?.Invoke();
        }

        public void SetLocalPlayer(Player player)
        {
            LocalPlayer = player;
        }

        public void StartHostEvent()
        {
            if (!NetworkClient.active)
            {
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    manager.StartHost();
                }
            }
        }

        public void StartClientEvent()
        {
            if (!NetworkClient.active)
            {
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    manager.StartClient();
                }
            }
        }

        public void StartServerOnly()
        {
            if (!NetworkClient.active)
            {
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    manager.StartServer();
                }
            }
        }

        private void SetNetworkAddress(string address)
        {
            manager.networkAddress = address;
        }
    }
}
