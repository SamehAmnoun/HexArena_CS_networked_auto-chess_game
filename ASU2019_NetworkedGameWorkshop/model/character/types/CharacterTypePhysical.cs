﻿using System.Collections.Generic;

namespace ASU2019_NetworkedGameWorkshop.model.character.types
{
    public class CharacterTypePhysical : CharacterType
    {

        public static readonly CharacterTypePhysical[] Archer = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                500,
                50, 100,
                2,
                90, 0.8f,
                25, 20),
            new CharacterTypePhysical(
                600,
                50, 100,
                2,
                110, 0.8f,
                30, 30),
            new CharacterTypePhysical(
                700,
                50, 100,
                2,
                125, 0.8f,
                40, 35),
        };// low range ranged mid hp,  dmg

        public static readonly CharacterTypePhysical[] Assassin = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                500,
                50, 100,
                1,
                120, 0.55f,
                25, 20),
            new CharacterTypePhysical(
                600,
                50, 100,
                1,
                150, 0.55f,
                35, 30),
            new CharacterTypePhysical(
                700,
                50, 100,
                1,
                190, 0.55f,
                45, 40),
        };//low atk speed high dmg mid hp melee

        public static readonly CharacterTypePhysical[] Monk = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                500,
                20, 100,
                1,
                60, 1.2f,
                35, 20),
            new CharacterTypePhysical(
                600,
                30, 100,
                1,
                70, 1.2f,
                40, 30),
            new CharacterTypePhysical(
                700,
                40, 100,
                1,
                80, 0.7f,
                45, 40),
        };//high atk speed mid dmg, hp

        public static readonly CharacterTypePhysical[] Ranger = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                500,
                50, 100,
                3,
                60, 0.75f,
                25, 20),
            new CharacterTypePhysical(
                600,
                50, 100,
                3,
                70, 0.75f,
                25, 20),
            new CharacterTypePhysical(
                700,
                50, 100,
                3,
                80, 0.75f,
                25, 20),
        };//high atk speed mid hp mid range

        public static readonly CharacterTypePhysical[] Sentinal = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                600,
                50, 100,
                1,
                50, 0.7f,
                50, 40),
            new CharacterTypePhysical(
                700,
                50, 100,
                1,
                70, 0.7f,
                60, 50),
            new CharacterTypePhysical(
                800,
                70, 100,
                1,
                80, 0.7f,
                70, 60),
        };//mid hp below avg dmg high armour

        public static readonly CharacterTypePhysical[] Slime = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                300,
                10,100,
                1,
                25, 1f,
                10, 10),
            new CharacterTypePhysical(
                400,
                10, 100,
                1,
                35, 1f,
                15, 15),
            new CharacterTypePhysical(
                800,
                50, 100,
                1,
                150, 1f,
                40, 40),
        };//low hp, dmg, armor then explodes


        public static readonly CharacterTypePhysical[] Sniper = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                250,
                10, 100,
                4,
                150, 0.45f,
                15, 20),
            new CharacterTypePhysical(
                350,
                20, 100,
                4,
                190, 0.45f,
                20, 25),
            new CharacterTypePhysical(
                450,
                30, 100,
                4,
                230, 0.45f,
                25, 20),
        };//long range low atk speed low hp high dmg

        public static readonly CharacterTypePhysical[] Warrior = new CharacterTypePhysical[]{
            new CharacterTypePhysical(
                700,
                20, 100,
                1,
                60, 0.7f,
                25, 20),
            new CharacterTypePhysical(
                800,
                40, 100,
                1,
                80, 0.7f,
                35, 30),
            new CharacterTypePhysical(
                900,
                50, 100,
                1,
                100, 0.7f,
                45, 40),
        };//high hp mid dmg, atk speed

        public CharacterTypePhysical(
            int healthPoints,
            int charge, int chargeMax,
            int range,
            int attackDamage, float attackSpeed,
            int armor, int magicResist)
            : base(healthPoints, charge, chargeMax, range, attackDamage, attackSpeed, armor, magicResist)
        {
        }

        public new static IEnumerable<CharacterTypePhysical[]> Values
        {
            get
            {
                yield return Archer;
                yield return Assassin;
                yield return Monk;
                yield return Ranger;
                yield return Sentinal;
                yield return Slime;
                yield return Sniper;
                yield return Warrior;
            }
        }
    }
}
