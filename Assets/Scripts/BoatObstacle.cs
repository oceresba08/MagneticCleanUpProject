using System.Collections;
using UnityEngine;

public class BoatObstacle : MonoBehaviour
{
    [SerializeField] private GameObject boatPrefab;
    GameObject currentBoat;
    float timePass = 0f;
    const float spawnWait = 10f;
    bool madeObject = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(onCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        timePass += Time.deltaTime;
        if (madeObject)
        {
            currentBoat.transform.position += transform.forward * 1.0f * Time.deltaTime;
        }
    }
    IEnumerator onCoroutine()
    {
        int i = 0;
        while (true)
        {
            Debug.Log("Hey " + (int)timePass + " " + i);
            if (i % 3 == 0)
            {
                if (Random.Range(0, 2) == 0)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                       currentBoat = Instantiate(boatPrefab, new Vector3(Random.Range(0, 63), 0.5f, 0), Quaternion.identity);
                       currentBoat.transform.rotation = Quaternion.Euler(0, 90, 0);
                    }
                    else
                    {
                        currentBoat = Instantiate(boatPrefab, new Vector3(Random.Range(0, 63), 0.5f, 58), Quaternion.identity);
                    }
                }
                else
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        currentBoat = Instantiate(boatPrefab, new Vector3(0, 0.5f, Random.Range(0, 58)), Quaternion.identity);
                    }
                    else
                    {
                        currentBoat = Instantiate(boatPrefab, new Vector3(63, 0.5f, Random.Range(0, 58)), Quaternion.identity);
                    }
                }
                madeObject = true;
            }
            i++;
            yield return new WaitForSeconds(10f);
        }
    }
}
