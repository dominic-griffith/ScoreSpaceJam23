using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private Leaderboard _leaderboard;
    [SerializeField] private TMP_InputField _playerNameInput;
    [SerializeField] private GameObject _camGrapple;
    [SerializeField] private GameObject _stopwatch;
    [SerializeField] private GameObject _titleScreen;
    [SerializeField] private Stopwatch _actualStopWatch;

    private bool _nameEntered;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(SetupRoutine());
    }

    public void SetPlayerName()
    {
        LootLockerSDKManager.SetPlayerName(_playerNameInput.text, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully set player name");
            }
            else
            {
                Debug.Log("Count not set player name"+response.Error);
            }
        });
        _nameEntered = true;
        _playerNameInput.gameObject.SetActive(false);
        _camGrapple.gameObject.SetActive(true);
        _stopwatch.gameObject.SetActive(true);
        _titleScreen.gameObject.SetActive(false);
        _actualStopWatch.Start();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public bool NameReady()
    {
        return _nameEntered;
    }

    IEnumerator SetupRoutine()
    {
        yield return LoginRoutine();
        yield return _leaderboard.FetchTopHighScoresRoutine();
        //yield return CheckIfNameEntered();
    }

    
    //IEnumerator CheckIfNameEntered()
    //{
    //    bool done = false;
    //    LootLockerSDKManager.GetPlayerName((response) =>
    //    {
    //        if (response.success)
    //        {
    //            _nameEntered = true;
    //            _playerNameInput.gameObject.SetActive(false);
    //            _camGrapple.gameObject.SetActive(true);
    //            _stopwatch.gameObject.SetActive(true);
    //            _actualStopWatch.Start();
    //            Cursor.lockState = CursorLockMode.Locked;
    //            Cursor.visible = false;
    //            done = true;
    //        }
    //        else {
    //            done = true;
    //        }
    //    });
    //    yield return new WaitWhile(() => done == false);
    //}
    

    IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Player was logged in");
                PlayerPrefs.SetString("PlayerID", response.player_id.ToString());
                done = true;
            }
            else
            {
                Debug.Log("Cound not start session" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }
}
