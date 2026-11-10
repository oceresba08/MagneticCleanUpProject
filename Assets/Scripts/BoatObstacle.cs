using UnityEngine;

public class BoatObstacle : MonoBehaviour
{
    [SerializeField] private GameObject boatPrefab;
    const float spawnWait = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        for (int i = 0; i < 10; i++)
        {
            Instantiate(boatPrefab, new Vector3(Random.Range(-9, 20), 0.5f, Random.Range(-8, 9)), Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float timePass = Time.deltaTime;
        if (timePass % spawnWait == 0f)
        {
            if (Random.Range(0, 1) == 0)
            {
                if (Random.Range(0, 1) == 0)
                {
                    Instantiate(boatPrefab, new Vector3(Random.Range(0, 63), 0.5f, 0), Quaternion.identity);
                }
                else
                {
                    Instantiate(boatPrefab, new Vector3(Random.Range(0, 63), 0.5f, 58), Quaternion.identity);
                }
            }
            else
            {
                if (Random.Range(0, 1) == 0)
                {
                    Instantiate(boatPrefab, new Vector3(0, 0.5f, Random.Range(0, 58)), Quaternion.identity);
                }
                else
                {
                    Instantiate(boatPrefab, new Vector3(63, 0.5f, Random.Range(0, 58)), Quaternion.identity);
                }
            }
            
        }
    }
}
