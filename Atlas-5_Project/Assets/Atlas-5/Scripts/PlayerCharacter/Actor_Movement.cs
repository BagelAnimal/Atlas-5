using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Actor_Movement : MonoBehaviour
{
    //TODO
    // Debug for air and wall jumping.
    //  - times airjumped
    //  - times walljumped
    //  - remaining airjumps
    //  - remaining walljumps
    // Walk viewbobbing.
    // Jump viewbob.
    // Land viewbob.
    // Crouching.
    // Sprinting.
    // Inverse force on Rigidbodies the player is jumping off of.
    [Header("Events")]
    [SerializeField]
    UnityEvent _startedWalkingEvent, _stoppedWalkingEvent, _landedEvent, _jumpedEvent;

    [Header("Debug")]
    [SerializeField] 
    private bool _isDebugEnabled = true;
    private TrailRenderer _trailRenderer;

    [Header("Movement")]
    /// <summary>
    /// Max movement speed in meters per second.
    /// </summary>
    [SerializeField, Range(0.0f, 100.0f)]
    private float _moveSpeed = 5.0f;

    /// <summary>
    /// The controller's max acceleration in meters per second.
    /// </summary>
    [SerializeField, Range(0.0f, 100.0f)]
    private float _responsiveness = 50.0f;

    /// <summary>
    /// The percentage of responsiveness that is maintained in mid-air.
    /// </summary>
    [SerializeField, Range(0.0f, 1.0f)]
    private float _airControl = 0.1f;

    private float _maxAirAcceleration;

    private Vector3 _newVelocity = Vector3.zero;
    private Vector3 _desiredVelocity = Vector3.zero;

    [Header("References")]
    /// <summary>
    /// What the player's input is parsed relative to.
    /// <summary>
    [SerializeField]
    private Transform _inputRotationParent;

    private Rigidbody _rigidbody;
    private Collider _collider;

    [Header("Jumping")]
    /// <summary>
    /// The height that the player jumps in meters.
    /// </summary>
    [SerializeField, Range(0.0f, 10.0f)]
    private float _jumpHeight = 2.0f;

    /// <summary>
    /// The max number of times that the player can jump - once
    /// off the ground, and the others in mid-air.
    /// </summary>
    [SerializeField, Range(0, 10)]
    private int _maxJumps = 0;

    /// <summary>
    /// The number of timesteps that the player can jump if
    /// they walk off of a ledge.
    /// </summary>
    [SerializeField, Min(0)]
    private int _coyoteTimeSteps = 10;

    [SerializeField, Min(0)]
    private int _jumpMemorySteps = 10;

    /// <summary>
    /// Whether or not the player can wall jump.
    /// </summary>
    [SerializeField]
    private bool _isWallJumpEnabled = false;

    /// <summary>
    /// Whether wall jumps are biased vertically to help with gaining height.
    /// </summary>
    [SerializeField]
    private bool _isWallJumpVertical = false;

    [SerializeField, Range(0.0f, 10.0f)]
    private float _gravityScale = 1.0f;

    private bool _isJumpRequested;
    private int _currentJumps;
    private int _groundContactCount, _steepContactCount;
    private int _stepsSinceLastGrounded, _stepsSinceLastJump;
    private int _stepsSinceLastJumpInput, _stepsSinceLastMoveInput;
    private bool _isOnGround => _groundContactCount > 0;
    private bool _isOnSteep => _steepContactCount > 0;

    [Header("Floor Angle Handling")]
    /// <summary>
    /// The max ground angle that the player can jump on. 1.0 is a 90 degree wall.
    /// </summary>
    [SerializeField, Range(0.0f, 90.0f)]
    private float _maxGroundAngle = 25.0f;

    [SerializeField, Range(0.0f, 90.0f)]
    private float _maxStairsAngle = 50.0f;

    [SerializeField, Range(0, 50)]
    private int _maxStepsBeforeFreeze = 10;

    [SerializeField]
    private PhysicMaterial _standStillMaterial;
    private PhysicMaterial _defaultMaterial;

    [Header("Floor Snapping")]
    /// <summary>
    /// The speed threshold for the player to stop snapping to the ground
    /// when moving over angles. Low values allow 'launching' when cresting a ridge.
    /// </summary>
    [SerializeField, Range(0.0f, 100.0f)]
    private float _maxSnapSpeed = 100.0f;

    /// <summary>
    /// The distance that the raycast will go to search for ground beneath the
    /// player when they first lose collision.
    /// </summary>
    [SerializeField, Min(0.0f)]
    private float _snapProbeDistance = 1.0f;

    /// <summary>
    /// The layermask accounted for when probing for floor below the player.
    /// </summary>
    [SerializeField]
    LayerMask _groundProbeMask = -1, _stairsProbeMask = -1;

    private float _minGroundDotProduct, _minStairsDotProduct;
    /// <summary>
    /// The minimum dot product of an angle the player is walking over that
    /// can be considered a steep floor angle as opposed to a wall.
    /// </summary>
    [SerializeField, Range(-1.0f, 1.0f)]
    private float _minSteepDotProduct = -0.1f;

    private Vector3 _contactNormal, _steepNormal;
    private float _floorAngle;
    private bool _isWalking = false;

    private void OnValidate()
    {
        _minGroundDotProduct = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
        _minStairsDotProduct = Mathf.Cos(_maxStairsAngle * Mathf.Deg2Rad);
        _maxAirAcceleration = _responsiveness * _airControl;
    }

    private void Awake()
    {
        GetComponentInChildren<MeshRenderer>().enabled = _isDebugEnabled;
        if(_isDebugEnabled == true && _trailRenderer != null)
        {
            _trailRenderer = GetComponentInChildren<TrailRenderer>();
            _trailRenderer.enabled = _isDebugEnabled;
        }
        _rigidbody = GetComponentInChildren<Rigidbody>();
        _rigidbody.useGravity = false;
        _collider = GetComponent<Collider>();
        _defaultMaterial = _collider.material;

        if (_inputRotationParent == null)
            _inputRotationParent = transform;
        
        OnValidate();
    }

    private void Update()
    {
        Vector2 input;
        ++_stepsSinceLastMoveInput;
        if(_stepsSinceLastMoveInput > _maxStepsBeforeFreeze && _isOnGround == true)
        {
            if(_standStillMaterial != null)
                _collider.material = _standStillMaterial;
        }
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        if (input != Vector2.zero)
        {
            _stepsSinceLastMoveInput = 0;
            _collider.material = _defaultMaterial;
        }
        input = Vector2.ClampMagnitude(input, 1.0f);
        _desiredVelocity = _inputRotationParent.TransformDirection(input.x, 0.0f, input.y) * _moveSpeed;
        _isJumpRequested |= Input.GetButtonDown("Jump");
        _rigidbody.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        UpdateGroundedState();
        UpdateVelocity();
        if (_isJumpRequested == true)
        {
            _isJumpRequested = false;
            TryJump();
        }
        EvaluateIsWalking();
        ClearGroundedState();
        UpdateGravity();
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void EvaluateIsWalking()
    {
        if (_isWalking == false && _groundContactCount > 0 && _desiredVelocity.magnitude > 0.0f)
        {
            _isWalking = true;
            _startedWalkingEvent.Invoke();
            return;
        }
        if (_isWalking == true && (_groundContactCount == 0 || _desiredVelocity.magnitude == 0.0f))
        {
            _isWalking = false;
            _stoppedWalkingEvent.Invoke();
        }
    }

    private void EvaluateCollision (Collision collision)
    {
        float minDotProduct = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; ++i)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minDotProduct)
            {
                ++_groundContactCount;
                _contactNormal += normal;
            }
            else if(normal.y > _minSteepDotProduct)
            {
                ++_steepContactCount;
                _steepNormal += normal;
            }
            _floorAngle = Mathf.Acos(normal.y) * Mathf.Rad2Deg;
        }
    }

    private void UpdateGroundedState()
    {
        ++_stepsSinceLastGrounded;
        ++_stepsSinceLastJump;
        if (_stepsSinceLastJumpInput > 0)
            ++_stepsSinceLastJumpInput;
        if(_isOnGround == true || SnapToGround() == true || CheckSteepContacts() == true)
        {
            if (_stepsSinceLastGrounded > 5)
            {
                _landedEvent.Invoke();
                _currentJumps = 0;
                if (_stepsSinceLastJumpInput > 0 && _stepsSinceLastJumpInput <= _jumpMemorySteps)
                {
                    Jump(_contactNormal);
                    Debug.Log("Memory jumped!");
                }
            }
            _stepsSinceLastGrounded = 0;
            if(_groundContactCount > 1)
                _contactNormal.Normalize();
            return;
        }
        _contactNormal = Vector3.up;
    }

    private void ClearGroundedState()
    {
        _groundContactCount = _steepContactCount = 0;
        _contactNormal = _steepNormal = Vector3.zero;
    }

    private void UpdateGravity()
    {
        float gravity = Physics.gravity.y * _gravityScale;
        _rigidbody.AddForce(new Vector3(0.0f, gravity, 0.0f), ForceMode.Acceleration);
    }

    private void TryJump()
    {
        Vector3 jumpDirection;
        if (_isOnGround == true)
        {
            jumpDirection = _contactNormal;
        }
        else if (_isWallJumpEnabled == true && _isOnSteep == true)
        {
            jumpDirection = _steepNormal;
            if (_isWallJumpVertical)
                jumpDirection = (jumpDirection + Vector3.up).normalized;
        }
        else if (_maxJumps > 0 && _currentJumps <= _maxJumps)
        {
            if (_currentJumps == 0)
                _currentJumps = 1;
            jumpDirection = _contactNormal;
        }
        else if (_currentJumps == 0 && _stepsSinceLastGrounded < _coyoteTimeSteps)
        {
            jumpDirection = _contactNormal;
        }
        else
        {
            _stepsSinceLastJumpInput = 1;
            return;
        }
        Jump(jumpDirection);
    }

    private void Jump(Vector3 jumpDirection)
    {
        _stepsSinceLastJump = 0;
        _stepsSinceLastJumpInput = 0;
        ++_currentJumps;
        // This 2.0 is derived from a kinematic formula and is being used to enforce percise jump heights in meters.
        float jumpSpeed = Mathf.Sqrt(2.0f * -Physics.gravity.y * _jumpHeight * _gravityScale);
        // Bias jumping toward verticality.
        float alignedSpeed = Vector3.Dot(_newVelocity, jumpDirection);
        jumpSpeed = Mathf.Max(jumpSpeed, alignedSpeed);
        _rigidbody.velocity += jumpDirection * jumpSpeed;
        _jumpedEvent.Invoke();
    }

    private void UpdateVelocity()
    {
        _newVelocity = _rigidbody.velocity;

        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(_newVelocity, xAxis);
        float currentZ = Vector3.Dot(_newVelocity, zAxis);

        float acceleration = _isOnGround ? _responsiveness : _maxAirAcceleration;
        float maxSpeedDelta = acceleration * Time.deltaTime;
        float newX = Mathf.MoveTowards(currentX, _desiredVelocity.x, maxSpeedDelta);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVelocity.z, maxSpeedDelta);

        _newVelocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

        _rigidbody.velocity = _newVelocity;
    }

    private Vector3 ProjectOnContactPlane (Vector3 vector)
    {
        return vector - _contactNormal * Vector3.Dot(vector, _contactNormal);
    }

    /// <summary>
    /// Determine whether the stair or ground dot products should be used when
    /// evaluating floor angle.
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    private float GetMinDot (int layer)
    {
        return (_stairsProbeMask & (1 << layer)) == 0 ? _minGroundDotProduct : _minStairsDotProduct;
    }

    bool SnapToGround()
    {
        if (_stepsSinceLastGrounded > 1)
            return false;
        if (_stepsSinceLastJump <= 2)
            return false;
        float speed = _rigidbody.velocity.magnitude;
        if (speed > _maxSnapSpeed)
            return false;
        if (!Physics.Raycast(_rigidbody.position, Vector3.down, out RaycastHit hit, _snapProbeDistance, _groundProbeMask))
            return false;
        if (hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
            return false;
        _groundContactCount = 1;
        _contactNormal = hit.normal;
        float dot = Vector3.Dot(_newVelocity, hit.normal);
        if(dot > 0.0f)
            _rigidbody.velocity = (_newVelocity - hit.normal * dot).normalized * speed;
        return true;
    }

    /// <summary>
    /// Handles the player getting stuck in crevasses where steep angles
    /// surround the player.
    /// </summary>
    /// <returns></returns>
    bool CheckSteepContacts ()
    {
        if (_steepContactCount > 1)
        {
            _steepNormal.Normalize();
            if(_steepNormal.y >= _minGroundDotProduct)
            {
                _groundContactCount = 1;
                _contactNormal = _steepNormal;
                return true;
            }
        }
        return false;
    }
}
