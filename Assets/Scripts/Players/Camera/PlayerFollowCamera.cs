using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
using System;

[RequireComponent(typeof(Camera))]
public class PlayerFollowCamera : MonoBehaviour
{
    private bool isWorking;

    [SerializeField] private Transform cameraPointT;
    [SerializeField] private Transform cameSlideT;
    [SerializeField] private Transform playerT;
    [SerializeField] private Transform cameraStartT;
    [SerializeField] private SplineComputer roadSpline;
    [SerializeField] private PlayerMover mover;
    [Space]
    [SerializeField] private float distanceToPlayer;
    [SerializeField] private float defaultHeight;
    [SerializeField] private float smooth;
    [SerializeField] private float rotateSmooth;
    [Header("Raycast settings")]
    [SerializeField] private LayerMask barrierLayer;
    [SerializeField] private float raycastDistance;
    [SerializeField] private float fadeDistance;
    [Header("Slide Settings")]
    [SerializeField] private float slideSmooth;
    [SerializeField] private bool isSliding;
    [SerializeField] private bool isReturning;
    [Header("Speed boost settings")]
    [SerializeField] private float speedBoostedFOV;
    [SerializeField] private float speedBoostedSmooth;
    [SerializeField] private float speedReturningSmooth;
    private Camera cam;
    private float defaultFOV;
    private float currentFOV;
    private float desiredFOV;
    [SerializeField] private bool isSpeedBoosted;
    [SerializeField] private bool isSpeedReturning;
    private float defaultSpeed;
    private float slideTime = .8f;
    private bool isStarting;
    private float actualHeight;
    private Transform myT;

    private void Intitalize()
    {
        if (playerT == null)
        {
            Debug.LogError("plyaer T in CameraFollow script is null");
        }
        cam = Camera.main;
        PlayerMover.OnSlide += OnSlide;
        PlayerMover.OnSpeedBoost += BoostSpeed;
        PlayerMover.OnSpeedBoostStopped += SpeedBoostStopped;
        myT = transform;
        defaultSpeed = mover.GetDefaultSpeed();
        actualHeight = defaultHeight;
        defaultFOV = cam.fieldOfView;
        isWorking = true;
        isStarting = true;
        StartLine.OnCrossStartLine += OnPlayerCrossLine;
    }

    private void OnPlayerCrossLine()
    {
        StartCoroutine(MoveToPlayer(1));
    }

    private void OnDestroy()
    {
        StartLine.OnCrossStartLine -= OnPlayerCrossLine;
    }

    private IEnumerator MoveToPlayer(float moveTime)
    {
        float timer = 0;
        isWorking = false;
        isStarting = false;
        while(timer<= moveTime)
        {
            timer += Time.deltaTime;
            myT.position = Vector3.Lerp(cameraStartT.position, cameraPointT.position + cameraPointT.up * actualHeight, timer/ moveTime);
            myT.rotation = Quaternion.Lerp(myT.rotation, Quaternion.Lerp(myT.rotation, Quaternion.LookRotation(playerT.position-myT.position,cameraPointT.up),timer/moveTime),timer/moveTime);
            yield return null;
        }
        Quaternion startRotation = myT.rotation;
        float rotateTimer = 0;
        float rotateTime = 1.2f;
        while(rotateTimer <= rotateTime)
        {
            rotateTimer += Time.deltaTime;
            myT.position = cameraPointT.position + cameraPointT.up * actualHeight;
            myT.rotation = Quaternion.Lerp(myT.rotation, cameraPointT.rotation, rotateTimer/ rotateTime);
            yield return null;
        }
        isStarting = false;
        isWorking = true;
    }

    private void OnDisable()
    {
        PlayerMover.OnSlide -= OnSlide;
        PlayerMover.OnSpeedBoost -= BoostSpeed;
        PlayerMover.OnSpeedBoostStopped -= SpeedBoostStopped;
    }

    private void Update()
    {
        if (isWorking == false) return; 
        actualHeight = defaultHeight;
        MoveAndRotateCamera();
        if((isSpeedBoosted || isSpeedReturning))
        {
            currentFOV = cam.fieldOfView;
            ChangeFOV();
        }
    }

    private void ChangeFOV()
    {
        if(currentFOV != desiredFOV)
        {
            float smooth = isSpeedBoosted ? speedBoostedSmooth : speedReturningSmooth;
            currentFOV = Mathf.Lerp(currentFOV, desiredFOV, smooth * Time.deltaTime + Time.deltaTime);
            cam.fieldOfView = currentFOV;
        }
        if (Mathf.Abs(currentFOV - desiredFOV) < .1f)
        {
            if (isSpeedBoosted)
            {
                isSpeedBoosted = false;
                SpeedBoostStopped(defaultFOV, true);
            }
            else if (isSpeedReturning)
            {
                isSpeedReturning = false;
                cam.fieldOfView = desiredFOV;
            }
        }
    }
    private void BoostSpeed()
    {
        isSpeedBoosted = true;
        isSpeedReturning = false;
        desiredFOV = speedBoostedFOV;
    }

    private void SpeedBoostStopped(float desiredSpeed, bool justStop)
    {
        if (desiredSpeed > defaultSpeed && !justStop) return;
        isSpeedBoosted = false;
        isSpeedReturning = true;
        desiredFOV = defaultFOV;
    }

    private void FixedUpdate()
    {
        if (isWorking == false) return;
        Raycast();
    }
    private void MoveAndRotateCamera()
    {
        if(isStarting)
        {
            myT.rotation = Quaternion.Lerp(myT.rotation, cameraStartT.rotation, rotateSmooth);
        }
        else
        {
            myT.rotation = Quaternion.Lerp(myT.rotation, cameraPointT.rotation, rotateSmooth);
        }
        Vector3 position;
        if (isSliding)
        {
            position = Vector3.Lerp(myT.position, cameSlideT.position, slideSmooth * Time.deltaTime);
        }
        else if(isReturning)
        {
            position = cameraPointT.position;
            position.y = Mathf.Lerp(myT.position.y,(cameraPointT.position + cameraPointT.up * actualHeight).y,slideSmooth * Time.deltaTime);
            if((myT.position - (cameraPointT.position + cameraPointT.up * actualHeight)).magnitude < .15f)
            {
                isReturning = false;
            }
        }
        else
        {
            if(isStarting)
            {
                position = cameraStartT.position;
                position += cameraStartT.up * actualHeight;
            }
            else
            {
                position = cameraPointT.position;
                position += cameraPointT.up * actualHeight;
            }
        }
        myT.position = position;
    }

    private SplineResult GetSplineResult(float percent) => roadSpline.Evaluate(percent);

    private float GetPercent(Vector3 playerPosition) => (float)roadSpline.Project(playerPosition);

    private void Raycast()
    {
        Vector3 camForward = myT.forward;
        camForward.y = 0;
        if (Physics.Raycast(myT.position, camForward, out RaycastHit hit, raycastDistance))
        {
            var barrier = hit.transform.GetComponent<Barrier>();
            if (barrier != null)
            {
                float distance = (myT.position - hit.point).magnitude;
                barrier.SetTransparent(distance / fadeDistance);
            }
        }
    }

    private float DistanceToPercent(float distance)
    {
        float length = roadSpline.CalculateLength();
        return (distance / length);
    }

    private void OnSlide()
    {
        isSliding = true;
        isReturning = false;
        StartCoroutine(WaitAndDoAction(slideTime, () => { isSliding = false; isReturning = true; }));
    }


    private IEnumerator WaitAndDoAction(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    public void InitializePlayerCamera(PlayerCameraData data)
    {
        this.playerT = data.playerT; ;
        this.cameraPointT = data.cameraPositionT;
        this.cameSlideT = data.slidePositionT;
        this.mover = data.mover;
        this.cameraStartT = data.cameraStartT;
        Intitalize();
    }
}
