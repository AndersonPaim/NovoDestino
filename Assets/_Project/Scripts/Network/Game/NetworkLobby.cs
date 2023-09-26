using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Pong;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NetworkLobby : NetworkBehaviour
{
    public delegate void SetPlayerNameHandler(Player player, string name);
    public static SetPlayerNameHandler OnSetPlayerName;

    public static event Action OnStartGame;

    [SerializeField] private GameSystem _gameSystem;
    [SerializeField] private Button _readyButton;
    //[SerializeField] private Button _nameButton;
    //[SerializeField] private GameObject _nameInputMenu;
    //[SerializeField] private GameObject _errorMessage;
    //[SerializeField] private TMP_InputField _nameInput;
    //[SerializeField] private UiLobbyPlayer _uiLobbyPlayer;
    //[SerializeField] private List<GameObject> _emptyLobbyUI;
    //[SerializeField] private Transform _uiLobbyPlayerPos;
    [SerializeField] private List<string> _scenes = new List<string>();

    [SyncVar]
    private int _readyPlayers;
    private Dictionary<String, UiLobbyPlayer> _playersConfirmed = new Dictionary<string, UiLobbyPlayer>();
    private bool _canStart = true;

    [SyncVar]
    private string _scene;

    private void Start()
    {
        Initialize();

        if (isServer)
        {
            int rand = UnityEngine.Random.Range(0, _scenes.Count);
            _scene = _scenes[rand];
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            HandleReadyButtonClicked();
        }
    }

    private void Initialize()
    {
        _readyButton.onClick.AddListener(HandleReadyButtonClicked);
        //_nameButton.onClick.AddListener(HandlerInputNameClicked);
        PlayerConnection.OnAddReady += AddReady;
        PlayerConnection.OnRemoveReady += RemoveReady;
    }

    /*private void HandlerInputNameClicked()
    {
        _nameInputMenu.SetActive(false);
        OnSetPlayerName?.Invoke(_gameSystem.LocalPlayer, _nameInput.text);
    }*/

    private void HandleReadyButtonClicked()
    {
        NetworkClient.localPlayer.GetComponent<PlayerConnection>().ReadyClick();

        //TODO ONLY ALLOW TO READY IF CLIENT AND HOST ARE CONNECTED

        /*if(!isServer)
		{
			NetworkClient.localPlayer.GetComponent<PlayerConnection>().ReadyClick();
		}
		else
		{
			_errorMessage.SetActive(true);
			StartCoroutine(CloseErrorMessage());
		}*/
    }

    /*private IEnumerator CloseErrorMessage()
    {
        yield return new WaitForSeconds(2);
        _errorMessage.SetActive(false);
    }*/

    private void AddReady(Player player)
    {
        Debug.Log(player.PlayerName);

        //_emptyLobbyUI[_readyPlayers].SetActive(false);
        _readyPlayers++;

        /*UiLobbyPlayer lobbyPlayer = Instantiate(_uiLobbyPlayer, _uiLobbyPlayerPos);
        lobbyPlayer.transform.SetAsFirstSibling();
        lobbyPlayer.SetupUIPlayer(player);
3
        _playersConfirmed.Add(player.PlayerName, lobbyPlayer);*/

        //if (_readyPlayers == 2)
        //{
        _canStart = true;
        StartCoroutine(StartGame());
        //}
    }

    private void RemoveReady(Player player)
    {
        /*_playersConfirmed[player.PlayerName].gameObject.SetActive(false);
        _playersConfirmed.Remove(player.PlayerName);*/

        _readyPlayers--;

        //_emptyLobbyUI[_readyPlayers].SetActive(true);

        if (_readyPlayers == 1)
        {
            _canStart = false;
        }
    }

    private IEnumerator StartGame()
    {
        yield return new WaitForSeconds(1);

        if (_canStart)
        {
            SetScene();
        }
    }

    private void SetScene()
    {
        SceneManager.LoadScene(_scene);
        OnStartGame?.Invoke();

        if (_scene == "Space")
        {
            _gameSystem.LocalPlayer.GetComponent<Rigidbody2D>().gravityScale = 4;
        }
    }
}
