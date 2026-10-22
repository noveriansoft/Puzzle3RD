using UnityEngine;

public class NPCAimController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void SetAim(bool aiming)
    {
        if (animator == null)
            return;

        animator.SetBool("IsAiming", aiming);
    }

    //npcAimController.SetAim(true);  // Angkat pistol
    //npcAimController.SetAim(false); // Turunkan pistol
}