
using UnityEngine;
using System.Collections.Generic;

public class MagnetController : MonoBehaviour
{
    [Header("Magnet Settings")]
    public float attractionRadius = 3.5f;
    public float attractionStrength = 20f;
    public float pickupDistance = 1.1f;
    public float dropForce = 2f;

    [Header("Magnet State")]
    public bool magnetIsOn = true;

    public float TotalWeight { get; private set; }
    public int CarriedCount => carriedItems.Count;

    private List<MagneticItem> carriedItems =
        new List<MagneticItem>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            magnetIsOn = !magnetIsOn;

            if (!magnetIsOn)
            {
                DropEverything();
            }

            Debug.Log("Magnet ON: " + magnetIsOn);
        }
    }

    void FixedUpdate()
    {
        if (!magnetIsOn)
        {
            return;
        }

        // Find nearby colliders.
        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            attractionRadius
        );

        foreach (Collider hit in nearbyObjects)
        {
            // Only attract objects with MagneticItem.
            MagneticItem item =
                hit.GetComponent<MagneticItem>();

            if (item == null || item.IsHeld)
            {
                continue;
            }

            Vector3 direction =
                transform.position - item.transform.position;

            float distance = direction.magnitude;

            if (distance <= pickupDistance)
            {
                CollectItem(item);
            }
            else if (distance > 0.001f)
            {
                item.Body.AddForce(
                    direction.normalized * attractionStrength,
                    ForceMode.Acceleration
                );
            }
        }
    }


    void CollectItem(MagneticItem item)
    {
        Rigidbody ballBody = GetComponent<Rigidbody>();

        item.AttachTo(ballBody);

        carriedItems.Add(item);
        TotalWeight += item.weight;

        Debug.Log(
            "Carrying: " + CarriedCount +
            " | Weight: " + TotalWeight
        );
    }


    void DropEverything()
    {
        foreach (MagneticItem item in carriedItems)
        {
            if (item != null)
            {
                item.Release(transform.position, dropForce);
            }
        }

        carriedItems.Clear();
        TotalWeight = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            attractionRadius
        );
    }
}
