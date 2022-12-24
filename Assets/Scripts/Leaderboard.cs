using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro;
using System;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private string _leaderboardID = "9874";
    [SerializeField] private TextMeshProUGUI _playerNames;
    [SerializeField] private TextMeshProUGUI _playerScores;
    [SerializeField] private int _numberScores = 20;


    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        bool done = false;
        string playerID = PlayerPrefs.GetString("PlayerID");
        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, _leaderboardID, (response) =>
        {
            if (response.success)
            {
                Debug.Log("Player uploaded score");
                done = true;
            }
            else
            {
                Debug.Log("Failed" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    public IEnumerator FetchTopHighScoresRoutine()
    {
        bool done = false;
        LootLockerSDKManager.GetScoreList(_leaderboardID, _numberScores, 0, (response) =>
        {
            if (response.success)
            {
                string tempPlayerNames = "";
                string tempPlayerScores = "";

                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    tempPlayerNames += members[i].rank + ". ";
                    if (members[i].player.name != "")
                    {
                        tempPlayerNames += members[i].player.name;
                    }
                    else
                    {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += FormatScores(members[i].score) + "\n";
                    tempPlayerNames += "\n";
                }
                done = true;
                _playerNames.text = tempPlayerNames;
                _playerScores.text = tempPlayerScores;
            }
            else
            {
                Debug.Log("Failed" + response.Error);
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    private string FormatScores(int score)
    {
        float timeScore = score;
        TimeSpan time = TimeSpan.FromSeconds(timeScore/1000f);
        return time.ToString(@"mm\:ss\:fff");
    }
}
