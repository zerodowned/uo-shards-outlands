using System;
using Server.Items;
using Server.Network;

namespace Server.Items
{
	[FlipableAttribute( 0xF43, 0xF44 )]
    public class Hatchet : BaseAxe, IUsesRemaining
	{
        public static int GetSBPurchaseValue() { return 1; }
        public static int GetSBSellValue() { return Item.SBDetermineSellPrice(GetSBPurchaseValue()); }

        public override int BaseMinDamage { get { return 1; } }
        public override int BaseMaxDamage { get { return 2; } }
        public override int BaseSpeed { get { return 40; } }

        public override int InitMinHits { get { return 40; } }
        public override int InitMaxHits { get { return 60; } }

        public override bool TrainingWeapon { get { return true; } }

        public override int IconItemId { get { return 3908; } }
        public override int IconHue { get { return Hue; } }
        public override int IconOffsetX { get { return 50; } }
        public override int IconOffsetY { get { return 37; } }

		[Constructable]
		public Hatchet() : base( 0xF43 )
		{
            Name = "hatchet";
			Weight = 3.0;

            UsesRemaining = 50;
            ShowUsesRemaining = true;
		}

        public override void OnSingleClick(Mobile from)
        {           
            base.OnSingleClick(from);

            LabelTo(from, "(Uses Remaining: " + UsesRemaining.ToString() + ")");
        }

		public Hatchet( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
		}
	}
}