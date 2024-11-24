using System.IO;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SceneStateManager : MonoBehaviour
{
    public struct SceneState
    {
        public Vector3 playerPosition, playerRotation;
        public GuardState[] guards;
        public bool[] interactablesTriggered;
    }

    [System.Serializable]
    public struct GuardState
    {
        public Vector3 position, rotation, target;
        public int waypointIndex, waypointDirection;
        public float currentWaypointTime;
    }

    static string sceneSaveFilePath
    {
        get => Path.Combine(Application.temporaryCachePath, $"SceneState_{SceneManager.GetActiveScene().buildIndex}.save");
    }

    static string inventorySaveFilePath
    {
        get => Path.Combine(Application.persistentDataPath, "InventoryState.save");
    }

    static string[] initialInteractableObjects;
    static Vector2 playerVelocity;
    static Vector2[] guardVelocitys;

    private void Start()
    {
        var interactionTriggerParent = StaticObjects.InteractionTriggerParent;
        initialInteractableObjects = new string[interactionTriggerParent.childCount];
        for (int i = 0; i < interactionTriggerParent.childCount; i++)
        {
            initialInteractableObjects[i] = interactionTriggerParent.GetChild(i).name;
        }

        LoadSceneState();
        if (!LoadInventoryState())
        {
            InventoryManager.Instance.SetSelectedSlot(0);
        }
    }

    private static void ResetState(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted state at {path}");
        }
    }

    public static void SaveSceneState(bool savePlayer = true, bool saveGuards = true)
    {
        var sceneState = new SceneState();

        if (savePlayer)
        {
            sceneState.playerPosition = PlayerController.Transform.position;
            sceneState.playerRotation = PlayerController.Instance.bodyTransform.eulerAngles;
        }

        if (saveGuards)
        {
            var guardTransforms = StaticObjects.Guards;
            var guards = new GuardState[guardTransforms.Length];

            for (int i = 0; i < guardTransforms.Length; i++)
            {
                var guardTransform = guardTransforms[i];
                var guardController = guardTransform.GetComponent<GuardController>();
                var guard = new GuardState()
                {
                    position = guardTransform.position,
                    rotation = guardController.bodyTransform.eulerAngles,
                    waypointIndex = guardController.waypointIndex,
                    waypointDirection = guardController.waypointDirection,
                    currentWaypointTime = guardController.currentWaypointTime
                };

                if (guardController.rotationSpeed != -1)
                {
                    // guard isn't in pursuit of the player
                    guard.target = guardController.agent.destination;
                }

                guards[i] = guard;
            }

            sceneState.guards = guards;
        }

        var interactionTriggerParent = StaticObjects.InteractionTriggerParent;
        sceneState.interactablesTriggered = new bool[interactionTriggerParent.childCount];
        for (int i = 0; i < interactionTriggerParent.childCount; i++)
        {
            sceneState.interactablesTriggered[i] = interactionTriggerParent.Find(initialInteractableObjects[i]) == null;
        }

        string saveFile = JsonUtility.ToJson(sceneState);
        File.WriteAllText(sceneSaveFilePath, saveFile);
    }

    public static bool LoadSceneState()
    {
        if (File.Exists(sceneSaveFilePath))
        {
            Debug.Log($"Loading existing temporary save from {sceneSaveFilePath}.");
        }
        else
        {
            Debug.Log($"No temporary save found at {sceneSaveFilePath}.");
            return false;
        }

        string saveFile = File.ReadAllText(sceneSaveFilePath);
        var sceneState = JsonUtility.FromJson<SceneState>(saveFile);

        if (sceneState.playerPosition != Vector3.zero)
        {
            PlayerController.Transform.position = sceneState.playerPosition;
            PlayerController.Instance.bodyTransform.eulerAngles = sceneState.playerRotation;
        }

        var guards = StaticObjects.Guards;
        if (sceneState.guards.Length == guards.Length)
        {
            for (int i = 0; i < guards.Length; i++)
            {
                var guardTransform = guards[i];
                var guardController = guardTransform.GetComponent<GuardController>();
                var guardSave = sceneState.guards[i];

                guardTransform.position = guardSave.position;
                guardController.bodyTransform.eulerAngles = guardSave.rotation;
                guardController.waypointIndex = guardSave.waypointIndex;
                guardController.waypointDirection = guardSave.waypointDirection;
                guardController.currentWaypointTime = guardSave.currentWaypointTime;

                if (guardSave.target == Vector3.zero)
                {
                    guardController.PursuePlayer();
                }
                else if (guardController.agent != null)
                {
                    guardController.agent.SetDestination(guardSave.target);
                }
            }
        }

        var interactionTriggerParent = StaticObjects.InteractionTriggerParent;
        for (int i = 0; i < sceneState.interactablesTriggered.Length; i++)
        {
            if (sceneState.interactablesTriggered[i])
            {
                var interactableObject = interactionTriggerParent.Find(initialInteractableObjects[i]);
                Destroy(interactableObject.gameObject);
            }
        }

        File.Delete(sceneSaveFilePath);
        return true;
    }

    public static void ResetSceneState() => ResetState(sceneSaveFilePath);

    public static void ReloadScene()
    {
        ResetSceneState();
        SceneTransitionFader.TransitionToScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void SetScenePause(bool isPaused)
    {
        PlayerController.Instance.enabled = !isPaused;
        PlayerController.Instance.animator.enabled = !isPaused;
        var playerRigidBody = PlayerController.Transform.GetComponent<Rigidbody2D>();
        if (isPaused)
        {
            playerVelocity = playerRigidBody.linearVelocity;
            playerRigidBody.linearVelocity = Vector2.zero;
        }
        else
        {
            playerRigidBody.linearVelocity = playerVelocity;
        }

        var clock = FindAnyObjectByType<AnalogClock>();
        clock.enabled = !isPaused;

        var guardTransforms = StaticObjects.Guards;
        if (isPaused)
        {
            guardVelocitys = new Vector2[guardTransforms.Length];
        }

        for (int i = 0; i < guardTransforms.Length; i++)
        {
            var guardTransform = guardTransforms[i];
            var guardController = guardTransform.GetComponent<GuardController>();
            var guardAgent = guardTransform.GetComponent<NavMeshAgent>();
            guardController.enabled = !isPaused;
            guardAgent.isStopped = isPaused;

            if (isPaused)
            {
                guardVelocitys[i] = guardAgent.velocity;
                guardAgent.velocity = Vector2.zero;
            }
            else
            {
                guardAgent.velocity = guardVelocitys[i];
            }
        }
    }

    public static void SaveInventoryState()
    {
        string saveFile = JsonUtility.ToJson(InventoryManager.Instance.inventoryState);
        File.WriteAllText(inventorySaveFilePath, saveFile);
    }

    public static bool LoadInventoryState()
    {
        if (File.Exists(inventorySaveFilePath))
        {
            Debug.Log($"Loading existing inventory state from {inventorySaveFilePath}.");
        }
        else
        {
            Debug.Log($"No inventory save file found at {inventorySaveFilePath}.");
            return false;
        }

        string saveFile = File.ReadAllText(inventorySaveFilePath);
        var inventoryState = JsonUtility.FromJson<InventoryManager.InventoryState>(saveFile);
        var inventoryManager = InventoryManager.Instance;
        inventoryManager.inventoryState = inventoryState;
        inventoryManager.SetSelectedSlot(inventoryState.selectedSlot);
        return true;
    }

    public static void ResetInventoryState() => ResetState(inventorySaveFilePath);

    public static void SaveAndLoadNewScene(string sceneName)
    {
        SaveSceneState();
        SaveInventoryState();
        SceneTransitionFader.TransitionToScene(sceneName);
    }

    public static void SaveAndLoadNewScene(int targetSceneBuildIndex)
    {
        SaveSceneState();
        SaveInventoryState();
        SceneTransitionFader.TransitionToScene(targetSceneBuildIndex);
    }
}