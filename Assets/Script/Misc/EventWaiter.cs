using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EventWaiter : MonoBehaviour
{
    [Header("Wait Settings")]
    [SerializeField] private float waitTime = 2f;

    [Header("Event After Wait")]
    [SerializeField] private UnityEvent onWaitFinished;

    public void WaitThenEvent()
    {
        StartCoroutine(WaitRoutine());
    }

    private IEnumerator WaitRoutine()
    {
        yield return new WaitForSeconds(waitTime);
        onWaitFinished?.Invoke();
    }
}