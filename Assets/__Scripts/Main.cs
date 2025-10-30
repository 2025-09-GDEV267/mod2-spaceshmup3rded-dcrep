using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;   // Enables the loading & reloading of scenes

[RequireComponent(typeof(BoundsCheck))]

[System.Serializable]
public class EnemyWave
{
    [System.Serializable]
    public class WaveData
    {
        public string enemyName;
        public int offsetXFromLastEnemy = 0;
        public float nextEnemySpawnDelay = 0.5f;
    }
    public List<WaveData> waves = new();
    public float nextWaveDelay = 2f;
}
[System.Serializable]
public class EnemyWaves
{
    public List<EnemyWave> enemies = new();
}


public class Main : MonoBehaviour
{
    static private Main S;                        // A private singleton for Main
    static private Dictionary<eWeaponType, WeaponDefinition> WEAP_DICT;


    [Header("Inscribed")]
    public bool spawnEnemies = true;

    public WaveScriptableObject[] enemyWavesSO;
    private int currentWaveIndex = 0;
    private int currentEnemyInWave = 0;
    private Vector3 waveStartPos;

    public GameObject[] prefabEnemies;
    public Dictionary<string, GameObject> prefabEnemyLookup;
    public EnemyWaves enemyWaves;

    public TextAsset enemyWaveJSONFile;

    public GameObject[] prefabRandomizedEnemies;               // Array of Enemy prefabs
    public float enemySpawnPerSecond = 0.5f;  // # Enemies spawned/second
    public float enemyInsetDefault = 1.5f;    // Inset from the sides
    public float gameRestartDelay = 2.0f;
    public GameObject prefabPowerUp;
    public WeaponDefinition[] weaponDefinitions;
    public eWeaponType[] powerUpFrequency = new eWeaponType[] {
                                     eWeaponType.blaster, eWeaponType.blaster,
                                     eWeaponType.spread,  eWeaponType.shield };
    private BoundsCheck bndCheck;

    void Awake()
    {
        S = this;
        // Set bndCheck to reference the BoundsCheck component on this 
        // GameObject
        bndCheck = GetComponent<BoundsCheck>();

        // Invoke SpawnEnemy() once (in 2 seconds, based on default values)
        //Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);                // a
        //Invoke(nameof(SpawnWave), 1f);
        //SpawnWave(1f);

        // A generic Dictionary with eWeaponType as the key
        WEAP_DICT = new Dictionary<eWeaponType, WeaponDefinition>();          // a
        foreach (WeaponDefinition def in weaponDefinitions)
        {
            WEAP_DICT[def.type] = def;
        }

        // Load waves from json
        enemyWaves = JsonUtility.FromJson<EnemyWaves>(enemyWaveJSONFile.text);

        //prefabEnemies.ToDictionary<int, GameObject>(prefabEnemyLookup)
        prefabEnemyLookup = new();
        
        for (int i = 0; i < prefabEnemies.Length; i++)
        {
            prefabEnemyLookup.Add("Enemy_" + i, prefabEnemies[i]);
        }
    }

    void Start()
    {
        //SpawnWaveSO(1f);
        SpawnWaveJSON(1f);
    }

    public void SpawnWaveSO(float waveBeginDelay)
    {
        if (currentWaveIndex >= enemyWavesSO.Length)
        {
            if (enemyWavesSO.Length < 1)
                return;
            currentWaveIndex = 0;
        }
        Debug.Log("Spawning Wave #" + currentWaveIndex + 1);
        currentEnemyInWave = 0;

        // Set the initial position for the spawning Enemies
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + 6;
        float xMax = bndCheck.camWidth - 6;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight;

        waveStartPos = pos;

        Invoke(nameof(WaveNextEnemySO), waveBeginDelay);
    }

    public void SpawnWaveJSON(float waveBeginDelay)
    {
        if (currentWaveIndex >= enemyWaves.enemies.Count)
        {
            if (enemyWaves.enemies.Count < 1)
                return;
            currentWaveIndex = 0;
        }
        Debug.Log("Spawning Wave #" + currentWaveIndex + 1);        
        currentEnemyInWave = 0;
        //EnemyWave currentWave = enemyWaves.enemies[currentWaveIndex];
        //Debug.Log("Dictionary lookup: " + prefabEnemyLookup[currentWave.waves[0].enemyName].name);

        // Set the initial position for the spawning Enemies
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + 6;
        float xMax = bndCheck.camWidth - 6;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight;

        waveStartPos = pos;

        Invoke(nameof(WaveNextEnemyJSON), waveBeginDelay);
    }

    private void WaveNextEnemySO()
    {
        GameObject go = Instantiate<GameObject>(enemyWavesSO[currentWaveIndex].waves[currentEnemyInWave].prefabEnemy);

        // Position the Enemy above the screen with a random x position
        float enemyInset = enemyInsetDefault;                                // d
        //if (go.GetComponent<BoundsCheck>() != null)
        //{                        // e
        //    enemyInset = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        //}
        waveStartPos.x += enemyWavesSO[currentWaveIndex].waves[currentEnemyInWave].offsetXFromLastEnemy;
        go.transform.position = waveStartPos;

        float nextEnemySpawnDelay = enemyWavesSO[currentWaveIndex].waves[currentEnemyInWave].nextEnemySpawnDelay;
        currentEnemyInWave++;
        if (currentEnemyInWave >= enemyWavesSO[currentWaveIndex].waves.Length)
        {
            currentWaveIndex++;
            // timeBetweenWaves
            SpawnWaveSO(enemyWavesSO[currentWaveIndex - 1].nextWaveDelay);
            return;
        }
        //else
        Invoke(nameof(WaveNextEnemySO), nextEnemySpawnDelay);
    }
    //...
    private void WaveNextEnemyJSON()
    {
        //GameObject go = Instantiate<GameObject>(enemyWavesSO[currentWaveIndex].waves[currentEnemyInWave].prefabEnemy);
        GameObject prefab = prefabEnemyLookup[enemyWaves.enemies[currentWaveIndex].waves[currentEnemyInWave].enemyName];
        GameObject go = Instantiate<GameObject>(prefab);

        // Position the Enemy above the screen with a random x position
        float enemyInset = enemyInsetDefault;                                // d
        //if (go.GetComponent<BoundsCheck>() != null)
        //{                        // e
        //    enemyInset = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        //}
        waveStartPos.x += enemyWaves.enemies[currentWaveIndex].waves[currentEnemyInWave].offsetXFromLastEnemy;
        go.transform.position = waveStartPos;

        float nextEnemySpawnDelay = enemyWaves.enemies[currentWaveIndex].waves[currentEnemyInWave].nextEnemySpawnDelay;
        currentEnemyInWave++;
        if (currentEnemyInWave >= enemyWaves.enemies[currentWaveIndex].waves.Count)
        {
            currentWaveIndex++;
            // timeBetweenWaves
            SpawnWaveJSON(enemyWaves.enemies[currentWaveIndex - 1].nextWaveDelay);
            return;
        }
        //else
        Invoke(nameof(WaveNextEnemyJSON), nextEnemySpawnDelay);
    }

    public void SpawnEnemy()
    {
        // If spawnEnemies is false, skip to the next invoke of SpawnEnemy()
        if (!spawnEnemies)
        {                                                // c
            Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);
            return;
        }

        // Pick a random Enemy prefab to instantiate
        int ndx = Random.Range(0, prefabRandomizedEnemies.Length);                     // b
        GameObject go = Instantiate<GameObject>(prefabRandomizedEnemies[ndx]);     // c

        // Position the Enemy above the screen with a random x position
        float enemyInset = enemyInsetDefault;                                // d
        if (go.GetComponent<BoundsCheck>() != null)
        {                        // e
            enemyInset = Mathf.Abs(go.GetComponent<BoundsCheck>().radius);
        }

        // Set the initial position for the spawned Enemy                    // f
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyInset;
        float xMax = bndCheck.camWidth - enemyInset;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyInset;
        go.transform.position = pos;
        // Invoke SpawnEnemy() again
        Invoke(nameof(SpawnEnemy), 1f / enemySpawnPerSecond);                // g
    }

    void DelayedRestart()
    {                                                   // c
                                                        // Invoke the Restart() method in gameRestartDelay seconds
        Invoke(nameof(Restart), gameRestartDelay);
    }

    void Restart()
    {
        // Reload __Scene_0 to restart the game
        // "__Scene_0" below starts with 2 underscores and ends with a zero.
        SceneManager.LoadScene("__Scene_0");                               // d
    }

    static public void HERO_DIED()
    {
        S.DelayedRestart();                                                  // b
    }

    /// <summary>
    /// Static function that gets a WeaponDefinition from the WEAP_DICT static
    ///   protected field of the Main class.
    /// </summary>
    /// <returns>The WeaponDefinition, or if there is no WeaponDefinition with
    ///   the eWeaponType passed in, returns a new WeaponDefinition with a 
    ///   eWeaponType of eWeaponType.none.</returns>
    /// <param name="wt">The eWeaponType of the desired WeaponDefinition</param>
    static public WeaponDefinition GET_WEAPON_DEFINITION(eWeaponType wt)
    {  // a
        if (WEAP_DICT.ContainsKey(wt))
        {                                      // b
            return (WEAP_DICT[wt]);
        }
        // If no entry of the correct type exists in WEAP_DICT, return a new 
        //   WeaponDefinition with a type of eWeaponType.none (the default value)
        return (new WeaponDefinition());                                     // c
    }

    /// <summary>
    /// Called by an Enemy ship whenever it is destroyed. It sometimes creates
    ///   a PowerUp in place of the destroyed ship.
    /// </summary>
    /// <param name="e"The Enemy that was destroyed</param
    static public void SHIP_DESTROYED(Enemy e)
    {
        // Potentially generate a PowerUp
        if (Random.value <= e.powerUpDropChance)
        { // Underlined red for now  // c
          // Choose a PowerUp from the possibilities in powerUpFrequency
            int ndx = Random.Range(0, S.powerUpFrequency.Length);           // d
            eWeaponType pUpType = S.powerUpFrequency[ndx];

            // Spawn a PowerUp
            GameObject go = Instantiate<GameObject>(S.prefabPowerUp);
            PowerUp pUp = go.GetComponent<PowerUp>();
            // Set it to the proper WeaponType
            pUp.SetType(pUpType);                                           // e

            // Set it to the position of the destroyed ship
            pUp.transform.position = e.transform.position;
        }
    }

}
