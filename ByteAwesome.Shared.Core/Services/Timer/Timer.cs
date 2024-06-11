namespace ByteAwesome.Services
{
    public interface ITimerService
    {
        int Period { get; set; }
        TimeSpan _timespan { get; set; }

        event EventHandler Elapsed;

        void Dispose();
        void SetInterval(TimeSpan interval);
    }
    public class TimerService : ITimerService, IDisposable
    {
        public int Period { get; set; }
        private Timer timer;

        public TimeSpan _timespan { get; set; }

        public void SetInterval(TimeSpan interval)
        {
            _timespan = interval;
            timer?.Change(TimeSpan.Zero, _timespan);
        }

        public event EventHandler Elapsed;

        public TimerService()
        {
            timer = new Timer(OnTimerElapsed);
        }

        private void OnTimerElapsed(object state)
        {
            Elapsed?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
