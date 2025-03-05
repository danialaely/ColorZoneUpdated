using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cube : MonoBehaviour
{
    private Vector2 previousPosition; // To track the last touch/mouse position
    private bool isDragging = false;

    public float scaleSpeed = 0.01f; // Speed of scaling
    public float maxZScale = 3f; // Maximum width
    public float minZScale = 0.5f; // Minimum width
    public float maxYScale = 3f; // Maximum height
    public float minYScale = 0.5f; // Minimum height
    private float speed = 1.0f;
    public FollowPlayer fp;

    private Vector3 initialPosition;
    public TMP_Text GameScore;
    public TMP_Text EndScore;
    public GameObject retryPanel;
    public GameObject tutPanel;

    public bool hit;

    private void Start()
    {
        initialPosition = transform.position;
        hit = false;
        tutPanel.gameObject.SetActive(true);
    }

    void Update()
    {
        float targetX = transform.position.x; // Default to current x-position
        float dist = transform.position.z - initialPosition.z;
        float scoreval = (int)(speed*dist);

        GameScore.text = scoreval.ToString();
        EndScore.text = "Score:" + scoreval.ToString();

        // Handle both mouse and touch inputs
        if (Input.touchCount > 0)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
        StartCoroutine(MovingForward(3.0f));
    }

    private void HandleMouseInput()
    {
        // Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            previousPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentPosition = Input.mousePosition;
            AdjustScale(currentPosition.x - previousPosition.x); // Only horizontal movement
            previousPosition = currentPosition;
        }
    }

    private void HandleTouchInput()
    {
        // Touch input
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            previousPosition = touch.position;
            isDragging = true;
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentPosition = touch.position;
            AdjustScale(currentPosition.x - previousPosition.x); // Only horizontal movement
            previousPosition = currentPosition;
        }
    }

    private void AdjustScale(float deltaX)
    {
        Vector3 currentScale = transform.localScale;

        // Horizontal movement affects Z scale (width) and inversely affects Y scale (height)
        if (Mathf.Abs(deltaX) > Mathf.Epsilon) // Ensure there is significant movement
        {
            currentScale.x = Mathf.Clamp(currentScale.x + deltaX * scaleSpeed, minZScale, maxZScale);
            currentScale.y = Mathf.Clamp(currentScale.y - deltaX * scaleSpeed, minYScale, maxYScale);
        }

        transform.localScale = currentScale;
    }

    IEnumerator MovingForward(float delay)
    {
        yield return new WaitForSeconds(delay);
        tutPanel.gameObject.SetActive(false);
        if (hit == false) 
        {
            speed += 0.005f;
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Obstacle")
        {
            hit = true;
            //StartCoroutine(StopTime(1.5f));
            //Time.timeScale = 0.0f;
            retryPanel.gameObject.SetActive(true);
            GameScore.gameObject.SetActive(false);
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "PosChange") 
        {
            Debug.Log("Rotating");
            //transform.position += new Vector3(1.0f, 0, 0.0f) * speed * Time.deltaTime;
            if (transform.rotation == Quaternion.Euler(0, 0, 0)) 
            {
                transform.rotation = Quaternion.Euler(0.0f,-90.0f,0.0f);
            fp.SetMoveRight(false);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0.0f,0.0f,0.0f);
            fp.SetMoveRight(true);
            }
        }
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        retryPanel.gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }

    IEnumerator StopTime(float del) 
    {
        yield return new WaitForSeconds(del);
        Time.timeScale = 0.0f;
    }
}
