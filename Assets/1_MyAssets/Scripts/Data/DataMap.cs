
using MessagePack;
using System.Collections.Generic;
using System.Diagnostics;

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class DataMap
{
    public string map_id;
    public bool won;
    public bool inFinishLine;
    public bool claimedRewardWin;
    public List<string> listCheckPoint;

    public DataMap(string map_id)
    {
        this.map_id = map_id;
        this.won = false;
        this.claimedRewardWin = false;
        this.inFinishLine = false;
        this.listCheckPoint = new List<string>();
    }

    public void Reset()
    {
        listCheckPoint.Clear();
        won = false;
        this.claimedRewardWin = false;
        this.inFinishLine = false;
    }

    public void WinMap()
    {
        won = true;
    }
    public void SetInFinishLine()
    {
        inFinishLine = true;
    }
    public void ClaimRewardWin()
    {
        claimedRewardWin = true;
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
