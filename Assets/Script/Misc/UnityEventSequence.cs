
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventSequence : MonoBehaviour
{
    [System.Serializable]
    public class SequenceStep
    {
        public string stepName;

        [Min(0f)]
        public float waitBefore = 0f;

        public UnityEvent actions;
    }

    [Header("Sequence")]
    [SerializeField] private List<SequenceStep> steps = new List<SequenceStep>();

    private Coroutine sequenceCoroutine;

    public void PlaySequence()
    {
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);

        sequenceCoroutine = StartCoroutine(PlaySequenceRoutine());
    }

    public void StopSequence()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }
    }

    private IEnumerator PlaySequenceRoutine()
    {
        foreach (SequenceStep step in steps)
        {
            if (step == null)
                continue;

            if (step.waitBefore > 0f)
                yield return new WaitForSeconds(step.waitBefore);

            step.actions?.Invoke();
        }

        sequenceCoroutine = null;
    }
}
