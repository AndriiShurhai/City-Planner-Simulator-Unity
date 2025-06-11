using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class TimeManager : MonoBehaviour
{
    [Header("Visual elements")]
    [SerializeField] private SpriteRenderer _overlayRenderer;
    [SerializeField] private Transform _clock;

    [Header("Time settings")]
    [SerializeField] private float _baseTimeSpeed = 1f;
    [SerializeField] private float _daysPerMonth = 30f;
    [SerializeField] private float _monthsPerYear = 12f;
    [SerializeField] private int _startingYear = 2025;

    [Header("Day/Night Settings")]
    [SerializeField] private Color _dayColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color _nightColor = new Color(0.05f, 0.05f, 0.2f, 0.6f);

    [SerializeField] private float _dawnTime = 6f;
    [SerializeField] private float _duskTime = 18f;

    private float _currentHour = 0f;
    private int _currentDay = 1;
    private int _currentMonth = 1;
    private int _currentYear;
    private float _timeMultiplier = 1f;
    private bool _isPaused = false;



    public event Action OnHourChanged;
    public event Action OnDayChanged;   
    public event Action OnWeekChanged;
    public event Action OnMonthChanged;
    public event Action OnYearChanged;
    public event Action OnDayNightTransition;

    public float CurrentHour => _currentHour;
    public float CurrentDay => _currentDay; 
    public float CurrentMonth => _currentMonth;
    public float CurrentYear => _currentYear;
    public bool IsDayTime => _currentHour >= _dawnTime && _currentHour < _duskTime;
    public float TimeMultiplier
    {
        get => _timeMultiplier;
        set => _timeMultiplier = Mathf.Max(0f, value);
    }

    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        _currentYear = _startingYear;
    }
    private void Update()
    {
        if (_isPaused) return;

        float previousHour = _currentHour;
        float previousDay = _currentDay;
        float previousMonth = _currentMonth;
        float previousYear = _currentYear;
        bool wasDaytime = IsDayTime;

        AdvanceTime();

        UpdateDayNightCycle();
        UpdateClockRotation();

        CheckForTimeEvents(previousHour, previousDay, previousMonth, previousYear, wasDaytime);

        //currentTime += Time.deltaTime * timeSpeed;
        //if (currentTime >= 24) currentTime -= 24f;

        //float theta = 0.5f - 0.5f * Mathf.Cos(Mathf.PI * currentTime / 12f);

        //overlayRenderer.color = Color.Lerp(dayColor, nightColor, theta);    
    }

    private void AdvanceTime()
    {
        float timeDelta = Time.deltaTime * _baseTimeSpeed * _timeMultiplier;

        _currentHour += timeDelta;

        if (_currentHour >= 24f)
        {
            _currentHour -= 24f;
            _currentDay++;

            if (_currentDay > _daysPerMonth)
            {
                _currentDay = 1;
                _currentMonth++;

                if (_currentMonth > _monthsPerYear)
                {
                    _currentMonth = 1;
                    _currentYear++;
                }
            }
        }
    }

    private void UpdateDayNightCycle()
    {
        if (_overlayRenderer != null)
        {
            float t;
            if (_currentHour >= _dawnTime && _currentHour < _duskTime)
            {
                t = Mathf.InverseLerp(_dawnTime, _duskTime, _currentHour);
            }
            else
            {
                float nightHour = _currentHour < _dawnTime ? _currentHour + 12f : _currentHour;
                t = Mathf.InverseLerp(_duskTime, _dawnTime + 12f, nightHour);
            }

            float curveT = Mathf.Cos((t * Mathf.PI * 2f) - Mathf.PI) * 0.5f + 0.5f;

            _overlayRenderer.color = Color.Lerp(_dayColor, _nightColor, curveT);

        }
    }

    private void UpdateClockRotation()
    {
        if (_clock != null)
        {
            float clockAngle = (_currentHour / 24f) * 360;
            _clock.rotation = Quaternion.Euler(0, 0, -clockAngle);
        }
    }

    private void CheckForTimeEvents(float previousHour, float previousDay, float previousMonth, float previousYear, bool wasDaytime)
    {
        if (Mathf.Floor(_currentHour) != Mathf.Floor(previousHour))
        {
            OnHourChanged?.Invoke();
        }

        if (_currentDay != previousDay)
        {
            OnDayChanged?.Invoke();
        }

        if (_currentDay == 7)
        {
            OnWeekChanged?.Invoke();
        }

        if (_currentMonth != previousMonth)
        {
            OnMonthChanged?.Invoke();
        }

        if (_currentYear != previousYear)
        {
            OnYearChanged?.Invoke();
        }

        if (wasDaytime != IsDayTime)
        {
            OnDayNightTransition?.Invoke();
        }
    } 

    public void SetTimeMultiplier(float multiplier)
    {
        _timeMultiplier = Mathf.Max(0, multiplier);
    }

    public void PauseTime()
    {
        _isPaused = true;
    }

    public void ResumeTime()
    {
        _isPaused = false;
    }

    public void TogglePause()
    {
        _isPaused = !_isPaused;
    }

    public void SkipToNextDay()
    {
        _currentHour = 0;
        _currentDay++;

        if (_currentDay > _daysPerMonth)
        {
            _currentDay = 1;
            _currentMonth++;

            if (_currentMonth > _monthsPerYear)
            {
                _currentMonth = 1;
                _currentYear++;
            }  
        }
        OnDayChanged?.Invoke();
    }

    public void SkipToNextMonth()
    {
        _currentHour = 0;
        _currentDay = 1;
        _currentMonth++;

        if (_currentMonth > _monthsPerYear)
        {
            _currentMonth = 1;
            _currentYear++;
        }
        OnDayChanged?.Invoke();
        OnMonthChanged?.Invoke();
    }

    public string GetTimeString()
    {
        int hour = Mathf.FloorToInt(_currentHour);
        int minute = Mathf.FloorToInt((_currentHour - hour) * 60);
        string amPm = (hour < 12) ? "AM" : "PM";
        int hour12 = (hour % 12 == 0) ? 12 : hour % 12;

        return $"{hour12}:{minute:D2} {amPm}";
    }

    public string GetDateString()
    {
        return $"Day {_currentDay}, Month {_currentMonth}, Year {_currentYear}";
    }
}
