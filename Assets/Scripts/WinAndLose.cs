using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinAndLose : MonoBehaviour
{
    [SerializeField] private Leaderboard _leaderboard;
    [SerializeField] private Stopwatch _stopwatch;
    [SerializeField] private GameObject _leaderBoardImage;

    private bool _gameWon;

    private void Update()
    {
        if(_gameWon && Input.GetKeyDown(KeyCode.Escape)) StartCoroutine(RestartGame());
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            WinGame();
            Debug.Log("Game has ended");
        }
    }

    private void WinGame()
    {
        _stopwatch.StopStopwatch();
        int score = Mathf.RoundToInt(_stopwatch.GetCurrentTime() * 1000f);
        StartCoroutine(SubmitScore(score));
        _leaderBoardImage.gameObject.SetActive(true);
        Time.timeScale = 0f;
        _gameWon = true;
    }

    public void LoseGame()
    {
        StartCoroutine(RestartGame());
    }

    IEnumerator RestartGame()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator SubmitScore(int score)
    {
        yield return _leaderboard.SubmitScoreRoutine(score);
    }
}
