using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class MemoryPuzzleManager : MonoBehaviour
{
    public GameObject puzzlePanel;
    public PuzzleInteractable puzzleInteractable;
    public ThirdPersonController playerController;

    public List<MemoryCard> cards;

    private MemoryCard firstCard;
    private MemoryCard secondCard;

    private bool isBusy;

    private int matchedPairs;
    private int totalPairs;

    private void Start()
    {
        totalPairs = cards.Count / 2;

        ShuffleCards();

        foreach (MemoryCard card in cards)
        {
            card.Initialize(this, card.pairId);
        }
    }

    public void SelectCard(MemoryCard card)
    {
        if (isBusy)
            return;

        if (card == firstCard)
            return;

        card.ShowCard();

        if (firstCard == null)
        {
            firstCard = card;
            return;
        }

        secondCard = card;

        StartCoroutine(CheckMatch());
    }

    private IEnumerator CheckMatch()
    {
        isBusy = true;

        yield return new WaitForSeconds(0.75f);

        if (firstCard.pairId == secondCard.pairId)
        {
            firstCard.Match();
            secondCard.Match();

            matchedPairs++;

            if (matchedPairs >= totalPairs)
            {
                PuzzleCompleted();
            }
        }
        else
        {
            firstCard.HideCard();
            secondCard.HideCard();
        }

        firstCard = null;
        secondCard = null;

        isBusy = false;
    }

    private void ShuffleCards()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            int randomIndex = Random.Range(i, cards.Count);

            MemoryCard temp = cards[i];
            cards[i] = cards[randomIndex];
            cards[randomIndex] = temp;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].transform.SetSiblingIndex(i);
        }
    }

    private void PuzzleCompleted()
    {
        Debug.Log("Puzzle Solved");

        ObjectiveManager.Instance.CompleteCardObjective();
        playerController.SetMovement(true);
        puzzleInteractable.ClosePuzzle();
        puzzleInteractable.DisableInteraction();
        //puzzlePanel.SetActive(false);
    }

    public void ResetPuzzle()
    {
        StopAllCoroutines();

        firstCard = null;
        secondCard = null;

        isBusy = false;
        matchedPairs = 0;

        foreach (MemoryCard card in cards)
        {
            card.ResetCard();
        }
    }

    public void ExitPuzzle()
    {
        ResetPuzzle();

        playerController.SetMovement(true);
        puzzleInteractable.ClosePuzzle();
        //puzzlePanel.SetActive(false);
    }
}
