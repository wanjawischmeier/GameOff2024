using System.IO;
using UnityEngine;

public class SaveCampusOutsideState : MonoBehaviour
{
    public struct CampusOutsideState
    {
        public Vector3 playerPosition, playerRotation;
        public GuardState[] guards;
        public InventoryManager.InventoryState inventoryState;
        // TODO: handle current guard waypoint
    }

    [System.Serializable]
    public struct GuardState
    {
        public Vector3 position, rotation, target;
        public int waypointIndex, waypointDirection;
        public float currentWaypointTime;
    }

    static string saveFilePath
    {
        get => Path.Combine(Application.temporaryCachePath + "campusOutsideState.save");
    }

    private void Start()
    {
        if (File.Exists(saveFilePath))
        {
            Debug.Log($"Loading existing temporary save from {saveFilePath}.");
            Load();
        }
        else
        {
            Debug.Log($"No temporary save found at {saveFilePath}.");
        }
    }

    public static void Save()
    {
        var guardTransforms = StaticObjectsCampusOutside.Guards;
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

        var campusOutsideState = new CampusOutsideState()
        {
            playerPosition = PlayerController.Transform.position,
            playerRotation = PlayerController.Instance.bodyTransform.eulerAngles,
            guards = guards
        };

        string saveFile = JsonUtility.ToJson(campusOutsideState);
        File.WriteAllText(saveFilePath, saveFile);
    }

    public static void Load()
    {
        string saveFile = File.ReadAllText(saveFilePath);
        var campusOutsideState = JsonUtility.FromJson<CampusOutsideState>(saveFile);

        PlayerController.Transform.position = campusOutsideState.playerPosition;
        PlayerController.Instance.bodyTransform.eulerAngles = campusOutsideState.playerRotation;

        var guards = StaticObjectsCampusOutside.Guards;
        for (int i = 0; i < guards.Length; i++)
        {
            var guardTransform = guards[i];
            var guardController = guardTransform.GetComponent<GuardController>();
            var guardSave = campusOutsideState.guards[i];

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

        File.Delete(saveFilePath);
    }
}
