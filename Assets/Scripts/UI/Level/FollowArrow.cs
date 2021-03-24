using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamteck.Splines;
using System;
using Players.Camera;

public class FollowArrow : MonoBehaviour
{
    private static float followDistance = 35f;
    private static Color transparent = new Color(0, 0, 0, 0);
    private static Camera cam;
    private static Transform camT;
    private RectTransform canvas;
    private Vector3 center;
    private bool isWorking;
    private Transform followTransform;
    private RectTransform myT;
    private Transform playerT;
    private Image myImage;
    private SplineComputer spline;
    private float outOfSightOffset = 20f;
    private Renderer renderer;

    public void Initialize(Transform enemyTransform)
    {
        center = new Vector3(Screen.width / 2, Screen.height / 2);
        followTransform = enemyTransform;
        myT = GetComponent<RectTransform>();
        isWorking = true;
        playerT = DataHolder.GetCurrentPlayer().transform;
        myImage = GetComponent<Image>();
        spline = DataHolder.GetSplineComputer();
        //cam = PlayerFollowCamera.followCamera;
        //camT = PlayerFollowCamera.followCamera.transform;
        canvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        renderer = followTransform.GetComponentInChildren<Renderer>();
        Finish.OnPlayerCrossFinish += DisableArrow;
    }

    private void DisableArrow(RacerStatus.RacerValues obj)
    {
        isWorking = false;
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        Finish.OnPlayerCrossFinish -= DisableArrow;
    }

    private void FixedUpdate()
    {
        if (isWorking == false) return;
        var myPercent = spline.Project(playerT.position);
        var followPercent = spline.Project(followTransform.position);
        Vector3 indicationPosition = cam.WorldToScreenPoint(followTransform.position);
        var viewportPoint = cam.WorldToViewportPoint(followTransform.position);
        if (renderer.isVisible || Mathf.Abs((float)(myPercent - followPercent)) > 0.015f)
        {
            myImage.color = transparent;
            return;
        }
        if (indicationPosition.z >= 0 & indicationPosition.x <= canvas.rect.width * canvas.localScale.x 
            && indicationPosition.y <= canvas.rect.height * canvas.localScale.x && indicationPosition.x >= 0f
            && indicationPosition.y >= 0)
        {
            indicationPosition.z = 0;
            //targetOutOfSight(false,indicationPosition);
        }
        else if(indicationPosition.z >= 0)
        {
            indicationPosition = OutOfRangeIndicatorPositionB(indicationPosition);
            targetOutOfSight(true, indicationPosition);
        }
        else
        {
            indicationPosition *= -1f;
            indicationPosition = OutOfRangeIndicatorPositionB(indicationPosition);
            targetOutOfSight(true, indicationPosition);
        }

        myT.position = indicationPosition;
    }

    private Vector3 OutOfRangeIndicatorPositionB(Vector3 indicatorPosition)
    {
        indicatorPosition.z = 0;
        Vector3 canvasCenter = new Vector3(canvas.rect.width, canvas.rect.height) / 2f * canvas.localScale.x;
        indicatorPosition -= canvasCenter;

        float divX = (canvas.rect.width / 2 - outOfSightOffset) / Mathf.Abs(indicatorPosition.x);
        float divY = (canvas.rect.height / 2 - outOfSightOffset) / Mathf.Abs(indicatorPosition.y);

        if(divX < divY)
        {
            float angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
            indicatorPosition.x = Mathf.Sign(indicatorPosition.x) * (canvas.rect.width * 0.5f - outOfSightOffset) * canvas.localScale.x;
            indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;

        }
        else
        {
            float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);
            indicatorPosition.y = Mathf.Sign(indicatorPosition.y) * (canvas.rect.height / 2f - outOfSightOffset) * canvas.localScale.y;
            indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;

        }

        indicatorPosition += center;
        return indicatorPosition;

    }

    private void targetOutOfSight(bool oos, Vector3 indicatorPosition)
    {
        if(oos)
        {
            myT.right = myT.position - center;
            myImage.color = Color.white;
        }
        else
        {
            myImage.color = transparent;
        }
    }

    //private Vector3  rootationOutOfSightTargetindicator(Vector3 indicatorPosition)
    //{
    //    Vector3 canvasCenter = new Vector3(canvas.rect.width / 2f, canvas.rect.height / 2) * canvas.localScale.x;

    //    float angle = Vector3.SignedAngle(Vector3.up, indicatorPosition - canvasCenter, Vector3.back);
    //    return new Vector3(0, 0, angle);
    //}
}
