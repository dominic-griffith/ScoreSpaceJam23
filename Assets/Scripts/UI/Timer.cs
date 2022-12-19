using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private WinAndLose _winAndLose;
    [SerializeField] private TMP_Text _currentTimeText;
    [SerializeField] private GameObject _snowParticles;
    [SerializeField] private GameObject _blizzardParticles;
    [SerializeField] private Animator _anim;
    [SerializeField] private float _startTime = 4f;

    private float _currentTime;
    private bool _timerActive;

    private void Start()
    {
        _currentTime = _startTime;
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
                _currentTime = 0f;
                _winAndLose.LoseGame();
            }
        }
        TimeSpan time = TimeSpan.FromSeconds(_currentTime);
        _currentTimeText.text = "Get out of the snow!\n" + time.Seconds.ToString();
    }

    public void StartTimer()
    {
        _anim.SetBool("TimerOn", true);
        _snowParticles.gameObject.SetActive(false);
        _blizzardParticles.gameObject.SetActive(true);
        _currentTimeText.gameObject.SetActive(true);
        _timerActive = true;
    }

    public void StopTimer()
    {
        _anim.SetBool("TimerOn", false);
        _blizzardParticles.gameObject.SetActive(false);
        _snowParticles.gameObject.SetActive(true);
        _currentTimeText.gameObject.SetActive(false);
        _timerActive = false;
    }

    public void ResetTimer()
    {
        Start();
    }
}
