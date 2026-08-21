using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Raccoon.EnumHolder
{
    public enum EventID
    {
        None,
    }
    public enum TypeShop
    {
        None, UpgradeShop, SellShop
    }

    public enum ItemState
    {
        Normal, Loot, Save,
    }

    public enum EnvironmentType
    {
        Normal, Lava, Night, Water, Radioactive, Heaven 
    }
    public enum ColorEffect
    {
        None, Red, Green, Blue, Yellow, Purple 
    }

    public enum TypeVFX
    {
        InGround, Jump, Coin, Dead, Spawn,
    }

    public enum TypeSkin
    {
        Face, Hair, Shirt, Trouser, Shoes, Bag
    }

    public enum TypeCurrency 
    {
        Gold, Gem
    }
    public enum EShopType
    {
        None,
        Ads,
        Currencies,
        IAP,
    }
    public enum Entry_SourceType
    {
        NextMap, NewMap, Retry
    }
    public enum FailReasonType
    {
        Fall, Obstacle
    }
}