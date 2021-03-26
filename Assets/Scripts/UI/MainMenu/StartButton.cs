using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace UI.MainMenu
{
	public class StartButton : MonoBehaviour, IPointerUpHandler
	{
		public static event Action        OnButtonPressed;
		[SerializeField] private Animator startButtonAnimator;
		[SerializeField] private TMP_Text text;

		private bool  _isActive;
		private Image _image;
		private Color _activeColor;
		private Color _inactiveColor;
		private bool  _isSelected;
		private void Start()
		{
			startButtonAnimator = GetComponent<Animator>();
			_image              = GetComponent<Image>();
			_activeColor        = _image.color;
			_inactiveColor      = _activeColor / 2;
			_isActive           = true;
		}

		private void OnEnable()
		{
			SkinnController.OnChangeActive += SetActive;
		}

		private void OnDisable()
		{
			SkinnController.OnChangeActive -= SetActive;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (!_isActive) return;
			if (_isSelected == false)
			{
				_isSelected = true;
				OnButtonPressed?.Invoke();
				startButtonAnimator.SetTrigger("start");
			}
		}

		private IEnumerator WaitAndLoad(float time, int scene)
		{
			yield return new WaitForSeconds(time);
			SceneManager.LoadSceneAsync(scene);
		}

		private void SetActive(bool active)
		{
			_isActive    = active;
			_image.color = _isActive ? _activeColor : _inactiveColor;
		}
	}
}
