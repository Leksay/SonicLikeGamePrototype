using System.Collections;
using Data.DataScripts;
using Internal;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;
namespace UI.MainMenu
{
	public class LoadingUI : MonoBehaviour, IRegistrable
	{
		[SerializeField] private GameObject      LoadImage;
		[SerializeField] private Animator        loadAnimator;
		[SerializeField] private Slider          loadSlider;
		[SerializeField] private ScenesContainer _scenesContainer;
		AsyncOperation                           _asyncOperation;

		private string open  = "open";
		private string close = "close";

		private void Awake()
		{
			var lui = Locator.GetObject<LoadingUI>();
			if (lui != null)
			{
				Destroy(gameObject);
				return;
			}
			Register();
			StartButton.OnButtonPressed += LoadLevel;
		}
		private void OnDestroy()
		{
			Unregister();
			StartButton.OnButtonPressed -= LoadLevel;
		}

		private void LoadLevel()
		{
			if (PlayerDataHolder.GetTutorial() == 0)
			{
				StartCoroutine(WaitAndLoad(.2f, _scenesContainer.scenes[0]));
			}
			else
			{
				var levelToLoad = Random.Range(0, _scenesContainer.scenes.Length);
				var levelId     = _scenesContainer.scenes[levelToLoad];
				if (_scenesContainer.scenes.Length > 1)
					for (var i = 0; i < 10; i++)
						if (levelToLoad == PreviousLevelData.previousLevel)
						{
							levelToLoad = Random.Range(0, _scenesContainer.scenes.Length);
							levelId     = _scenesContainer.scenes[levelToLoad];
						}
						else break;
				LoadImage.SetActive(true);
				loadAnimator.SetTrigger("open");
				PreviousLevelData.previousLevel = levelToLoad;
				StartCoroutine(WaitAndLoad(.5f, levelId));
			}
		}

		private IEnumerator WaitAndLoad(float time, string sceneId)
		{
			yield return new WaitForSeconds(time);
			_asyncOperation           =  SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Single);
			_asyncOperation.completed += SceneLoaded;
			yield return null;
		}

		private void SceneLoaded(AsyncOperation operation)
		{
			_asyncOperation.completed -= SceneLoaded;
			_asyncOperation           =  null;
			loadSlider.value         =  1;
			StartCoroutine(WaitAndAction(1.1f));
		}

		private IEnumerator WaitAndAction(float time)
		{
			yield return new WaitForSeconds(time);
			loadAnimator.SetTrigger("close");
			yield return new WaitForSeconds(time);
			PauseController.Resume();
		}

		private void Update()
		{
			if (_asyncOperation != null)
				loadSlider.value = _asyncOperation.progress;
		}
		public void Register()   => Locator.Register(typeof(LoadingUI), this);
		public void Unregister() => Locator.Unregister(typeof(LoadingUI));
	}
}
