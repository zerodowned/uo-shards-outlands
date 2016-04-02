using System;
using Server;
using Server.Engines.Craft;
using System.Collections.Generic;

namespace Server.Items
{
	public enum PotionEffect
	{
		CureLesser,
		Cure,
		CureGreater,
		Agility,
		AgilityGreater,
		Strength,
		StrengthGreater,
		PoisonLesser,
		Poison,
		PoisonGreater,
		PoisonDeadly,
		Refresh,
		RefreshTotal,
		HealLesser,
		Heal,
		HealGreater,
		ExplosionLesser,
		Explosion,
		ExplosionGreater,
		Custom,
		Conflagration,
		ConflagrationGreater,
		MaskOfDeath,		// Mask of Death is not available in OSI but does exist in cliloc files
		MaskOfDeathGreater,	// included in enumeration for compatability if later enabled by OSI
		ConfusionBlast,
		ConfusionBlastGreater,
		Invisibility,
		Parasitic,
		Darkglow,
        LesserMagicResist,
        MagicResist,
        GreaterMagicResist
	}

	public abstract class BasePotion : Item, ICraftable, ICommodity
	{
		private PotionEffect m_PotionEffect;

		public override int MaxStack
		{
			get { return 10; }
		}

		public PotionEffect PotionEffect
		{
			get
			{
				return m_PotionEffect;
			}
			set
			{
				m_PotionEffect = value;
				InvalidateProperties();
			}
		}

		int ICommodity.DescriptionNumber { get { return LabelNumber; } }
		bool ICommodity.IsDeedable { get { return (Core.ML); } }

		public override int LabelNumber{ get{ return 1041314 + (int)m_PotionEffect; } }

		public BasePotion( int itemID, PotionEffect effect ) : base( itemID )
		{
			m_PotionEffect = effect;

			Stackable = true;
			Weight = 1.0;
		}

		public BasePotion( Serial serial ) : base( serial )
		{
		}

		public virtual bool RequireFreeHand{ get{ return true; } }

		public static bool HasFreeHand( Mobile m )
		{
			Item handOne = m.FindItemOnLayer( Layer.OneHanded );
			Item handTwo = m.FindItemOnLayer( Layer.TwoHanded );

			if ( handTwo is BaseWeapon )
				handOne = handTwo;

			return ( handOne == null || handTwo == null );
		}

		public override void OnDoubleClick( Mobile from )
		{
            if (from.CombatProhibited)
            {
                from.SendMessage("That action is temporarily disabled.");
                return;
            }
			if ( !Movable )
				return;

            if (Engines.ConPVP.DuelContext.CheckSuddenDeath(from))
            {
                from.SendMessage(0x22, "You cannot use this item when in sudden death.");
                return;
            }

			if ( from.InRange( this.GetWorldLocation(), 1 ) )
			{
				if ( !RequireFreeHand || HasFreeHand( from ) )
					Drink( from );
				else
					from.SendLocalizedMessage( 502172 ); // You must have a free hand to drink a potion.
			}
			else
			{
				from.SendLocalizedMessage( 502138 ); // That is too far away for you to use
			}
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 1 ); // version

			writer.Write( (int) m_PotionEffect );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

			switch ( version )
			{
				case 1:
				case 0:
				{
					m_PotionEffect = (PotionEffect)reader.ReadInt();
					break;
				}
			}

			if( version ==  0 )
				Stackable = Core.ML;
		}

		public abstract void Drink( Mobile from );

		public static void PlayDrinkEffect( Mobile m )
		{
			m.RevealingAction();

			m.PlaySound( 0x2D6 );

			#region Dueling
			if ( !Engines.ConPVP.DuelContext.IsFreeConsume( m ) )
				m.AddToBackpack( new Bottle() );
			#endregion

			if ( m.Body.IsHuman /*&& !m.Mounted*/ )
				m.Animate( 34, 5, 1, true, false, 0 );
		}

		public static int EnhancePotions( Mobile m )
		{
			int EP = AosAttributes.GetValue( m, AosAttribute.EnhancePotions );
			if ( Core.ML && EP > 50 )
				EP = 50;
			return EP;
		}

		public static TimeSpan Scale( Mobile m, TimeSpan v )
		{
			if ( !Core.AOS )
				return v;

			double scalar = 1.0 + ( 0.01 * EnhancePotions( m ) );

			return TimeSpan.FromSeconds( v.TotalSeconds * scalar );
		}

		public static double Scale( Mobile m, double v )
		{
			if ( !Core.AOS )
				return v;

			double scalar = 1.0 + ( 0.01 * EnhancePotions( m ) );

			return v * scalar;
		}

		public static int Scale( Mobile m, int v )
		{
			if ( !Core.AOS )
				return v;

			return AOS.Scale( v, 100 + EnhancePotions( m ) );
		}

		public override bool StackWith( Mobile from, Item dropped, bool playSound )
		{
			if( dropped is BasePotion && ((BasePotion)dropped).m_PotionEffect == m_PotionEffect )
				return base.StackWith( from, dropped, playSound );

			return false;
		}

		protected override void OnAmountChange(int oldValue)
		{
			int newValue = this.Amount;

			UpdateTotal(this, TotalType.Items, newValue - oldValue);
		}

		public override int GetTotal(TotalType type)
		{
			int total = base.GetTotal(type);

			if (type == TotalType.Items)
			{
				return Amount - 1;          // RunUO seems to treat TotalItems as a 0-based count for some reason
			}

			return total;
		}

		#region ICraftable Members

		public int OnCraft( int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue )
		{
			if ( craftSystem is DefAlchemy )
			{
				Container pack = from.Backpack;

				if ( pack != null )
				{
					if ( (int) PotionEffect >= (int) PotionEffect.Invisibility )
						return 1;

					List<PotionKeg> kegs = pack.FindItemsByType<PotionKeg>();

					for ( int i = 0; i < kegs.Count; ++i )
					{
						PotionKeg keg = kegs[i];

						if ( keg == null )
							continue;

						if ( keg.Held <= 0 || keg.Held >= 100 )
							continue;

						if ( keg.Type != PotionEffect )
							continue;

						++keg.Held;

						Consume();
						from.AddToBackpack( new Bottle() );

						return -1; // signal placed in keg
					}
				}
			}

			return 1;
		}

		#endregion
	}
}