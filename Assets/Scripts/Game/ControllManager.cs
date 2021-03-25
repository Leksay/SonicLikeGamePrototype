using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllManager : MonoBehaviour
{
	public static ControllManager           Instance { get; private set; }
	private       List<IPlayerControllable> _controllables = new List<IPlayerControllable>();
	private void Awake()
	{
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);
	}

	public void GiveControl() => _controllables.ForEach(c => c.StartPlayerControl());

	public void RemoveControl() => _controllables.ForEach(c => c.StopPlayerControl());

	public void RegisterControllable(IPlayerControllable controllable) => _controllables.Add(controllable);

	public void Destroy() => Destroy(gameObject);
}
