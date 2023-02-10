using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPerson_Camera : MonoBehaviour
{
    const float FULL_ROTATION = 360.0f;

    [Header("User Settings")]
    [SerializeField, Range(0.0f, 100.0f)]
    private float _lookSensitivity = 5.0f;

    [SerializeField]
    private bool _pitchInverted;

    [SerializeField]
    private bool _yawInverted;

    [Header("References")]
    [SerializeField] Transform _pitchPivot;
    [SerializeField] Transform _yawPivot;
    [SerializeField] Rigidbody _rigidbody;

    [Header("Pitch Clamping")]
    [SerializeField, Range(-360.0f, 360.0f)]
    private float _minPitch = -85.0f;
    [SerializeField, Range(-360.0f, 360.0f)]
    private float  _maxPitch = 85.0f;

    [Header("Smoothing")]
    [SerializeField, Range(0.0f, 1.0f)]
    private float _cameraInterpolant = 0.75f;

    private float _yawRotation;
    private float _pitchRotation;

    private float _yawInput = 0.0f;

    private void OnValidate()
    {
        if (_maxPitch < _minPitch)
            _maxPitch = _minPitch;
    }

    private void Awake()
    {
        OnValidate();
        _yawInput = _yawPivot.localRotation.eulerAngles.y-90.0f;
        _yawRotation = _pitchPivot.localRotation.eulerAngles.x;
        if (_yawPivot == null)
            _yawPivot = transform;
        if (_rigidbody == null)
            _rigidbody = GetComponentInChildren<Rigidbody>();
    }

    void Update()
    {
        UpdatePitchRotation();
        UpdateYawRotation();
    }

    private void UpdatePitchRotation()
    {
        float pitchInput = Input.GetAxis("Mouse Y");
        if (_pitchInverted == false)
            pitchInput = -pitchInput;
        _pitchRotation += pitchInput * _lookSensitivity;
        _pitchRotation = Mathf.Clamp(_pitchRotation, _minPitch, _maxPitch);
        Quaternion newRot = Quaternion.Euler(_pitchRotation, 0.0f, 0.0f);
        Quaternion targetRot = Quaternion.Lerp(_pitchPivot.localRotation, newRot, _cameraInterpolant);
        _pitchPivot.localRotation = targetRot;
    }

    private void UpdateYawRotation()
    {
        _yawInput += Input.GetAxis("Mouse X");
        if(_yawInverted == true)
            _yawInput = -_yawInput;
        _yawRotation = _yawInput * _lookSensitivity;
        if (_yawRotation < 0.0f)
            _yawRotation += FULL_ROTATION;
        else if (_yawRotation >= FULL_ROTATION)
            _yawRotation -= FULL_ROTATION;
        Quaternion newRot = Quaternion.Euler(0.0f, _yawRotation, 0.0f);
        Quaternion targetRot = Quaternion.Lerp(_yawPivot.localRotation, newRot, _cameraInterpolant);
        if (_rigidbody == null)
        {
            _yawPivot.localRotation = targetRot;
            return;
        }
        _rigidbody.MoveRotation(targetRot);
    }
}
