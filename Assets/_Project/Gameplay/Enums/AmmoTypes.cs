using UnityEngine;

public enum AmmoType { BasicShell, BasicLaser }
public class AmmoTypes
{
    public static AmmoTypeData GetAmmoData(AmmoType at)
    {
        switch (at)
        {
            case AmmoType.BasicShell: return Resources.Load<AmmoTypeData>("AmmoTypes/BasicShellData");
            case AmmoType.BasicLaser: return Resources.Load<AmmoTypeData>("AmmoTypes/BasicLaserData");
            default: return null;
        }
    }
}
