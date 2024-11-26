using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAnimation : MonoBehaviour
{
    [SerializeField] private float bounceAmplitude = 0.05f; // Height of the bounce
    [SerializeField] private float bounceFrequency = 5f;    // Speed of the bounce
    [SerializeField] private float spinSpeed = 100f;        // Speed of the spin

    private Vector3 originalPosition;

    // Start is called before the first frame update
    void Start()
    {
        // Store the initial position of the item
        originalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new Y position with a sine wave for the bounce
        float bounceOffset = Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;

        // Apply the bounce animation while maintaining original X and Z
        transform.localPosition = new Vector3(
            originalPosition.x,
            originalPosition.y + bounceOffset,
            originalPosition.z
        );

        // Apply the spin animation around the Y-axis
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
    }
}
