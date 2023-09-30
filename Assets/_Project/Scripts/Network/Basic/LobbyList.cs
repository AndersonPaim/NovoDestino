using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
//using ParrelSync;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyList : MonoBehaviour
{
    [SerializeField] private LobbyEntryUI _lobbyEntryUIObj;
    [SerializeField] private Transform _lobbyUIPos;
    [SerializeField] private GameObject _listObject;
    [SerializeField] private LobbyUI _lobbyUI;
    [SerializeField] private GameObject _createLobbyUI;
    [SerializeField] private Button _lobbyCreationButton;
    [SerializeField] private Button _updateButton;
    [SerializeField] private Button _confirmCreateButton;
    [SerializeField] private TMP_InputField _lobbyName;
    [SerializeField] private TMP_InputField _maxPlayers;
    [SerializeField] private NetworkRelay _relay;

    private List<LobbyEntryUI> _lobbyList = new List<LobbyEntryUI>();
    public const string JoinCodeKey = "j";

    private async void Start()
    {
        _confirmCreateButton.onClick.AddListener(CreateLobby);
        _lobbyCreationButton.onClick.AddListener(ShowLobbyCreationUI);
        _updateButton.onClick.AddListener(CreateLobbyList);

        await Authenticate();
    }

    private async Task Authenticate()
    {
        var options = new InitializationOptions();

        /*#if UNITY_EDITOR
                options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
        #endif*/

        await UnityServices.InitializeAsync(options);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        CreateLobbyList();
    }

    private void ShowLobbyCreationUI()
    {
        _createLobbyUI.SetActive(true);
    }

    private async void CreateLobby()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(int.Parse(_maxPlayers.text));

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        CreateLobbyOptions options = new CreateLobbyOptions
        {
            Data = new Dictionary<string, DataObject> { { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) } }
        };

        _createLobbyUI.SetActive(false);
        Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(_lobbyName.text, int.Parse(_maxPlayers.text), options);
        CreateLobbyList();
        _listObject.SetActive(false);
        _relay.CreateGame(a);
        _lobbyUI.gameObject.SetActive(true);
        _lobbyUI.Initialize(lobby, _relay);
    }

    private async void CreateLobbyList()
    {
        foreach (LobbyEntryUI lobbyEntry in _lobbyList)
        {
            lobbyEntry.gameObject.SetActive(false);
        }

        QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync();

        foreach (Lobby lobby in lobbies.Results)
        {
            LobbyEntryUI lobbyUI = Instantiate(_lobbyEntryUIObj, _lobbyUIPos);
            lobbyUI.Initialize(lobby, _relay, _lobbyUI);
            _lobbyList.Add(lobbyUI);
        }
    }
}
