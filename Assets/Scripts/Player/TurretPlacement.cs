using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretPlacement : MonoBehaviour
{
    [SerializeField]
    private LayerMask placementCheckMask;

    [SerializeField]
    private LayerMask placementColliderMask;

    [SerializeField] 
    private Camera playerCamera;

    private GameObject currentPlacingTower;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPlacingTower != null) 
        {

            Ray cameraRay = playerCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(cameraRay, out hitInfo, 100f, placementColliderMask)) 
            {
                currentPlacingTower.transform.position = hitInfo.point;
            }

            if (Input.GetMouseButtonDown(1))
            {
                Destroy(currentPlacingTower);
                currentPlacingTower = null;
                return;
            }

            if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject != null) 
            {
                if (!hitInfo.collider.gameObject.CompareTag("CantPlace"))
                {
                    BoxCollider towerCollider = currentPlacingTower.gameObject.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;

                    Vector3 boxCenter = currentPlacingTower.gameObject.transform.position + towerCollider.center;
                    Vector3 halfExtents = towerCollider.size / 2;

                    if (Physics.CheckBox(boxCenter, halfExtents, Quaternion.identity, placementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        GameManager.towersInGame.Add(currentPlacingTower.GetComponent<TowerBehaviour>());

                        towerCollider.isTrigger = false;
                        currentPlacingTower = null;
                    }
                }
            }
        }
    }

    public void setTowerToPlace(GameObject tower) 
    {
        currentPlacingTower = Instantiate(tower, Vector3.zero, Quaternion.identity);
    }
}
