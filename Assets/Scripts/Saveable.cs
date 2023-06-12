using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Saveable
{
    public string Save();
    public void Restore(string data);
}
