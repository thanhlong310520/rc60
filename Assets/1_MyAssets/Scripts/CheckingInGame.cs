using UnityEngine;

public class CheckingInGame : MonoBehaviour
{
    public GameTimer checkMapTime;
    public GameTimer checkPointTime;
    public string lastCheckPoint;
    public string mapId;
    public int playCount;
    public void OnStartMap(string mapId)
    {
        checkMapTime.Begin();
        checkPointTime.Begin();
        lastCheckPoint = "start";
        this.mapId = mapId;
    }

    public void OnInCheckPoint(string idCheckPoint)
    {
        float timeToCheckPoint = checkPointTime.End();
        checkMapTime.Begin();
    }

    void SendCheckPoint()
    {

    }
}
