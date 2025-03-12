using ActualLab.Fusion;

namespace Timer;

public class TimerState
{
    public bool IsRunning { get; set; }
    public string? FormattedTimeLeft { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? EndsAt { get; set; }

    public double? ProgressPercentage { get; set; }

    public List<TimerRecord>? TimerHistory { get; set; }
}

public class TimerService : IComputeService
{
    private readonly object _lock = new();
    private int _totalSeconds;
    private DateTime? _startedAt;
    private DateTime? _endsAt;
    private bool _isRunning;
    private System.Timers.Timer? _timer;
    private List<TimerRecord> _timerHistory = new();
    private TimerRepostory _repo;
    private DateTime _changeTime = DateTime.Now;
    int _hours;
    int _minutes;
    int _seconds;

    public TimerService(TimerRepostory repo)
    {
        _repo = repo;
    }

    [ComputeMethod]
    public virtual Task<(TimerState, DateTime)> GetTimerState()
    {

        _timerHistory = _repo.GetTimerHistory().ToList();
        return Task.FromResult((new TimerState() { ProgressPercentage = _totalSeconds > 0 ? (1.0 - (double)_totalSeconds / (_hours * 3600 + _minutes * 60 + _seconds)) * 100 : 0, FormattedTimeLeft = $"{_totalSeconds / 3600:D2}:{(_totalSeconds % 3600) / 60:D2}:{_totalSeconds % 60:D2}", StartedAt = _startedAt, EndsAt = _endsAt, IsRunning = _isRunning, TimerHistory = _timerHistory }, _changeTime));
    }

    public async Task StartTimer(int hours, int minutes, int seconds)
    {
        _hours = hours;
        _minutes = minutes;
        _seconds = seconds;


        _totalSeconds = hours * 3600 + minutes * 60 + seconds;
        _startedAt = DateTime.Now;
        _endsAt = _startedAt?.AddSeconds(_totalSeconds);
        _isRunning = true;

        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += async (sender, e) =>
        {
            if (_totalSeconds > 0)
            {
                _totalSeconds--;
            }
            else
            {
                _timer?.Stop();
                _isRunning = false;
                await SaveTimerRecord();
            }
            using (Invalidation.Begin())
                await GetTimerState();

        };
        _timer.Start();

        using (Invalidation.Begin())
            await GetTimerState();
    }

    public async Task PauseTimer()
    {

        _timer?.Stop();
        _isRunning = false;

        using (Invalidation.Begin())
            await GetTimerState();
    }

    private Task SaveTimerRecord()
    {
         _repo.Save(new TimerRecord { Start = _startedAt, End = _endsAt });
        _timerHistory = _repo.GetTimerHistory().ToList();
        return Task.CompletedTask;
    }
}
