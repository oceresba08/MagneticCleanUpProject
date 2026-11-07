
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MagnetController))]
public class WeightlessCargoBoost : MonoBehaviour
{
    [Header("Strength Boost Physics")]
    public float boostedItemMass = 0.01f;

    private MagnetController magnet;
    private PowerUpController powerUps;

    private struct OriginalPhysics
    {
        public float mass;
        public bool useGravity;
    }

    private Dictionary<Rigidbody, OriginalPhysics> originals =
        new Dictionary<Rigidbody, OriginalPhysics>();

    private void Awake()
    {
        magnet = GetComponent<MagnetController>();
        powerUps = GetComponent<PowerUpController>();
    }

    private void FixedUpdate()
    {
        bool strengthActive =
            powerUps != null &&
            powerUps.StrengthMultiplier > 1f;

        HashSet<Rigidbody> currentlyCarried =
            new HashSet<Rigidbody>();

        foreach (MagneticItem item in magnet.CarriedItems)
        {
            if (item == null || item.Body == null)
                continue;

            Rigidbody body = item.Body;
            currentlyCarried.Add(body);

            if (strengthActive)
            {
                // Save original physics only once.
                if (!originals.ContainsKey(body))
                {
                    originals.Add(body, new OriginalPhysics
                    {
                        mass = body.mass,
                        useGravity = body.useGravity
                    });
                }

                // Make cargo nearly weightless.
                body.mass = boostedItemMass;
                body.useGravity = false;
            }
        }

        // Restore physics when the boost expires
        // or an item is deposited/dropped.
        List<Rigidbody> toRestore =
            new List<Rigidbody>();

        foreach (var pair in originals)
        {
            if (!strengthActive ||
                !currentlyCarried.Contains(pair.Key))
            {
                Rigidbody body = pair.Key;

                if (body != null)
                {
                    body.mass = pair.Value.mass;
                    body.useGravity = pair.Value.useGravity;
                }

                toRestore.Add(pair.Key);
            }
        }

        foreach (Rigidbody body in toRestore)
            originals.Remove(body);
    }

    private void OnDisable()
    {
        RestoreEverything();
    }

    private void OnDestroy()
    {
        RestoreEverything();
    }

    private void RestoreEverything()
    {
        foreach (var pair in originals)
        {
            if (pair.Key == null)
                continue;

            pair.Key.mass = pair.Value.mass;
            pair.Key.useGravity = pair.Value.useGravity;
        }

        originals.Clear();
    }
}
