using MessagePack;
using Raccoon.EnumHolder;
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
    public long diamond;
    public string lastTimeClaimReward;
    public int dayReward;

    public Dictionary<TypeSkin, string> dicCurrentSkin;
    public Dictionary<TypeSkin, List<string>> dicOwnSkin;


    [NonSerialized]
    [IgnoreMember]
    public Action<long> onChangeCoin;
    [NonSerialized]
    [IgnoreMember]
    public Action<long> onChangeDiamond;

    public CharacterData(string id, string name)
    {
        this.id = id;
        this.name = name;
        coin = 0;
        diamond = 0;
        lastTimeClaimReward = "";
        dayReward = 0;
        dicCurrentSkin = new Dictionary<TypeSkin, string>();
        dicOwnSkin = new Dictionary<TypeSkin, List<string>>();
    }
    public void ChangeFashion(TypeSkin type, string id)
    {
        dicCurrentSkin[type] = id;
    }
    public void AddCoin(long add)
    {
        coin += add;
        onChangeCoin?.Invoke(coin);
    }

    public void AddDiamond(long add)
    {
        diamond += add;
        onChangeDiamond?.Invoke(diamond);
    }

    public string GetIdCurrentSkin(TypeSkin type)
    {
        if (dicCurrentSkin.ContainsKey(type))
        {
            return dicCurrentSkin[type];
        }
        return null;
    }   

    public void AddOwnSkin(TypeSkin type, string id)
    {
        if (!dicOwnSkin.ContainsKey(type))
        {
            dicOwnSkin[type] = new List<string>();
        }
        if (!dicOwnSkin[type].Contains(id))
        {
            dicOwnSkin[type].Add(id);
        }
    }
    public bool IsOwnSkin(TypeSkin type, string id)
    {
        if (dicOwnSkin.ContainsKey(type))
        {
            return dicOwnSkin[type].Contains(id);
        }
        return false;
    }

    public long GetCoin => coin;


    public void SetLastTimeClaimReward(string time)
    {
        lastTimeClaimReward = time;
    }
    public void SetDayReward(int day)
    {
        dayReward = day;
    }

}

