public interface IBoostable 
{
    void BoostSpeed(float time, float value);
    void StopAllBoosters();
    void ShildBoost(float time);
    void StopShild();
    void MagnetFieldBoost(float time);
    void StopMagnetField();
}
