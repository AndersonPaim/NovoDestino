using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Mirror.Examples.Pong
{
    public class PlayerConnection : NetworkBehaviour
    {
        public delegate void LocalPlayerHandler(Player player);
        public delegate void ReadyHandler(Player player);
        public static LocalPlayerHandler OnLocalPlayer;
        public static ReadyHandler OnAddReady;
        public static ReadyHandler OnRemoveReady;
        public delegate void PlayerDeadHandler(Player player);
        public static PlayerDeadHandler OnPlayerDead;

        public TextMeshProUGUI isReady;
        public TextMeshProUGUI isLocal;
        public TextMeshProUGUI isAuth;


        [SerializeField] private Player _player;
        //[SerializeField] private GameObject _bulletObj;
        //[SerializeField] private UIManager _uiManager;

        public Player LocalPlayer { get; set; }
        public bool IsReady = false;

        private void Start()
        {
            if (hasAuthority)
            {
                LocalPlayer = _player;
                OnLocalPlayer?.Invoke(_player);

                if (isClient)
                {
                    NetworkLobby.OnSetPlayerName += CmdSetPlayerName;
                    //Player.OnShoot += CmdShoot;
                }
            }
        }

        private void Update()
        {
            isReady.text = "IS READY: " + IsReady.ToString();
            isLocal.text = "IS LOCAL: " + isLocalPlayer.ToString();
            isAuth.text = "IS OWNED: " + isOwned.ToString();

            if (isServer)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    RpcChangeScene("Ventos");
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    RpcChangeScene("Space");
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    RpcChangeScene("Angular");
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    RpcChangeScene("Ice");
                }
            }
        }

        [ClientRpc]
        private void RpcChangeScene(string scene)
        {
            if (scene == "Space")
            {
                _player.GetComponent<Rigidbody2D>().gravityScale = 4;
            }
            else
            {
                _player.GetComponent<Rigidbody2D>().gravityScale = 9;
            }

            SceneManager.LoadScene(scene);
            StartCoroutine(SetUIPlayers());

        }

        private IEnumerator SetUIPlayers()
        {
            yield return new WaitForSeconds(1);
            //_uiManager = FindObjectOfType<UIManager>();
            //_uiManager.SetupPlayer(_player);
        }

        private void OnDestroy()
        {
            if (hasAuthority)
            {
                if (isClient)
                {
                    NetworkLobby.OnSetPlayerName -= CmdSetPlayerName;
                    //Player.OnShoot -= CmdShoot;
                }
            }
        }
        public void ReadyClick()
        {
            if (IsReady)
            {
                IsReady = false;

                if (isClient && hasAuthority)
                {
                    CmdRemoveReady(LocalPlayer);
                }
                else if (isServer)
                {
                    RpcRemoveReady(LocalPlayer);
                }
            }
            else
            {
                IsReady = true;

                if (isServer)
                {
                    Debug.Log(LocalPlayer);
                    RpcAddReady(LocalPlayer);
                }
                else if (isClient)
                {
                    CmdAddReady(LocalPlayer);
                }

            }
        }

        [Command]
        private void CmdSetPlayerName(Player player, string name)
        {
            player.PlayerName = name;
        }

        [Command]
        public void CmdAddReady(Player player)
        {
            RpcAddReady(player);

            if (isServerOnly)
            {
                OnAddReady?.Invoke(player);
            }
        }

        [Command]
        public void CmdPlayerDead(Player player)
        {
            Debug.Log("CmdPlayerDead");
            RpcPlayerDead(player);
        }

        [Command]
        public void CmdRemoveReady(Player player)
        {
            RpcRemoveReady(player);

            if (isServerOnly)
            {
                OnRemoveReady?.Invoke(player);
            }
        }

        [Command]
        private void CmdShoot(Vector3 pos, Quaternion rot, GameObject bullet)
        {
            /*GameObject obj = Instantiate(_bulletObj, pos, rot);
            obj.GetComponent<Bullet>().Shoot();
            NetworkServer.Spawn(obj);*/
        }

        [ClientRpc]
        public void RpcAddReady(Player player)
        {
            OnAddReady?.Invoke(player);
        }

        [ClientRpc]
        public void RpcRemoveReady(Player player)
        {
            OnRemoveReady?.Invoke(player);
        }

        [ClientRpc]
        public void RpcPlayerDead(Player player)
        {
            OnPlayerDead?.Invoke(player);
        }

    }
}
