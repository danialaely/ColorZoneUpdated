using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class Ball : MonoBehaviour
{
    float speed = 30.0f;
    float touchspeed = 200.0f;
    float tempSpeed;

    public Camera mainCamera; // Reference to the main camera
    public float yPosition = 0f; // Fixed y position for the ball
    public float zPosition = 0f; // Fixed z position for the ball

    public GameObject retryPanel;
    private Vector3 lastMousePosition;

    public GameObject tutorialPanel;
    public GameObject scorePanel;
    public GameObject pausePanel;
    public TMP_Text GameScore;
    public TMP_Text EndScore;
    float scoreval;
    private int highscore;

    private Vector3 initialPosition;

    public Material purpleMat;
    public Material yellowMat;
    public Material cyanMat;
    private Renderer ballRenderer;

    public string statisticName = "HighScore"; // Name of the statistic used for the leaderboard
    public bool isPaused;
    public TMP_Text ThreeTwoOneTxt;
    public GameObject pauseBtn;
    
    private void Start()
    {
        StartCoroutine(ActiveTutPanel(2.9f));
        StartCoroutine(DeactivateTutPanel(5.0f));
        initialPosition = transform.position;
        ballRenderer = GetComponent<Renderer>();
        FetchHighScoreFromPlayFab();
        isPaused = false;
        ThreeTwoOneTxt.gameObject.SetActive(false);
        pauseBtn.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        float targetX = transform.position.x; // Default to current x-position
        float dist = transform.position.z-initialPosition.z;
        scoreval = (int) dist;

        GameScore.text = scoreval.ToString();
        EndScore.text = "Score:"+scoreval.ToString();

        StartCoroutine(MovingForward(3.0f));

        if (Input.GetMouseButton(0))
        {
            MoveWithMouse();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); // Get the first touch
            Vector3 touchPosition = touch.position; // Get touch position

            // Convert the screen position to world coordinates
            Ray ray = mainCamera.ScreenPointToRay(touchPosition);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Assuming the ground is on the y=0 plane

            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPosition = ray.GetPoint(distance);
                // Smoothly move the ball towards the target position, keeping y and z constant
                transform.position = Vector3.Lerp(transform.position, new Vector3(targetPosition.x, transform.position.y, transform.position.z), touchspeed * Time.deltaTime);
            }
        }

        if (transform.position.y <= 1) 
        {
            //Time.timeScale = 0.0f;
            retryPanel.gameObject.SetActive(true);
            scorePanel.gameObject.SetActive(false);
        }
    }

    void MoveWithMouse()
    {
        // Get the mouse position in screen coordinates
        Vector3 mousePosition = Input.mousePosition;

        // Convert the screen x position to world coordinates
        // Set a fixed depth (z-axis) to maintain the ball's position in the 3D space
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0));
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Assuming the ground is on the y=0 plane

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 targetPosition = ray.GetPoint(distance);

            // Smoothly move the ball towards the target position, keeping y and z constant
            transform.position = Vector3.Lerp(transform.position, new Vector3(targetPosition.x, transform.position.y, transform.position.z), touchspeed * Time.deltaTime);
        }
    }

    void MoveWithTouch(Touch touch)
    {
        float moveX = touch.deltaPosition.x * touchspeed * Time.deltaTime;
        transform.Translate(moveX, 0, 0);
    }

    IEnumerator MovingForward(float delay) 
    {
        yield return new WaitForSeconds(delay);
        if (!isPaused) 
        {
            speed += 0.01f;
            transform.position += new Vector3(0, 0, 1.0f) * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle") 
        {
            Time.timeScale = 0.0f;
            retryPanel.gameObject.SetActive(true);
            scorePanel.gameObject.SetActive(false);
            Debug.Log("HighScore:" + highscore);
            Debug.Log("Current Score:" + scoreval);
            if (scoreval >= highscore) 
            {
                highscore = (int)scoreval;
                Debug.Log("New HighScore: " + highscore);
                UpdateHighScoreInPlayFab(highscore); // Update high score in PlayFab
            }
        }
    }

    public void PlayAgain() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        retryPanel.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void MenuClick()
    {
        SceneManager.LoadScene("SampleScene");
        retryPanel.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }

    IEnumerator ActiveTutPanel(float del) 
    {
        yield return new WaitForSeconds(del);
        tutorialPanel.gameObject.SetActive(true);
        scorePanel.gameObject.SetActive(true);
    }

    IEnumerator DeactivateTutPanel(float del)
    {
        yield return new WaitForSeconds(del);
        tutorialPanel.gameObject.SetActive(false);
        pauseBtn.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "purpleWall")
        {
            ballRenderer.material = purpleMat;
        }
        else if (other.gameObject.tag == "cyanWall") 
        {
            ballRenderer.material = cyanMat;
        }
        else if (other.gameObject.tag == "yellowWall")
        {
            ballRenderer.material = yellowMat;
        }
    }

    private void FetchHighScoreFromPlayFab()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
            result =>
            {
                foreach (var stat in result.Statistics)
                {
                    if (stat.StatisticName == statisticName)
                    {
                        highscore = stat.Value;
                        Debug.Log("HighScore fetched from PlayFab: " + highscore);
                        return;
                    }
                }

                // If "HighScore" statistic is not found, default to 0
                highscore = 0;
                Debug.Log("No HighScore statistic found in PlayFab. Defaulting to 0.");
            },
            error =>
            {
                Debug.LogError("Error fetching player statistics from PlayFab: " + error.GenerateErrorReport());
            });
    }

    private void UpdateHighScoreInPlayFab(int newHighScore)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new System.Collections.Generic.List<StatisticUpdate>
        {
            new StatisticUpdate { StatisticName = statisticName, Value = newHighScore }
        }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request,
            result =>
            {
                Debug.Log("HighScore successfully updated in PlayFab: " + newHighScore);
            },
            error =>
            {
                Debug.LogError("Error updating HighScore in PlayFab: " + error.GenerateErrorReport());
            });
    }

    public void PauseBtn() 
    {
        //Time.timeScale = 0.0f;
        tempSpeed = speed;
        speed = 0;
        pausePanel.SetActive(true);
        scorePanel.SetActive(false);
        isPaused = true;
    }

    public void ResumeBtn() 
    {
        //Time.timeScale = 1.0f;
        speed = tempSpeed;
        pausePanel.SetActive(false);
        scorePanel.SetActive(true);
        ThreeTwoOneTxt.gameObject.SetActive(true);
        ThreeTwoOneTxt.text = "3";
        StartCoroutine(Two(1.0f));
        //StartCoroutine(Three(2.0f));
        //StartCoroutine(Three(3.0f));
        StartCoroutine(Resumenow(3.0f));
    }

    IEnumerator Resumenow(float del) 
    {
        yield return new WaitForSeconds(del);   
        isPaused = false;
        ThreeTwoOneTxt.gameObject.SetActive(false);
    }

    public bool GamePaused() 
    {
        //resumetimer = 3;
        //ThreeTwoOneTxt.text = resumetimer.ToString();
        return isPaused;
    }

    
    IEnumerator Two(float del) 
    {
        yield return new WaitForSeconds(del);
        ThreeTwoOneTxt.text = "2";
        StartCoroutine(One(1.0f));
    }
    IEnumerator One(float del) 
    {
        yield return new WaitForSeconds(del);
        ThreeTwoOneTxt.text = "1";
        StartCoroutine(GoDisable(1.0f));
    }
    IEnumerator GoDisable(float del) 
    {
        yield return new WaitForSeconds(del);
        ThreeTwoOneTxt.gameObject.SetActive(false);
    }

    
}
