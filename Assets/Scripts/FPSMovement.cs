using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSMovement : MonoBehaviour
{
    public Animator animator;
    public float sensitivity = 200f;  // Sensitivity for the mouse movement
    public Transform playerBody;      // Reference to the player's body for rotating

   // private float xRotation = 0f;     // Track the current x-axis rotation
    public GameObject bulletPrefab;
    public Transform bulletSpawnPos;
    GameObject bullet;

    public Transform head;
    public ParticleSystem shootEffect;

    public AudioSource src;
    public AudioClip shootClip;
    public AudioClip reloadClip;
    //public AudioClip walkingClip;
    public TMP_Text bulletsTxt;
    private int bullets;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // Locks the cursor to the game screen
        animator.SetBool("isWalking",false);
        bullets = 24;
        bulletsTxt.text = bullets.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Shoot();
        RotateHorizontal();
        bullet.transform.position += transform.forward * 25 * Time.deltaTime;
    }

    private void LateUpdate()
    {
        Vector3 e = head.eulerAngles;
        e.x -= Input.GetAxis("Mouse Y")*0.1f;
        e.x = RestrictAngles(e.x, -85f, 85f);
        head.eulerAngles = e;
    }

    public static float RestrictAngles(float angle, float angleMin, float angleMax) 
    {
        if (angle > 180)
        {
            angle -= 360;
        }
        else if (angle < -180) 
        {
            angle += 360;
        }

        if (angle > angleMax) 
        {
            angle = angleMax; 
        }
        if (angle < angleMin) 
        {
            angle = angleMin;
        }

        return angle;
    }

    void Movement() 
    {
        if (Input.GetKey(KeyCode.W))
        {
            Debug.Log("Move Forward");
            transform.position += transform.forward * 5 * Time.deltaTime;
            animator.SetBool("isWalking", true);
            //src.PlayOneShot(walkingClip);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Debug.Log("Move Right");
            transform.position += -transform.right * 5 * Time.deltaTime;
            animator.SetBool("isWalking", true);
            //src.PlayOneShot(walkingClip);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Debug.Log("Move Backward");
            transform.position += -transform.forward * 5 * Time.deltaTime;
            animator.SetBool("isWalking", true);
            //src.PlayOneShot(walkingClip);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Debug.Log("Move Left");
            transform.position += transform.right * 5 * Time.deltaTime;
            animator.SetBool("isWalking", true);
            //src.PlayOneShot(walkingClip);
        }
        else if (Input.GetKeyDown(KeyCode.R)) 
        {
            Reload();
        }
        else
        {
            animator.SetBool("isWalking", false);
            //src.Pause();
        }
    }
    void Shoot() 
    {
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetBool("isShooting", true);
            shootEffect.Play();
            Debug.Log("Shooting");
            bullets -= 1;
            bulletsTxt.text = bullets.ToString();
            ShootSound();
            StartCoroutine(FinishShooting(0.5f));
            bullet = Instantiate(bulletPrefab, bulletSpawnPos.position , transform.rotation * Quaternion.Euler(0,90,0));
            //StartCoroutine(DestroyBullet(3.0f));
        }
        else 
        {
            animator.SetBool("isShooting", false);
        }
        //bullet.transform.position += transform.forward*25*Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bullet") 
        {
            Debug.Log("Dying"); //Camera Shake
            Destroy(collision.gameObject);
        }
    }

    void Reload() 
    {
        animator.SetBool("isReloading",true);
        bullets = 24;
        bulletsTxt.text = bullets.ToString();
        src.PlayOneShot(reloadClip);
        StartCoroutine(FinishReloading(1.0f));
    }

    void RotateHorizontal()
    {
        transform.Rotate(Vector3.up*Input.GetAxis("Mouse X")*2f);
    }


    IEnumerator FinishReloading(float del) 
    {
        yield return new WaitForSeconds(del);
        animator.SetBool("isReloading",false);
    }

    IEnumerator FinishShooting(float del) 
    {
        yield return new WaitForSeconds(del);
        shootEffect.Stop();
    }

    IEnumerator DestroyBullet(float del) 
    {
        yield return new WaitForSeconds(del);
        Destroy(bullet);
    }

    public void ShootSound() 
    {
        src.clip = shootClip;
        src.PlayOneShot(shootClip);
    }
}
