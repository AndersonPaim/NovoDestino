using TMPro;
using UnityEngine;

namespace Mirror.Examples.Pong
{
    public class UiLobbyPlayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        public void SetupUIPlayer(Player player)
        {
            _nameText.text = player.PlayerName;
        }
    }
}