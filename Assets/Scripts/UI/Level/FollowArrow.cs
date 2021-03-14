using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamteck.Splines;
public class FollowArrow : MonoBehaviour
{
    private static float followDistance = 35f;
    private static Color transparent = new Color(0, 0, 0, 0);
    private static RectTransform canvas;
    private static Camera cam;
    private static Transform camT;
    private Vector3 center;
    private bool isWorking;
    private Transform followTransform;
    private RectTransform myT;
    private Transform playerT;
    private Image myImage;
    private SplineComputer spline;
    public void Initialize(Transform enemyTransform)
    {
        center = new Vector3(Screen.width / 2, Screen.height / 2);
        followTransform = enemyTransform;
        myT = GetComponent<RectTransform>();
        isWorking = true;
        playerT = DataHolder.GetCurrentPlayer().transform;
        myImage = GetComponent<Image>();
        spline = DataHolder.GetSplineComputer();
        cam = PlayerFollowCamera.followCamera;
        camT = PlayerFollowCamera.followCamera.transform;
        canvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    private void FixedUpdate()
    {
        if (isWorking == false) return;

        //var toPosition = followTransform.position;
        //var screenPos = cam.WorldToScreenPoint(followTransform.position);
        //var direction = camT.position - followTransform.position;
        //var screenPos = cam.WorldToScreenPoint(toPosition);
        //screenPos.x = Mathf.Clamp(screenPos.x, 100, Screen.width - 100);
        //screenPos.y = Mathf.Clamp(screenPos.y, 100, Screen.height - 100);
        //myT.position = screenPos;
        //myT.right = screenPos - center;
        //myT.position = screenPos;

        var screenPos = cam.WorldToScreenPoint(followTransform.position);
        var v = (screenPos - center);
        screenPos.x = Mathf.Clamp(screenPos.x, 100, Screen.width - 100);
        screenPos.y = Mathf.Clamp(screenPos.y, 100, Screen.height - 100);
        //Vector2 playerPos = new Vector2(playerT.position.x, playerT.position.z);
        //Vector2 followPos = new Vector2(followTransform.position.x, followTransform.position.z);

        //Vector2 direction = (playerPos - followPos).normalized;
        //print(direction);
        //float arrowXPos = Mathf.Clamp((direction * 1000f).x, canvas.sizeDelta.x / -2, canvas.sizeDelta.x / 2);
        //float arrowYPos = Mathf.Clamp((direction * 1000f).y, canvas.sizeDelta.y / -2, canvas.sizeDelta.y / 2);

        myT.position = screenPos;
        myT.right = v;
    }
}
