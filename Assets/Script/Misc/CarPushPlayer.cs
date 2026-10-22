using UnityEngine;
using StarterAssets;

public class CarPushPlayer : MonoBehaviour
{
    [SerializeField] private float pushStrength = 4f;

    private void OnCollisionStay(Collision collision)
    {
        FirstPersonController player =
            collision.collider.GetComponentInParent<FirstPersonController>();

        if (player == null)
            return;

        CharacterController controller =
            player.GetComponent<CharacterController>();

        if (controller == null)
            return;

        Vector3 pushDirection =
            player.transform.position - transform.position;

        pushDirection.y = 0f;
        pushDirection.Normalize();

        controller.Move(
            pushDirection * pushStrength * Time.deltaTime
        );
    }
}