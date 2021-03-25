using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dreamteck.Splines;
using System;
using Players.Camera;

public class FollowArrow : MonoBehaviour
{
	private static float          followDistance = 35f;
	private static Color          transparent    = new Color(0, 0, 0, 0);
	private static Camera         _cam;
	private        RectTransform  _canvas;
	private        Vector3        _center;
	private        bool           _isWorking;
	private        Transform      _followTransform;
	private        RectTransform  _myT;
	private        Transform      _playerT;
	private        Image          _myImage;
	private        SplineComputer _spline;
	private        float          _outOfSightOffset = 20f;
	private        Renderer       _renderer;

	public void Initialize(Transform enemyTransform)
	{
		_center                    =  new Vector3(Screen.width / 2, Screen.height / 2);
		_followTransform           =  enemyTransform;
		_myT                       =  GetComponent<RectTransform>();
		_isWorking                 =  true;
		_playerT                   =  DataHolder.GetCurrentPlayer().transform;
		_myImage                   =  GetComponent<Image>();
		_spline                    =  DataHolder.GetSplineComputer();
		_cam                       =  Camera.main;
		_canvas                    =  GetComponentInParent<Canvas>().GetComponent<RectTransform>();
		_renderer                  =  _followTransform.GetComponentInChildren<Renderer>();
		Finish.OnPlayerCrossFinish += DisableArrow;
	}

	private void DisableArrow(RacerStatus.RacerValues obj)
	{
		_isWorking = false;
		gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		Finish.OnPlayerCrossFinish -= DisableArrow;
	}

	private void FixedUpdate()
	{
		if (_isWorking == false) return;
		var myPercent          = _spline.Project(_playerT.position);
		var followPercent      = _spline.Project(_followTransform.position);
		var indicationPosition = _cam.WorldToScreenPoint(_followTransform.position);
		if (_renderer.isVisible || Mathf.Abs((float)(myPercent - followPercent)) > 0.015f)
		{
			_myImage.color = transparent;
			return;
		}
		if (indicationPosition.z >= 0 & indicationPosition.x <= _canvas.rect.width * _canvas.localScale.x
		    && indicationPosition.y <= _canvas.rect.height * _canvas.localScale.x && indicationPosition.x >= 0f
		    && indicationPosition.y >= 0)
		{
			indicationPosition.z = 0;
		}
		else if (indicationPosition.z >= 0)
		{
			indicationPosition = OutOfRangeIndicatorPositionB(indicationPosition);
			TargetOutOfSight(true, indicationPosition);
		}
		else
		{
			indicationPosition *= -1f;
			indicationPosition =  OutOfRangeIndicatorPositionB(indicationPosition);
			TargetOutOfSight(true, indicationPosition);
		}

		_myT.position = indicationPosition;
	}

	private Vector3 OutOfRangeIndicatorPositionB(Vector3 indicatorPosition)
	{
		indicatorPosition.z = 0;
		var canvasCenter = new Vector3(_canvas.rect.width, _canvas.rect.height) / 2f * _canvas.localScale.x;
		indicatorPosition -= canvasCenter;

		var divX = (_canvas.rect.width  / 2 - _outOfSightOffset) / Mathf.Abs(indicatorPosition.x);
		var divY = (_canvas.rect.height / 2 - _outOfSightOffset) / Mathf.Abs(indicatorPosition.y);

		if (divX < divY)
		{
			var angle = Vector3.SignedAngle(Vector3.right, indicatorPosition, Vector3.forward);
			indicatorPosition.x = Mathf.Sign(indicatorPosition.x)  * (_canvas.rect.width * 0.5f - _outOfSightOffset) * _canvas.localScale.x;
			indicatorPosition.y = Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.x;
		}
		else
		{
			var angle = Vector3.SignedAngle(Vector3.up, indicatorPosition, Vector3.forward);
			indicatorPosition.y = Mathf.Sign(indicatorPosition.y)   * (_canvas.rect.height / 2f - _outOfSightOffset) * _canvas.localScale.y;
			indicatorPosition.x = -Mathf.Tan(Mathf.Deg2Rad * angle) * indicatorPosition.y;
		}

		indicatorPosition += _center;
		return indicatorPosition;

	}

	private void TargetOutOfSight(bool oos, Vector3 indicatorPosition)
	{
		if (oos)
		{
			_myT.right     = _myT.position - _center;
			_myImage.color = Color.white;
		}
		else
		{
			_myImage.color = transparent;
		}
	}
}
