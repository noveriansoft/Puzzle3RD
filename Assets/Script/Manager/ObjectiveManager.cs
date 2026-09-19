using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Obj")]
    [SerializeField] private GameObject busObj;

    [Header("Objective Text")]
    [SerializeField]
    private string objectiveDoor =
        "Objective: Buka pintu Kopdes";

    [SerializeField]
    private string objectiveCard =
        "Objective: Selesaikan puzzle kartu di depan kasir";

    [SerializeField]
    private string objectiveBus =
        "Objective: Masuk ke bus dan pulang";

    private int currentObjective = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetObjective(0);
    }

    public void CompleteDoorObjective()
    {
        SetObjective(1);
    }

    public void CompleteCardObjective()
    {
        SetObjective(2);
        busObj.gameObject.SetActive(true);
    }

    private void SetObjective(int objectiveIndex)
    {
        currentObjective = objectiveIndex;

        switch (currentObjective)
        {
            case 0:
                objectiveText.text = objectiveDoor;
                break;

            case 1:
                objectiveText.text = objectiveCard;
                break;

            case 2:
                objectiveText.text = objectiveBus;
                break;
        }
    }
}
