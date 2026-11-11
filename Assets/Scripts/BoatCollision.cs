using UnityEngine;

public class BoatCollision : MonoBehaviour
{
    [SerializeField] private MagnetController magnet;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Submarine"))
        {
            magnet.magnetIsOn = !magnet.magnetIsOn;

            if (!magnet.magnetIsOn)
            {
                magnet.DropEverything();
            }
        }
    }
}
