using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.AddtionalEventStructures
{
    public class DataEventArgs : EventArgs
    {
        public GameData gameData; 
    }
    public class DamageEventArgs : EventArgs
    {
        public Damage damage;
    }
    public class EventBroadcaster
    {
        public event EventHandler<DataEventArgs> SendLoadedData;
        public event EventHandler<DataEventArgs> SendNewData;
        public event EventHandler<EventArgs> SaveData;
        public event EventHandler<DamageEventArgs> SendDamage;

        public void LoadGame(object sender, GameData data)
        {
            EventHandler<DataEventArgs> handler = SendLoadedData;

            if (handler != null)
                handler(sender, new DataEventArgs { gameData = data });
        }
        public void SaveGame(object sender)
        {
            EventHandler<EventArgs> handler = SaveData;

            if (handler != null)
                handler(sender, new EventArgs { });
        }
        public void NewGame(object sender)
        {
            EventHandler<DataEventArgs> handler = SendLoadedData;
            GameData data = new GameData();

            if (handler != null)
                handler(sender, new DataEventArgs { gameData = data });
        }
        public void ReceiveHit(object sender, Damage dmg)
        {
            EventHandler<DamageEventArgs> handler = SendDamage;

            if (handler != null)
                handler(sender, new DamageEventArgs { damage = dmg });
        }
    }
}
