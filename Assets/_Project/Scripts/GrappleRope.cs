using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleRope : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;

    [Header("Settings")]
    public int _quality = 200;
    public float _damper = 14;
    public float _strength = 800;
    public float _velocity = 15;
    public float _waveCount = 3;
    public float _waveHeight = 1;
    public AnimationCurve _affectCurve;

    private Spring_MLab _spring;
    private LineRenderer _lineRenderer;
    private Vector3 _currentGrapplePosition;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _spring = new Spring_MLab();
        _spring.SetTarget(0);
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void DrawRope()
    {
        if (!_playerMovement.IsSwinging && !_playerMovement.CanDrawRope)
        {
            _currentGrapplePosition = _playerMovement.GrapplePos.position;

            _spring.Reset();

            if (_lineRenderer.positionCount > 0)
            {
                _lineRenderer.positionCount = 0;
            }

            return;
        }

        if (!_playerMovement.CanDrawRope)
        {
            return;
        }

        if (_lineRenderer.positionCount == 0)
        {
            _spring.SetVelocity(_velocity);
            _lineRenderer.positionCount = _quality + 1;
        }

        _spring.SetDamper(_damper);
        _spring.SetStrength(_strength);
        _spring.Update(Time.deltaTime);

        Vector3 grapplePoint = _playerMovement.GrapplePoint;
        Vector3 gunTipPosition = _playerMovement.GrapplePos.position;

        Vector3 up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

        _currentGrapplePosition = Vector3.Lerp(_currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        for (int i = 0; i < _quality + 1; i++)
        {
            float delta = i / (float)_quality;
            Vector3 offset = up * _waveHeight * Mathf.Sin(delta * _waveCount * Mathf.PI) * _spring.Value * _affectCurve.Evaluate(delta);
            _lineRenderer.SetPosition(i, Vector3.Lerp(gunTipPosition, _currentGrapplePosition, delta) + offset);
        }
    }
}
