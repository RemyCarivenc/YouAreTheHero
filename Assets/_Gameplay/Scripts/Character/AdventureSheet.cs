using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AdventureSheet
{
    public List<DataPlayer.KaiDiscipline> kaiDisciplines;
    public List<DataPlayer.Weapons> weapons; 
    public List<DataPlayer.Objects> objects;
    public List<DataPlayer.SpecialObjects> specialObjects;
    public int food;
    public int gold;

    public int dexterity;
    public int endurance;
}
