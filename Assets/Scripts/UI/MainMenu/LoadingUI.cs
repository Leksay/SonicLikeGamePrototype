using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Data.DataScripts;
using UI.MainMenu;
using Random = UnityEngine.Random;
public class LoadingUI : MonoBehaviour
{
	[SerializeField] private GameObject      LoadImage;
	[SerializeField] private Animator        loadAnimator;
	[SerializeField] private Slider          loadSlider;
	[SerializeField] private ScenesContainer _scenesContainer;
	AsyncOperation                           asyncOperation;
	private bool                             loading;

	private string open  = "open";
	private string close = "close";

	private void Start()
	{
		loading                     =  false;
		StartButton.OnButtonPressed += LoadLevel;
	}

	private void OnDestroy()
	{
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
		asyncOperation           =  SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Single);
		asyncOperation.completed += SceneLoaded;
		loading                  =  true;
		yield return null;
	}

	private void SceneLoaded(AsyncOperation operation)
	{
		asyncOperation.completed -= SceneLoaded;
		loading                  =  false;
		loadSlider.value         =  1;
		StartCoroutine(WaitAndAction(1.1f));
	}

	private IEnumerator WaitAndAction(float time)
	{
		yield return new WaitForSeconds(time);
		loadAnimator.SetTrigger("close");
	}
	
	private void Update()
	{
		if (loading)
			loadSlider.value = asyncOperation.progress;
	}
}
