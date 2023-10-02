using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using DG.Tweening;

public class Grenade : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particle;
    [SerializeField] private float _grenadePullForce;
    [SerializeField] private float _grenadeRange;
    [SerializeField] private float _destroyTime;

    private Rigidbody _rb;
    private bool _isEnabled = true;
    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isEnabled)
        {
            return;
        }
        Vector3 addAltitude = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);
        _isEnabled = false;
        _rb.isKinematic = true;
        _particle.Play();

        Collider[] nearObjects = Physics.OverlapSphere(transform.position, _grenadeRange);

        foreach (Collider nearObject in nearObjects)
        {
            Rigidbody objectRb = nearObject.GetComponent<Rigidbody>();

            if (objectRb != null)
            {
                nearObject.gameObject.transform.DOMove(addAltitude, 0.5f);
            }
        }

        DestroyDelay();
    }

    private async void DestroyDelay()
    {
        await Task.Delay((int)_destroyTime * 1000);
        Destroy(gameObject);
    }
}
