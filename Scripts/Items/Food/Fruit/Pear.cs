using System;

namespace Server.Items
{
    public class Pear : Food
    {
        [Constructable]
        public Pear(): this(1)
        {
        }

        [Constructable]
        public Pear(int amount): base( 0x994)
        {
            Name = "pear";

            Weight = 1.0;
            Amount = amount;
        }

        public Pear(Serial serial): base(serial)
        {
        }
        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}