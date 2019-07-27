﻿using ASU2019_NetworkedGameWorkshop.model.character;
using ASU2019_NetworkedGameWorkshop.model.character.types;
using ASU2019_NetworkedGameWorkshop.model.grid;
using ASU2019_NetworkedGameWorkshop.model.ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using static ASU2019_NetworkedGameWorkshop.controller.StageManager;
using static ASU2019_NetworkedGameWorkshop.model.ui.StageTimer;

namespace ASU2019_NetworkedGameWorkshop.controller
{
    public class GameManager
    {
        private const int GAMELOOP_INTERVAL = 50, TICK_INTERVAL = 1000;
        private const int GRID_HEIGHT = 6, GRID_WIDTH = 7;

        private readonly Grid grid;
        private readonly GameForm gameForm;
        private readonly Timer timer;
        private readonly StageTimer stageTimer;
        private readonly Stopwatch stopwatch;
        private readonly StageManager stageManager;

        private long nextTickTime;
        private bool updateCanvas;

        /// <summary>
        /// Elapsed Time in ms.
        /// <para>Increments even if program execution was paused (uses system time).</para>
        /// </summary>
        public long ElapsedTime { get { return stopwatch.ElapsedMilliseconds; } }
        public List<Character> TeamBlue { get; private set; }
        public List<Character> TeamRed { get; private set; }
        public Tile SelectedTile { get; set; }

        public GameManager(GameForm gameForm)
        {
            this.gameForm = gameForm;
            grid = new Grid(GRID_WIDTH, GRID_HEIGHT,
                (int)((gameForm.Width - (Tile.WIDTH * GRID_WIDTH)) / 2),
                (int)((gameForm.Height - (Tile.HEIGHT * GRID_HEIGHT)) / 2) + 30,
                this);//temp values 

            TeamBlue = new List<Character>();
            TeamRed = new List<Character>();

            stageTimer = new StageTimer(this);
            stageManager = new StageManager(stageTimer, TeamBlue, TeamRed, grid, this);
            stageTimer.switchStageEvent += stageManager.switchStage;

            stopwatch = new Stopwatch();
            timer = new Timer
            {
                Interval = GAMELOOP_INTERVAL //Arbitrary: 20 ticks per sec
            };
            timer.Tick += new EventHandler(gameLoop);

            //Debugging 
            TeamRed.Add(new Character(grid, grid.Tiles[6, 5], Character.Teams.Red, CharacterTypePhysical.Melee, this));
            TeamRed.Add(new Character(grid, grid.Tiles[5, 5], Character.Teams.Red, CharacterTypePhysical.Archer, this));
            TeamBlue.Add(new Character(grid, grid.Tiles[4, 0], Character.Teams.Blue, CharacterTypePhysical.Melee, this));
            TeamBlue.Add(new Character(grid, grid.Tiles[0, 3], Character.Teams.Blue, CharacterTypePhysical.Archer, this));
        }

        public void startTimer()
        {
            gameStart();
            timer.Start();
        }

        public void mouseClick(MouseEventArgs e)
        {
            if (stageManager.CurrentGameStage == GameStage.Buy)
            {
                if (e.Button == MouseButtons.Right)
                {
                    deselectSelectedTile();
                }
                else if (e.Button == MouseButtons.Left)
                {
                    Tile tile = grid.getSelectedHexagon(e.X, e.Y);
                    if (tile != null)
                    {
                        selectTile(tile);
                    }
                }
            }
        }

        private void selectTile(Tile tile)
        {
            if (SelectedTile == tile)
            {
                deselectSelectedTile();
            }
            else if (SelectedTile == null)
            {
                SelectedTile = tile;
                SelectedTile.Selected = true;
                updateCanvas = true;
            }
            else
            {
                Character temp = SelectedTile.CurrentCharacter;
                SelectedTile.CurrentCharacter = tile.CurrentCharacter;
                tile.CurrentCharacter = temp;
                deselectSelectedTile();
            }
        }

        public void deselectSelectedTile()
        {
            if (SelectedTile != null)
            {
                SelectedTile.Selected = false;
                SelectedTile = null;
                updateCanvas = true;
            }
        }

        public void updatePaint(PaintEventArgs e)
        {
            grid.draw(e.Graphics);

            TeamBlue.ForEach(character => character.draw(e.Graphics));
            TeamRed.ForEach(character => character.draw(e.Graphics));

            stageTimer.draw(e.Graphics);

            if (true)//debugging
            {
                grid.drawDebug(e.Graphics);
                TeamBlue.ForEach(character => character.drawDebug(e.Graphics));
                TeamRed.ForEach(character => character.drawDebug(e.Graphics));
            }
        }

        private void gameStart()
        {
            stopwatch.Start();
            stageTimer.resetTimer(StageTime.DEBUGGING);//Debugging
            //stageTimer.resetTimer(StageTime.BUY_TIME);
        }

        private void gameLoop(object sender, EventArgs e)
        {
            updateCanvas = stageTimer.update() || updateCanvas;

            if (stageManager.CurrentGameStage == GameStage.Buy)
            {
                updateCanvas = stageUpdateBuy() || updateCanvas;
            }
            else if (stageManager.CurrentGameStage == GameStage.Fight)
            {
                updateCanvas = stageUpdateFight() || updateCanvas;
            }

            if (updateCanvas)
            {
                gameForm.Refresh();
                //gameForm.Invalidate();
            }
        }

        private bool stageUpdateBuy()
        {
            bool updateCanvas = false;
            return updateCanvas;
        }

        private bool stageUpdateFight()
        {
            if (TeamBlue.Count(e => !e.IsDead) == 0 || TeamRed.Count(e => !e.IsDead) == 0)
            {
                stageTimer.endTimer();
                return true;
            }

            bool updateCanvas = false;

            foreach (Character character in TeamBlue.Where(e => !e.IsDead))
            {
                updateCanvas = character.update() || updateCanvas;
            }
            foreach (Character character in TeamRed.Where(e => !e.IsDead))
            {
                updateCanvas = character.update() || updateCanvas;
            }


            if (nextTickTime < ElapsedTime)
            {
                nextTickTime = ElapsedTime + TICK_INTERVAL;
                foreach (Character character in TeamBlue.Where(e => !e.IsDead))
                {
                    updateCanvas = character.tick() || updateCanvas;
                }
                foreach (Character character in TeamRed.Where(e => !e.IsDead))
                {
                    updateCanvas = character.tick() || updateCanvas;
                }
            }

            return updateCanvas;
        }
    }
}
