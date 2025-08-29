using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] private Transform target; // The target to follow
    [SerializeField] private bool followX = true; // Follow the X position
    [SerializeField] private bool followY = true; // Follow the Y position
    [SerializeField] private bool followZ = true; // Follow the Z position

    Vector3 offset; // Offset from the target's position

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (target == null) {
            Debug.LogError("Target not set for Follow script on " + gameObject.name);
            return;
        }

        // Calculate the initial offset
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return; // If no target, do nothing
        Vector3 newPosition = target.position + offset;

        // Apply the offset based on the follow flags
        if (!followX) newPosition.x = transform.position.x;
        if (!followY) newPosition.y = transform.position.y;
        if (!followZ) newPosition.z = transform.position.z;

        // Update the position of this GameObject
        transform.position = newPosition;
    }
}
