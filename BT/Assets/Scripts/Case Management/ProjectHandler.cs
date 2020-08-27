using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Manages all of the case projects
public class ProjectHandler : MonoBehaviour {
    public List<ProjectFile> openProjects, closedProjects, submittedProjects;
    public ProjectFile currentWorkingProject;
    public CrimeScene currentWorkingCrimeScene;

    // Data result types
    public enum DataType { SceneView, TrajView, NMRView, ThermalView, MatView }
    public DataType activeDataType;

    // Project interface elements
    public Text openProjectList;
    public RawImage openProjectListBgd;
    public RawImage openProjectListHighlight;
    public Color openProjectListHighlightUnselectedColor, openProjectListHighlightSelectedColor;
    public float openProjectListHighlightFlashTime;
    public Vector2 openProjectListHighlightPosition, openProjectListHighlightInitialPosition;
    public int openProjectListHighlightHeight;

    // Results interface elements
    public Text resultsTxt, notesTxt;
    public RawImage activeDataWindow, resultsActual, notesBgd;
    public List<RawImage> trajSetOverlays, NMROverlays;
    public Texture noDataTexture, blankTexture;

    // Crime scene viewer
    public Transform crimeSceneStage;
    public RenderTexture crimeSceneStageView;
    public float crimeSceneStageRefDistance;

    // Unassigned evidence
    private List<CrimeSceneObject> evidencePool;

    private bool showingCrimeSceneOnStage;

    private int currentWorkingProjectIndex;

    private Text closedProjectList;
    private RawImage closedProjectListBgd;

    private Text submittedProjectList;
    private RawImage submittedProjectListBgd;

    private bool projectSelected, openProjectListHighlightFadeOrUnfade;
    private float openProjectListHighlightFlashRefTime;

    void Start() {
        currentWorkingProjectIndex = 0;
        currentWorkingProject = openProjects[0];
        openProjectListHighlightInitialPosition = openProjectListHighlight.rectTransform.position;
        openProjectListHighlightPosition = openProjectListHighlightInitialPosition;

        projectSelected = false;

        activeDataType = DataType.SceneView;
        showingCrimeSceneOnStage = false;
    }

    void Update() {
        if (!projectSelected) {
            ManageOpenProjectListHighlightFading();
        }

        EvidenceData curEvidence = currentWorkingProject.currentEvidence;

        if (curEvidence != null) {
            curEvidence.SetProjectHandlerMode();
            curEvidence.UpdateResultsAndNotes();
        } else {
            if (currentWorkingProject.piecesOfEvidence.Count > 0)   { curEvidence = currentWorkingProject.piecesOfEvidence[0]; }
            else                                                    { activeDataWindow.texture = noDataTexture; }
        }

        switch (activeDataType) {
            case DataType.SceneView:
                activeDataWindow.texture = crimeSceneStageView;
                foreach (CrimeScene sc in currentWorkingProject.crimeScenes) {
                    AssignEvidenceToScene(sc);
                    SetCenterStageObjectAndMaxViewDistance(sc);
                    PositionAllSceneStageObjects(sc);
                    ShowCrimeSceneStageElements(sc);
                }
                break;
            case DataType.TrajView:
                break;
            case DataType.NMRView:
                break;
            case DataType.ThermalView:
                break;
            case DataType.MatView:
                break;
        }
    }

    public bool IsProjectSelected() {
        return projectSelected;
    }

    public int GetProjectFileIndexFromName(string name) {
        for (int i = 0; i < openProjects.Count; i++) {
            if (openProjects[i].projectName == name) {
                return i;
            }
        }

        return 0;
    }

    public int GetCrimeSceneIndexFromName(string name) {
        foreach (ProjectFile project in openProjects) {
            for (int i = 0; i < project.crimeScenes.Count; i++) {
                if (project.crimeScenes[i].sceneName == name) {
                    return i;
                }
            }
        }

        return 0;
    }

    public void SelectHighlightedProject() {
        projectSelected = true;
        openProjectListHighlight.color = openProjectListHighlightSelectedColor;
    }

    public void DeselectHighlightedProject() {
        projectSelected = false;
        openProjectListHighlight.color = openProjectListHighlightUnselectedColor;
        openProjectListHighlightFlashRefTime = Time.time;
    }

    public void HighlightNextProject() {
        if (currentWorkingProjectIndex == openProjects.Count) {
            currentWorkingProjectIndex = 0;
            currentWorkingProject = openProjects[currentWorkingProjectIndex];
            openProjectListHighlightPosition = openProjectListHighlightInitialPosition;
        }
        else {
            currentWorkingProjectIndex++;
            currentWorkingProject = openProjects[currentWorkingProjectIndex];
            openProjectListHighlightPosition = openProjectListHighlightPosition + new Vector2(0, -openProjectListHighlightHeight);
        }
    }

    public void HighlightPreviousProject() {
        if (currentWorkingProjectIndex == 0) {
            currentWorkingProjectIndex = openProjects.Count - 1;
            currentWorkingProject = openProjects[currentWorkingProjectIndex];
            openProjectListHighlightPosition = openProjectListHighlightPosition + new Vector2(0, openProjectListHighlightHeight * (openProjects.Count - 1));
        }
        else {
            currentWorkingProjectIndex--;
            currentWorkingProject = openProjects[currentWorkingProjectIndex];
            openProjectListHighlightPosition = openProjectListHighlightPosition + new Vector2(0, openProjectListHighlightHeight);
        }
    }

    public void ShowProjectPanels() {
        openProjectListHighlight.enabled = true;
        closedProjectListBgd.enabled = true;
        submittedProjectListBgd.enabled = true;

        openProjectList.enabled = true;
        closedProjectList.enabled = true;
        submittedProjectList.enabled = true;

        openProjectListHighlightFlashRefTime = Time.time;
    }

    public void HideProjectPanels() {
        openProjectListBgd.enabled = false;
        openProjectListHighlight.enabled = false;
        closedProjectListBgd.enabled = false;
        submittedProjectListBgd.enabled = false;

        openProjectList.enabled = false;
        closedProjectList.enabled = false;
        submittedProjectList.enabled = false;
    }

    public void ManageOpenProjectListHighlightFading() {
        if (openProjectListHighlightFadeOrUnfade) {
            Color highlightColor = openProjectListHighlight.color;
            Color newColor = new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightColor.a * Time.time * 0.9f);
            if (openProjectListHighlight.color.a > 0.1) { openProjectListHighlight.color = newColor; }
            if (Time.time - openProjectListHighlightFlashRefTime > openProjectListHighlightFlashTime) { openProjectListHighlightFadeOrUnfade = false; }
        }
        else {
            Color highlightColor = openProjectListHighlight.color;
            Color newColor = new Color(highlightColor.r, highlightColor.g, highlightColor.b, highlightColor.a * Time.time * 1.1f);
            if (openProjectListHighlight.color.a < 1) { openProjectListHighlight.color = newColor; }
            if (Time.time - openProjectListHighlightFlashRefTime > openProjectListHighlightFlashTime) { openProjectListHighlightFadeOrUnfade = true; }
        }
    }

    public void ImportCrimeSceneElementIntoPool(Transform element, CrimeSceneObject.ObjClass objectClass, string crimeSceneName) {
        if (element.Find("ID").GetComponent<ID>() != null) {
            CrimeSceneObject newObject = new CrimeSceneObject(element, objectClass);

            evidencePool.Add(newObject);
        }
    }

    public void AssignEvidenceToScene(CrimeScene scene) {
        foreach (CrimeSceneObject obj in evidencePool) {
            if (obj.sceneName == scene.sceneName) {
                scene.crimeSceneElements.Add(obj);
                evidencePool.Remove(obj);
            }
        }
    }

    private void SetCenterStageObjectAndMaxViewDistance(CrimeScene scene) {
        // Find max distance between crime scene objects to determine size of stage for purposes of camera use/zooming
        float maxDistanceFromCenterObject = crimeSceneStageRefDistance;
        float minDistanceBetweenEvidence = crimeSceneStageRefDistance;
        int minDistanceObjectIndex = 0;
        if (scene.crimeSceneElements.Count > 2) {
            for (int i = 0; i < scene.crimeSceneElements.Count - 1; i++) {
                for (int j = i + 1; j < scene.crimeSceneElements.Count; i++) {
                    float evidenceDistance = Mathf.Abs(Vector3.Distance(scene.crimeSceneElements[j].obj.position,
                                                                        scene.crimeSceneElements[i].obj.position));
                    if (evidenceDistance < minDistanceBetweenEvidence) {
                        minDistanceBetweenEvidence = evidenceDistance;
                        minDistanceObjectIndex = i;
                        scene.centerObject = scene.crimeSceneElements[i];
                    }

                    if (evidenceDistance > maxDistanceFromCenterObject) {
                        scene.maxViewCamDistance = evidenceDistance;
                    }
                }
            }
        }
    }

    // Move the invisible copy crime scene object to the crime scene stage, position it relative to the center 
    private void PositionAllSceneStageObjects(CrimeScene scene) {
        foreach (CrimeSceneObject obj in scene.crimeSceneElements) {
            if (obj != scene.centerObject) {
                Vector3 refPos = obj.obj.position - scene.centerObject.obj.position;
                obj.objCopy.position = crimeSceneStage.position + refPos;
            }
        }
    }

    private void ShowCrimeSceneStageElements(CrimeScene scene) {
        if (scene.isSelected) {
            foreach (CrimeSceneObject obj in scene.crimeSceneElements) {
                if (obj.objCopy.GetComponent<SkinnedMeshRenderer>() != null) { obj.objCopy.GetComponent<SkinnedMeshRenderer>().enabled = true; }
                if (obj.objCopy.GetComponent<MeshRenderer>() != null) { obj.objCopy.GetComponent<MeshRenderer>().enabled = true; }
            }
        }
        else {
            foreach (CrimeSceneObject obj in scene.crimeSceneElements) {
                if (obj.objCopy.GetComponent<SkinnedMeshRenderer>() != null) { obj.objCopy.GetComponent<SkinnedMeshRenderer>().enabled = false; }
                if (obj.objCopy.GetComponent<MeshRenderer>() != null) { obj.objCopy.GetComponent<MeshRenderer>().enabled = false; }
            }
        }
    }
}