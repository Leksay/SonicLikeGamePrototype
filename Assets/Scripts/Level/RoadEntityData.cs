[System.Serializable]
public class RoadEntityData
{
	public EntityType  entityType;
	public EnemyType   enemyType;
	public BarrierType barrierType;

	public float percentAtRoad;
	public float height;
	public float offset;
	public int   roadCount;

	public int desiredRoad;

	public static RoadEntityData CreateCoin(float          percent, float height,  int   road)             => new RoadEntityData(EntityType.Coins,   EnemyType.Ground, BarrierType.Flying_SinglePath, percent, height, 0, road, road);
	public static RoadEntityData CreateEnemy(EnemyType     type,    float percent, float height, int road) => new RoadEntityData(EntityType.Enemy,   type,             BarrierType.Flying_SinglePath, percent, height, 0, road, road);
	public static RoadEntityData CreateBarrier(BarrierType type,    float percent, float height, int road) => new RoadEntityData(EntityType.Barrier, EnemyType.Ground, type,                          percent, height, 0, road, road);
	public RoadEntityData()
	{
		desiredRoad = -1;
	}
	public RoadEntityData(EntityType entityType, EnemyType enemyType, BarrierType barrierType, float percentAtRoad, float height, float offset, int roadCount, int desiredRoad)
	{
		this.entityType    = entityType;
		this.enemyType     = enemyType;
		this.barrierType   = barrierType;
		this.percentAtRoad = percentAtRoad;
		this.height        = height;
		this.offset        = offset;
		this.roadCount     = roadCount;
		this.desiredRoad   = desiredRoad;
	}
}
