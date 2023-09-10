
using UnityEngine;

public interface IDamageable
{
    void ReceiveDamage(float damage, DamageType damageType, Vector3 damagePos);
}

public enum DamageType
{
    Void,
    Solar,
    Arc,
    Kinect,
}

