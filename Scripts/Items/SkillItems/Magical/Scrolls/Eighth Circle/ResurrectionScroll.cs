using System;
using Server;
using Server.Items;

namespace Server.Items
{
	public class ResurrectionScroll : SpellScroll
	{
        public override int IconItemId { get { return 8039; } }
        public override int IconHue { get { return Hue; } }
        public override int IconOffsetX { get { return 45; } }
        public override int IconOffsetY { get { return 40; } }

		[Constructable]
		public ResurrectionScroll() : this( 1 )
		{
            Name = "resurrection scroll";
		}

		[Constructable]
		public ResurrectionScroll( int amount ) : base( 58, 8039, amount )
		{
            Name = "resurrection scroll";
		}

		public ResurrectionScroll( Serial serial ) : base( serial )
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