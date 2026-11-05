
using UnityEngine;

public class LevelZone : MonoBehaviour
{
    public enum ZoneType
    {
        Start,
        Collector
    }

    public ZoneType zoneType;
    public LevelRunManager levelManager;
    public Rigidbody playerBody;

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != playerBody)
            return;

        if (levelManager != null)
            levelManager.SetZoneState(zoneType, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody != playerBody)
            return;

        if (levelManager != null)
            levelManager.SetZoneState(zoneType, false);
    }
}
