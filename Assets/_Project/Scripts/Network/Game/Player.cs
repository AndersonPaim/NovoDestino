using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace Mirror.Examples.Pong
{
    public class Player : NetworkBehaviour
    {
        [SerializeField] private PlayerConnection _connection;
        [SerializeField] private TextMeshProUGUI _nicknameText;
        [SerializeField] private Rigidbody _rb;

        public PlayerConnection Connection => _connection;

        private bool _isGrounded = true;
        private int _direction = 1;
        [SyncVar]
        private int _playerNumber;
        private bool _canMove = false;
        private GameObject _currentWeapon;
        private bool _isRunning;
        private bool _isJumping;
        private Animator _anim;

        [SyncVar(hook = nameof(SetNicknameText))]
        public string PlayerName;
        public int PlayerNumber => _playerNumber;

        public void SetupPlayer(int number)
        {
            _playerNumber = number;
        }

        public IEnumerator SpawnDelay()
        {
            yield return new WaitForSeconds(1);
            SpawnPosition();
        }

        public void SpawnPosition()
        {
            _canMove = true;

            if (_playerNumber == 1)
            {
                transform.position = new Vector3(15, 100, 0);
            }
            else
            {
                transform.position = new Vector3(-25, 100, 0);
            }

            _rb.useGravity = true;

            StartCoroutine(SetUIPlayers());

        }

        private IEnumerator SetUIPlayers()
        {
            yield return new WaitForSeconds(1);
            //_uiManager = FindObjectOfType<UIManager>();
            //_uiManager.SetupPlayer(this);
        }

        private void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
            NetworkLobby.OnStartGame += SpawnPosition;
            //_anim = GetComponent<Animator>();
            //_audioManager = Transform.FindObjectOfType<AudioManager>();

            if (!hasAuthority)
            {
                //GetComponent<AudioListener>().enabled = false;
            }
        }

        private void Update()
        {

            if (_connection.hasAuthority && isLocalPlayer && _canMove)
            {
                //InputListener();
            }

            if (_connection.hasAuthority)
            {
                /*_anim.SetBool("isRunning", _isRunning);
                _anim.SetBool("isJumping", _isJumping);
                _anim.SetBool("isGrounded", _isGrounded);*/
            }
        }

        private void SetNicknameText(string oldValue, string newValue)
        {
            _nicknameText.text = newValue;
        }
    }
}
