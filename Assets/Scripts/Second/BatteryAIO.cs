﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BatteryAIO : MonoBehaviour
{
    public static BatteryAIO _instance { get; private set; }

    //Look Settings
    [Header("Look Settings")]
    [Space(8)]
    public bool enableCameraMovement = true;
    public Vector3 rotationRange = new Vector3(45,90, Mathf.Infinity);
    [Range(0.01f, 100)]    public float mouseSensitivity = 10f;
    [Range(0.01f, 100)]    public float dampingTime = 0.05f;   
    public bool relativeMotionMode = true;
    public bool lockAndHideMouse = true;
    public Transform playerCamera;
    private Vector3 targetAngles;
    private Vector3 followAngles;
    private Vector3 followVelocity;
    private Vector3 originalRotation;
    //Audio Settings
    AudioSource audioSource;
    [Range(0, 10)] private float Volume = 5f;
    AudioClip  shootClip;

    //Shoot Settings
    [Header("Shoot Settings")]
    [Space(8)]
    float timer = 0.3f;
    float tictock = 0;
    LayerMask monsterMask;
    LayerMask wallMask;
    Vector3 targetPos;
    [HideInInspector] public bool gameOver = false;
    GameObject bullet;

    private void Awake()
    {
        _instance = this;
        //Look Settings - Awake
        originalRotation = transform.localRotation.eulerAngles;
    }

    private void Start()
    {
        //Look Settings - Start
        enableCameraMovement = true;
        lockAndHideMouse = true;
        if (lockAndHideMouse) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        //AudioSettings
        if (GetComponent<AudioSource>() == null) { gameObject.AddComponent<AudioSource>(); }
        audioSource = GetComponent<AudioSource>();    
        shootClip = Resources.Load<AudioClip>("Sound/Effect/refract");
        //ShootSettings
        bullet = Resources.Load<GameObject>("Prefabs/bullet");
    }

    private void Update()
    {

        #region Look Settings - Update
        if (!gameOver)
        {
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
        }
        if (enableCameraMovement)
        {
            float mouseXInput ,  mouseYInput;
            if (relativeMotionMode)
            {
                mouseXInput = Input.GetAxis("Mouse Y");
                mouseYInput = Input.GetAxis("Mouse X");
                if (targetAngles.y > 180) { targetAngles.y -= 360; followAngles.y -= 360; } else if (targetAngles.y < -180) { targetAngles.y += 360; followAngles.y += 360; }
                if (targetAngles.x > 180) { targetAngles.x -= 360; followAngles.x -= 360; } else if (targetAngles.x < -180) { targetAngles.x += 360; followAngles.x += 360; }
                targetAngles.y += mouseYInput * mouseSensitivity;
                targetAngles.x += mouseXInput * mouseSensitivity;
                targetAngles.y = Mathf.Clamp(targetAngles.y, -0.5f * rotationRange.z, 0.5f * rotationRange.z);
                targetAngles.x = Mathf.Clamp(targetAngles.x, -0.5f * rotationRange.x, 0.5f * rotationRange.y);
            }
            else
            {
                mouseXInput = Input.mousePosition.y;
                mouseYInput = Input.mousePosition.x;
                targetAngles.y = Mathf.Lerp(rotationRange.y * -0.5f, rotationRange.y * 0.5f, mouseXInput / Screen.width);
                targetAngles.x = Mathf.Lerp(rotationRange.x * -0.5f, rotationRange.x * 0.5f, mouseXInput / Screen.height);
            }
        }
        followAngles = Vector3.SmoothDamp(followAngles, targetAngles, ref followVelocity, dampingTime);
        playerCamera.localRotation = Quaternion.Euler(-followAngles.x + originalRotation.x, 0, 0);
        transform.localRotation = Quaternion.Euler(0, followAngles.y + originalRotation.y, 0);
        #endregion

        #region Shoot Settings - Update        
        if (!gameOver)
        {
            //方向就是摄像机正前方呗       Camera.main.transform.forward            
            tictock += Time.deltaTime;//计时器限制，2s后才能发射下一次
            if (tictock > timer)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    Shoot();
                    tictock = 0;
                }
            }
        }
        #endregion

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            GameManager2._instance.Change2PlayerView();
        }
    }   
    void Shoot()
    {
        Camera cam = playerCamera.GetComponent<Camera>();
        Vector3 centerPoint =cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth / 2, cam.pixelHeight / 2, 2));
        GameObject bulletGo= Instantiate(bullet,centerPoint+playerCamera.transform.forward*10,Quaternion.identity);
        bulletGo.GetComponent<Rigidbody>().velocity = playerCamera.transform.forward * 200;
    }
    public void GameOver()
    {
        gameOver = true;
        enableCameraMovement = false;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
    }

}

