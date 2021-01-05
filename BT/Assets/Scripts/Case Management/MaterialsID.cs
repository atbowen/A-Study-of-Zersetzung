using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Evidence/Materials ID")]
public class MaterialsID : EvidenceData
{
    public List<ElementConcentration> elementalResults;
    public Texture materialImage;

    public float density, viscosity, sapConc;
    public string sapPurityTxt, additionalComments;

    public override void DetermineMatches() {
        
    }

    public override string GetResults() {
        ProjectHandler projHandler = FindObjectOfType<ProjectHandler>();
        ProjectFile currentProject = projHandler.currentWorkingProject;
        projHandler.activeDataWindow.texture = this.materialImage;

        string results;

        results = "Elem/comp:  ";

        foreach (ElementConcentration elem in elementalResults) {
            results = results + elem.element + "(" + elem.concPPM.ToString() + ")" + ",  ";
        }

        string elementalResultsTxt = "Found:  ";
        string propertiesTxt = "Properties:  ";
        string additionalCommentsTxt = "Add'l:  ";
        string propertiesNotesTxt = "";

        foreach (ElementConcentration element in this.elementalResults) {
            elementalResultsTxt = elementalResultsTxt + "(" + element.element + ")-  " + element.concPPM + ";  ";
        }

        if (this.density != 0) { propertiesTxt = propertiesTxt + "Dens- " + this.density + "g/mL;  "; }
        if (this.viscosity != 0) { propertiesTxt = propertiesTxt + "Visc- " + this.viscosity + "Pa/s;  "; }
        if (this.sapConc != 0) { propertiesTxt = propertiesTxt + "Vh- " + this.sapConc + "%"; }
        if (this.sapPurityTxt != "") { propertiesTxt = propertiesTxt + "\n" + this.sapPurityTxt + "."; }
        if (this.additionalComments != "") { additionalCommentsTxt = additionalCommentsTxt + this.additionalComments; }
        if (this.notes != "") { propertiesNotesTxt = this.notes; }

        projHandler.resultsTxt.text = elementalResultsTxt + "\n" + propertiesTxt + "\n" + additionalCommentsTxt + "\n";
        projHandler.notesTxt.text = propertiesNotesTxt;

        return results;
    }

    public override string GetResults(int referenceIndexToShow) {
        return additionalComments;
    }

    public override string GetNotes() {
        return notes;
    }

    public override string GetNotes(int referenceIndexToShow) {
        return notes;
    }
}
