using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OurAi : MonoBehaviour
{
    public Transform[] patrolPoints;
    public int targetPoint;
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        targetPoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position == patrolPoints[targetPoint].position) 
        {
            increaseTargetInt();
        }
            LookAtTarget(patrolPoints[targetPoint].position);
        transform.position = Vector3.MoveTowards(transform.position, patrolPoints[targetPoint].position, speed*Time.deltaTime);
    }

    void increaseTargetInt() 
    {
        targetPoint++;
        if(targetPoint >= patrolPoints.Length) 
        {
            targetPoint = 0;
        }
    }

    private void LookAtTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0; // Keep the AI looking only on the horizontal plane
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5); // Smooth rotation
    }
}
