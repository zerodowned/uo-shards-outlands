using System;
using Server;

namespace Server.Items
{
	public class Subdue : Scythe
	{
		public override int LabelNumber{ get{ return 1094930; } } // Subdue [Replica]

		public override int InitMinHits{ get{ return 150; } }
		public override int InitMaxHits{ get{ return 150; } }
        
		[Constructable]
		public Subdue()
		{
			Hue = 0x2cb;
		}

		public Subdue( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );
		}
		
		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
	}
}
