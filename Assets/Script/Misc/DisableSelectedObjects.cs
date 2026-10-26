
using UnityEngine;

public class DisableSelectedObjects : MonoBehaviour
{
    [Header("Objects to Disable")]
    [SerializeField] private GameObject[] objectsToDisable;

    public void DisableObjects()
    {
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
