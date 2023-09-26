using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Examples.Pong;
using UnityEngine;

public class GrappleRope : NetworkBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private PlayerConnection _playerConnection;
    public SyncList<Vector3> _linePoints = new SyncList<Vector3>();

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

    private void ClientDrawRope(uint id)
    {
        if (!_playerConnection.isLocalPlayer && id == _playerConnection.netId)
        {
            Debug.Log(gameObject.name + " RECEIVE RPC: " + _playerConnection.isLocalPlayer);
            DrawRope();
        }
    }

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _spring = new Spring_MLab();
        _spring.SetTarget(0);
        PlayerConnection.OnDrawRope += ClientDrawRope;
    }

    private void LateUpdate()
    {
        StartDrawRope();

        if (!_playerConnection.isOwned)
        {
            _lineRenderer.positionCount = _linePoints.Count;

            for (int i = 0; i < _linePoints.Count; i++)
            {
                _lineRenderer.SetPosition(i, _linePoints[i]);
            }
        }
    }

    private void StartDrawRope()
    {
        if (!_playerMovement.IsSwinging && !_playerMovement.CanDrawRope)
        {
            _currentGrapplePosition = _playerMovement.GrapplePos.position;

            _spring.Reset();

            if (_lineRenderer.positionCount > 0)
            {
                _lineRenderer.positionCount = 0;
                UpdateServerRope();
            }

            return;
        }

        if (!_playerMovement.CanDrawRope)
        {
            return;
        }

        if (_playerConnection.isOwned)
        {
            DrawRope();
        }
    }

    private void DrawRope()
    {
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

        UpdateServerRope();
    }

    private void UpdateServerRope()
    {
        Vector3[] pos = new Vector3[_lineRenderer.positionCount];
        _lineRenderer.GetPositions(pos);
        DefineLineRenderer(pos);
    }

    [Server]
    private void SendLineRenderer(Vector3[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (_linePoints.Count <= i)
            {
                _linePoints.Add(points[i]);
            }
            else
            {
                _linePoints[i] = points[i];
            }
        }

        Debug.Log("GRAPPLE SEND: " + _linePoints.Count);
    }

    [Command]
    private void DefineLineRenderer(Vector3[] points)
    {
        if (NetworkClient.isConnected)
        {
            SendLineRenderer(points);
            Debug.Log("GRAPPLE DEFINE");
        }
    }
}
