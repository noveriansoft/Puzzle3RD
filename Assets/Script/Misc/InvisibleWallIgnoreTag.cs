using UnityEngine;

public class InvisibleWallIgnoreTag : MonoBehaviour
{
    [SerializeField] private string[] ignoredTags;

    private Collider wallCollider;

    private void Awake()
    {
        wallCollider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ShouldIgnore(collision.collider))
        {
            Physics.IgnoreCollision(
                wallCollider,
                collision.collider,
                true
            );
        }
    }

    private bool ShouldIgnore(Collider other)
    {
        foreach (string tag in ignoredTags)
        {
            if (other.CompareTag(tag))
                return true;
        }

        return false;
    }
}