using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrant : MonoBehaviour
{
    public string charge;
    [TextArea]
    public string instructions;

    public bool warrantIsActive;
    public enum WarrantType { Bench, Search, Extradition, Dispossessory}
    public WarrantType TypeOfWarrant;

    public Texture mugshot;
}
