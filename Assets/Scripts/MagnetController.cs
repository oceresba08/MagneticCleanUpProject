
using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MagnetController : MonoBehaviour { 
    [Header("Magnet Settings")]
    public float attractionRadius = 3.5f;
    public float attractionStrength = 20f;
    public float pickupDistance = 1.1f;
    public float dropForce = 2f;

    [Header("Magnet State")]
    public bool magnetIsOn = true;

    // Statistics that other scripts can read.
    public int Score { get; private set; }
    public int CarriedValue { get; private set; }
    public float TotalWeight { get; private set; }
    public int BankedValue { get; private set; }
    public int BankedCount { get; private set; }
    public int CarriedCount => carriedItems.Count;

    public event Action<int, Vector3> OnItemCollected;

    private Rigidbody ballBody;

    private List<MagneticItem> carriedItems =
        new List<MagneticItem>();

    // Prevents scoring the same item repeatedly.
    private HashSet<MagneticItem> discoveredItems =
        new HashSet<MagneticItem>();

    void Awake()
    {
        ballBody = GetComponent<Rigidbody>();
    }

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
            return;

        Collider[] nearbyObjects = Physics.OverlapSphere(
            transform.position,
            attractionRadius
        );

        foreach (Collider hit in nearbyObjects)
        {
            MagneticItem item =
                hit.GetComponent<MagneticItem>();

            if (item == null || !item.isActiveAndEnabled || item.IsHeld)
                continue;

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
        // Remember where it was before attaching.
        Vector3 pickupPosition = item.transform.position;

        item.AttachTo(ballBody);
        carriedItems.Add(item);

        CarriedValue += item.value;
        TotalWeight += item.weight;

        if (discoveredItems.Add(item))
        {
            Score += item.value;
        }

        // Tell any listening UI that an item was collected.
        OnItemCollected?.Invoke(
            item.value,
            pickupPosition
        );

        Debug.Log(
            "Score: " + Score +
            " | Value: $" + CarriedValue +
            " | Items: " + CarriedCount
        );
    }


    void DropEverything()
    {
        foreach (MagneticItem item in carriedItems)
        {
            if (item != null)
            {
                item.Release(
                    transform.position,
                    dropForce
                );
            }
        }

        carriedItems.Clear();

        // Dropped items are no longer being carried.
        CarriedValue = 0;
        TotalWeight = 0f;

        // Score stays the same!
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            attractionRadius
        );
    }

    public int BankCargo()
    {
        int depositedCount = 0;

        foreach (MagneticItem item in carriedItems)
        {
            if (item == null)
                continue;

            // Remove the physical joint and drop the object.
            item.Release(transform.position, 0f);

            // Record its value permanently for this run.
            BankedValue += item.value;
            BankedCount++;
            depositedCount++;

            // Prevent this metal from being collected again.
            // Keep its Rigidbody and Collider active.
            item.enabled = false;
        }

        carriedItems.Clear();

        CarriedValue = 0;
        TotalWeight = 0f;

        Debug.Log("Banked " + depositedCount +
                  " items. Total: $" + BankedValue);

        return depositedCount;
    }

}
