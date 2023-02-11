using System;

namespace Atlas5.Scripts.Utility.Timers
{
    public class Timer
    {
        public float RemainingSeconds { get; private set; }
        public event Action OnTimerEnd;
        public Timer(float duration)
        {
            SetTimer(duration);
        }
        public void SetTimer(float seconds)
        {
            RemainingSeconds = seconds;
        }
        public void Tick(float deltaTime)
        {
            if (RemainingSeconds == 0)
                return;
            RemainingSeconds -= deltaTime;
            CheckIfFinished();
        }
        public void CheckIfFinished()
        {
            if (RemainingSeconds > 0.0f)
                return;
            RemainingSeconds = 0.0f;
            OnTimerEnd?.Invoke();
        }
    }
}
