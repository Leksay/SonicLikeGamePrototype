using UnityEngine;

public interface IOpponentMover 
{
    int CurrentRoadID { get;}

    void ChangePath(SwipeInput.SwipeType swipeType);
    void DoJump();
    void DoSlide();
    void SetRoad(int roadId);
    void Initialize(OpponentsData opponentsData, LevelData levelData);
}
