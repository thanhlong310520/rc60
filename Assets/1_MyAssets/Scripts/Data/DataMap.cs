
using MessagePack;
using System.Collections.Generic;

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class DataMap
{
    public string map_id;
    public bool won;
    public List<string> listCheckPoint;

    public DataMap(string map_id)
    {
        this.map_id = map_id;
        this.won = false;
        this.listCheckPoint = new List<string>();
    }

    public void Reset()
    {
        listCheckPoint.Clear();
        won = false;
    }

    public void WinMap()
    {
        won = true;
    }

    public bool AddCheckPoint(string cp)
    {
        if (!listCheckPoint.Contains(cp))
        {
            listCheckPoint.Add(cp);
            return true;
        }
        return false;
    }
}
