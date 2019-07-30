﻿using ASU2019_NetworkedGameWorkshop.model.character;
using ASU2019_NetworkedGameWorkshop.model.spell.types;
using ASU2019_NetworkedGameWorkshop.Properties;
using System.Collections.Generic;
using System.Drawing;

namespace ASU2019_NetworkedGameWorkshop.model.spell
{
    public class Spells
    {
        private readonly static Dictionary<int, Spells[]> spellsList;
        public int Id { get; set; }

        public static int ID = 0;

        public static Spells[] getSpell(int id)
        {
            return spellsList[id];
        }


        public static readonly Spells[] AwesomeFireball = new Spells[]{
            new Spells(
                100,
                new Target(false, CastTarget.CurrentTarget),
                SpellType.Damage,
                Resources.fireball_red_1,
                1),
            new Spells(
                200,
                new Target(false, CastTarget.CurrentTarget),
                SpellType.Damage,
                Resources.fireball_red_1,
                2)
            };

        public static readonly Spells[] AwesomeFireballAOE = new Spells[]{
            new Spells(
                200,
                new Target(false, true, 3, CastTarget.CurrentTarget),
                SpellType.Damage,
                Resources.fireball_acid_3,
                1) };

        public static readonly Spells[] AwesomeFireballRandom = new Spells[]{
            new Spells(
                200,
                new Target(false, CastTarget.Random),
                SpellType.Damage,
                Resources.fireball_sky_1,
                1) };

        public static readonly Spells[] Heal = new Spells[]{
            new Spells(
                1000,
                new Target(false, CastTarget.Self),
                SpellType.Heal,
                Resources.heal_jade_1,
                1) };

        public static readonly Spells[] VeigarDebug = new Spells[]{
            new Spells(
                0,
                new Target(false, CastTarget.Self),
                new SpellType(new StatusEffect(StatusType.HealthPoints, 1.2f, 1100, StatusEffect.StatusEffectType.Multiplier)),
                Resources.fireball_sky_1,
                1) };

        public static readonly Spells[] Execute = new Spells[]{
            new Spells(
                200,
                new Target(false, CastTarget.LowHealth),
                SpellType.Damage,
                Resources.fireball_red_1,
                1) };
        static Spells()
        {
            spellsList = new Dictionary<int, Spells[]>();
            foreach (var spell in Values)
            {
                foreach (var spellLevel in spell)
                {
                    spellLevel.Id = ID;
                }
                spellsList[ID++] = spell;
            }
        }

        public static IEnumerable<Spells[]> Values
        {
            get
            {
                yield return AwesomeFireball;
                yield return AwesomeFireballAOE;
                yield return AwesomeFireballRandom;
                yield return Heal;
                yield return VeigarDebug;
                yield return Execute;
            }
        }

        public int AbilityValue { get; private set; }
        public Target Target { get; private set; }
        public SpellType SpellType { get; private set; }
        public Image Image { get; private set; }
        public int Level { get; private set; }
        public Spells(int abilityValue, Target target, SpellType spellType, Image image, int level)
        {
            AbilityValue = abilityValue;
            Target = target;
            SpellType = spellType;
            Image = image;
            Level = level;
        }

        private List<Character> specifyTargets(Character caster)
        {
            Target.Caster = caster;
            return Target.getTargets();
        }

        public void castSpell(Character caster)
        {
            SpellType.cast(specifyTargets(caster), AbilityValue);
        }

    }
}
