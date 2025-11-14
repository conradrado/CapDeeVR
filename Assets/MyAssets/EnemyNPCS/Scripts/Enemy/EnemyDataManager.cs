using UnityEngine;

public class EnemyDataManager : MonoBehaviour {
    public EnemyData _enemyData;

    void Start()
    {
        GetEnemyData();
    }
    
    void GetEnemyData()
    {
        print($"Enemy Name : {_enemyData.Name}" +
            $"\nEnemy Description : {_enemyData.Description}" +
            $"\nEnemy MaxHP : {_enemyData.MaxHP}" +
            $"\nEnemy Damage : {_enemyData.Damage}");
    }

}