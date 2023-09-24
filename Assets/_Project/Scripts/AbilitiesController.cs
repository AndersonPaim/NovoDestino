using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform _grenadeThrowPoint;
    [SerializeField] private GameObject _voidGrenadePrefab;
    [SerializeField] private float _grenadeThrowForce;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 200))
        {
            _grenadeThrowPoint.LookAt(hit.point);
        }

        Debug.DrawLine(Camera.main.transform.position, hit.point, Color.red);
    }

    public void Grenade()
    {
        _animator.SetTrigger("ThrowGrenade");

    }

    public void ThrowGrenade()
    {
        GameObject grenade = Instantiate(_voidGrenadePrefab, _grenadeThrowPoint.position, _grenadeThrowPoint.rotation);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(grenade.transform.forward * _grenadeThrowForce, ForceMode.Force);
    }
}
