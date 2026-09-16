using UnityEngine;
using UnityEngine.UI;

public class MemoryCard : MonoBehaviour
{
    public int pairId;

    public Button button;

    public GameObject front;
    public GameObject back;

    private MemoryPuzzleManager manager;

    public bool IsMatched { get; private set; }

    public void Initialize(MemoryPuzzleManager puzzleManager, int id)
    {
        manager = puzzleManager;
        pairId = id;

        HideCard();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClickCard);

        Debug.Log("hide all");
    }

    private void OnClickCard()
    {
        manager.SelectCard(this);
    }

    public void ShowCard()
    {
        front.SetActive(true);
        back.SetActive(false);
    }

    public void HideCard()
    {
        if (IsMatched)
            return;

        front.SetActive(false);
        back.SetActive(true);
    }

    public void Match()
    {
        IsMatched = true;
        front.SetActive(true);
        back.SetActive(false);

        button.interactable = false;
    }

    public void ResetCard()
    {
        IsMatched = false;

        button.interactable = true;

        front.SetActive(false);
        back.SetActive(true);
    }
}
