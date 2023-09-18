using UnityEngine;

public class Knife : MonoBehaviour
{
    private Rigidbody _rb;

    public void Throw(float force)
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(transform.right * force);
    }
}