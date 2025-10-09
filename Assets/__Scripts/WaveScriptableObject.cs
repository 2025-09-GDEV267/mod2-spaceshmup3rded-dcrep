using UnityEngine;


[CreateAssetMenu(fileName = "WaveScriptableObject", menuName = "Scriptable Objects/WaveScriptableObject")]
public class WaveScriptableObject : ScriptableObject
{
    [System.Serializable]
    public class WaveData
    {
        public GameObject prefabEnemy;
        public int offsetXFromLastEnemy = 0;
        public float nextEnemySpawnDelay = 0.5f;
    }
    public WaveData[] waves;
    public float nextWaveDelay = 2f;
}
