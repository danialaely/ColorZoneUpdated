using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayFabLogin : MonoBehaviour
{
    public GameObject SettingsPanel;
    public GameObject EditPanel;
    public GameObject LeaderBoardPanel;
    public GameObject LoadingPanel;
    public TMP_InputField usernameInputField; // Input field for player nickname
    public TMP_Text username;
    public TMP_Text HighScoreTitle;
    public TMP_Text HighScoreTxt;
    private int highscore;

    public string statisticName = "HighScore"; // Name of the statistic used for the leaderboard
    [SerializeField] private GameObject playerStatePrefab; // Prefab for PlayerStateObject
    [SerializeField] private Transform contentParent;      // Parent object (Content in the ScrollView)
    
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private float duration = 2.0f; // Time to fill slider
    private static bool hasSceneLoadedBefore = false; // Static variable (doesn't reset unless game restarts)

    public void Start()
    {
        //Note: Setting title Id here can be skipped if you have set the value in Editor Extensions already.
        if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
        {
            PlayFabSettings.TitleId = "86780"; // Please change this value to your own titleId from PlayFab Game Manager
        }

        if (loadingSlider != null)
        {
            StartCoroutine(FillSlider());
        }

        if (!hasSceneLoadedBefore)
        {
            Debug.Log("First time loading the scene.");
            LoadingPanel.SetActive(true);
            hasSceneLoadedBefore = true; // Set true so next load skips animation
        }
        else 
        {
            LoadingPanel.SetActive(false);
        }
            // var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true };
            // PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);

#if UNITY_ANDROID
            var requestAndroid = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileID(), CreateAccount = true };
        PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginAndroidSuccess, OnLoginAndroidFailure);
#elif UNITY_WEBGL            
        var requestWebGL = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(requestWebGL, OnLoginWebGLSuccess, OnLoginWebGLFailure);

#endif
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Congratulations, you made your first successful API call!");
    }
    private void OnLoginAndroidSuccess(LoginResult result)
    {
        FetchHighScoreFromPlayFab();
        Debug.Log("Successfully Loggedin with Android ID");
        GetPlayerDisplayName();
        GetLeaderboard();
        //LoadingPanel.SetActive(false);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }
    private void OnLoginAndroidFailure(PlayFabError error)
    {
        Debug.LogError(error.GenerateErrorReport());
    }

    //Webgl
    private void OnLoginWebGLSuccess(LoginResult result)
    {
        FetchHighScoreFromPlayFab();
        Debug.Log("Successfully Logged in with WebGL Custom ID");
        GetPlayerDisplayName();
        GetLeaderboard();
        //LoadingPanel.SetActive(false); // Uncomment if you have a loading panel
    }

    private void OnLoginWebGLFailure(PlayFabError error)
    {
        Debug.LogError("WebGL Login Failed: " + error.GenerateErrorReport());
    }

    private void OnRegisterSuccess(RegisterPlayFabUserRequest result) 
    {
        Debug.Log("Congratulations, you made your first successful API call!");

    }
    private void OnRegisterFailure(PlayFabError error) 
    {
        Debug.Log(error.GenerateErrorReport());

    }

    public static string ReturnMobileID() 
    {
        string deviceID = SystemInfo.deviceUniqueIdentifier;
        return deviceID;
    }

    private void GetPlayerDisplayName()
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(),
            result =>
            {
                string displayName = result.PlayerProfile?.DisplayName;
                if (string.IsNullOrEmpty(displayName))
                {
                    Debug.Log("No display name set. Default name or custom logic can be applied.");
                    string randomUsername = GenerateRandomUsername();
                    SetInitialUsername(randomUsername);
                }
                else
                {
                    Debug.Log($"Player's current display name: {displayName}");
                    username.text = displayName;
                }
            },
            error =>
            {
                Debug.LogError($"Error retrieving player profile: {error.GenerateErrorReport()}");
            });
    }

    public void SettingsBtn() 
    {
        SettingsPanel.SetActive(true);
        HighScoreTitle.gameObject.SetActive(false);
        HighScoreTxt.gameObject.SetActive(false);
    }

    public void EditUserName() 
    {
        EditPanel.SetActive(true);
    }

    public void ConfirmUserNameBtn() 
    {
        EditPanel.SetActive(false);
        SaveNickname();
    }
    public void CrossEditPanel() 
    {
        EditPanel.SetActive(false);
    }

    public void SettingsBackBtn() 
    {
        SettingsPanel.SetActive(false);
        HighScoreTitle.gameObject.SetActive(true);
        HighScoreTxt.gameObject.SetActive(true);
    }

    // Save the player's nickname
    private void SaveNickname()
    {
        if (usernameInputField == null)
        {
            Debug.LogError("UI components are not assigned!");
            return;
        }

        string nickname = usernameInputField.text.Trim();

        

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = nickname
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnNicknameUpdateSuccess, OnNicknameUpdateFailure);
    }

    private void OnNicknameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
    {
        username.text = result.DisplayName;
        Debug.Log($"Nickname updated successfully: {result.DisplayName}");
    }

    private void OnNicknameUpdateFailure(PlayFabError error)
    {
        username.text = "Failed to save nickname. Try again!";
        Debug.LogError($"Error saving nickname: {error.GenerateErrorReport()}");
    }

    private void FetchHighScoreFromPlayFab()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
            result =>
            {
                bool highScoreFound = false;

                foreach (var stat in result.Statistics)
                {
                    if (stat.StatisticName == "HighScore")
                    {
                        highscore = stat.Value;
                        Debug.Log("HighScore fetched from PlayFab: " + highscore);
                        HighScoreTxt.text = highscore.ToString(); // Update UI text
                        highScoreFound = true;
                        break;
                    }
                }

                // If no high score is found, default to 0
                if (!highScoreFound)
                {
                    highscore = 0;
                    Debug.Log("No HighScore statistic found in PlayFab. Defaulting to 0.");
                    HighScoreTxt.text = highscore.ToString(); // Update UI text
                }
            },
            error =>
            {
                Debug.LogError("Error fetching player statistics from PlayFab: " + error.GenerateErrorReport());
            });
    }

    private string GenerateRandomUsername()
    {
        string prefix = "Player";
        int randomNumber = Random.Range(1000, 9999); // Generate a random 4-digit number
        return prefix + randomNumber;
    }

    private void SetInitialUsername(string username)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = username
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request,
            result =>
            {
                Debug.Log("Username successfully set to: " + result.DisplayName);
            },
            error =>
            {
                Debug.LogError("Failed to set username: " + error.GenerateErrorReport());
            });
    }

    public void GetLeaderboard()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0, // Starting position in the leaderboard (0 = top)
            MaxResultsCount = 10 // Number of leaderboard entries to fetch
        };

        PlayFabClientAPI.GetLeaderboard(request,
            result =>
            {
                Debug.Log("Leaderboard fetched successfully.");
                DisplayLeaderboard(result.Leaderboard);
            },
            error => Debug.LogError("Error fetching leaderboard: " + error.GenerateErrorReport()));
    }

    private void DisplayLeaderboard(List<PlayerLeaderboardEntry> leaderboard)
    {
        Debug.Log("Rank\tName\t\tScore");

        // Clear existing leaderboard entries
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Populate the leaderboard
        foreach (var entry in leaderboard)
        {
            // Instantiate the prefab
            GameObject playerStateObject = Instantiate(playerStatePrefab, contentParent);

            // Get references to the text components
            TMP_Text snoText = playerStateObject.transform.GetChild(0).GetComponent<TMP_Text>();
            TMP_Text playerNameText = playerStateObject.transform.GetChild(1).GetComponent<TMP_Text>();
            TMP_Text hsText = playerStateObject.transform.GetChild(2).GetComponent<TMP_Text>();

            // Set the values
            snoText.text = (entry.Position + 1).ToString(); // Serial number
            playerNameText.text = string.IsNullOrEmpty(entry.DisplayName) ? "Anonymous" : entry.DisplayName; // Player name
            hsText.text = entry.StatValue.ToString(); // High score

            Debug.Log($"{entry.Position + 1}\t{playerNameText.text}\t\t{entry.StatValue}");
        }

        LoadingPanel.SetActive(false);
    }

    public void OpenLeaderboardPanel() 
    {
        LeaderBoardPanel.SetActive(true);
    }
    public void DeactiveLeaderboardPanel() 
    {
        LeaderBoardPanel.SetActive(false);
    }

    private IEnumerator FillSlider()
    {
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            loadingSlider.value = Mathf.Lerp(0, 1, timeElapsed / duration); // Smoothly interpolate value
            yield return null;
        }
        loadingSlider.value = 1; // Ensure slider reaches full
    }
}