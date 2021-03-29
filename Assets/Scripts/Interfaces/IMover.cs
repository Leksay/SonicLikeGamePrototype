public interface IMover 
{
    void AddSpeed(float speed);
    void ReduceSpeed(float speed);
    void SetStartRoad(int roadId, double position = 0d);
    float GetPercent();
}
