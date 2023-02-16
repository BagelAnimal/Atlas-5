using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Atlas5.Scripts.Utility.Events
{
    public class EventOnTrigger : MonoBehaviour
    {
        [SerializeField] UnityEvent _event;
        [SerializeField] Type _componentFilter;
        private void OnTriggerEnter(Collider other)
        {
            
        }
    }
}