using UnityEngine;

public class EnemyBear : MonoBehaviour, IDamageable
{
    [SerializeField] private float _health;
    [SerializeField] private GameObject _bloodParticle;

    public void ReceiveDamage(float damage, DamageType damageType, Vector3 damagePos)
    {
        GameObject bloodParticle = Instantiate(_bloodParticle);
        bloodParticle.transform.position = damagePos;
    }
}
