using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Animator cameraAnimator;
    float speed = 30.0f;
    bool moveRight;
    public Ball ball;

    //public Cube cb;

    private void Start()
    {
        moveRight = true;
    }

    private void Update()
    {
        StartCoroutine(MovingForward(3.0f));
    }

    IEnumerator MovingForward(float delay)
    {
        yield return new WaitForSeconds(delay);
        cameraAnimator.enabled = false;
        if (!ball.GamePaused()) 
        {
            speed += 0.01f;
        }
        // if (cb.hit == false) {}
        if (moveRight && !ball.GamePaused())
        {
            transform.position += new Vector3(0, 0, 1.0f) * speed * Time.deltaTime;
        }
        //else{  transform.position += new Vector3(-1.0f, 0, 0) * speed * Time.deltaTime;}
    }

    public void SetMoveRight(bool right)
    {
        moveRight = right;
    }

}
