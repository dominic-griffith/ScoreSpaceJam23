using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private TMP_Text _currentTimeText;
    [SerializeField] private float startTime;

    private float _currentTime;
    private bool _timerActive;

    private void Start()
    {
        _currentTime = startTime;
    }

    private void Update()
    {
        DecrementTime();
    }
    private void DecrementTime()
    {
        if (_timerActive == true)
        {
            _currentTime -= Time.deltaTime;
            if (_currentTime <= 0)
            {
                _timerActive = false;
                Start();
            }
        }
        TimeSpan time = TimeSpan.FromSeconds(_currentTime);
        _currentTimeText.text = time.Seconds.ToString();
    }

    public void StartTimer()
    {
        _timerActive = true;
    }

    public void StopTimer()
    {
        _timerActive = false;
    }
}
