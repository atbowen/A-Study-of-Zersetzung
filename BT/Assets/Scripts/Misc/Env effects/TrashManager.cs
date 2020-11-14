using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    [SerializeField]
    public List<TrashSpawnArea> trashAreas;

    public int numOfSpawnAttemptsPerObject;

    // Start is called before the first frame update
    void Start()
    {
        // Run spawn for all areas
        foreach (TrashSpawnArea area in trashAreas) {

            //// Clear the list of predetermined spawn point checks/bools for interactable objects, then add a check--set to true--for each predetermined spawn point
            //area.predeterminedInteractablePointsAvailable.Clear();
            //foreach (Transform point in area.predeterminedInteractablePoints) { area.predeterminedInteractablePointsAvailable.Add(true); }

            // If the area is spawning
            if (area.spawnInArea && numOfSpawnAttemptsPerObject > 0) {

                // Spawn the interactable objects
                for (int i = 0; i < area.numOfInteractables; i++) {

                    int randInteractableIndex = Random.Range(0, area.interactables.Count);

                    GameObject curObj = area.interactables[randInteractableIndex];

                    if (curObj.GetComponent<Collider>() != null) {

                        Collider col = curObj.GetComponent<Collider>();
                        float colHalfHeight = col.bounds.size.y / 2;
                        Vector3 spawnOffset = new Vector3(0, colHalfHeight, 0);

                        // Spawn the random interactable 
                        if (area.spawnInPredeterminedZones && area.predeterminedInteractablePoints.Count > 0) {
                            Transform spawnPoint = area.predeterminedInteractablePoints[Random.Range(0, area.predeterminedInteractablePoints.Count - 1)];

                            // Check for overlapping colliders from half-height of collider above spawn point (using half-height of collider as radius)
                            Collider[] obstacleCols = Physics.OverlapSphere(spawnPoint.position + spawnOffset, colHalfHeight);

                            // If there is even one overlapping collider, grant another chance to make up for it
                            // If there are no overlapping colliders, spawn the object at the point
                            // Either way, remove the spawn point from the list
                            if (obstacleCols.Length > 0)    { i--; }
                            else                            { Instantiate(curObj, spawnPoint.position, Quaternion.identity); }

                            area.predeterminedInteractablePoints.Remove(spawnPoint);
                        }
                        else {

                            bool foundAFreePoint = false;
                            int attemptNumber = 0;

                            while (!foundAFreePoint && attemptNumber < numOfSpawnAttemptsPerObject) {

                                Vector3 spawnPoint = new Vector3(Random.Range(area.lowerBoundPoint.x, area.upperBoundPoint.x),
                                                                    Random.Range(area.lowerBoundPoint.y, area.upperBoundPoint.y),
                                                                    Random.Range(area.lowerBoundPoint.z, area.upperBoundPoint.z));

                                Collider[] cols = Physics.OverlapSphere(spawnPoint + new Vector3(0, col.bounds.size.y / 2, 0), col.bounds.size.y / 2);

                                if (cols.Length == 0) {
                                    Instantiate(curObj, spawnPoint + spawnOffset, Quaternion.identity);
                                    foundAFreePoint = true;
                                }

                                attemptNumber++;
                            }
                        }
                    }
                }

                // Spawn the trash
                // Trash objects do not necessarily have colliders
                for (int i = 0; i < area.numOfTrashObjects; i++) {

                    int randTrashIndex = Random.Range(0, area.trashObjects.Count);

                    GameObject curObj = area.trashObjects[randTrashIndex];

                    Vector3 spawnPoint, spawnOffset;

                    if (curObj.GetComponent<Collider>() != null) {

                        Collider col = curObj.GetComponent<Collider>();
                        float colHalfHeight = col.bounds.size.y / 2;
                        spawnOffset = new Vector3(0, colHalfHeight, 0);

                        bool foundAFreePoint = false;
                        int attemptNumber = 0;

                        while (!foundAFreePoint && attemptNumber < numOfSpawnAttemptsPerObject) {

                            spawnPoint = new Vector3(Random.Range(area.lowerBoundPoint.x, area.upperBoundPoint.x),
                                                            Random.Range(area.lowerBoundPoint.y, area.upperBoundPoint.y),
                                                            Random.Range(area.lowerBoundPoint.z, area.upperBoundPoint.z));

                            Collider[] cols = Physics.OverlapSphere(spawnPoint + new Vector3(0, col.bounds.size.y / 2, 0), col.bounds.size.y / 2);

                            if (cols.Length == 0) {
                                Instantiate(curObj, spawnPoint + spawnOffset, Quaternion.identity);
                                foundAFreePoint = true;
                            }

                            attemptNumber++;
                        }
                    }
                    else if (curObj.GetComponent<MeshRenderer>() != null) {
                        MeshRenderer mesh = curObj.GetComponent<MeshRenderer>();
                        float tranHalfHeight = mesh.bounds.size.y / 2;
                        spawnOffset = new Vector3(0, tranHalfHeight, 0);

                        spawnPoint = new Vector3(Random.Range(area.lowerBoundPoint.x, area.upperBoundPoint.x),
                                                            Random.Range(area.lowerBoundPoint.y, area.upperBoundPoint.y),
                                                            Random.Range(area.lowerBoundPoint.z, area.upperBoundPoint.z));

                        Instantiate(curObj, spawnPoint + spawnOffset, Quaternion.identity);
                    }
                }

                // Spawn the bumpable objects
                // Similar to spawning interactables, but no predetermined spawn points
                for (int i = 0; i < area.numOfBumpables; i++) {

                    int randBumpableIndex = Random.Range(0, area.bumpables.Count);

                    GameObject curObj = area.bumpables[randBumpableIndex];

                    Vector3 spawnPoint, spawnOffset;

                    if (curObj.GetComponent<Collider>() != null) {

                        Collider col = curObj.GetComponent<Collider>();
                        float colHalfHeight = col.bounds.size.y / 2;
                        spawnOffset = new Vector3(0, colHalfHeight, 0);

                        bool foundAFreePoint = false;
                        int attemptNumber = 0;

                        while (!foundAFreePoint && attemptNumber < numOfSpawnAttemptsPerObject) {

                            spawnPoint = new Vector3(Random.Range(area.lowerBoundPoint.x, area.upperBoundPoint.x),
                                                            Random.Range(area.lowerBoundPoint.y, area.upperBoundPoint.y),
                                                            Random.Range(area.lowerBoundPoint.z, area.upperBoundPoint.z));

                            Collider[] cols = Physics.OverlapSphere(spawnPoint + new Vector3(0, col.bounds.size.y / 2, 0), col.bounds.size.y / 2);

                            if (cols.Length == 0) {
                                Instantiate(curObj, spawnPoint + spawnOffset, Quaternion.identity);
                            }
                        }
                    }
                }

                // Spawn the floaters

                for (int i = 0; i < area.numOfFloaters; i++) {

                    int randFloaterIndex = Random.Range(0, area.floaters.Count);

                    GameObject curObj = area.floaters[randFloaterIndex];

                    Vector3 spawnPoint, spawnOffset;

                    if (curObj.GetComponent<MeshRenderer>() != null) {
                        MeshRenderer mesh = curObj.GetComponent<MeshRenderer>();
                        float tranHalfHeight = mesh.bounds.size.y / 2;
                        spawnOffset = new Vector3(0, tranHalfHeight, 0);

                        spawnPoint = new Vector3(Random.Range(area.lowerBoundPoint.x, area.upperBoundPoint.x),
                                                            Random.Range(area.lowerBoundPoint.y, area.upperBoundPoint.y),
                                                            Random.Range(area.lowerBoundPoint.z, area.upperBoundPoint.z));

                        Transform newFloatObject =  Instantiate(curObj, spawnPoint + spawnOffset, Quaternion.identity).transform;

                        area.AddFloater(newFloatObject);
                    }
                }
            }
        }
    }

    

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (TrashSpawnArea area in trashAreas) {

            if (area.spawnInArea) {

                // Temp list for floaters marked for destruction
                List<FloaterObject> floatersToDestroy = new List<FloaterObject>();

                foreach (FloaterObject tran in area.existingFloaters) {

                    tran.floater.position = tran.floater.position + tran.currentDispl * Time.deltaTime;
                    tran.floater.Rotate(tran.currentEulerRot * Time.deltaTime);

                    if (Time.time - tran.displChangeRefTime > tran.currentDisplTime) {
                        tran.randDisplTimeMin = Random.Range(area.randFloaterDisplTimeMinMin, area.randFloaterDisplTimeMinMax);
                        tran.randDisplTimeMax = Random.Range(area.randFloaterDisplTimeMaxMin, area.randFloaterDisplTimeMaxMax);
                        tran.currentDisplTime = Random.Range(tran.randDisplTimeMin, tran.randDisplTimeMax);

                        float deltaX = Random.Range(area.randFloaterDisplMin.x, area.randFloaterDisplMax.x);
                        float deltaY = Random.Range(area.randFloaterDisplMin.y, area.randFloaterDisplMax.y);
                        float deltaZ = Random.Range(area.randFloaterDisplMin.z, area.randFloaterDisplMax.z);

                        float rotX = Random.Range(area.randFloaterEulerRotMin.x, area.randFloaterEulerRotMax.x);
                        float rotY = Random.Range(area.randFloaterEulerRotMin.y, area.randFloaterEulerRotMax.y);
                        float rotZ = Random.Range(area.randFloaterEulerRotMin.z, area.randFloaterEulerRotMax.z);

                        tran.currentDispl = new Vector3(deltaX, deltaY, deltaZ);
                        tran.currentEulerRot = new Vector3(rotX, rotY, rotZ);

                        tran.displChangeRefTime = Time.time;
                    }

                    if (tran.floater.position.x > area.upperBoundPoint.x ||
                        tran.floater.position.y > area.upperBoundPoint.y ||
                        tran.floater.position.z > area.upperBoundPoint.z ||
                        tran.floater.position.x < area.lowerBoundPoint.x ||
                        tran.floater.position.y < area.lowerBoundPoint.y ||
                        tran.floater.position.z < area.lowerBoundPoint.z) {

                        floatersToDestroy.Add(tran);
                        Destroy(tran.floater.gameObject);
                    }
                }


                // Remove all of the destroyed floaters from the existingFloaters List
                // Will get errors if removing within the foreach loop
                foreach (FloaterObject floater in floatersToDestroy) { area.existingFloaters.Remove(floater); }

                floatersToDestroy.Clear();

                if (area.existingFloaters.Count < area.numOfFloaters) {
                    int randFloaterIndex = Random.Range(0, area.floaters.Count);

                    GameObject curObj = area.floaters[randFloaterIndex];

                    Vector3 spawnPoint, spawnOffset;

                    if (curObj.GetComponent<MeshRenderer>() != null) {
                        MeshRenderer mesh = curObj.GetComponent<MeshRenderer>();
                        float tranHalfHeight = mesh.bounds.size.y / 2;
                        spawnOffset = new Vector3(0, tranHalfHeight, 0);

                        spawnPoint = new Vector3(Random.Range(area.lowerBoundPoint.x, area.upperBoundPoint.x),
                                                            Random.Range(area.lowerBoundPoint.y, area.upperBoundPoint.y),
                                                            Random.Range(area.lowerBoundPoint.z, area.upperBoundPoint.z));

                        Transform newFloatObject = Instantiate(curObj, spawnPoint + spawnOffset, Quaternion.identity).transform;

                        area.AddFloater(newFloatObject);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class TrashSpawnArea
    {
        public bool spawnInArea;
        
        public Vector3 lowerBoundPoint, upperBoundPoint;

        public List<GameObject> trashObjects;
        public List<GameObject> interactables;
        public List<GameObject> bumpables;
        public List<GameObject> floaters;

        [HideInInspector]
        public List<FloaterObject> existingFloaters = new List<FloaterObject>();

        public int numOfTrashObjects;
        public int numOfInteractables;
        public int numOfBumpables;
        public int numOfFloaters;

        public float randFloaterDisplTimeMinMin, randFloaterDisplTimeMinMax,        // e.g., "MinMin" is the minimum possible value of the minimum time before exerting a force on the floater
                randFloaterDisplTimeMaxMin, randFloaterDisplTimeMaxMax;             // Creating random limits for random ranges

        public Vector3 randFloaterDisplMin, randFloaterDisplMax, randFloaterEulerRotMin, randFloaterEulerRotMax;

        public bool spawnInPredeterminedZones;
        public List<Transform> predeterminedInteractablePoints;

        public void AddFloater(Transform newObj) {
            // Create new FloaterObject and set the Transform to the current Game Object's transform
            FloaterObject newFloat = new FloaterObject();
            newFloat.floater = newObj;

            // Get random time range limits for the time between applied forces, then use those limits to generate a random time delay for the first applied force
            newFloat.randDisplTimeMin = Random.Range(randFloaterDisplTimeMinMin, randFloaterDisplTimeMinMax);
            newFloat.randDisplTimeMax = Random.Range(randFloaterDisplTimeMaxMin, randFloaterDisplTimeMaxMax);
            newFloat.currentDisplTime = Random.Range(newFloat.randDisplTimeMin, newFloat.randDisplTimeMax);

            float deltaX = Random.Range(randFloaterDisplMin.x, randFloaterDisplMax.x);
            float deltaY = Random.Range(randFloaterDisplMin.y, randFloaterDisplMax.y);
            float deltaZ = Random.Range(randFloaterDisplMin.z, randFloaterDisplMax.z);

            float rotX = Random.Range(randFloaterEulerRotMin.x, randFloaterEulerRotMax.x);
            float rotY = Random.Range(randFloaterEulerRotMin.y, randFloaterEulerRotMax.y);
            float rotZ = Random.Range(randFloaterEulerRotMin.z, randFloaterEulerRotMax.z);

            newFloat.currentDispl = new Vector3(deltaX, deltaY, deltaZ);
            newFloat.currentEulerRot = new Vector3(rotX, rotY, rotZ);

            // Set the reference time for determining when the next force is applied
            newFloat.displChangeRefTime = Time.time;

            // Add the FloaterObject to this area's list of floaters
            existingFloaters.Add(newFloat);
        }
    }

    public class FloaterObject {
        public Transform floater;
        public float displChangeRefTime, currentDisplTime;
        public float randDisplTimeMin, randDisplTimeMax;
        public Vector3 currentDispl, currentEulerRot;
    }
}
