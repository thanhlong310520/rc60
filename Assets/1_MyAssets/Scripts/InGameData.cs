using MessagePack;
using Raccoon.EnumHolder;
using UnityEngine;

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class InGameData
{
    public string mapID;
    public int playCount;

    //reset
    public int attempID;
    public float mapTime;


    // checkpoint
    public string checkpointID;
    public float checkPointTime; // khoang cach giua 2 checkpoint 
    public int reviveCount; // số lượng lần resume.
    public int failBeforeCP; // số lần chết mỗi cp, reset mỗi check point
    public int map_skip; // số lần skip trong map
    public int deadCount; // tổng số lần chết.
    public bool winMap;

    public InGameData(string mapID)
    {
        this.mapID = mapID;
        this.playCount = 1;
        this.attempID = 0;
        this.mapTime = 0;
        this.checkpointID = "Start";
        this.checkPointTime = 0;
        this.reviveCount = 0;
        failBeforeCP = 0;
        map_skip = 0; 
        deadCount = 0;
        winMap = false;
    }

    public void AddAttemp() { attempID++; }

    public void Reset()
    {
        if (winMap)
        {
            playCount++;
            attempID = 0;
            winMap = false;
        }
        this.mapTime = 0;
        this.checkpointID = "Start";
        this.checkPointTime = 0;
        this.reviveCount = 0;
        failBeforeCP = 0;
        map_skip = 0;
        deadCount = 0;
    }
    public void SetCheckpoint(string id)
    {
        this.checkpointID = id;
    }

    public void NewCheckpoint(string id)
    {
        this.checkpointID = id;
        failBeforeCP = 0;
        checkPointTime = 0;
    }
    public void WinMap()
    {
        winMap = true;
    }

    public string Show()
    {
        string result = "";
        result += $"mapID + {mapID}  ";
        result += $"playcount + {playCount}  ";
        result += $"attempt + {attempID}  ";
        result += $"checkpointID + {checkpointID}  ";
        result += $"checkpointTime + {checkPointTime}  ";
        result += $"mapTime + {mapTime}  ";
        return result;
    }
}


