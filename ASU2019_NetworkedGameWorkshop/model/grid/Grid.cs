﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace ASU2019_NetworkedGameWorkshop.model.grid {
    class Grid : GraphicsObject {
        private const float halfWidth = (Tile.WIDTH / 2);//in tile ?
        private const float hexC = halfWidth * 0.57735026919f;

        private readonly int gridWidth, gridHeight;
        private readonly int startingX, startingY;

        public Tile[,] Tiles { get; set; }

        //todo temporary
        public List<Tile> path;

        public Grid(int gridWidth, int gridHeight, int startingX, int startingY)
        {
            if (gridWidth < 0 ||
                gridHeight < 0 ||
                startingX < 0 ||
                startingY < 0)
                throw new ArgumentOutOfRangeException("Negative Input");//not descriptive 

            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.startingX = startingX;
            this.startingY = startingY;

            Tiles = new Tile[gridWidth, gridHeight];
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    Tiles[x, y] = new Tile(x, y, startingX, startingY);
                }
            }
            PathFinding p = new PathFinding(this);
            p.findPath(6, 7, 3, 4);
        }

        internal Tile getSelectedHexagon(int x, int y) {
            x -= startingX;
            y -= startingY;

            float tileHeight = (Tile.HEIGHT - hexC);

            int row = (int) (y / tileHeight);

            bool rowIsOdd = row % 2 == 1;

            int column = rowIsOdd ?
                (int) ((x - halfWidth) / Tile.WIDTH) :
                column = (int) (x / Tile.WIDTH);

            double relY = y - (row * tileHeight);
            double relX = rowIsOdd ?
                (x - (column * Tile.WIDTH)) - halfWidth :
                x - (column * Tile.WIDTH);

            float m = (hexC / halfWidth);

            if(relY < (-m * relX) + hexC) {
                row--;
                if(!rowIsOdd)
                    column--;
            } else if(relY < m * relX - hexC) {
                row--;
                if(rowIsOdd)
                    column++;
            }
            if(column < gridWidth
                && row < gridHeight
                && column > -1
                && row > -1)
                return tiles[column, row];
            return null;
        }

        public override void draw(Graphics graphics)
        {
            foreach (Tile tile in Tiles)
            {
                if (path != null && path.Contains(tile))
                {
                    tile.draw2(graphics);

                }
                else if (tile.Walkable)
                    tile.draw(graphics);
                //else
                    //tile.draw3(graphics);
            }
        }

        //valid for  odd_r hexagons
        public List<Tile> getNeighbours(Tile tile)
        {

            List<Tile> neighbours = new List<Tile>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ((x == -1 && y == -1) ||
                        (x == -1 && y == 1) ||
                        (x == 0 && y == 0))
                        continue;

                    int gridX, gridY;
                    //checks if even
                    if ((tile.Y & 1) != 0)
                    {
                        gridX = tile.X + x;
                        gridY = tile.Y + y;
                    }
                    else
                    {
                        gridX = tile.X - x;
                        gridY = tile.Y - y;
                    }

                    if ((gridX >= 0 && gridX < gridWidth) &&
                        (gridY >= 0 && gridY < gridHeight))
                    {
                        neighbours.Add(Tiles[gridX, gridY]);
                        Console.WriteLine("added with x = {0} and y = {1}",x,y);
                    }
                }
            }
            return neighbours;
        }
    }
}
