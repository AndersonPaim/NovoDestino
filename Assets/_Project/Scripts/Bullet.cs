using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _destroyTime;

    private Rigidbody _rb;

    public void Shoot(float shootForce)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(transform.forward * shootForce);
        DestroyDelay();
    }

    private async void DestroyDelay()
    {
        await Task.Delay((int)_destroyTime * 1000);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}
