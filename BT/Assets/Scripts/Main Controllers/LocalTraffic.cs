using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalTraffic : MonoBehaviour
{
    public List<RoadPath> roadPaths;
    public bool spawningCars, randomTurns;
    public float spawnRateSecPerCar, spawnRateDeltaInSec, proximityToChangeWaypoints;
    public List<GameObject> carPrefabs;
    public List<RoadIntersection> intersections;

    private List<RoadPath> spawnPaths = new List<RoadPath>();                   // Excludes all of the intersection paths for the purpose of spawning new cars
    private List<NPCVehicle> vehicles = new List<NPCVehicle>();
    private List<NPCVehicle> vehiclesToDestroy = new List<NPCVehicle>();
    private RoadPath currentPathToAssign;
    private float carSpawnRateActual, carSpawnTimerRef;
    
    // Start is called before the first frame update
    void Start()
    {
        if (roadPaths.Count > 0) {
            foreach(RoadPath path in roadPaths) {
                if (path.canSpawnCars) { spawnPaths.Add(path); }
            }
        }

        if (intersections.Count > 0) {
            foreach(RoadIntersection intersect in intersections) {
                intersect.SetNorthAndSouthGo(Random.Range(0, 2) == 0);
            }
        }

        carSpawnRateActual = Random.Range(spawnRateSecPerCar - spawnRateDeltaInSec, spawnRateSecPerCar + spawnRateDeltaInSec);
        carSpawnTimerRef = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Change traffic lights when time
        if (intersections.Count > 0) {
            foreach(RoadIntersection intersect in intersections) { intersect.CheckTimeAndIfReadyChangeLight(); }
        }

        // Add a new car to the road when time
        if (spawningCars && Time.time - carSpawnTimerRef > carSpawnRateActual) {

            // Select random car prefab from list and assign a random path on the road system
            GameObject randomCar = carPrefabs[Random.Range(0, carPrefabs.Count)];
            currentPathToAssign = spawnPaths[Random.Range(0, spawnPaths.Count)];

            // Check if any cars on the road are at the spawn point chosen for new car
            bool assignedPathIsBlocked = false;
            foreach(NPCVehicle veh in vehicles) {
                if (veh.GetWaypoint() == currentPathToAssign.roadWaypoints[0]) { assignedPathIsBlocked = true; }
            }

            // If the spawn point isn't blocked by any existing cars, put the new car there, add it to the list of cars, and restart timer
            if (!assignedPathIsBlocked) {
                GameObject newCar = Instantiate(randomCar, currentPathToAssign.roadWaypoints[0].position, Quaternion.identity);
                NPCVehicle newCarAI = newCar.GetComponent<NPCVehicle>();
                newCarAI.SetPath(currentPathToAssign, 1);
                vehicles.Add(newCar.GetComponent<NPCVehicle>());

                carSpawnRateActual = Random.Range(spawnRateSecPerCar - spawnRateDeltaInSec, spawnRateSecPerCar + spawnRateDeltaInSec);
                carSpawnTimerRef = Time.time;
            }
        }


    }

    // Add a vehicle to the list of vehicles to be destroyed
    // Should be accessed by NPCVehicle scripts
    public void AddVehicleToDestroyList(NPCVehicle carToDestroy) {
        Destroy(carToDestroy.gameObject);
        vehicles.Remove(carToDestroy);
    }

    // Destroys every vehicle prefab in the vehiclesToDestroy list
    public void CheckVehicleDestroyListAndDestroy() {
        if (vehiclesToDestroy.Count > 0) {
            foreach (NPCVehicle vehicle in vehiclesToDestroy) {
                Destroy(vehicle.gameObject);
                vehiclesToDestroy.Remove(vehicle);
            }
        }
    }
}

// Path for car prefabs to follow
[System.Serializable]
public class RoadPath {
    public string routeName;
    public bool canSpawnCars;
    public List<Transform> roadWaypoints;
    [SerializeField]
    public List<WaypointCarInstructions> waypointInstructions;
    public int numOfWaypointsToSearchAheadForApplyingBrakes;
    public Transform startWaypoint;
    public bool connectsToOtherPaths;
    [SerializeField]
    public List<RoadPathConnection> connections;
}

// Info in each waypoint to help car do the following:
// - Tilt at angle on banked turns
// - Know how many cars are moving to this waypoint
[System.Serializable]
public class WaypointCarInstructions {
    public float maxBankAngle;
    public int waypointNumOfOccupants;
}

// Useful for transitioning cars from the end of one RoadPath to the beginning of another
[System.Serializable]
public class RoadPathConnection {
    public Transform OldPathEndpoint, NewPathStartPoint;
}

// Contains information for an intersection, useful for avoiding collisions
[System.Serializable]
public class RoadIntersection {
    public Transform[] northWaypoints, southWaypoints, westWaypoints, eastWaypoints;
    public Transform northLeftStart, northLeftEnd, southLeftStart, southLeftEnd, westLeftStart, westLeftEnd, eastLeftStart, eastLeftEnd;
    public List<Transform> northAtIntersection, southAtIntersection, westAtIntersection, eastAtIntersection;
    public Transform trafficLight, northIndicator, southIndicator, westIndicator, eastIndicator;
    public Light northLight, southLight, westLight, eastLight;
    public Color go, stop;
    public float lightChangeWaitTime, lightChangeRefTime;

    private bool northAndSouthGo, westAndEastGo;
    private int leftTurnsInProgress;

    public void SetNorthAndSouthGo(bool northSouthGo) {
        northAndSouthGo = northSouthGo;
        westAndEastGo = !northSouthGo;

        lightChangeRefTime = Time.time;

        switch (northSouthGo) {
            case true:
                SetMeshRendererMaterial(northIndicator, go);
                SetMeshRendererMaterial(southIndicator, go);
                SetMeshRendererMaterial(westIndicator, stop);
                SetMeshRendererMaterial(eastIndicator, stop);

                northLight.intensity = 1.5f;
                southLight.intensity = 1.5f;
                westLight.intensity = 2;
                eastLight.intensity = 2;
                break;
            case false:
                SetMeshRendererMaterial(northIndicator, stop);
                SetMeshRendererMaterial(southIndicator, stop);
                SetMeshRendererMaterial(westIndicator, go);
                SetMeshRendererMaterial(eastIndicator, go);

                northLight.intensity = 2;
                southLight.intensity = 2;
                westLight.intensity = 1.5f;
                eastLight.intensity = 1.5f;
                break;
        }
    }

    // Change traffic light when time
    public void CheckTimeAndIfReadyChangeLight() {
        if (Time.time - lightChangeRefTime > lightChangeWaitTime) {
            SetNorthAndSouthGo(!northAndSouthGo);
        }
    }

    //
    public bool CanProceedThroughLight(Transform wp) {
        foreach (Transform wayp in northWaypoints) {
            if (wp == wayp) {
                if (leftTurnsInProgress > 0) { return false; }
                if (northAndSouthGo)    { return true; } 
                else                    { return false; }
            }
        }
        foreach (Transform wayp in southWaypoints) {
            if (wp == wayp) {
                if (northAndSouthGo)    { return true; } 
                else                    { return false; }
            }
        }
        foreach (Transform wayp in westWaypoints) {
            if (wp == wayp) {
                if (northAndSouthGo)    { return false; } 
                else                    { return true; }
            }
        }
        foreach (Transform wayp in eastWaypoints) {
            if (wp == wayp) {
                if (northAndSouthGo)    { return false; } 
                else                    { return true; }
            }
        }

        return true;
    }

    public bool AlertLeftTurn(Transform wp) {
        if (wp == northLeftStart || wp == southLeftStart || wp == westLeftStart || wp == eastLeftStart) {
            leftTurnsInProgress++;
            Debug.Log(leftTurnsInProgress);
            return true;
        }

        return false;
    }

    public bool LeftTurnEnded(Transform wp) {
        if (wp == northLeftEnd || wp == southLeftEnd || wp == westLeftEnd || wp == eastLeftEnd) {
            leftTurnsInProgress--;
            return true;
        }

        return false;
    }

    public bool IAmAtIntersection(Transform wp) {
        foreach(Transform point in northAtIntersection) {
            if (wp == point) { return true; }
        }
        foreach (Transform point in southAtIntersection) {
            if (wp == point) { return true; }
        }
        foreach (Transform point in westAtIntersection) {
            if (wp == point) { return true; }
        }
        foreach (Transform point in eastAtIntersection) {
            if (wp == point) { return true; }
        }

        return false;
    }

    public bool SomeoneIsTurningLeft() {
        if (leftTurnsInProgress > 0)    { return true; }
        else                            { return false; }
    }

    private void SetMeshRendererMaterial(Transform tran, Color col) {
        MeshRenderer meshRend = tran.GetComponent<MeshRenderer>();
        foreach (Material mater in meshRend.materials) {
            mater.SetColor("_Color", col);
        }
    }
}

 
