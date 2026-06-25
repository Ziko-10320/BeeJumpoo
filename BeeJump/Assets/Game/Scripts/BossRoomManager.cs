using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossRoomManager : MonoBehaviour
{
    [Header("Timer")]
    public float totalTime = 90f;

    [Header("Camera")]
    public Camera mainCamera;
    public Transform cameraLockedPosition;

    [Header("Door")]
    public GameObject door;

    [Header("Player")]
    public Transform playerSpawnPoint;
    public PlayerController player;

    [Header("Phase 1 (0-20s)")]
    public float phase1Duration = 20f;
    public int phase1WaypointCount = 2;
    public float phase1EnemySpeed = 3f;
    public float phase1SpawnInterval = 8f;

    [Header("Phase 2 (20-60s)")]
    public float phase2Duration = 40f;
    public int phase2WaypointCount = 3;
    public float phase2EnemySpeed = 5f;
    public float phase2SpawnInterval = 6f;
    public int phase2SpikeCount = 2;
    public float phase2SpikeInterval = 5f;

    [Header("Phase 3 (60-90s)")]
    public int phase3WaypointCount = 5;
    public float phase3EnemySpeed = 8f;
    public float phase3SpawnInterval = 4f;
    public int phase3SpikeCount = 4;
    public float phase3SpikeInterval = 3f;
    public int phase3ShooterCount = 2;
    public float phase3ShooterInterval = 6f;
    [Header("Shooter Spawn Positions")]
    public Transform[] shooterSpawnPoints;
    [Header("Prefabs")]
    public GameObject waypointEnemyPrefab;
    public GameObject shooterEnemyPrefab;
    public GameObject spikePrefab;

    [Header("Spawn Zones")]
    public Transform leftSpawnPoint;     // off screen left
    public Transform rightSpawnPoint;    // off screen right
    public Transform[] spikeSpawnZones;  // ceiling/wall/floor positions
    public bool bossActive = false;
    public float timer = 0f;
    // ?? private ???????????????????????????????????????????????????????????????
   
    private int currentPhase = 0;
  
    private List<GameObject> activeEnemies = new List<GameObject>();
    private CameraFollow cameraFollow;

    void Start()
    {
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }

    public void StartBossFight()
    {
        bossActive = true;
        timer = 0f;
        currentPhase = 0;

        // Lock camera
        if (cameraFollow != null)
            cameraFollow.enabled = false;

        StartCoroutine(SmoothCameraToPosition(cameraLockedPosition.position));

        // Close door
        if (door != null)
            door.SetActive(true);

        StartCoroutine(BossLoop());
    }
    IEnumerator SmoothCameraToPosition(Vector3 targetPos)
    {
        Vector3 target = new Vector3(targetPos.x, targetPos.y, -10f);
        float elapsed = 0f;
        float duration = 1.5f;  // expose this if u want inspector control
        Vector3 startPos = mainCamera.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease in out so it feels smooth not robotic
            t = t * t * (3f - 2f * t);
            mainCamera.transform.position = Vector3.Lerp(startPos, target, t);
            yield return null;
        }

        mainCamera.transform.position = target;
    }
    IEnumerator BossLoop()
    {
        StartCoroutine(TrackTimer());
        // Phase 1
        currentPhase = 1;
        Coroutine p1 = StartCoroutine(SpawnWaypointEnemiesLoop(
            phase1WaypointCount, phase1EnemySpeed, phase1SpawnInterval));

        yield return new WaitForSeconds(phase1Duration);
        StopCoroutine(p1);

        // Phase 2
        currentPhase = 2;
        Coroutine p2w = StartCoroutine(SpawnWaypointEnemiesLoop(
            phase2WaypointCount, phase2EnemySpeed, phase2SpawnInterval));
        Coroutine p2s = StartCoroutine(SpawnSpikesLoop(
            phase2SpikeCount, phase2SpikeInterval));

        yield return new WaitForSeconds(phase2Duration);
        StopCoroutine(p2w);
        StopCoroutine(p2s);

        // Phase 3
        currentPhase = 3;
        Coroutine p3w = StartCoroutine(SpawnWaypointEnemiesLoop(
            phase3WaypointCount, phase3EnemySpeed, phase3SpawnInterval));
        Coroutine p3s = StartCoroutine(SpawnSpikesLoop(
            phase3SpikeCount, phase3SpikeInterval));
        Coroutine p3sh = StartCoroutine(SpawnShootersLoop(
            phase3ShooterCount, phase3ShooterInterval));

        yield return new WaitForSeconds(totalTime - phase1Duration - phase2Duration);
        StopCoroutine(p3w);
        StopCoroutine(p3s);
        StopCoroutine(p3sh);

        BossWin();
    }
    IEnumerator TrackTimer()
    {
        timer = 0f;
        while (bossActive)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
    IEnumerator SpawnWaypointEnemiesLoop(int count, float speed, float interval)
    {
        while (bossActive)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnWaypointEnemy(speed);
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator SpawnSpikesLoop(int count, float interval)
    {
        while (bossActive)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnSpike();
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator SpawnShootersLoop(int count, float interval)
    {
        while (bossActive)
        {
            for (int i = 0; i < count; i++)
            {
                SpawnShooter();
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnWaypointEnemy(float speed)
    {
        if (waypointEnemyPrefab == null) return;

        // Pick a random side to enter from
        bool fromLeft = Random.value > 0.5f;
        Transform spawnPoint = fromLeft ? leftSpawnPoint : rightSpawnPoint;

        GameObject enemy = Instantiate(waypointEnemyPrefab, spawnPoint.position, Quaternion.identity);
        BossWaypointEnemy bwe = enemy.GetComponent<BossWaypointEnemy>();

        if (bwe != null)
            bwe.Init(speed, fromLeft, GetRoomBounds());

        activeEnemies.Add(enemy);
    }

    void SpawnSpike()
    {
        if (spikePrefab == null || spikeSpawnZones.Length == 0) return;

        // Pick random spawn zone
        Transform zone = spikeSpawnZones[Random.Range(0, spikeSpawnZones.Length)];
        GameObject spike = Instantiate(spikePrefab, zone.position, zone.rotation);
        activeEnemies.Add(spike);
    }

    void SpawnShooter()
    {
        if (shooterEnemyPrefab == null || shooterSpawnPoints.Length == 0) return;

        Transform spawnPoint = shooterSpawnPoints[
            Random.Range(0, shooterSpawnPoints.Length)];

        GameObject shooter = Instantiate(
            shooterEnemyPrefab, spawnPoint.position, Quaternion.identity);

        // Read direction from the spawn point itself
        ShooterSpawnPoint sp = spawnPoint.GetComponent<ShooterSpawnPoint>();
        EnemyShooter es = shooter.GetComponent<EnemyShooter>();

        if (sp != null && es != null)
            es.shootDirection = sp.shootDirection;

        activeEnemies.Add(shooter);
    }

    // Returns room bounds for enemy path generation
    public Bounds GetRoomBounds()
    {
        return new Bounds(transform.position, transform.localScale);
    }

   

    void BossWin()
    {
        bossActive = false;
        StopAllCoroutines();

        // Clear all active enemies
        foreach (GameObject e in activeEnemies)
            if (e != null) Destroy(e);
        activeEnemies.Clear();

        // Unlock camera
        if (cameraFollow != null)
            cameraFollow.enabled = true;

        // Open door
        if (door != null)
            door.SetActive(false);

        Debug.Log("BOSS: Player survived! Boss fight complete.");
    }

    public void PlayerDied()
    {
        if (!bossActive) return;

        StopAllCoroutines();

        // Clear enemies
        foreach (GameObject e in activeEnemies)
            if (e != null) Destroy(e);
        activeEnemies.Clear();

        // Respawn player
        player.transform.position = playerSpawnPoint.position;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // Restart
        StartBossFight();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}