[System.Serializable]
public class RoadEntityData 
{
    public EntityType entityType;
    public EnemyType enemyType;
    public BarrierType barrierType;

    public float percentAtRoad;
    public float height;
    public float offset;
    public int roadCount;

    public int desiredRoad = -1;
}
