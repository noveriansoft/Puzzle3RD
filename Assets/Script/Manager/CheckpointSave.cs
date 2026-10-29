using System;
using System.Collections.Generic;
using UnityEngine;

public static class CheckpointSave
{
    public static bool ContinueRequested { get; set; }

    [Serializable]
    private class DoorState
    {
        public string id;
        public bool isLocked;
    }

    [Serializable]
    private class DoorSnapshot
    {
        public List<DoorState> doors = new List<DoorState>();
    }

    private static string GetKey(string sceneName)
    {
        return $"Checkpoint_{sceneName}";
    }

    private static string GetDoorsKey(string sceneName)
    {
        return $"{GetKey(sceneName)}_Doors";
    }

    public static bool HasCheckpoint(string sceneName)
    {
        return PlayerPrefs.HasKey(GetKey(sceneName));
    }

    public static int LoadIndex(string sceneName)
    {
        return PlayerPrefs.GetInt(GetKey(sceneName), -1);
    }

    public static void Save(string sceneName, int stepIndex)
    {
        DoorSnapshot snapshot = new DoorSnapshot();
        HashSet<string> usedIDs = new HashSet<string>();

        PlayerRaycastDoor[] doors =
            UnityEngine.Object.FindObjectsByType<PlayerRaycastDoor>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (PlayerRaycastDoor door in doors)
        {
            if (door.gameObject.scene.name != sceneName)
                continue;

            string id = door.CheckpointID;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning(
                    "Door checkpoint ID is missing.",
                    door
                );
                continue;
            }

            if (!usedIDs.Add(id))
            {
                Debug.LogError(
                    $"Duplicate door checkpoint ID: {id}",
                    door
                );
                continue;
            }

            snapshot.doors.Add(new DoorState
            {
                id = id,
                isLocked = door.IsLocked
            });
        }

        PlayerPrefs.SetInt(GetKey(sceneName), stepIndex);

        PlayerPrefs.SetString(
            GetDoorsKey(sceneName),
            JsonUtility.ToJson(snapshot)
        );

        SaveObjectSwaps(sceneName);
        SavePhoneProgress(sceneName);
        PlayerPrefs.Save();
    }

    public static void RestoreDoorLocks(string sceneName)
    {
        string key = GetDoorsKey(sceneName);

        // Older saves may not contain door states.
        if (!PlayerPrefs.HasKey(key))
            return;

        DoorSnapshot snapshot =
            JsonUtility.FromJson<DoorSnapshot>(
                PlayerPrefs.GetString(key)
            );

        if (snapshot == null || snapshot.doors == null)
            return;

        PlayerRaycastDoor[] doors =
            UnityEngine.Object.FindObjectsByType<PlayerRaycastDoor>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (PlayerRaycastDoor door in doors)
        {
            if (door.gameObject.scene.name != sceneName)
                continue;

            foreach (DoorState state in snapshot.doors)
            {PlayerPrefs.Save();
                if (state.id != door.CheckpointID)
                    continue;

                door.RestoreCheckpointLock(state.isLocked);
                break;
            }
        }
    }

    public static void Clear(string sceneName)
    {
        PlayerPrefs.DeleteKey(GetKey(sceneName));
        PlayerPrefs.DeleteKey(GetDoorsKey(sceneName));
        PlayerPrefs.DeleteKey(GetObjectSwapsKey(sceneName));
        PlayerPrefs.DeleteKey(GetPhoneKey(sceneName));

        PlayerPrefs.Save();
    }

    [Serializable]
    private class ObjectSwapSnapshot
    {
        public List<string> triggeredIDs = new List<string>();
    }

    private static string GetObjectSwapsKey(string sceneName)
    {
        return $"{GetKey(sceneName)}_ObjectSwaps";
    }

    private static void SaveObjectSwaps(string sceneName)
    {
        ObjectSwapSnapshot snapshot = new ObjectSwapSnapshot();
        HashSet<string> usedIDs = new HashSet<string>();

        ObjectSwapTrigger[] triggers =
            UnityEngine.Object.FindObjectsByType<ObjectSwapTrigger>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (ObjectSwapTrigger trigger in triggers)
        {
            if (trigger.gameObject.scene.name != sceneName)
                continue;

            string id = trigger.CheckpointID;

            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning("Object swap checkpoint ID is missing.", trigger);
                continue;
            }

            if (!usedIDs.Add(id))
            {
                Debug.LogError($"Duplicate object swap ID: {id}", trigger);
                continue;
            }

            if (trigger.HasTriggered)
                snapshot.triggeredIDs.Add(id);
        }

        PlayerPrefs.SetString(
            GetObjectSwapsKey(sceneName),
            JsonUtility.ToJson(snapshot)
        );
    }

    public static void RestoreObjectSwaps(string sceneName)
    {
        string key = GetObjectSwapsKey(sceneName);

        if (!PlayerPrefs.HasKey(key))
            return;

        ObjectSwapSnapshot snapshot =
            JsonUtility.FromJson<ObjectSwapSnapshot>(
                PlayerPrefs.GetString(key)
            );

        if (snapshot == null || snapshot.triggeredIDs == null)
            return;

        HashSet<string> triggeredIDs =
            new HashSet<string>(snapshot.triggeredIDs);

        ObjectSwapTrigger[] triggers =
            UnityEngine.Object.FindObjectsByType<ObjectSwapTrigger>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (ObjectSwapTrigger trigger in triggers)
        {
            if (trigger.gameObject.scene.name != sceneName)
                continue;

            if (triggeredIDs.Contains(trigger.CheckpointID))
                trigger.RestoreTriggeredState();
        }
    }

    private static string GetPhoneKey(string sceneName)
    {
        return $"{GetKey(sceneName)}_Phone";
    }

    private static void SavePhoneProgress(string sceneName)
    {
        PhoneContactUI phone = PhoneContactUI.Instance;

        if (phone == null || phone.gameObject.scene.name != sceneName)
        {
            PlayerPrefs.DeleteKey(GetPhoneKey(sceneName));
            return;
        }

        PlayerPrefs.SetString(
            GetPhoneKey(sceneName),
            phone.CaptureCheckpointData()
        );
    }

    public static void RestorePhoneProgress(string sceneName)
    {
        string key = GetPhoneKey(sceneName);

        if (!PlayerPrefs.HasKey(key))
            return;

        PhoneContactUI phone = PhoneContactUI.Instance;

        if (phone == null || phone.gameObject.scene.name != sceneName)
            return;

        phone.RestoreCheckpointData(PlayerPrefs.GetString(key));
    }
}