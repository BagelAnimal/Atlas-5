using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Atlas5.Scripts.Utility.Timers
{
    public class TimerBehavior : MonoBehaviour
    {
        [SerializeField] 
        private float _duration = 1.0f;
        public float Duration
        {
            get { return _duration; }
            set { _duration = value; }
        }

        [SerializeField] 
        private UnityEvent _onTimerEnd = null;
        public UnityEvent OnTimerEnd
        {
            get { return _onTimerEnd; }
            set { _onTimerEnd = value; }
        }

        private Timer timer = null;

        private void Awake()
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
        }

        public void SetTimer()
        {
            timer.SetTimer(_duration);
        }

        public void SetTimer(float duration, UnityAction listener)
        {
            _duration = duration;
            _onTimerEnd.RemoveAllListeners();
            _onTimerEnd.AddListener(listener);
            SetTimer();
        }
    }

}