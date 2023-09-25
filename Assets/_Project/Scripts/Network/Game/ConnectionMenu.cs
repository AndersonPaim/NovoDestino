using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectionMenu : MonoBehaviour
{
    public delegate void GameConnectionHandler();
    public GameConnectionHandler OnStartHost;
    public GameConnectionHandler OnStartClient;

    public delegate void SetNetworkAddressHandler(string address);
    public SetNetworkAddressHandler OnSetNetworkAddress;

    [SerializeField] private TMP_InputField _InputField;

    public void HostButton()
    {
        OnStartHost?.Invoke();
        gameObject.SetActive(false);
    }

    public void ClientButton()
    {
        if(_InputField.text == null)
        {
            return;
        }

        OnSetNetworkAddress?.Invoke(_InputField.text);
        OnStartClient?.Invoke();
        gameObject.SetActive(false);
    }

    public void ServerOnlyButton()
    {
///
    }
}
