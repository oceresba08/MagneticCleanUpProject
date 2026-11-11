using System.Collections;
using UnityEngine;

public class BoatObstacle : MonoBehaviour
{
    [SerializeField] private GameObject boatPrefab;
    GameObject currentBoat;
    float timePass = 0f;
    float speed = 5f;
    bool madeObjectLeft, madeObjectRight, reverse = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(onCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        timePass += Time.deltaTime;
        if (madeObjectLeft)
        {
            if (reverse)
            {
                currentBoat.transform.position += transform.forward * speed * Time.deltaTime;
            }
            else
            {
                currentBoat.transform.position -= transform.forward * speed * Time.deltaTime;
            }
        }
        else if (madeObjectRight)
        {
            
            if (reverse)
            {
                currentBoat.transform.position -= transform.right * speed * Time.deltaTime;
            }
            else
            {
                currentBoat.transform.position += transform.right * speed * Time.deltaTime;
            }
        }
    }
    IEnumerator onCoroutine()
    {
        int i = 0;
        while (true)
        {
            //Destroy(currentBoat);
            Debug.Log("Hey " + (int)timePass + " " + i);
            if (i % 3 == 0)
            {
                reverse = false;
                madeObjectRight = false;
                madeObjectLeft = false;
                if (Random.Range(0, 2) == 0)
                {
                    if (Random.Range(0, 2) == 0)
                    {
                       currentBoat = Instantiate(boatPrefab, new Vector3(Random.Range(0, 63), 4.0f, 0), Quaternion.identity);
                       currentBoat.transform.rotation = Quaternion.Euler(0, 270, 0);
                    }
                    else
                    {
                        currentBoat = Instantiate(boatPrefab, new Vector3(Random.Range(0, 63), 4.0f, 58), Quaternion.identity);
                        currentBoat.transform.rotation = Quaternion.Euler(0, 90, 0);
                        reverse = true;
                    }
                    madeObjectLeft = true;
                }
                else
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        currentBoat = Instantiate(boatPrefab, new Vector3(0, 4.0f, Random.Range(0, 58)), Quaternion.identity);
                        reverse = true;
                    }
                    else
                    {
                        currentBoat = Instantiate(boatPrefab, new Vector3(63, 4.0f, Random.Range(0, 58)), Quaternion.identity);
                    }
                    madeObjectRight = true;
                }
            }
            i++;
            yield return new WaitForSeconds(5f);
        }
    }
}
