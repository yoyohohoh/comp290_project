using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [SerializeField] private float homingSpeed = 5f;
    [SerializeField] private float rotationSpeed = 200f;
    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void Update()
    {
        if (target == null)
        {
            // Destroy or deactivate the projectile if no target exists
            Destroy(gameObject);
            return;
        }

        // Calculate the direction to the target
        Vector3 direction = (target.position - transform.position).normalized;

        // Rotate towards the target
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        // Move towards the target
        transform.position += transform.forward * homingSpeed * Time.deltaTime;
    }
}
