﻿﻿using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Commands;
using Server.Misc;
using Server.Gumps;
using Server.Network;
using Server.Mobiles;
using Server.Items;
using Server.Targeting;

using Server.Accounting;
using System.Linq;
using Server.Custom;
using System.Text;

namespace Server
{    
    public static class LiarsDicePersistance
    {
        public static LiarsDicePersistanceItem PersistanceItem;

        public static List<LiarsDiceInstance> m_Instances = new List<LiarsDiceInstance>();

        public static Timer m_Timer;

        public static void Initialize()
        {
            Timer.DelayCall(TimeSpan.FromTicks(1), delegate
            {
                if (PersistanceItem == null)
                    PersistanceItem = new LiarsDicePersistanceItem();

                else if (PersistanceItem.Deleted)
                    PersistanceItem = new LiarsDicePersistanceItem();
            });
        }

        public static void Serialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); //Version

            //Version 0  
        }

        public static void Deserialize(GenericReader reader)
        {
            int version = reader.ReadEncodedInt();

            //Version 0
            if (version >= 0)
            {
                
            }        
        }
    }

    public class LiarsDicePersistanceItem : Item
    {
        public override string DefaultName { get { return "LiarsDicePersistance"; } }

        public LiarsDicePersistanceItem(): base(0x0)
        {
            Movable = false;                        
        }

        public LiarsDicePersistanceItem(Serial serial): base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); //Version

            LiarsDicePersistance.Serialize(writer);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            Movable = false;

            LiarsDicePersistance.PersistanceItem = this;
            LiarsDicePersistance.Deserialize(reader);
        }
    }

    public class LiarsDiceInstance : Item
    {
        public enum CurrentActionType
        {
            LobbyWaitingForPlayers,
            StartCountdown,
            DiceRolls,
            PlayerBidding,
            LiarCallout,
            CalloutResolution,
            GameResolution
        }

        public CurrentActionType m_CurrentAction = CurrentActionType.LobbyWaitingForPlayers;
        public TimeSpan m_ActionTimeRemaining = TimeSpan.FromSeconds(30);
        public int m_CurrentPlayerIndex = 0;
        public int m_LiarCalloutPlayerIndex = 0;

        public bool m_Started = false;
        public bool m_Completed = false;        

        public DateTime m_CreatedOn;
        public PlayerMobile m_CreatedBy;       
        
        public int m_Pot = 0;

        public List<LiarsDicePlayer> m_Players = new List<LiarsDicePlayer>();

        public Timer m_Timer;

        [Constructable]
        public LiarsDiceInstance(): base(0x0)
        {
            Movable = false;

            //-----

            LiarsDicePersistance.m_Instances.Add(this);
        }

        public LiarsDiceInstance(Serial serial): base(serial)
        {
        }

        public int GetPlayerIndex(LiarsDicePlayer player)
        {
            return m_Players.IndexOf(player);
        }

        public void StartGame()
        {
            m_Timer = new LiarsDiceTimer(this);
            m_Timer.Start();
        }

        public void RefreshGumps()
        {
        }

        public bool RandomizeStartingPlayer()
        {
            List<LiarsDicePlayer> activePlayers = new List<LiarsDicePlayer>();

            for (int a = 0; a < m_Players.Count; a++)
            {
                LiarsDicePlayer player = m_Players[a];

                if (player == null) continue;
                if (!player.m_Active) continue;

                activePlayers.Add(player);
            }

            if (activePlayers.Count > 0)  
            {           
                LiarsDicePlayer startingPlayer = activePlayers[Utility.RandomMinMax(0, activePlayers.Count - 1)];            
                m_CurrentPlayerIndex = GetPlayerIndex(startingPlayer);

                return true;
            }

            else   
            {
                EndGame();
   
                return false;
            }
        }

        public bool NextPlayer()
        {
            int m_NewIndex = m_CurrentPlayerIndex;

            bool foundNewPlayer = false;

            for (int a = 0; a < m_Players.Count; a++)
            {
                m_NewIndex++;

                if (m_NewIndex >= m_Players.Count)
                    m_NewIndex = 0;

                LiarsDicePlayer player = m_Players[m_NewIndex];

                if (player == null) continue;
                if (player.m_Active)
                {
                    m_CurrentPlayerIndex = m_NewIndex;
                    foundNewPlayer = true;

                    break;
                }
            }

            if (foundNewPlayer)
                return true;
            else
            {
                EndGame();
                return false;
            }
        }

        public void BeginRound()
        {
            foreach (LiarsDicePlayer player in m_Players)
            {
                if (player == null) continue;
                if (!player.m_Active) continue;

                player.m_BidCount = 0;
                player.m_BidValue = 0;

                player.m_Dice.Clear();

                for (int a = 0; a < player.m_DiceRemaining; a++)
                {
                    int diceValue = Utility.RandomMinMax(1, 6);
                    player.m_Dice.Add(diceValue);
                }
            }

            if (RandomizeStartingPlayer())
            {
                m_CurrentAction = CurrentActionType.PlayerBidding;
                m_ActionTimeRemaining = TimeSpan.FromSeconds(30);
            }
        }

        public void PlaceBid(LiarsDicePlayer player, int bidCount, int bidValue)
        {
            if (player == null) 
                return;

            player.m_BidCount = bidCount;
            player.m_BidValue = bidValue;

            if (NextPlayer())
            {
                m_CurrentAction = CurrentActionType.PlayerBidding;
                m_ActionTimeRemaining = TimeSpan.FromSeconds(30);
            }
        }

        public bool CanCallOutPlayer(LiarsDicePlayer playerFrom, LiarsDicePlayer playerTarget)
        {
            if (playerFrom == null) return false;
            if (playerTarget == null) return false;
            if (!playerFrom.m_Active) return false;
            if (!playerTarget.m_Active) return false;

            if (playerFrom == playerTarget) return false;
            if (playerTarget.m_BidCount == 0) return false;
            if (playerTarget.m_BidValue == 0) return false;

            return true;
        }

        public void CallOutPlayer(LiarsDicePlayer playerFrom, LiarsDicePlayer playerTarget)
        {
            if (playerFrom == null) return;
            if (playerTarget == null) return;
            if (!playerFrom.m_Active) return;
            if (!playerTarget.m_Active) return;

            m_LiarCalloutPlayerIndex = GetPlayerIndex(playerFrom);
            m_CurrentAction = CurrentActionType.LiarCallout;
            m_ActionTimeRemaining = TimeSpan.FromSeconds(5);
        }

        public void ResolveCallOut()
        {
        }

        public void EndGame()
        {
        }

        public class LiarsDiceTimer : Timer
        {
            public LiarsDiceInstance m_Instance;

            public LiarsDiceTimer(LiarsDiceInstance instance): base(TimeSpan.Zero, TimeSpan.FromSeconds(5))
            {
                m_Instance = instance;

                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                #region Inactive Conditions

                if (m_Instance == null)
                {
                    Stop();
                    return;
                }

                else if (m_Instance.Deleted)
                {
                    Stop();
                    return;
                }

                if (!m_Instance.m_Started)
                {
                    Stop();
                    return;
                }

                else if (m_Instance.m_Completed)
                {
                    Stop();
                    return;
                }

                #endregion

                switch (m_Instance.m_CurrentAction)
                {
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); //Version

            //Version 0
            writer.Write((int)m_CurrentAction);
            writer.Write(m_ActionTimeRemaining);
            writer.Write(m_CurrentPlayerIndex);
            writer.Write(m_Started);
            writer.Write(m_Completed);
            writer.Write(m_CreatedOn);
            writer.Write(m_CreatedBy);
            writer.Write(m_Pot);

            writer.Write(m_Players.Count);
            for (int a = 0; a < m_Players.Count; a++)
            {        
                LiarsDicePlayer player = m_Players[a];

                writer.Write(player.m_Player);
                writer.Write(player.m_Active);
                writer.Write(player.m_TurnsSkipped);
                writer.Write(player.m_BidCount);
                writer.Write(player.m_BidValue);
                writer.Write(player.m_DiceRemaining);

                writer.Write(player.m_Dice.Count);
                for (int b = 0; b < player.m_Dice.Count; b++)
                {
                    writer.Write(player.m_Dice[b]);
                }
            }
        }        

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            //Version 0
            if (version >= 0)
            {
                m_CurrentAction = (CurrentActionType)reader.ReadInt();
                m_ActionTimeRemaining = reader.ReadTimeSpan();
                m_CurrentPlayerIndex = reader.ReadInt();
                m_Started = reader.ReadBool();
                m_Completed = reader.ReadBool();
                m_CreatedOn = reader.ReadDateTime();
                m_CreatedBy = (PlayerMobile)reader.ReadMobile();
                m_Pot = reader.ReadInt();

                int playerCount = reader.ReadInt();
                for (int a = 0; a < playerCount; a++)
                {
                    LiarsDicePlayer liarsDicePlayer = new LiarsDicePlayer((PlayerMobile)reader.ReadMobile());
                    liarsDicePlayer.m_Active = reader.ReadBool();
                    liarsDicePlayer.m_TurnsSkipped = reader.ReadInt();
                    liarsDicePlayer.m_BidCount = reader.ReadInt();
                    liarsDicePlayer.m_BidValue = reader.ReadInt();
                    liarsDicePlayer.m_DiceRemaining = reader.ReadInt();

                    int diceCount = reader.ReadInt();

                    for (int b = 0; b < diceCount; b++)
                    {
                        liarsDicePlayer.m_Dice.Add(reader.ReadInt());
                    }

                    m_Players.Add(liarsDicePlayer);
                }
            }

            //-----

            LiarsDicePersistance.m_Instances.Add(this);

            if (m_Started && !m_Completed)
            {
                m_Timer = new LiarsDiceTimer(this);
                m_Timer.Start();
            }
        }
    }

    public class LiarsDicePlayer
    {
        public PlayerMobile m_Player;
        public bool m_Active;
        public int m_TurnsSkipped = 0;

        public int m_BidCount = 0;
        public int m_BidValue = 0;
        public int m_DiceRemaining = 5;

        public List<int> m_Dice = new List<int>();

        public LiarsDicePlayer(PlayerMobile player)
        {
            m_Player = player;
        }
    }
}
