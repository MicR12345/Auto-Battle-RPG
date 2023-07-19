using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FactionResourceManager : MonoBehaviour
{
    [InspectorName("Resources")]
    public FactionResourcesWrapper resourcesWrapper = new FactionResourcesWrapper();
    public void AddResource(string faction, string name, int value)
    {
        resourcesWrapper.AddFactionResource(faction, name, value);
    }
    public bool RemoveResource(string faction,string name,int value)
    {
        return resourcesWrapper.RemoveFactionResource(faction, name, value);
    }
    public int GetResourceValue(string faction,string name)
    {
        return resourcesWrapper.GetFactionResource(faction,name);
    }
    [System.Serializable]
    public class FactionResourcesWrapper
    {
        public List<Faction> factions = new List<Faction>();
        public void AddFactionResource(string faction, string name, int value)
        {
            Faction f = FindFaction(faction);
            if (f != null)
            {
                f.AddResource(name, value);
            }
            else
            {
                Faction newF = new Faction();
                newF.name = faction;
                newF.AddResource(name, value);
                factions.Add(newF);
            }
        }
        public bool RemoveFactionResource(string faction, string name, int value)
        {
            Faction f = FindFaction(faction);
            if (f != null)
            {
                bool removed = f.RemoveResource(name, value);
                return removed;
            }
            else
            {
                return false;
            }
        }
        public int GetFactionResource(string faction, string name)
        {
            Faction f = FindFaction(faction);
            if (f != null)
            {
                return f.GetResourceValue(name);
            }
            else return 0;
        }
        public Faction FindFaction(string name)
        {
            foreach (Faction faction in factions)
            {
                if (faction.name == name)
                {
                    return faction;
                }
            }
            return null;
        }
        public FactionResourcesWrapper()
        {
            this.factions = new List<Faction>();
        }
        [System.Serializable]
        public class Faction
        {
            public string name;
            public List<Resource> resources = new List<Resource>();
            public void AddResource(string name, int value)
            {
                Resource resource = FindResource(name);
                if (resource != null)
                {
                    resource.value = resource.value + value;
                }
                else
                {
                    resources.Add(new Resource(name, value));
                }
            }
            public bool RemoveResource(string name, int value)
            {
                Resource resource = FindResource(name);
                if (resource != null)
                {
                    resource.value = resource.value - value;
                    return true;
                }
                return false;
            }
            public int GetResourceValue(string name)
            {
                Resource r = FindResource(name);
                if (r != null)
                {
                    return r.value;
                }
                return 0;
            }
            public Resource FindResource(string name)
            {
                foreach (Resource resource in resources)
                {
                    if (name == resource.name)
                    {
                        return resource;
                    }
                }
                return null;
            }
        }
    
    }
    [System.Serializable]
    public class Resource
    {
        public string name;
        public int value;
        public Resource(string name,int value)
        {
            this.name = name;
            this.value = value;
        }
        public Resource()
        {
            this.name = "";
            this.value = 0;
        }
    }
}
