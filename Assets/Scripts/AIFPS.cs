using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFPS : MonoBehaviour
{
    public Animator animator;

    public float distance = 10;
    public float angle = 30;
    public float height = 1.0f;
    public Color meshColor = Color.red;
    public int scanFrequency = 30;
    public LayerMask layers;
    public LayerMask occlusionLayers;
    public List<GameObject> Objects = new List<GameObject>();

    Collider[] colliders = new Collider[50];
    Mesh mesh;
    int count;
    float scanInterval;
    float scanTimer;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;
    public float bulletSpeed = 20f;
    public float shootInterval = 1f; // Time in seconds between shots
    private float shootTimer = 0f; // Tracks time since last shot

    // Start is called before the first frame update
    void Start()
    {
        scanInterval = 1.0f / scanFrequency;
    }

    // Update is called once per frame
    void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0) 
        {
            scanTimer += scanInterval;
            Scan();
        }


        // Check if the player is in sight
        foreach (GameObject obj in Objects)
        {
            if (obj.CompareTag("Player")) // Assuming the player has a tag "Player"
            {
                LookAtTarget(obj);
                    Shoot();
                if (shootTimer <= 0f)
                {
                    shootTimer = shootInterval; // Reset the shoot timer
                }
              //  break; // Stop after finding the player
            }
        }
    }

    private void Scan() 
    {
        count = Physics.OverlapSphereNonAlloc(transform.position, distance, colliders, layers, QueryTriggerInteraction.Collide);

        Objects.Clear();
        for (int i = 0; i < count; i++) 
        {
            GameObject obj = colliders[i].gameObject;
            if (IsInSight(obj)) 
            {
                Objects.Add(obj);
            }
        }
    }

    public bool IsInSight(GameObject obj) 
    {
        Vector3 origin = transform.position;
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;
        if (direction.y < 0 || direction.y > height) 
        {
            return false;
        }

        direction.y = 0;
        float deltaAngle = Vector3.Angle(direction,transform.forward);
        if (deltaAngle > angle) 
        {
            return false;
        }

        origin.y += height / 2;
        dest.y = origin.y;
        if (Physics.Linecast(origin,dest,occlusionLayers)) 
        {
            return false;
        }

        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet") 
        {
            animator.SetBool("isDying",true);
            Destroy(collision.gameObject);
            StartCoroutine(Dying(2.0f));
        }
    }

    IEnumerator Dying(float delay) 
    {
        yield return new WaitForSeconds(delay);
        Destroy(this.gameObject);
    }

    Mesh CreateWedgeMesh() 
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments*4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 bottomCenter = Vector3.zero;
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0)*Vector3.forward * distance;
        Vector3 bottomRight = Quaternion.Euler(0, angle, 0)*Vector3.forward * distance;

        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;

        int vert = 0;
        // left Side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;

        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;

        // right Side
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;

        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i=0; i<segments; ++i) 
        {
             bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * distance;
             bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * distance;

             topRight = bottomRight + Vector3.up * height;
             topLeft = bottomLeft + Vector3.up * height;

            // far Side
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;

            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;

            // top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;

            // bottom
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;

            currentAngle += deltaAngle;
        }

        

        for (int i=0; i< numVertices; i++) 
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnValidate()
    {
        mesh = CreateWedgeMesh();
        scanInterval = 1.0f / scanFrequency;
    }

    private void OnDrawGizmos()
    {
        if (mesh) 
        {
            Gizmos.color = meshColor;
            Gizmos.DrawMesh(mesh, transform.position, transform.rotation);
        }

        Gizmos.DrawSphere(transform.position,distance);
        for (int i=0; i< count; ++i) 
        {
            Gizmos.DrawSphere(colliders[i].transform.position, 0.2f);
        }

        Gizmos.color = Color.green;
        foreach (var obj in Objects) 
        {
            Gizmos.DrawSphere(obj.transform.position, 0.2f);
        }
    }

    private void LookAtTarget(GameObject target)
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;
        direction.y = 0; // Keep the AI looking only on the horizontal plane
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5); // Smooth rotation
    }

    private void Shoot()
    {
        // Instantiate a bullet and set its velocity
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPos.position, bulletSpawnPos.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPos.forward * bulletSpeed;
        animator.SetBool("isAttacking",true);
        StartCoroutine(FinishedShooting(1.0f));
    }

    IEnumerator FinishedShooting(float del) 
    {
        yield return new WaitForSeconds(del);
        animator.SetBool("isAttacking",false);
    }
}
