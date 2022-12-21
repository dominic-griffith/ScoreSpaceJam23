using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Stopwatch : MonoBehaviour
{
    [SerializeField] private TMP_Text _currentTimeText;

    private float _currentTime;
    private bool _stopwatchActive;

    public void Start()
    {
        _currentTime = 0f;
        StartStopwatch();
    }

    private void Update()
    {
        IncrementTime();
    }

    private void IncrementTime()
    {
        if(_stopwatchActive == true)
        {
            _currentTime += Time.deltaTime;
        }
        TimeSpan time = TimeSpan.FromSeconds(_currentTime);
        _currentTimeText.text = time.ToString(@"mm\:ss\:fff");
    }

    public float GetCurrentTime()
    {
        return _currentTime;
    }

    public void StartStopwatch()
    {
        _stopwatchActive = true;
    }

    public void StopStopwatch()
    {
        _stopwatchActive = false;
    }
}
