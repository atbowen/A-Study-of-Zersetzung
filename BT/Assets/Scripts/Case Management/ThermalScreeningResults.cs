using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Evidence/Thermal Results")]
public class ThermalScreeningResults : EvidenceData
{
    public Texture calorimetryChart;
    public ThermalDataPoint[] maxima;

    public override void SetProjectHandlerMode() {
        projHandler = FindObjectOfType<ProjectHandler>();
        projHandler.activeDataType = ProjectHandler.DataType.ThermalView;
    }

    public override void UpdateResultsAndNotes() {
        projHandler.activeDataWindow.texture = this.calorimetryChart;

        string results;

        results = "Reaction heats at:\n     ";

        if (maxima.Length > 0) {
            for (int i = 0; i < maxima.Length; i++) {
                results = results + "Time(min): " + maxima[i].time + "     Temp(C): " + maxima[i].temperature + 
                    "     Pressure(UaPA): " + maxima[i].pressure + "\n     ";
            }
        } else { results = results + "[No data]\n     "; }

        projHandler.resultsTxt.text = results;
        projHandler.notesTxt.text = this.notes;
    }
}