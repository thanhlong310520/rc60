using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlayerSerializable
{
    [System.Serializable]
    public class SettingDataSave
    {
        public bool isSound;
        public bool isMusic;
        public bool isVibrate;

        public SettingDataSave(bool isSound, bool isMusic, bool isVibrate)
        {
            this.isSound = isSound;
            this.isMusic = isMusic;
            this.isVibrate = isVibrate;
        }
    }
    [System.Serializable]
    public class TileClaimed
    {
        public string packName;
        public int bestscore;
        public List<int> tileClaimeds;
        public TileClaimed(string packName, int bestscore, List<int> tileClaimeds)
        {
            this.packName = packName;
            this.bestscore = bestscore;
            this.tileClaimeds = tileClaimeds;
        }
    }
    [System.Serializable]
    public class DataInGame
    {
        public bool isTutorial = false;
        public int currentscore = 0;
        public string nameSkinCurrent = "Pack_1";
        public int coin = 0;
        public int skill_1 = 1;
        public int skill_2 = 1;
        public int skill_3 = 1;
        public int skill_4 = 1;
        public List<string> packSkinClaimeds = new();
    }
    public SettingDataSave setting;
    public List<TileClaimed> tileClaimed = new();
    public DataInGame dataInGame = new();
    public PlayerSerializable()
    {
        setting = new SettingDataSave(true, true, true);
        tileClaimed = new();
    }
    public void AddDataTileClaimed(string namePack)
    {
        tileClaimed.Add(new TileClaimed(namePack, 0, new()));
    }
    public void AddIdFruit(int id)
    {
        var data = tileClaimed.FirstOrDefault(x => x.packName == dataInGame.nameSkinCurrent);
        if (data != null)
        {
            if (!data.tileClaimeds.Contains(id))
            {
                data.tileClaimeds.Add(id);
            }
        }
        else
        {
            tileClaimed.Add(new TileClaimed(dataInGame.nameSkinCurrent, 0, new List<int> { id }));
        }
    }
    public TileClaimed GetTileByNamePack(string namePack)
    {
        return tileClaimed.FirstOrDefault(x => x.packName == namePack);
    }
    public List<int> GetTileClaimedByNamePack(string namePack)
    {
        return GetTileByNamePack(namePack).tileClaimeds;
    }
    public int GetBestScore()
    {
        return GetTileByNamePack(dataInGame.nameSkinCurrent).bestscore;
    }
    public int GetCoin()
    {
        return dataInGame.coin;
    }
    public void SetBestScoreToPack(int bestscore)
    {
        GetTileByNamePack(dataInGame.nameSkinCurrent).bestscore = bestscore;
    }
}

