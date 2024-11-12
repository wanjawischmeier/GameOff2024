using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSceneState : MonoBehaviour
{
    public struct SceneState
    {
        public Vector3 playerPosition, playerRotation;
        public GuardState[] guards;
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

    private void Start()
    {
        LoadScene();
        LoadInventory();
    }

    public static void SaveScene(bool savePlayer = true, bool saveGuards = true)
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

        string saveFile = JsonUtility.ToJson(sceneState);
        File.WriteAllText(sceneSaveFilePath, saveFile);
    }

    public static bool LoadScene()
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
                else
                {
                    guardController.agent.SetDestination(guardSave.target);
                }
            }
        }

        File.Delete(sceneSaveFilePath);
        return true;
    }

    public static void SaveInventory()
    {
        string saveFile = JsonUtility.ToJson(InventoryManager.Instance.inventoryState);
        File.WriteAllText(inventorySaveFilePath, saveFile);
    }

    public static bool LoadInventory()
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
        InventoryManager.Instance.inventoryState = JsonUtility.FromJson<InventoryManager.InventoryState>(saveFile);
        return true;
    }
}
