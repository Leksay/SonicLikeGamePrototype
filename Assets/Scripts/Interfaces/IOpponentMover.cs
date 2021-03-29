using Data.DataScripts;
using UnityEngine;

public interface IOpponentMover
{
	int CurrentRoadID { get; }

	void ChangePath(SwipeInput.SwipeType swipeType);
	void DoJump();
	void DoSlide();
	void SetRoad(int              roadId,        double    position = 0d);
	void Initialize(OpponentsData opponentsData, LevelData levelData);
}
