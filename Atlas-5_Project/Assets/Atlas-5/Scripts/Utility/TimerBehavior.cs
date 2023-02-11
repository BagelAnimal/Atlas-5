using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Atlas5.Scripts.Utility.Timers
{
    public class TimerBehavior : MonoBehaviour
    {
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private UnityEvent _onTimerEnd = null;

        private Timer timer = null;

        private void Start()
        {
            timer = new Timer(_duration);
            timer.OnTimerEnd += HandleTimerEnd;
        }

        private void Update()
        {
            timer.Tick(Time.deltaTime);
        }

        private void HandleTimerEnd()
        {
            _onTimerEnd.Invoke();
            Destroy(this);
        }
    }

}