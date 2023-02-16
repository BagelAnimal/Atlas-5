using Atlas5.Scripts.Utility.Timers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    NavMeshAgent _agent;
    private TimerBehavior _internalTimer;

    [SerializeField] 
    Transform _target;

    [SerializeField] 
    float _attackWindupTime, _attackSustainTime, _attackDecayTime, _attackCooldownTime = 1.0f;

    [SerializeField]
    float _attackDistance = 2.0f;

    [SerializeField] 
    bool _isAttacking = false;
    bool _canAttack = true;
    private void Awake()
    {
        _agent = GetComponentInChildren<NavMeshAgent>();
        _internalTimer = gameObject.AddComponent<TimerBehavior>();
        _internalTimer.enabled = false;
    }
    private void Update()
    {
        _agent.destination = _target.position;
        if(_canAttack == true && _isAttacking == false)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        float distance = Vector3.Distance(transform.position, _target.position);
        if(distance <= _attackDistance)
        {
            StartAttack();
        }
    }
    private void StartAttack()
    {
        _isAttacking = true;
        _canAttack = false;
        _agent.isStopped = true;
        _internalTimer.enabled = true;
        _internalTimer.SetTimer(_attackWindupTime, SustainAttack);
    }
    private void SustainAttack()
    {
        _internalTimer.SetTimer(_attackSustainTime, DecayAttack);
    }

    private void DecayAttack()
    {
        _internalTimer.SetTimer(_attackDecayTime, EndAttack);
    }

    private void EndAttack()
    {
        _agent.isStopped = false;
        _internalTimer.SetTimer(_attackCooldownTime, EndCooldown);
        _isAttacking = false;
    }

    private void EndCooldown()
    {
        _internalTimer.enabled = false;
        _canAttack = true;
    }
}
