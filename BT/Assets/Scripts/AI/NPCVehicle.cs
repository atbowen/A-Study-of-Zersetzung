using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCVehicle : MonoBehaviour
{
    public bool hovers;
    public float topSpeed, topSpeedDelta, leftTurnFactor, avoidLeftTurnFactor, acceleration, brake;

    private LocalTraffic trafficController;
    private RoadPath travelPath;                                    // Current road this vehicle is traveling on
    private Transform currentWaypoint;                              // Current waypoint on road this vehicle is traveling towards
    private int currentWaypointIndex;                               // Index of currentWaypoint in travelPath.roadWaypoints
    private float actualTopSpeed, currentSpeed;                     // Actual top speed = rng of top speed and delta; current speed is the final calculated speed
    private bool iAmTurningLeft, iAmAvoidingLeftPittsburghLeft;     // Flags to hopefully prevent traffic accidents at intersections
    private Vector3 upDir, initialUpDir;
    private float lastBankAngle;

    private enum DrivingState { MovingToNextPoint, Stopped}         // Stopped state currently not used
    private DrivingState State;
    
    // Start is called before the first frame update
    void Start()
    {
        trafficController = FindObjectOfType<LocalTraffic>();

        actualTopSpeed = Random.Range(topSpeed - topSpeedDelta, topSpeed + topSpeedDelta);      // Calculates initial random top speed

        State = DrivingState.MovingToNextPoint;                                                 // Set to start driving

        iAmTurningLeft = false;                                                                 // Not turning left
        iAmAvoidingLeftPittsburghLeft = false;                                                  // Don't have to slow down to avoid a left turn
        upDir = this.transform.up;
        initialUpDir = this.transform.up;
        lastBankAngle = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (State) {
            case DrivingState.MovingToNextPoint:

                // As long as this vehicle has a RoadPath, do all of this
                if (travelPath != null) {

                    currentWaypoint = travelPath.roadWaypoints[currentWaypointIndex];       // Destination is the currentWaypoint transform

                    // Creates a copy 
                    //Transform copy = new GameObject().transform;
                    //copy.rotation = this.transform.rotation;
                    //copy.position = this.transform.position;
                    //copy.rotation = Quaternion.AngleAxis(travelPath.waypointInstructions[currentWaypointIndex].maxBankAngle, Vector3.forward);
                    //Destroy(copy.gameObject);

                    this.transform.LookAt(currentWaypoint.position, upDir);
                    bool canProceed = true;
                    foreach(RoadIntersection intersect in trafficController.intersections) {
                        if (!intersect.CanProceedThroughLight(currentWaypoint)) { canProceed = false; }

                        iAmTurningLeft = intersect.AlertLeftTurn(currentWaypoint);
                        if (iAmTurningLeft) { iAmTurningLeft = !intersect.LeftTurnEnded(currentWaypoint); }

                        if (intersect.IAmAtIntersection(currentWaypoint) && !iAmTurningLeft && intersect.SomeoneIsTurningLeft()) {
                            canProceed = false;
                            iAmAvoidingLeftPittsburghLeft = true;
                        }
                    }

                    //Destroy(copy.gameObject);

                    if (travelPath.waypointInstructions[currentWaypointIndex].waypointNumOfOccupants > 1) { canProceed = false; }
                    for (int i = 1; i < travelPath.numOfWaypointsToSearchAheadForApplyingBrakes + 1; i++) {
                        if (currentWaypointIndex + i < travelPath.roadWaypoints.Count) {
                            if (travelPath.waypointInstructions[currentWaypointIndex + i].waypointNumOfOccupants > 0) { canProceed = false; }
                        }
                    }


                    if (!canProceed) {
                        if (currentSpeed > 0)  { currentSpeed -= brake; }
                        else { currentSpeed = 0; }
                    } else {
                        if (currentSpeed < actualTopSpeed)  { currentSpeed += acceleration; }
                    }
                    if (iAmTurningLeft)                         { this.transform.Translate(0, 0, currentSpeed * leftTurnFactor * Time.deltaTime); } 
                    else if (iAmAvoidingLeftPittsburghLeft)     { this.transform.Translate(0, 0, currentSpeed * avoidLeftTurnFactor * Time.deltaTime); }
                    else                                        { this.transform.Translate(0, 0, currentSpeed * Time.deltaTime); }
                    this.transform.Translate(0, 0, currentSpeed * Time.deltaTime);


                    if (Mathf.Abs(Vector3.Distance(this.transform.position, currentWaypoint.position)) < trafficController.proximityToChangeWaypoints) {

                        if (travelPath.connectsToOtherPaths && travelPath.connections.Count > 0) {

                            bool thisIsAConnectionPoint = false;
                            Transform newStartPoint = null;

                            foreach (RoadPathConnection connector in travelPath.connections) {
                                if (connector.OldPathEndpoint == currentWaypoint) {
                                    newStartPoint = connector.NewPathStartPoint;
                                    thisIsAConnectionPoint = true;
                                }
                            }

                            if (thisIsAConnectionPoint) {

                                List<RoadPath> possiblePaths = new List<RoadPath>();
                                foreach (RoadPath path in trafficController.roadPaths) {
                                    if (path.startWaypoint == newStartPoint) { possiblePaths.Add(path); }
                                }
                                
                                if (possiblePaths.Count > 0) {
                                    travelPath.waypointInstructions[currentWaypointIndex].waypointNumOfOccupants--;
                                    int randConnectionIndex = Random.Range(0, possiblePaths.Count);
                                    if (possiblePaths.Count == 1 || !trafficController.randomTurns) {travelPath = possiblePaths[0];}
                                    else {travelPath = possiblePaths[randConnectionIndex];}

                                    currentWaypointIndex = 1;
                                    travelPath.waypointInstructions[currentWaypointIndex].waypointNumOfOccupants++;
                                }
                            } else {
                                if (currentWaypointIndex < travelPath.roadWaypoints.Count - 1) {
                                    currentWaypointIndex++;

                                    if (travelPath.waypointInstructions[currentWaypointIndex].maxBankAngle == 0) { upDir = initialUpDir; } 
                                    else {
                                        upDir = Quaternion.AngleAxis(travelPath.waypointInstructions[currentWaypointIndex].maxBankAngle
                                                - lastBankAngle, this.transform.forward) * this.transform.up;
                                        lastBankAngle = travelPath.waypointInstructions[currentWaypointIndex].maxBankAngle;

                                        travelPath.waypointInstructions[currentWaypointIndex].waypointNumOfOccupants++;
                                        travelPath.waypointInstructions[currentWaypointIndex - 1].waypointNumOfOccupants--;
                                    }
                                } 
                                else {
                                    travelPath.waypointInstructions[currentWaypointIndex].waypointNumOfOccupants--;
                                    trafficController.AddVehicleToDestroyList(this);
                                }
                            }
                        } else {

                            if (currentWaypointIndex < travelPath.roadWaypoints.Count - 1) {
                                currentWaypointIndex++;

                                if (travelPath.waypointInstructions[currentWaypointIndex].maxBankAngle == 0) { upDir = initialUpDir; } 
                                else {
                                    upDir = Quaternion.AngleAxis(travelPath.waypointInstructions[currentWaypointIndex].maxBankAngle
                                                    - lastBankAngle, this.transform.forward) * this.transform.up;
                                    lastBankAngle = travelPath.waypointInstructions[currentWaypointIndex].maxBankAngle;
                                }

                                travelPath.waypointInstructions[currentWaypointIndex].waypointNumOfOccupants++;
                                travelPath.waypointInstructions[currentWaypointIndex - 1].waypointNumOfOccupants--;
                            }
                            else {
                                travelPath.waypointInstructions[currentWaypointIndex].waypointNumOfOccupants--;
                                trafficController.AddVehicleToDestroyList(this);
                            }
                        }
                    }
                }
                break;
            case DrivingState.Stopped:
                break;
        }
    }

    public void SetPath(RoadPath path, int waypointIndex) {
        travelPath = path;
        currentWaypointIndex = waypointIndex;
    }

    public Transform GetWaypoint() {
        if (currentWaypoint != null) { return currentWaypoint; }
        else { return null; }
    }

    public RoadPath GetRoadPathFromStartWaypoint(Transform wp) {
        if (trafficController.roadPaths.Count > 0) {
            foreach (RoadPath road in trafficController.roadPaths) {
                if (road.startWaypoint == wp) {
                    return road;
                }
            }
        }

        return null;
    }
}
