using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dreamteck.Splines;
using System;

[RequireComponent(typeof(Camera))]
public class PlayerFollowCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraPointT;
    [SerializeField] private Transform cameSlideT;
    [SerializeField] private Transform playerT;
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
    private Camera cam;
    private float defaultFOV;
    private float currentFOV;
    private float desiredFOV;
    private bool isSpeedBoosted;
    private bool isSpeedReturning;

    private float slideTime = .8f;

    private float actualHeight;
    private Transform myT;

    private void Start()
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
        actualHeight = defaultHeight;
        defaultFOV = cam.fieldOfView;
    }

    private void OnDisable()
    {
        PlayerMover.OnSlide -= OnSlide;
        PlayerMover.OnSpeedBoost -= BoostSpeed;
        PlayerMover.OnSpeedBoostStopped -= SpeedBoostStopped;
    }

    private void Update()
    {
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
            cam.fieldOfView = Mathf.Lerp(currentFOV, desiredFOV, speedBoostedSmooth * Time.deltaTime);
            if(Mathf.Abs(currentFOV - desiredFOV) < .1f)
            {
                isSpeedBoosted = false;
                isSpeedReturning = false;
                cam.fieldOfView = desiredFOV;
            }
        }
    }

    private void FixedUpdate()
    {
        Raycast();
    }
    private void MoveAndRotateCamera()
    {
        myT.rotation = Quaternion.Lerp(myT.rotation, cameraPointT.rotation, rotateSmooth);
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
            position = cameraPointT.position;
            position += cameraPointT.up * actualHeight;
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

    private void BoostSpeed()
    {
        isSpeedBoosted = true;
        isSpeedReturning = false;
        desiredFOV = speedBoostedFOV;
    }

    private void SpeedBoostStopped()
    {
        isSpeedBoosted = false;
        isSpeedReturning = true;
        desiredFOV = defaultFOV;
    }

    private IEnumerator WaitAndDoAction(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
}
