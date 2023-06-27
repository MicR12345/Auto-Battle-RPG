using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public interface Component
{
    public bool isStatic();
    public string getName();
    public SimpleComponent getRequiredFields();
}
public class SimpleComponent : Component
{
    public string name;
    public int count = 1;
    public List<(string, TMP_InputField.ContentType)> fields;
    public List<SimpleComponent> subComponents;

    string Component.getName()
    {
        return name;
    }

    SimpleComponent Component.getRequiredFields()
    {
        return this;
    }

    bool Component.isStatic()
    {
        return false;
    }
}
