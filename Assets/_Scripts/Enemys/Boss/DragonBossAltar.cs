using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DragonBossAltar : MonoBehaviour
{
    [Header("Boss - referencias internas del prefab")]
    [SerializeField] private GameObject bossPrefab;

    [Tooltip("Hijo del prefab del altar. Punto donde se instancia inicialmente el dragon.")]
    [SerializeField] private Transform bossSpawnPoint;

    [Tooltip("Hijo del prefab del altar. Punto de vuelo usado durante Fase 1.")]
    [SerializeField] private Transform bossFlightAnchor;

    [Tooltip("Hijo del prefab del altar. Punto de tierra para Fase 2 y 3.")]
    [SerializeField] private Transform bossGroundAnchor;

    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Punto desde el que se mide el rango. Si queda null usa el transform del altar.")]
    [SerializeField] private Transform interactionPoint;

    [SerializeField, Min(0.5f)] private float interactionRadius = 3f;
    [SerializeField, Min(0f)] private float activationDelay = 0.5f;

    [Tooltip("Opcional. GameObject UI con texto tipo 'E - Invocar Dragon'.")]
    [SerializeField] private GameObject interactionPrompt;

    [Header("Difficulty")]
    [Tooltip("Opcional. Si queda null, el altar lo busca automaticamente al ser instanciado.")]
    [SerializeField] private WaveSpawner3D waveSpawner;
    [SerializeField] private bool useWaveDifficulty = true;
    [SerializeField, Min(0f)] private float fallbackDifficulty = 0f;

    [Header("Hordes during boss")]
    [Tooltip("Las hordas NO se detienen. Este valor sólo cambia su frecuencia.")]
    [SerializeField] private bool reduceHordesOnBoss = true;

    [Tooltip("1 = igual que antes. 0.5 = aproximadamente mitad de frecuencia.")]
    [SerializeField, Range(0.1f, 1f)]
    private float bossHordeSpawnRateMultiplier = 0.5f;

    [Header("Feedback")]
    [SerializeField] private GameObject activationVFX;
    [SerializeField] private Transform activationVFXPoint;
    [SerializeField, Min(0f)] private float activationVFXLifetime = 3f;

    [Tooltip("Opcional: visual/runa del altar que se apaga tras usarlo.")]
    [SerializeField] private GameObject disableAfterActivation;

    [Header("Diagnostics")]
    [SerializeField] private bool logDebug;

    public bool HasBeenActivated => activated;
    public bool PlayerIsInRange => playerIsInRange;
    public FinalDragonBoss SpawnedBoss => spawnedBoss;

    private bool activated;
    private bool playerIsInRange;
    private FinalDragonBoss spawnedBoss;
    private Coroutine activationRoutine;
    private Transform player;
    private float nextPlayerSearchTime;

    private void Awake()
    {
        SetPromptVisible(false);
    }

    private void Start()
    {
        ResolveRuntimeReferences();
        ResolvePlayer();
        ValidatePrefabSetup();
    }

    private void ResolveRuntimeReferences()
    {
        // El TerrainGenerator instancia este altar en runtime.
        // Por eso evitamos referencias de escena guardadas dentro del prefab.
        if (waveSpawner == null)
            waveSpawner = FindFirstObjectByType<WaveSpawner3D>();
    }

    private void ResolvePlayer()
    {
        if (player != null)
            return;

        if (Time.time < nextPlayerSearchTime)
            return;

        nextPlayerSearchTime = Time.time + 1f;

        if (string.IsNullOrEmpty(playerTag))
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void ValidatePrefabSetup()
    {
        if (bossPrefab == null)
        {
            Debug.LogError(
                "[DragonBossAltar] Falta Boss Prefab.",
                this);
        }

        if (bossFlightAnchor == null)
        {
            Debug.LogWarning(
                "[DragonBossAltar] Falta Boss Flight Anchor. " +
                "Crea un Empty hijo del prefab llamado, por ejemplo, FlightAnchor.",
                this);
        }

        if (bossGroundAnchor == null)
        {
            Debug.LogWarning(
                "[DragonBossAltar] Falta Boss Ground Anchor. " +
                "Crea un Empty hijo del prefab llamado, por ejemplo, GroundAnchor.",
                this);
        }

        if (waveSpawner == null && useWaveDifficulty)
        {
            Debug.LogWarning(
                "[DragonBossAltar] No se encontro WaveSpawner3D. " +
                "Se usara Fallback Difficulty y las hordas no se ajustaran.",
                this);
        }
    }

    private void Update()
    {
        if (activated)
            return;

        ResolvePlayer();
        UpdatePlayerRange();

        if (!playerIsInRange)
            return;

        Keyboard keyboard = Keyboard.current;

        if (keyboard != null &&
            keyboard.eKey.wasPressedThisFrame)
        {
            if (logDebug)
            {
                Debug.Log(
                    "[DragonBossAltar] E presionada dentro del rango. Activando boss.",
                    this);
            }

            ActivateBoss();
        }
    }

    private void UpdatePlayerRange()
    {
        bool wasInRange = playerIsInRange;

        if (player == null)
        {
            playerIsInRange = false;
        }
        else
        {
            Transform point =
                interactionPoint != null
                    ? interactionPoint
                    : transform;

            float sqrDistance =
                (player.position - point.position).sqrMagnitude;

            playerIsInRange =
                sqrDistance <= interactionRadius * interactionRadius;
        }

        if (wasInRange == playerIsInRange)
            return;

        SetPromptVisible(playerIsInRange);

        if (logDebug)
        {
            Debug.Log(
                playerIsInRange
                    ? "[DragonBossAltar] Player entro en rango."
                    : "[DragonBossAltar] Player salio del rango.",
                this);
        }
    }

    public void ActivateBoss()
    {
        if (activated || activationRoutine != null)
            return;

        // Seguridad si TerrainGenerator instancia y activa este objeto
        // antes de que Start() haya corrido.
        ResolveRuntimeReferences();
        ResolvePlayer();
        UpdatePlayerRange();

        if (!PlayerIsInRange)
        {
            if (logDebug)
            {
                Debug.Log(
                    "[DragonBossAltar] ActivateBoss ignorado: Player fuera de rango.",
                    this);
            }

            return;
        }

        if (bossPrefab == null)
        {
            Debug.LogError(
                "[DragonBossAltar] No hay Boss Prefab asignado.",
                this);
            return;
        }

        activated = true;
        SetPromptVisible(false);

        activationRoutine =
            StartCoroutine(ActivationRoutine());
    }

    private IEnumerator ActivationRoutine()
    {
        SpawnActivationFeedback();

        // Las hordas continúan. Sólo reducimos su frecuencia.
        if (reduceHordesOnBoss &&
            waveSpawner != null)
        {
            waveSpawner.SetSpawnRateMultiplier(
                bossHordeSpawnRateMultiplier);
        }

        if (disableAfterActivation != null)
            disableAfterActivation.SetActive(false);

        if (activationDelay > 0f)
            yield return new WaitForSeconds(activationDelay);

        SpawnBoss();

        activationRoutine = null;
    }

    private void SpawnBoss()
    {
        Vector3 position =
            bossSpawnPoint != null
                ? bossSpawnPoint.position
                : (bossFlightAnchor != null
                    ? bossFlightAnchor.position
                    : transform.position);

        Quaternion rotation =
            bossSpawnPoint != null
                ? bossSpawnPoint.rotation
                : (bossFlightAnchor != null
                    ? bossFlightAnchor.rotation
                    : Quaternion.identity);

        GameObject bossObject =
            Instantiate(
                bossPrefab,
                position,
                rotation);

        spawnedBoss =
            bossObject.GetComponent<FinalDragonBoss>();

        if (spawnedBoss == null)
        {
            Debug.LogError(
                "[DragonBossAltar] El prefab debe tener FinalDragonBoss en el GameObject root.",
                bossObject);

            Destroy(bossObject);
            return;
        }

        // Los anchors son objetos DE LA ESCENA, no referencias guardadas en el prefab.
        spawnedBoss.ConfigureArenaAnchors(
            bossFlightAnchor,
            bossGroundAnchor);

        float difficulty =
            GetBossDifficulty();

        spawnedBoss.BeginBossFightWithDifficulty(
            difficulty);

        if (logDebug)
        {
            Debug.Log(
                "[DragonBossAltar] Boss invocado | Difficulty = " +
                difficulty.ToString("F2") +
                " | Horde rate = " +
                (waveSpawner != null
                    ? waveSpawner.RuntimeSpawnRateMultiplier.ToString("F2")
                    : "N/A"),
                this);
        }
    }

    private float GetBossDifficulty()
    {
        if (useWaveDifficulty &&
            waveSpawner != null)
        {
            return Mathf.Max(
                0f,
                waveSpawner.GetCurrentDifficulty());
        }

        return Mathf.Max(
            0f,
            fallbackDifficulty);
    }

    private bool IsPlayer(Collider other)
    {
        if (other == null)
            return false;

        if (other.CompareTag(playerTag))
            return true;

        Transform root =
            other.transform.root;

        return root != null &&
               root.CompareTag(playerTag);
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(visible);
    }

    private void SpawnActivationFeedback()
    {
        if (activationVFX == null)
            return;

        Transform point =
            activationVFXPoint != null
                ? activationVFXPoint
                : transform;

        GameObject vfx =
            Instantiate(
                activationVFX,
                point.position,
                point.rotation);

        if (activationVFXLifetime > 0f)
            Destroy(vfx, activationVFXLifetime);
    }

    public void ResetAltarForTesting()
    {
        if (spawnedBoss != null)
            return;

        if (activationRoutine != null)
        {
            StopCoroutine(activationRoutine);
            activationRoutine = null;
        }

        activated = false;
        playerIsInRange = false;
        SetPromptVisible(false);

        if (waveSpawner != null)
            waveSpawner.ResetSpawnRateMultiplier();

        if (disableAfterActivation != null)
            disableAfterActivation.SetActive(true);
    }

    private void OnDisable()
    {
        playerIsInRange = false;
        SetPromptVisible(false);
    }

    // Opcional: TerrainGenerator puede elegir otro boss en runtime.
    // Si el prefab ya tiene Boss Prefab asignado, no hace falta usarlo.
    public void SetBossPrefab(GameObject prefab)
    {
        bossPrefab = prefab;
    }

    private void OnDrawGizmosSelected()
    {
        Transform point =
            interactionPoint != null
                ? interactionPoint
                : transform;

        Gizmos.DrawWireSphere(
            point.position,
            interactionRadius);

        if (bossSpawnPoint != null)
        {
            Gizmos.DrawWireSphere(
                bossSpawnPoint.position,
                1.25f);
        }

        if (bossFlightAnchor != null)
        {
            Gizmos.DrawWireSphere(
                bossFlightAnchor.position,
                1.5f);
        }

        if (bossGroundAnchor != null)
        {
            Gizmos.DrawWireSphere(
                bossGroundAnchor.position,
                2f);
        }

        if (bossFlightAnchor != null &&
            bossGroundAnchor != null)
        {
            Gizmos.DrawLine(
                bossFlightAnchor.position,
                bossGroundAnchor.position);
        }
    }
}
