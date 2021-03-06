using ASU2019_NetworkedGameWorkshop.controller;
using ASU2019_NetworkedGameWorkshop.controller.networking;
using ASU2019_NetworkedGameWorkshop.controller.networking.game;
using ASU2019_NetworkedGameWorkshop.model.character.types;
using ASU2019_NetworkedGameWorkshop.model.grid;
using ASU2019_NetworkedGameWorkshop.model.spell;
using ASU2019_NetworkedGameWorkshop.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static ASU2019_NetworkedGameWorkshop.model.character.StatusEffect;

namespace ASU2019_NetworkedGameWorkshop.model.character
{
    public class Character : GraphicsObject
    {
        public enum Teams { Red, Blue };
        private static readonly Font DEBUG_FONT = new Font("Roboto", 10f);

        public readonly Teams team;
        public readonly GameManager gameManager;
        public readonly GameNetworkManager gameNetworkManager;
        public readonly Grid grid;

        private readonly StatBar hpBar, charageBar;
        private readonly Brush brush;
        private readonly bool isBlue;
        public CharacterType[] characterType { get; private set; }
        private readonly Dictionary<StatusType, int> statsAdder;
        private readonly Dictionary<StatusType, float> statsMultiplier;
        private bool spellsUIVisibleBuy = false;
        private int id;
        private List<StatusEffect> statusEffects;

        private long nextAtttackTime;

        public Dictionary<Spells[], int> SpellLevel { get; set; }
        public Spells[] DefaultSkill { get; set; }
        public ChooseSpell ChooseSpell { get; set; }
        public InactiveSpell InactiveSpell { get; set; }
        public List<Spells[]> ActiveSpells { get; set; }
        public List<Spells[]> InactiveSpells { get; set; }
        public bool SpellReady { get; set; }


        public Dictionary<StatusType, int> Stats { get; private set; }
        public Tile CurrentTile { get; set; }
        public Character CurrentTarget { get; private set; }

        /// <summary>
        /// CharacterType according to the Character's current level.
        /// </summary>
        public CharacterType CharacterType
        {
            get
            {
                return characterType[CurrentLevel];
            }
        }
        public bool IsDead { get; set; }
        public int CurrentLevel { get; private set; }
        public Tile ToMoveTo { get; private set; }

        public List<Spells[]> LearnedSpells { get; }

        public Character(Grid grid,
                         Tile currentTile,
                         Teams team,
                         CharacterType[] characterType,
                         GameManager gameManager,
                         GameNetworkManager gameNetworkManager)
        {
            this.grid = grid;
            currentTile.CurrentCharacter = this;
            this.team = team;
            SpellReady = false;
            this.characterType = characterType;
            this.gameManager = gameManager;
            this.gameNetworkManager = gameNetworkManager;
            ChooseSpell = new ChooseSpell(this, ActiveSpells, gameNetworkManager);
            InactiveSpell = new InactiveSpell(this, InactiveSpells, gameNetworkManager);

            Stats = CharacterType.statsCopy();
            ActiveSpells = new List<Spells[]>();
            InactiveSpells = new List<Spells[]>();
            LearnedSpells = new List<Spells[]>();
            SpellLevel = new Dictionary<Spells[], int>();
            brush = (team == Teams.Blue) ? Brushes.BlueViolet : Brushes.Red;
            isBlue = (team == Teams.Blue);
            statusEffects = new List<StatusEffect>();

            IsDead = false;

            statsMultiplier = new Dictionary<StatusType, float>();
            statsAdder = new Dictionary<StatusType, int>();
            foreach (StatusType statusType in Enum.GetValues(typeof(StatusType)))
            {
                statsAdder.Add(statusType, 0);
                statsMultiplier.Add(statusType, 1f);
            }

            hpBar = new StatBar(this,
                team == Teams.Blue ? Brushes.GreenYellow : Brushes.OrangeRed, 0);
            charageBar = new StatBar(this, Brushes.Blue, 1);
        }

        /// <summary>
        /// Increases the character's Health Points by healValue after applying modifiers.
        /// 
        /// <para>Can NOT increases the Character's Health Points above the Character's type max Health Points.</para>
        /// </summary>
        /// <param name="healValue">the amount the character should heal.</param>
        /// <exception cref="ArgumentException">if the healValue is Negative.</exception>
        public void healHealthPoints(int healValue)
        {
            if (healValue < 0)
            {
                throw new ArgumentException("healValue should be positive: " + healValue);
            }
            Stats[StatusType.HealthPoints] = Math.Min(Stats[StatusType.HealthPoints] + healValue,
                                                        Stats[StatusType.HealthPointsMax]);
        }

        public void learnSpell(Spells[] spell)
        {
            SpellLevel.Add(spell, 0);
            LearnedSpells.Add(spell);
            InactiveSpells.Add(spell);
        }
        public void upgradeSpell(Spells[] spell)
        {
            if (SpellLevel[spell] < spell.Count() - 1)
                SpellLevel[spell] += 1;
        }

        /// <summary>
        /// Decreases the character's Health Points by healValue after applying modifiers.
        /// </summary>
        /// <param name="dmgValue">The damage the character took.</param>
        /// <param name="damageType">The type of damage the Character took.</param>
        /// <exception cref="ArgumentException">if the dmgValue is Negative.</exception>
        public void takeDamage(int dmgValue, DamageType damageType)
        {
            if (dmgValue < 0)
            {
                throw new ArgumentException("dmgValue should be positive: " + dmgValue);
            }

            Stats[StatusType.HealthPoints] -= (int)(dmgValue * 100 /
                (100 + (damageType == DamageType.MagicDamage ? Stats[StatusType.Armor] : Stats[StatusType.MagicResist])));
            if (Stats[StatusType.HealthPoints] <= 0)
            {
                Stats[StatusType.HealthPoints] = 0;
                IsDead = true;

                if (SpellReady == true)
                {
                    hideChooseSpellUI();
                }

                CurrentTile.CurrentCharacter = null;
                CurrentTile = null;
                if (ToMoveTo != null)
                    ToMoveTo.Walkable = true;
            }
            else
            {
                Stats[StatusType.Charge] = Math.Min(Stats[StatusType.Charge] + 8, Stats[StatusType.ChargeMax]);
            }

        }

        public void reset()
        {
            Stats = CharacterType.statsCopy();
            statusEffects.Clear();
            IsDead = false;
            CurrentTarget = null;
            ToMoveTo = null;
            hideChooseSpellUI();
        }

        public void addStatusEffect(StatusEffect statusEffect)
        {
            statusEffect.RemoveEffectTimeStamp += gameManager.ElapsedTime;
            applyStatusEffect(statusEffect);
            statusEffects.Add(statusEffect);
        }

        public void resetMana()
        {
            Stats[StatusType.Charge] = 0;
        }

        public bool tick()
        {
            if (ToMoveTo != null)
            {
                CurrentTile.CurrentCharacter = null;
                CurrentTile.Walkable = true;
                ToMoveTo.CurrentCharacter = this;
                ToMoveTo = null;
                if (SpellReady)
                {
                    ChooseSpell.refreshLocation(this);
                }
                return true;
            }
            return false;
        }

        public void showChooseSpell()
        {
            ChooseSpell.refreshLocation(this);
            gameManager.addRangeToForm(ChooseSpell);
            SpellReady = true;
        }

        public void hideChooseSpellUI()
        {
            gameManager.removeRangeFromForm(ChooseSpell);
            SpellReady = false;
        }
        public void hideAllSpellUI()
        {
            gameManager.removeRangeFromForm(ChooseSpell, InactiveSpell);
            SpellReady = false;
        }

        public bool updateBuy()
        {
            if (!spellsUIVisibleBuy && this.CurrentTile == gameManager.SelectedTile && LearnedSpells.Count != 0)
            {
                ChooseSpell.refreshPanel(this, ActiveSpells);
                InactiveSpell.refreshPanel(InactiveSpells);
                gameManager.addRangeToForm(ChooseSpell);
                if (team == Teams.Blue)
                {
                    gameManager.addRangeToForm(InactiveSpell);
                }
                spellsUIVisibleBuy = true;
                return true;
            }
            else if (this.CurrentTile != gameManager.SelectedTile)
            {
                gameManager.removeRangeFromForm(InactiveSpell, ChooseSpell);
                spellsUIVisibleBuy = false;
                return true;
            }
            return false;
        }
        public bool update()
        {
            if (spellsUIVisibleBuy)
            {
                gameManager.removeRangeFromForm(InactiveSpell, ChooseSpell);
                spellsUIVisibleBuy = false;
                return true;
            }
            statusEffects = statusEffects.Where(effect =>
            {
                if (effect.RemoveEffectTimeStamp < gameManager.ElapsedTime)
                {
                    foreach (StatusEffect item in statusEffects)
                    {
                        if (statusEffects.IndexOf(effect) < statusEffects.IndexOf(item))
                        {
                            item.inverseValue();
                            applyStatusEffect(item);
                            effect.inverseValue();
                            applyStatusEffect(effect);
                            item.inverseValue();
                            applyStatusEffect(item);
                            return false;
                        }
                    }
                    effect.inverseValue();
                    applyStatusEffect(effect);
                    return false;
                }
                return true;
            }).ToList();


            if (Stats[StatusType.Charge] == Stats[StatusType.ChargeMax]
                && ActiveSpells.Count != 0)
            {
                if (DefaultSkill == null)
                {
                    int charIndex = gameManager.TeamBlue.IndexOf(this);
                    DefaultSkill = ActiveSpells[0];
                    gameNetworkManager.enqueueMsg(NetworkMsgPrefix.DefaultSkill, GameNetworkUtilities.serializeSpellActionMoving(ActiveSpells[0], charIndex));
                }
                DefaultSkill[SpellLevel[DefaultSkill]].castSpell(this);
                hideChooseSpellUI();
                resetMana();
            }

            if (ToMoveTo == null)
            {
                if (CurrentTarget == null
                    || CurrentTarget.IsDead
                    || PathFinding.getDistance(CurrentTile, CurrentTarget.CurrentTile) > Stats[StatusType.Range])
                {
                    List<Tile> path = null;
                    try
                    {
                        (path, CurrentTarget) = PathFinding.findPathToClosestEnemy(CurrentTile, team, grid, gameManager);

                    }
                    catch (PathFinding.PathNotFoundException)
                    {
                        return false;
                    }
                    if (PathFinding.getDistance(CurrentTile, CurrentTarget.CurrentTile) > Stats[StatusType.Range])
                    {
                        ToMoveTo = path[0];
                        ToMoveTo.Walkable = false;
                    }
                }
                else
                {
                    if (gameManager.ElapsedTime > nextAtttackTime)
                    {
                        nextAtttackTime = gameManager.ElapsedTime + Stats[StatusType.AttackSpeed];
                        CurrentTarget.takeDamage(Stats[StatusType.AttackDamage], DamageType.PhysicalDamage);
                        Stats[StatusType.Charge] = Math.Min(Stats[StatusType.Charge] + 4, Stats[StatusType.ChargeMax]);

                        return true;
                    }
                }
            }
            return false;
        }

        public void levelUp()
        {
            if (CurrentLevel < CharacterType.MAX_CHAR_LVL - 1)
            {
                CurrentLevel++;
                Stats = CharacterType.statsCopy();
            }
        }

        private void applyStatusEffect(StatusEffect statusEffect)
        {
            if (statusEffect.Type == StatusEffectType.Adder)
            {
                statsAdder[statusEffect.StatusType] += (int)statusEffect.Value;
                Stats[statusEffect.StatusType] = (int)Math.Round(Stats[statusEffect.StatusType] + statusEffect.Value);
            }
            else
            {

                statsMultiplier[statusEffect.StatusType] *= statusEffect.Value;

                Stats[statusEffect.StatusType] = (int)Math.Round(Stats[statusEffect.StatusType] * statusEffect.Value);
            }

        }
        public override void draw(Graphics graphics)
        {

            if (!IsDead)
            {
                Bitmap image;
                if (isBlue)
                {
                    image = this.CharacterType.blueImage;
                }
                else
                {
                    image = this.CharacterType.redImage;
                }
                graphics.DrawImage(image, new Point((int)(CurrentTile.centerX - image.Width / 1.5), (int)(CurrentTile.centerY - image.Height / 1.5)));

                hpBar.updateTrackedAndDraw(graphics, Stats[StatusType.HealthPoints], Stats[StatusType.HealthPointsMax]);
                charageBar.updateTrackedAndDraw(graphics, Stats[StatusType.Charge], Stats[StatusType.ChargeMax]);
            }
        }

        /// <summary>
        /// Draws a string containing the Characters Classes on the character.
        /// 
        /// <para>Calls the DrawDebug() of the Character's Statbars.</para>
        /// </summary>
        /// <param name="graphics">graphics object to draw on.</param>
        public override void drawDebug(Graphics graphics)
        {
            if (!IsDead)
            {
                graphics.DrawString(CharacterType.ToString(),
                DEBUG_FONT, Brushes.White,
                CurrentTile.centerX - CharacterType.WIDTH_HALF,
                CurrentTile.centerY);

                hpBar.drawDebug(graphics);
                charageBar.drawDebug(graphics);
            }
        }

    }
}
