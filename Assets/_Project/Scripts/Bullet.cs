using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _destroyTime;
    [SerializeField] private ParticleSystem _hitParticle;
    [SerializeField] private Collider _collider;

    private Rigidbody _rb;
    private DamageType _damageType;
    private float _damage;

    public void Shoot(float shootForce, float damage, DamageType damageType)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(transform.forward * shootForce);
        DestroyDelay();
        _damageType = damageType;
        _damage = damage;
    }

    private async void DestroyDelay()
    {
        await Task.Delay((int)_destroyTime * 1000);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLLIDE");
        _hitParticle.Play();
        _collider.enabled = false;
        //Destroy(gameObject, 1);

        IDamageable damageable = other.GetComponent<IDamageable>();

        if(damageable != null)
        {
            damageable.ReceiveDamage(_damage, _damageType, transform.position);
        }
    }
}
