﻿using System;
using Server;
using Server.Targeting;
using Server.Network;
using Server.Mobiles;
using Server.Multis;
using Server.Gumps;
using System.Collections;
using System.Collections.Generic;

namespace Server.Items
{
    public class HunterOutfittingShipUpgrade : ShipUpgradeDeed
    {
        [Constructable]
        public HunterOutfittingShipUpgrade(): base()
        {
            Name = "hunter";

            m_OutfittingUpgrade = ShipUpgrades.OutfittingType.Hunter;

            m_UpgradeType = ShipUpgrades.UpgradeType.Outfitting;
        }

        public HunterOutfittingShipUpgrade(Serial serial): base(serial)
        {
        } 

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); //version 
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}