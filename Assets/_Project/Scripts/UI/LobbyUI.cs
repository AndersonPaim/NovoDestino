using System.Collections;
using Mirror;
using Mirror.Examples.Pong;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _capacityText;
    [SerializeField] private GameObject _waitingHostText;
    [SerializeField] private Button _startButton;
    private Lobby _lobby;
    private NetworkRelay _relay;

    public void Initialize(Lobby lobby, NetworkRelay relay)
    {
        _lobby = lobby;
        _relay = relay;
        _startButton.onClick.AddListener(StartGame);
        _nameText.text = lobby.Name;
        _capacityText.text = (lobby.MaxPlayers - lobby.AvailableSlots) + "/" + lobby.MaxPlayers;

        if ((lobby.MaxPlayers - lobby.AvailableSlots) > 1)
        {
            _waitingHostText.SetActive(true);
            _startButton.gameObject.SetActive(false);
        };

        StartCoroutine(HearthBeatLobby(15));

        /*LobbyEventCallbacks events = new LobbyEventCallbacks();

        events.PlayerJoined += players =>
        {
            CreateLobbyList();
        };*/
    }

    private IEnumerator HearthBeatLobby(float time)
    {
        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(_lobby.Id);
            yield return new WaitForSeconds(time);
        }
    }

    private void StartGame()
    {
        NetworkClient.localPlayer.GetComponent<PlayerConnection>().ReadyClick();
    }
}
