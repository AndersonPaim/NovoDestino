using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class NetworkRelay : MonoBehaviour
{
    [SerializeField] private UnityTransport _transport;
    public const string JoinCodeKey = "j";

    public void CreateGame(Allocation a)
    {
        _transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);
        NetworkManager.singleton.StartHost();
    }

    public async void JoinGame(Lobby lobby)
    {
        Debug.Log("JOIN CODE: " + lobby.Data[JoinCodeKey].Value);
        JoinAllocation a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);
        _transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
        NetworkManager.singleton.StartClient();
    }
}
