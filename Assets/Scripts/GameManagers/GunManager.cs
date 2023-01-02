using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GunManager : MonoBehaviour
{
    public List<Gun> gunList = new();
    readonly Dictionary<int, Gun> guns = new();

    private void Start()
    {
addGuns();
    }
    public Gun GetGunByID(int id)
    {
        if (guns.Count == 0)
        {
            addGuns();
        }
        return guns.GetValueOrDefault(id);
    }

    public int GetGunIdByGun(Gun gun)
    {
        return guns.FirstOrDefault(x => x.Value == gun).Key;
    }

    void addGuns()
    {
        for (int i = 0; i < gunList.Count; i++)
        {
            guns.Add(i,gunList[i]);
        }
    }
}
