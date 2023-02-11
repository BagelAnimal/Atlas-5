using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    NavMeshAgent _agent;
    [SerializeField] Transform _target;
    [SerializeField] float _attackRange = 3;

    [SerializeField] float _attackDelay = 3;
    float _attackDelayTimer = 0.0f;

    [SerializeField] float _attackStartTime = 0.5f;
    float _attackStartTimer = 0.0f;

    [SerializeField] float _attackSustainTime = 0.5f;
    float _attackSustainTimer = 0.0f;

    [SerializeField] float _attackDecayTime = 0.5f;
    float _attackDecayTimer = 0.0f;

    [SerializeField] bool _isAttacking = false;
    private void Awake()
    {
        _agent = GetComponentInChildren<NavMeshAgent>();
        ResetAttackTimer();
    }
    private void Update()
    {
        _agent.destination = _target.position;
        UpdateAttackTimer();
    }

    private void TryAttack()
    {
        float distance = Vector3.Distance(transform.position, _target.position);
        if (distance <= _attackRange)
            Attack();
    }
    private void Attack()
    {
        Debug.Log("Attacked!");
    }

    private void UpdateAttackTimer()
    {
        _attackDelayTimer -= Time.deltaTime;
        if (_attackDelayTimer <= 0.0f)
        {
            TryAttack();
            ResetAttackTimer();
        }
    }

    private void ResetAttackTimer()
    {
        _attackDelayTimer = _attackDelay;
    }
}
