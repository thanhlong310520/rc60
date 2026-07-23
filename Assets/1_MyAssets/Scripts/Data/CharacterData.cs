using MessagePack;
using System;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class CharacterData
{
    public string id;
    public string name;
    public long coin;
    public bool isUnlockGate;
    public string currentFashion;
    public CharactorUpgrade upgrade;
    public Dictionary<string, DataBrainInSlot> dicBotInSlots;
    public List<string> listBotUnlock;
    public List<string> listSlotGate2Unlock;
    public int indexRebirth = -1;


    [NonSerialized]
    [IgnoreMember]
    public Action<long> onChangeCoin;


    public CharactorUpgrade GetUpgrade()
    {
        if(upgrade == null) upgrade = new CharactorUpgrade();   
        return upgrade;
    }



    public CharacterData(string id, string name)
    {
        this.id = id;
        this.name = name;
        coin = 0;
        indexRebirth = -1;
        isUnlockGate = false;
        upgrade = new CharactorUpgrade();
        currentFashion = "FashionData_Brain";
        dicBotInSlots = new Dictionary<string, DataBrainInSlot>();
        listBotUnlock = new List<string>();
        listSlotGate2Unlock = new List<string>();
    }
    
    public bool AddItemUnlock(string id)
    {
        if(listBotUnlock.Contains(id)) return false;
        listBotUnlock.Add(id);
        return true;
    }

    public void ChangeFashion(string id)
    {
        currentFashion = id;
    }
    public void AddCoin(long add)
    {
        coin += add;
        onChangeCoin?.Invoke(coin);
    }

    public void SetUnlockGate(bool unLock)
    {
        Debug.Log("Data unlock");
        isUnlockGate = unLock;  
    }
    public void UnlockSlotGate2(string id)
    {
        if(listSlotGate2Unlock.Contains(id)) return;
        listSlotGate2Unlock.Add(id);
    }
    public long GetCoin => coin;


    public int GetIndexRebirth()
    {
        return indexRebirth;
    }
    public void UpdateIndexRebirth()
    {
        indexRebirth++;
    }

}

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class CharactorUpgrade
{
    
}

[MessagePackObject(keyAsPropertyName: true)]
[System.Serializable]
public class DataBrainInSlot
{
    
}
