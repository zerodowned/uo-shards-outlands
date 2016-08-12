using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Server;
using Server.Commands;
using Server.Gumps;
using Server.Items;
using Server.Mobiles;
using Server.Network;
using Server.Spells;
using Server.Spells.First;
using Server.Spells.Second;
using Server.Spells.Third;
using Server.Spells.Fourth;
using Server.Spells.Fifth;
using Server.Spells.Sixth;
using Server.Spells.Seventh;
using Server.Spells.Eighth;

namespace Server
{
    public class ArenaRuleset: Item
    {
        public enum MatchTypeType
        {
            Unranked1vs1,
            Unranked2vs2,
            Unranked3vs3,
            Unranked4vs4,

            Ranked1vs1,
            Ranked2vs2,
            Ranked3vs3,
            Ranked4vs4,
        }

        public enum ArenaRulesetFailureType
        {
            None,
            Dead,
            NotInArenaRegion,
            Transformed,
            EquipmentRestriction,
            WeaponPoisonedAtStart
        }

        public enum EquipmentRestrictionType
        {
            GMRegularMaterials,
            NoRestrictions
        }

        public enum ResourceConsumptionType
        {
            Normal,
            UnlimitedReagentsBandages,
            UnlimitedEverything
        }

        public enum RoundDurationType
        {
            ThreeMinutes,
            FiveMinutes,
            TenMinutes,
            FifteenMinutes,
            TwentyMinutes
        }

        public enum SuddenDeathModeType
        {
            DamageIncreasedTwentyFivePercent,
            DamageIncreasedFiftyPercent,
            DamageIncreasedOneHundredPercent,

            DamageIncreasedTenPercentPerMinute,
            DamageIncreasedTwentyFivePercentPerMinute,
            DamageIncreasedTwentyFiftyPercentPerMinute,

            HealingReducedTwentyFivePercent,
            HealingReducedFiftyPercent,
            NoHealingAllowed
        }

        public MatchTypeType m_MatchType = MatchTypeType.Unranked1vs1;
                
        [CommandProperty(AccessLevel.GameMaster)]
        public int TeamSize
        {
            get
            {
                switch (m_MatchType)
                {
                    case MatchTypeType.Unranked1vs1: return 1; break;
                    case MatchTypeType.Unranked2vs2: return 2; break;
                    case MatchTypeType.Unranked3vs3: return 3; break;
                    case MatchTypeType.Unranked4vs4: return 4; break;

                    case MatchTypeType.Ranked1vs1: return 1; break;
                    case MatchTypeType.Ranked2vs2: return 2; break;
                    case MatchTypeType.Ranked3vs3: return 3; break;
                    case MatchTypeType.Ranked4vs4: return 4; break;
                }

                return 1;
            }            
        }

        public EquipmentRestrictionType m_EquipmentRestriction = EquipmentRestrictionType.GMRegularMaterials;
        public ResourceConsumptionType m_ResourceConsumption = ResourceConsumptionType.Normal;
        public bool m_AllowPoisonedWeapons = false;
        public bool m_ItemsLoseDurability = true;
        public RoundDurationType m_RoundDuration = RoundDurationType.ThreeMinutes; //TEST: Ten Minutes
        public SuddenDeathModeType m_SuddenDeathMode = SuddenDeathModeType.DamageIncreasedTwentyFivePercentPerMinute;

        public List<ArenaSpellRestriction> m_SpellRestrictions = new List<ArenaSpellRestriction>();
        public List<ArenaItemRestriction> m_ItemRestrictions = new List<ArenaItemRestriction>();

        [Constructable]
        public ArenaRuleset(): base(0x0)
        {
            Visible = false;
            Movable = false;

            CreateSpellRestrictionEntries();
            CreateItemRestrictionEntries();
        }

        public ArenaRuleset(Serial serial): base(serial)
        {
        }

        public void CreateSpellRestrictionEntries()
        {
            //Custom Restrictions            
            m_SpellRestrictions.Add(new ArenaSpellRestriction(typeof(PoisonSpell), ArenaSpellRestriction.SpellRestrictionModeType.Unlimited));
            m_SpellRestrictions.Add(new ArenaSpellRestriction(typeof(PoisonFieldSpell), ArenaSpellRestriction.SpellRestrictionModeType.Unlimited));

            m_SpellRestrictions.Add(new ArenaSpellRestriction(typeof(ParalyzeSpell), ArenaSpellRestriction.SpellRestrictionModeType.Unlimited));
            m_SpellRestrictions.Add(new ArenaSpellRestriction(typeof(ParalyzeFieldSpell), ArenaSpellRestriction.SpellRestrictionModeType.Unlimited));

            m_SpellRestrictions.Add(new ArenaSpellRestriction(typeof(MeteorSwarmSpell), ArenaSpellRestriction.SpellRestrictionModeType.Unlimited));
            m_SpellRestrictions.Add(new ArenaSpellRestriction(typeof(ChainLightningSpell), ArenaSpellRestriction.SpellRestrictionModeType.Unlimited));
            m_SpellRestrictions.Add(new ArenaSpellRestriction(typeof(EarthquakeSpell), ArenaSpellRestriction.SpellRestrictionModeType.Unlimited));
        }

        public void CreateItemRestrictionEntries()
        {
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(Pouch), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BaseHealPotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BaseCurePotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BaseRefreshPotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BaseStrengthPotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BaseAgilityPotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BaseExplosionPotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BasePoisonPotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
            m_ItemRestrictions.Add(new ArenaItemRestriction(typeof(BaseMagicResistPotion), ArenaItemRestriction.ItemRestrictionModeType.Unlimited));
        }

        public bool IsSpellAllowed(PlayerMobile player, Type spellType)
        {
            if (player.m_CompetitionContext == null) return true;
            if (player.m_CompetitionContext.m_ArenaParticipant == null) return true;
            if (player.m_CompetitionContext.m_ArenaParticipant.m_SpellUsages == null) return true;
            
            //TEST: Add Always Restricted Spells
            
            foreach (ArenaSpellRestriction spellRestriction in m_SpellRestrictions)
            {
                if (spellRestriction == null)
                    continue;

                //Custom 
                if (spellRestriction.m_SpellType == spellType)
                {
                    int maxUsesAllowed = 0;

                    switch (spellRestriction.m_RestrictionMode)
                    {
                        case ArenaSpellRestriction.SpellRestrictionModeType.Disabled: maxUsesAllowed = 0; break;
                        case ArenaSpellRestriction.SpellRestrictionModeType.OneUse: maxUsesAllowed = 1; break;
                        case ArenaSpellRestriction.SpellRestrictionModeType.ThreeUses: maxUsesAllowed = 3; break;
                        case ArenaSpellRestriction.SpellRestrictionModeType.FiveUses: maxUsesAllowed = 5; break;
                        case ArenaSpellRestriction.SpellRestrictionModeType.TenUses: maxUsesAllowed = 10; break;
                        case ArenaSpellRestriction.SpellRestrictionModeType.TwentyFiveUses: maxUsesAllowed = 25; break;
                        case ArenaSpellRestriction.SpellRestrictionModeType.FiftyUses: maxUsesAllowed = 50; break;
                        case ArenaSpellRestriction.SpellRestrictionModeType.Unlimited: maxUsesAllowed = 10000; break;
                    }

                    if (maxUsesAllowed == 0)
                        return false;

                    ArenaSpellUsage arenaSpellUsage = player.m_CompetitionContext.m_ArenaParticipant.GetSpellUsage(spellType);

                    if (arenaSpellUsage != null)
                    {
                        if (arenaSpellUsage.m_Uses >= maxUsesAllowed)
                            return false;

                        else
                            return true;
                    }
                }
            }

            return true;
        }

        public bool IsItemAllowed(PlayerMobile player, Item item)
        {
            if (player.m_CompetitionContext == null) return true;
            if (player.m_CompetitionContext.m_ArenaParticipant == null) return true;
            if (player.m_CompetitionContext.m_ArenaParticipant.m_SpellUsages == null) return true;

            Type itemType = item.GetType();

            foreach (ArenaItemRestriction itemRestriction in m_ItemRestrictions)
            {
                if (itemRestriction == null)
                    continue;

                if (itemRestriction.m_ItemType == typeof(Pouch))
                {
                    if (item is TrapableContainer)
                    {
                        TrapableContainer container = item as TrapableContainer;

                        if (container.TrapType != TrapType.None)
                        {
                            int maxUsesAllowed = 0;

                            switch (itemRestriction.m_RestrictionMode)
                            {
                                case ArenaItemRestriction.ItemRestrictionModeType.Disabled: maxUsesAllowed = 0; break;
                                case ArenaItemRestriction.ItemRestrictionModeType.OneUse: maxUsesAllowed = 1; break;
                                case ArenaItemRestriction.ItemRestrictionModeType.ThreeUses: maxUsesAllowed = 3; break;
                                case ArenaItemRestriction.ItemRestrictionModeType.FiveUses: maxUsesAllowed = 5; break;
                                case ArenaItemRestriction.ItemRestrictionModeType.TenUses: maxUsesAllowed = 10; break;
                                case ArenaItemRestriction.ItemRestrictionModeType.TwentyFiveUses: maxUsesAllowed = 25; break;
                                case ArenaItemRestriction.ItemRestrictionModeType.FiftyUses: maxUsesAllowed = 50; break;
                                case ArenaItemRestriction.ItemRestrictionModeType.Unlimited: maxUsesAllowed = 10000; break;
                            }

                            if (maxUsesAllowed == 0)
                                return false;

                            ArenaItemUsage arenaItemUsage = player.m_CompetitionContext.m_ArenaParticipant.GetItemUsage(itemType);

                            if (arenaItemUsage != null)
                            {
                                if (arenaItemUsage.m_Uses >= maxUsesAllowed)
                                    return false;

                                else
                                    return true;
                            }
                        }
                    }                    
                }

                else if (itemType == itemRestriction.m_ItemType)
                {
                    int maxUsesAllowed = 0;

                    switch (itemRestriction.m_RestrictionMode)
                    {
                        case ArenaItemRestriction.ItemRestrictionModeType.Disabled: maxUsesAllowed = 0; break;
                        case ArenaItemRestriction.ItemRestrictionModeType.OneUse: maxUsesAllowed = 1; break;
                        case ArenaItemRestriction.ItemRestrictionModeType.ThreeUses: maxUsesAllowed = 3; break;
                        case ArenaItemRestriction.ItemRestrictionModeType.FiveUses: maxUsesAllowed = 5; break;
                        case ArenaItemRestriction.ItemRestrictionModeType.TenUses: maxUsesAllowed = 10; break;
                        case ArenaItemRestriction.ItemRestrictionModeType.TwentyFiveUses: maxUsesAllowed = 25; break;
                        case ArenaItemRestriction.ItemRestrictionModeType.FiftyUses: maxUsesAllowed = 50; break;
                        case ArenaItemRestriction.ItemRestrictionModeType.Unlimited: maxUsesAllowed = 10000; break;
                    }

                    if (maxUsesAllowed == 0)
                        return false;

                    ArenaItemUsage arenaItemUsage = player.m_CompetitionContext.m_ArenaParticipant.GetItemUsage(itemType);

                    if (arenaItemUsage != null)
                    {
                        if (arenaItemUsage.m_Uses >= maxUsesAllowed)
                            return false;

                        else
                            return true;
                    }
                }
            }

            return true;
        }

        public ArenaRulesetFailureType CheckForRulesetViolations(PlayerMobile player)
        {
            if (player == null) return ArenaRulesetFailureType.NotInArenaRegion;
            if (player.Deleted) return ArenaRulesetFailureType.NotInArenaRegion;

            if (!player.Alive)
                return ArenaRulesetFailureType.Dead;            
            
            //TEST: Not in Arena Zone

            //TEST: Disguised / Polymorphed / Incognito

            //TEST: Restricted Equipment

            //TEST: Weapon Poisoned At Start

            return ArenaRulesetFailureType.None;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);

            //Version 0
            writer.Write((int)m_MatchType);
            writer.Write((int)m_EquipmentRestriction);
            writer.Write((int)m_ResourceConsumption);
            writer.Write(m_AllowPoisonedWeapons);
            writer.Write(m_ItemsLoseDurability);
            writer.Write((int)m_RoundDuration);
            writer.Write((int)m_SuddenDeathMode);

            writer.Write(m_SpellRestrictions.Count);
            for (int a = 0; a < m_SpellRestrictions.Count; a++)
            {
                if (m_SpellRestrictions[a].m_SpellType == null)
                    writer.Write("null");
                else
                    writer.Write(m_SpellRestrictions[a].m_SpellType.ToString());

                writer.Write((int)m_SpellRestrictions[a].m_RestrictionMode);
            }

            writer.Write(m_ItemRestrictions.Count);
            for (int a = 0; a < m_ItemRestrictions.Count; a++)
            {
                if (m_ItemRestrictions[a].m_ItemType == null)
                    writer.Write("null");
                else
                    writer.Write(m_ItemRestrictions[a].m_ItemType.ToString());

                writer.Write((int)m_ItemRestrictions[a].m_RestrictionMode);
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();

            //Version 0
            if (version >= 0)
            {
                m_MatchType = (MatchTypeType)reader.ReadInt();
                m_EquipmentRestriction = (EquipmentRestrictionType)reader.ReadInt();
                m_ResourceConsumption = (ResourceConsumptionType)reader.ReadInt();
                m_AllowPoisonedWeapons = reader.ReadBool();
                m_ItemsLoseDurability = reader.ReadBool();
                m_RoundDuration = (RoundDurationType)reader.ReadInt();
                m_SuddenDeathMode = (SuddenDeathModeType)reader.ReadInt();

                int spellRestrictionCount = reader.ReadInt();
                for (int a = 0; a < spellRestrictionCount; a++)
                {
                    string typeText = reader.ReadString();
                    Type spellType = null;

                    if (typeText != "null")
                        spellType = Type.GetType(typeText);

                    ArenaSpellRestriction.SpellRestrictionModeType restrictionMode = (ArenaSpellRestriction.SpellRestrictionModeType)reader.ReadInt();

                    if (spellType != null)
                        m_SpellRestrictions.Add(new ArenaSpellRestriction(spellType, restrictionMode));
                }

                int itemRestrictionCount = reader.ReadInt();
                for (int a = 0; a < itemRestrictionCount; a++)
                {
                    string typeText = reader.ReadString();
                    Type itemType = null;

                    if (typeText != "null")
                        itemType = Type.GetType(typeText);

                    ArenaItemRestriction.ItemRestrictionModeType restrictionMode = (ArenaItemRestriction.ItemRestrictionModeType)reader.ReadInt();

                    if (itemType != null)
                        m_ItemRestrictions.Add(new ArenaItemRestriction(itemType, restrictionMode));
                }
            }
        }
    }

    public class ArenaSpellRestriction
    {
        public enum SpellRestrictionModeType
        {
            Disabled,
            Unlimited,
            OneUse,
            ThreeUses,
            FiveUses,
            TenUses,
            TwentyFiveUses,
            FiftyUses,
        }

        public Type m_SpellType;
        public SpellRestrictionModeType m_RestrictionMode = SpellRestrictionModeType.Unlimited;

        public ArenaSpellRestriction(Type spellType, SpellRestrictionModeType restrictionMode)
        {
            m_SpellType = spellType;
            m_RestrictionMode = restrictionMode;
        }
    }

    public class ArenaItemRestriction
    {
        public enum ItemRestrictionModeType
        {
            Disabled,
            Unlimited,
            OneUse,
            ThreeUses,
            FiveUses,
            TenUses,
            TwentyFiveUses,
            FiftyUses,
        }

        public Type m_ItemType;
        public ItemRestrictionModeType m_RestrictionMode = ItemRestrictionModeType.Unlimited;

        public ArenaItemRestriction(Type itemType, ItemRestrictionModeType restrictionMode)
        {
            m_ItemType = itemType;
            m_RestrictionMode = restrictionMode;
        }
    }
}