using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _destroyTime;
    [SerializeField] private ParticleSystem _hitParticle;
    [SerializeField] private Collider _collider;

    private Rigidbody _rb;
    private DamageType _damageType;
    private RaycastHit hit;
    private float _damage;

    public void Shoot(float shootForce, float damage, DamageType damageType)
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 200))
        {
            gameObject.transform.LookAt(hit.point);

            ParticleSystem hitParticle = Instantiate(_hitParticle, hit.point, _hitParticle.transform.rotation);
            hitParticle.transform.LookAt(Camera.main.transform.position);
            hitParticle.transform.Translate(Vector3.forward * 0.5f);
            hitParticle.Play();

            float totalDuration = hitParticle.main.duration + hitParticle.main.startDelay.constant;
            DestroyDelay(hitParticle.gameObject, totalDuration);
            Destroy(gameObject);
            Debug.Log("Entrou");
        }

        if (gameObject.transform != null)
        {
            gameObject.transform.DOMove(hit.point, hit.distance / shootForce);
        }

        DestroyDelay(gameObject, _destroyTime);
        _damageType = damageType;
        _damage = damage;
    }

    private async void DestroyDelay(GameObject whatToDestroy, float delay)
    {
        await Task.Delay((int)delay * 1000);

        if (whatToDestroy != null)
        {
            Destroy(whatToDestroy);
        }
    }

    private void Update()
    {
        float distanceToTarget = Vector3.Distance(hit.point, transform.position);
        if (distanceToTarget <= 10 && gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
