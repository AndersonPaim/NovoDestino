using System.Collections.Generic;
using Mirror.Examples.Pong;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyEntryUI : MonoBehaviour
{
    [SerializeField] private PlayerConnection _connection;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _capacityText;
    [SerializeField] private Button _joinButton;
    private Lobby _lobby;
    private LobbyUI _lobbyUI;
    private NetworkRelay _relay;
    public const string JoinCodeKey = "j";

    public void Initialize(Lobby lobby, NetworkRelay relay, LobbyUI lobbyUI)
    {
        _lobby = lobby;
        _relay = relay;
        _lobbyUI = lobbyUI;
        _joinButton.onClick.AddListener(JoinLobby);
        _nameText.text = lobby.Name;
        _capacityText.text = (lobby.MaxPlayers - lobby.AvailableSlots) + "/" + lobby.MaxPlayers;
    }

    private async void JoinLobby()
    {
        Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(_lobby.Id);
        _relay.JoinGame(lobby);
        _lobbyUI.gameObject.SetActive(true);
        _lobbyUI.Initialize(lobby, _relay);
    }
}
