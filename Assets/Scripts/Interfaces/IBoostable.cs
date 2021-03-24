public interface IBoostable 
{
    void BoostSpeed(float time, float value);
    void StopAllBoosters();
    void ShieldBoost(float time);
    void StopShield();
    void MagnetFieldBoost(float time);
    void StopMagnetField();
}
