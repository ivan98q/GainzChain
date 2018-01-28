using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Timer {

    [SerializeField]
    bool _active;

    [SerializeField]
    float _length;

    [SerializeField]
    float _currentTime;

    [SerializeField]
    TimerDirection _direction = TimerDirection.Down;

    [SerializeField]
    bool _autoReset;

    [SerializeField]
    bool _autoDisable;

    public TimerEvent OnChangeTime;
    public UnityEvent OnElapsed;
    public UnityEvent OnLow;
    public UnityEvent OnHigh;

    #region Properties
    public bool Active {
        get { return _active; }
        set { _active = value; }
    }

    public bool AutoDisable {
        get { return _autoDisable; }
        set { _autoDisable = value; }
    }

    public bool AutoReset {
        get { return _autoReset; }
        set { _autoReset = value; }
    }

    public float CurrentTime {
        get { return _currentTime; }
        set {
            if(CurrentTime == value) return;

            _currentTime = value;

            if(OnChangeTime != null) OnChangeTime.Invoke(CurrentTime);

            if(CurrentTime == 0) {
                if(OnLow != null) OnLow.Invoke();
            }
            else if(Length > 0 && CurrentTime == Length) {
                if(OnHigh != null) OnHigh.Invoke();
            }
        }
    }

    public TimerDirection Direction {
        get { return _direction; }
        set { _direction = value; }
    }

    public bool HasElapsed {
        get { return Direction == TimerDirection.Down ? CurrentTime == 0 : CurrentTime == Length; }
    }

    public float Length {
        get { return _length; }
        set {
            if(_length == value) return;

            _length = value;
        }
    }

    public float NormalizedTime {
        get { return CurrentTime / Length; }
        set { CurrentTime = Mathf.LerpUnclamped(0, Length, value); }
    }
    #endregion

    public Timer(float length) {
        _length = length;
    }
    public Timer(float length, TimerDirection direction) {
        _length = length;
        _direction = direction;
    }

    void FireEvent() {
        if(OnElapsed != null) OnElapsed.Invoke();
    }

    public void Reset() {
        Reset(false);
    }
    public void Reset(bool deactivate) {
        if(deactivate) Active = false;

        if(Direction == TimerDirection.Down) SetHigh();
        else SetLow();
    }

    public void SetHigh() {
        CurrentTime = Length;
    }

    public void SetLow() {
        CurrentTime = 0;
    }

    public void Start() {
        Start(false);
    }
    public void Start(bool reset) {
        if(reset) Reset();

        Active = true;
    }

    public void Stop() {
        Active = false;
    }

    public void Update() {
        if(!Active) return;

        if(Direction == TimerDirection.Down) {
            if(CurrentTime > 0) {
                CurrentTime = Mathf.Max(CurrentTime - Time.deltaTime, 0);

                if(CurrentTime == 0) {
                    if(AutoReset) CurrentTime = Length;
                    if(AutoDisable) Active = false;

                    FireEvent();
                }
            }
            else {
                if(AutoReset) {
                    CurrentTime = Length;

                    FireEvent();
                }

                if(AutoDisable) Active = false;
            }
        }
        else {
            if(CurrentTime < Length) {
                CurrentTime = Mathf.Min(CurrentTime + Time.deltaTime, Length);

                if(CurrentTime == Length) {
                    if(AutoReset) CurrentTime = 0;
                    if(AutoDisable) Active = false;

                    FireEvent();
                }
            }
        }
    }

    public static implicit operator float(Timer timer) {
        return timer.CurrentTime;
    }

    public enum TimerDirection {
        Down,
        Up
    }

    [System.Serializable]
    public class TimerEvent : UnityEvent<float> { }
}