using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveMono : MonoBehaviour,SaveInterface
{
    public abstract void Save();

    public abstract void Load();
}
