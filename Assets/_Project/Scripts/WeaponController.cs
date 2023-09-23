using System.Threading.Tasks;
using UnityEngine;
using Cinemachine;
using TMPro;
using DG.Tweening;
using UnityEditor.Experimental.GraphView;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera _eyeCamera;
    [SerializeField] private CinemachineVirtualCamera _scopeCamera;
    [SerializeField] private PlayerCam _playerCam;
    [SerializeField] private float _shootForce;
    [SerializeField] private ParticleSystem _shootParticle;
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private Transform _bulletPosition;
    [SerializeField] private Transform _recoilPivot;
    [SerializeField] private GameObject _hipFireCrosshair;
    [SerializeField] private float _fireRate;
    [SerializeField] private Vector2 _recoilDistance;
    [SerializeField] private float _magazine;
    [SerializeField] private float _damage;
    [SerializeField] private DamageType _damageType;
    [SerializeField] private TextMeshProUGUI _bulletsText;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private AudioObj _audioObj;

    private Animator _animator;
    private NewControls _input;
    private bool _canShoot = true;
    private bool _isAiming = false;
    private bool _isReloading = false;
    private bool _isShooting = false;
    private float _bullets;

    public bool IsAiming => _isAiming;

    public void ReloadWeapon()
    {
        _bullets = _magazine;
        _bulletsText.text = _bullets.ToString();
        _isReloading = false;
    }

    public void StartReload()
    {
        _isReloading = true;
        StopAim();
    }

    public void StopReload()
    {
        _isReloading = false;
    }

    public void ShootBullet()
    {
        if (_bullets <= 0)
        {
            _animator.SetBool("Shoot", false);
            Reload();
        }
        else
        {
            Bullet bullet = Instantiate(_bulletPrefab);

            AudioObj audio = Instantiate(_audioObj);
            audio.transform.position = transform.position;
            audio.Initialize(_audioClip);

            bullet.transform.localPosition = _bulletPosition.transform.position;
            bullet.transform.localRotation = _bulletPosition.transform.rotation;
            bullet.Shoot(_shootForce, _damage, _damageType);
            _shootParticle.Play();
            _bullets--;
            _bulletsText.text = _bullets.ToString();
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _animator = GetComponent<Animator>();
        _input = new NewControls();
        _input.Enable();
        _input.Player.Shoot.started += _ => Shoot();
        _input.Player.Shoot.canceled += _ => StopShoot();
        _input.Player.Aim.performed += _ => Aim();
        _input.Player.Aim.canceled += _ => StopAim();
        _input.Player.Reload.performed += _ => Reload();

        _bullets = _magazine;
        _bulletsText.text = _bullets.ToString();
    }

    private void Reload()
    {
        if (_bullets < _magazine && !_isReloading)
        {
            _animator.SetTrigger("Reload");
            _isShooting = false;
            _playerCam.ResetRecoilOffset();
        }
    }

    private void Aim()
    {
        bool isRunning = _animator.GetCurrentAnimatorStateInfo(0).IsName("RunningWeaponLoop");

        if (_isReloading || isRunning)
        {
            return;
        }

        _isAiming = true;
        _eyeCamera.Priority = 0;
        _scopeCamera.Priority = 1;
        _hipFireCrosshair.SetActive(false);
    }

    public void StopAim()
    {
        _isAiming = false;
        _eyeCamera.Priority = 1;
        _scopeCamera.Priority = 0;
        _hipFireCrosshair.SetActive(true);
    }

    private void Shoot()
    {
        if (_canShoot && _bullets > 0)
        {
            float fireRate = (_fireRate / 60000) * 166;
            _animator.SetFloat("FireRate", fireRate);
            _animator.SetBool("Shoot", true);
            _isShooting = true;
            Recoil();
        }
        else
        {
            Reload();
        }
    }

    private async void StopShoot()
    {
        _animator.SetBool("Shoot", false);
        _canShoot = false;
        await Task.Delay(100);
        _canShoot = true;
        _isShooting = false;
        _playerCam.ResetRecoilOffset();
    }

    private async void Recoil()
    {
        while (_isShooting)
        {
            float rightOrLeft = Random.Range(-_recoilDistance.y, _recoilDistance.y);
            float doGoUp = Random.Range(0, _recoilDistance.x);
            Vector3 direction = new Vector3(doGoUp, rightOrLeft, 0);

            _playerCam.SetRecoilOffset(direction);
            await Task.Delay(50);
        }
    }
}
