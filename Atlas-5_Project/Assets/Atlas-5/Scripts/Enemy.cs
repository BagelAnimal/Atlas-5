using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    NavMeshAgent _agent;
    [SerializeField] Transform _target;
    private void Awake()
    {
        _agent = GetComponentInChildren<NavMeshAgent>();
    }
    private void Update()
    {
        _agent.destination = _target.position;
    }
}
