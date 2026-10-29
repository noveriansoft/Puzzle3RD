using UnityEngine;

public class EnableObjectAction : MonoBehaviour
{
    [Header("Objects To Enable")]
    [SerializeField] private GameObject[] objectsToEnable;

    [Header("Spawn")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private Transform spawnPoint;

    [Header("Settings")]
    [SerializeField] private bool runOnce = true;

    private bool hasRun;

    private void OnEnable()
    {
        if (runOnce && hasRun)
            return;

        hasRun = true;

        if (objectsToEnable != null)
        {
            foreach (GameObject target in objectsToEnable)
            {
                if (target != null)
                    target.SetActive(true);
            }
        }

        if (prefabToSpawn != null)
        {
            Transform point = spawnPoint != null
                ? spawnPoint
                : transform;

            Instantiate(
                prefabToSpawn,
                point.position,
                point.rotation
            );
        }
    }
}