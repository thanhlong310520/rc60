using Raccoon.EnumHolder;
using Raccoon.Controller;
using UnityEngine;

public class CheckingInGame : MonoBehaviour
{
    public GameTimer checkMapTime;
    public GameTimer checkPointTime;

    InGameData data;

    public void OnStartMap(InGameData data)
    {
        this.data = data;
        data.AddAttemp();

        if (checkMapTime == null) checkMapTime = new GameTimer();
        if(checkPointTime == null) checkPointTime = new GameTimer();
        checkMapTime.Begin();
        checkPointTime.Begin();


        Debug.Log("[Check InGame] " +  data.Show());
        GameFirebase.SendEvent("gameplay_start",
            "mapID", data.mapID,
            "playCount", data.playCount.ToString(),
            "checkpoint", data.checkpointID
            );
    }

    public void OnNewCheckPoint(string idCheckPoint)
    {
        float timeToCheckPoint = checkPointTime.End();
        timeToCheckPoint += data.checkPointTime;
        Debug.Log("[Check InGame] " + idCheckPoint + " " + timeToCheckPoint);

        data.NewCheckpoint(idCheckPoint);
        checkMapTime.Begin();
    }
    public void OnFinishGame()
    {
        float timeInGame = checkMapTime.End();
        timeInGame += data.mapTime;
        Debug.Log("[Check InGame] " + data.mapID + " " + timeInGame);

    }

    public void SetWinMap()
    {
        data.WinMap();
        OnFinishGame();
    }
    public void OnPlayerFail(string idCheckPoint,FailReasonType type)
    {
        Debug.Log("[Check InGame] " + "player fail " + " " + idCheckPoint);
        data.deadCount++;
        data.failBeforeCP++;

    }

    public void SetIsSkip(bool isSkip)
    {
        if (!isSkip) data.reviveCount++;
        else data.map_skip++;
    }
    public void SaveDataWhenOutInGame()
    {
        data.checkPointTime += checkPointTime.End();
        data.mapTime += checkMapTime.End();
    }

    private void OnDisable()
    {
        SaveDataWhenOutInGame();
        Debug.Log("[Check InGame] " + "OnDisable");

    }


}
