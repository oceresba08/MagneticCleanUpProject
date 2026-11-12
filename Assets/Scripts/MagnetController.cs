
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.AI.Assistant.Agents;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MagnetController : MonoBehaviour
{

    [Header("Crab Attack")]
    public float crabDisableDuration = 2f;

    private float magnetDisabledUntil = 0f;

    [Header("Magnet Settings")]
    public float attractionRadius = 3.5f;
    public float attractionStrength = 20f;
    public float pickupDistance = 1.1f;
    public float dropForce = 2f;

    [Header("Magnet State")]
    public bool magnetIsOn = true;

    [Header("UI")]
    public TMP_Text knockedText;

    bool showKnocked = false;

    // Statistics that other scripts can read.
    public int Score { get; private set; }
    public int CarriedValue { get; private set; }
    public float TotalWeight { get; private set; }
    public int BankedValue { get; private set; }
    public int BankedCount { get; private set; }
    public int CarriedCount => carriedItems.Count;

    public IReadOnlyList<MagneticItem> CarriedItems => carriedItems;

    public event Action<int, Vector3> OnItemCollected;

    private Rigidbody ballBody;

    private List<MagneticItem> carriedItems =
        new List<MagneticItem>();

    // Prevents scoring the same item repeatedly.
    private HashSet<MagneticItem> discoveredItems =
        new HashSet<MagneticItem>();

    private PowerUpController powerUps;

    void Awake()
    {
        powerUps = GetComponent<PowerUpController>();
        ballBody = GetComponent<Rigidbody>();
        knockedText.gameObject.SetActive(false);
    }


    void Update()
    {
        // Restore the magnet after the cooldown.
        if (magnetDisabledUntil > 0f)
        {
            StartCoroutine(onCoroutine());
            if (Time.time >= magnetDisabledUntil)
            {
                magnetDisabledUntil = 0f;
                magnetIsOn = true;
            }
            // Ignore E during the crab cooldown.
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            magnetIsOn = !magnetIsOn;

            if (!magnetIsOn)
                DropEverything();
        }

        StartCoroutine(onCoroutine());
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

            float effectivePickupDistance = pickupDistance *
    (powerUps != null ? powerUps.StrengthMultiplier : 1f);

            if (distance <= effectivePickupDistance)
            {
                CollectItem(item);
            }

            else if (distance > 0.001f)
            {
                // Check whether Strength Boost is active.
                float boost = powerUps != null
                    ? powerUps.StrengthMultiplier
                    : 1f;

                // Apply magnetic attraction with the boost.
                item.Body.AddForce(
                    direction.normalized * attractionStrength * boost,
                    ForceMode.Acceleration
                );
            }

        }
    }


    public void GetHitByCrab()
    {
        showKnocked = true;
        // Drop everything the ball is carrying.
        DropEverything();

        // Turn off the magnet temporarily.
        magnetIsOn = false;

        // Record when the magnet can work again.
        magnetDisabledUntil =
            Time.time + crabDisableDuration;
        
        Debug.Log("Magnet disabled by crab!");
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


    public void DropEverything()
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

            PawnShopLootTag tag =
            item.GetComponent<PawnShopLootTag>();

            if (tag != null)
            {
                PawnShopLootLog.RecordItem(tag.kind);
            }

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

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Submarine"))
        {
            magnetIsOn = !magnetIsOn;
            knockedText.gameObject.SetActive(true);
            if (!magnetIsOn)
            {
                DropEverything();
            }
            showKnocked = true;
        }
    }

    /*void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Submarine"))
        {
            showKnocked = true;
        }
    }*/

    IEnumerator onCoroutine()
    {
        if (showKnocked)
        {
            knockedText.gameObject.SetActive(true);
            yield return new WaitForSeconds(3f);
            showKnocked = false;
        }
        else
        {
            knockedText.gameObject.SetActive(false);
        }

    }
}
