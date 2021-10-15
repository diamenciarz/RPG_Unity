using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class BaseNode : Node {

    //[Input] public int entry;
    public virtual string GetString()
    {
        return null;
    }
    public virtual string GetValues()
    {
        return null;
    }
}