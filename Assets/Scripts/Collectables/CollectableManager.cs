using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    [Header("Checker settings")]
    public Transform obstacleChecker;

    [Range(1, 5)]
    public int rayDistance = 3;

    [Header("Collectable prefabs")]
    public GameObject coinPrefab;
    public GameObject shieldPrefab;
    public GameObject attackPrefab;

    [Header("Collectable settings")]
    [Range(0.5f, 2f)]
    public float collectableDistance = 0.5f;

    [Header("Straight pattern")]
    [Range(0.1f, 1f)]
    public float curveSmooth = 0.2f;

    [Range(1f, 5f)]
    public float maxObstacleHeight = 1.5f;

    [Header("Sin pattern")]
    [Range(1, 10)]
    public int amplitude = 1;

    [Range(1, 5)]
    public int period = 1;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        Vector3 positionTarget = Vector3.up;
        RaycastHit currentObstacle = new RaycastHit();

        while(true){
            positionTarget += Vector3.forward * collectableDistance;
            obstacleChecker.position = positionTarget;

            RaycastHit hit;

            if(Physics.Raycast(obstacleChecker.position, Vector3.forward, out hit, rayDistance)){
                if(!hit.collider.CompareTag("Obstacle") ||
                    (hit.collider.CompareTag("Indestructible") && hit.transform.parent.CompareTag("ChangeDirection"))){
                        yield return null;
                }

                else if(!currentObstacle.collider){
                    currentObstacle = hit;
                }
            }

            if(currentObstacle.collider){
                float zDistance = obstacleChecker.position.z - currentObstacle.transform.position.z;

                if(Mathf.Abs(zDistance) <= rayDistance){
                    if(currentObstacle.collider.bounds.max.y <= maxObstacleHeight){
                        Transform collectableTemp = Instantiate(coinPrefab, positionTarget, Quaternion.identity).transform;
                        collectableTemp.SetParent(transform);

                        positionTarget += Vector3.up * -Mathf.Sign(zDistance) * Mathf.Sqrt(Mathf.Abs(zDistance)) * curveSmooth;

                        if(zDistance == rayDistance){
                            positionTarget += Vector3.forward * collectableDistance;
                            collectableTemp = Instantiate(coinPrefab, positionTarget, Quaternion.identity).transform;
                            collectableTemp.SetParent(transform);
                        }

                    }

                    Vector3 upObstacle = new Vector3(obstacleChecker.position.x, currentObstacle.collider.bounds.max.y, currentObstacle.transform.position.z);
                    Debug.DrawLine(obstacleChecker.position,  upObstacle, Color.red, 0f);
                }

                else{
                    currentObstacle = new RaycastHit();
                }
            }

            Debug.DrawLine(obstacleChecker.position, obstacleChecker.position + Vector3.forward * rayDistance, Color.green, 0f);
            yield return null;
        }
    }
}
