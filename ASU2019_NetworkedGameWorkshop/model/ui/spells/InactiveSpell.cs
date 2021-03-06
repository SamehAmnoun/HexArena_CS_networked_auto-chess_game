﻿using ASU2019_NetworkedGameWorkshop.controller.networking;
using ASU2019_NetworkedGameWorkshop.controller.networking.game;
using ASU2019_NetworkedGameWorkshop.model.character;
using ASU2019_NetworkedGameWorkshop.model.grid;
using ASU2019_NetworkedGameWorkshop.model.spell;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ASU2019_NetworkedGameWorkshop.model
{
    public class InactiveSpell : Panel
    {
        private readonly GameNetworkManager gameNetworkManager;
        private const float WIDTH = Tile.WIDTH - 30 * 2,
            WIDTH_HALF = WIDTH / 2f,
            height = 8;
        private const float HEX_OFFSET_Y = Tile.HEIGHT * 0.2f;
        private const int BACK_PADDING_H = 6;
        private const float BACK_OFFSET_X = WIDTH_HALF + BACK_PADDING_H / 2f,
            BACK_OFFSET_Y = 2f;
        private const float BACK_WIDTH = WIDTH + BACK_PADDING_H + 4,
            BACK_HEIGHT = height + BACK_OFFSET_Y + 12;

        private const int ROW_HEIGHT = 20;
        private const int IMAGE_SIZE = 18;
        private const int IMAGE_PADDING_Y = 2,
                          IMAGE_PADDING_X = 2;
        private readonly Character character;
        private readonly float offsetY;
        private readonly float backOffsetY;

        public InactiveSpell(Character character, List<Spells[]> spells, GameNetworkManager gameNetworkManager)
        {
            this.gameNetworkManager = gameNetworkManager;
            BackColor = Color.White;
            this.character = character;
            offsetY = -Tile.HALF_HEIGHT - 1 * BACK_HEIGHT + HEX_OFFSET_Y;
            backOffsetY = offsetY - (BACK_OFFSET_Y / 2f);
            refreshPanel(spells);


        }
        public void refreshPanel(List<Spells[]> spells)
        {
            if (spells == null)
            {
                return;
            }
            Size = new Size((int)BACK_WIDTH, (int)BACK_HEIGHT + ROW_HEIGHT * (int)((spells.Count - 1) / 3));
            Location = new Point((int)(character.CurrentTile.centerX - BACK_OFFSET_X), (int)(character.CurrentTile.centerY + backOffsetY + 44));
            Controls.Clear();
            for (int i = 0; i < spells.Count; i++)
            {

                PictureBox pics = new PictureBox
                {

                    Image = spells[i][character.SpellLevel[spells[i]]].Image,
                    Size = new Size(IMAGE_SIZE, IMAGE_SIZE),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Location = new Point(i % 3 * IMAGE_SIZE + IMAGE_PADDING_X * (i % 3 + 1), IMAGE_PADDING_Y + (20) * (int)(i / 3))
                };

                pics.MouseClick += new MouseEventHandler(mouseEvent(spells, i, pics));

                Controls.Add(pics);
            }
        }

        private MouseEventHandler mouseEvent(List<Spells[]> spells, int k, PictureBox pic)
        {
            return (sender, e) =>
            {
                int charIndex = character.gameManager.TeamBlue.IndexOf(character);
                Spells[] currentSpell = spells[k];
                if (character.ActiveSpells.Count == 0)
                {
                    character.DefaultSkill = currentSpell;
                    gameNetworkManager.enqueueMsg(NetworkMsgPrefix.DefaultSkill, GameNetworkUtilities.serializeSpellActionMoving(currentSpell, charIndex));
                }
                character.ActiveSpells.Add(currentSpell);
                gameNetworkManager.enqueueMsg(NetworkMsgPrefix.AddActiveSpells, GameNetworkUtilities.serializeSpellAction(currentSpell, character.CurrentTile));
                character.InactiveSpells.Remove(currentSpell);
                refreshPanel(character.InactiveSpells);
                character.ChooseSpell.refreshPanel(character, character.ActiveSpells);
            };
        }
    }
}