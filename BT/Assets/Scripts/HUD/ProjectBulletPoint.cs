using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ProjectBulletPoint
{
    public bool isKnown;

    [SerializeField]
    private string title, description;
    [SerializeField]
    private Texture image;

    public string GetTitle() {
        return title;
    }

    public string GetDescription() {
        return description;
    }

    public Texture GetImage() {
        return image;
    }
}
