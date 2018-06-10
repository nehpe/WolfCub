using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System;

namespace Cub
{
    public class CubGame : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Texture2D spacer;
        private Texture2D cursor;
        private Texture2D grid1;
        private Texture2D grid2;
        private Texture2D wall;
        private SpriteFont font;
        private int level;
        private Vector2 startPos;
        private List<string> map;
        private float movespeed;
        private int turnspeed;
        private int gridsize;
        private bool view3D;
        private bool cone;
        private bool fisheye;

        public bool FState { get; private set; }

        private bool fullbright;
        private bool textures;
        private bool TabState;

        private Vector2 cDir;
        int w;
        int h;

        float wallheight;
        private Vector2 cCam;
        private Vector2 centerScreen;
        float fov;
        private Vector2 endPos;
        private Vector2 cPos;
        private Vector2 cOffset;

        public bool BtnState { get; private set; }
        public bool RState { get; private set; }
        public bool TState { get; private set; }
        public bool BState { get; private set; }

        public CubGame()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = cVars.SCREEN_WIDTH;
            graphics.PreferredBackBufferHeight = cVars.SCREEN_HEIGHT;

            graphics.ApplyChanges();

            Content.RootDirectory = "Content";

            IsMouseVisible = true;

            Window.Title = string.Format("Fenrir - {0}", cVars.VERSION);

        }

        protected override void Initialize()
        {
            endPos = new Vector2(0, 0);
            int cDirDist = 10;
            movespeed = 0.6f;
            turnspeed = 92;
            gridsize = 32;

            view3D = false;
            cone = false;
            fisheye = false;
            fullbright = false;
            textures = true;

            cDir = new Vector2(0, -cDirDist);

            w = GraphicsDevice.Viewport.Width;
            h = GraphicsDevice.Viewport.Height;

            fov = 60.0f;
            float std = 4 / 3f;

            float aspectMulti = (std + (w / (float)h) / std) / 2;

            wallheight = aspectMulti * 2.5f;

            // convert from degrees to width of fov
            cCam = new Vector2(fov * (10 / 9) / 100 * cDirDist, 0);

            centerScreen = new Vector2(w / 2f, h / 2f);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            spacer = Content.Load<Texture2D>("sprites/spacer");
            cursor = Content.Load<Texture2D>("sprites/target");
            grid1 = Content.Load<Texture2D>("sprites/grid1");
            grid2 = Content.Load<Texture2D>("sprites/grid2");
            wall = Content.Load<Texture2D>("sprites/wall");

            font = Content.Load<SpriteFont>("fonts/ui");

            level = 1;
            startPos = new Vector2(200, 200);

            map = LoadMap(level.ToString());
            cPos = startPos;
            cOffset = cPos - centerScreen;


        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here

            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Left) ||
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.A) ||
                GamePad.GetState(PlayerIndex.One).DPad.Left == ButtonState.Pressed)
                PlayerTurn("left");
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Right) ||
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.D) ||
                GamePad.GetState(PlayerIndex.One).DPad.Right == ButtonState.Pressed)
                PlayerTurn("right");

            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Up) ||
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.W) ||
                GamePad.GetState(PlayerIndex.One).DPad.Up == ButtonState.Pressed)
                PlayerMove("forward");
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Down) ||
                Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.S) ||
                GamePad.GetState(PlayerIndex.One).DPad.Down == ButtonState.Pressed)
                PlayerMove("back");

            // toggle fullbright on/off in 3d mode
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.B) && BState != true)
            {
                fullbright = !fullbright;
                BState = true;
            }
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.B) && BState != false)
                BState = false;

            // toggle fisheye lens on/off in 3d mode
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.F) && FState != true)
            {
                fisheye = !fisheye;
                FState = true;
            }
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.F) && FState != false)
                FState = false;

            // toggle textures on/off in 3d mode
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.T) && TState != true)
            {
                textures = !textures;
                TState = true;
            }
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.T) && TState != false)
                TState = false;

            // toggle camera tracers on/off in 2d mode
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.R) && RState != true)
            {
                cone = !cone;
                RState = true;
            }
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.R) && RState != false)
                RState = false;

            // switch between 3d/map mode
            if (Keyboard.GetState(PlayerIndex.One).IsKeyDown(Keys.Tab) && TabState != true)
            {
                view3D = !view3D;
                TabState = true;
            }
            if (Keyboard.GetState(PlayerIndex.One).IsKeyUp(Keys.Tab) && TabState != false)
                TabState = false;

            // switch between 3d/map mode - Zune/gamepad
            if (GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.A) && BtnState != true)
            {
                view3D = !view3D;
                BtnState = true;
            }
            if (GamePad.GetState(PlayerIndex.One).IsButtonUp(Buttons.A) && BtnState != false)
                BtnState = false;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            if (view3D == false)
            {
                for (int y = 0; y < map.Count; ++y)
                {
                    for (int x = 0; x < map[0].Length; ++x)
                    {
                        Vector2 pos = new Vector2(gridsize * x, gridsize * y);

                        pos = pos - cOffset;
                        drawTile(getTex(map[y][x]), pos);
                    }
                }

                spriteBatch.Draw(cursor, centerScreen, Color.White);
                spriteBatch.Draw(cursor, centerScreen + cDir, Color.Red);
                spriteBatch.Draw(cursor, centerScreen + cDir + cCam, Color.Green);
                spriteBatch.Draw(cursor, centerScreen + cDir - cCam, Color.Green);
                spriteBatch.Draw(spacer, centerScreen + GetCoords(cDir, 200), Color.Red);

            }

            Vector2 textpos = new Vector2(40, -10);
            Vector2 cell = GetCell(cPos);

            // begin movable logic?


            // draw floor/ceiling
            if (view3D)
            {
                for (int y = 0; y < h; y++)
                {
                    int centerDist = Math.Abs(y - h / 2);
                    float multi = 50 / (float)h;
                    float cflt = centerDist * multi / 100F + 0.15F;
                    if (y > h / 2)
                        cflt += 0.2F;
                    Color cval = new Color(new Vector3(cflt));
                    spriteBatch.Draw(spacer, new Rectangle(0, y, w, 1), cval);
                }
            }

            //int w = 45;
            for (int x = 0; x < w; x++)
            {
                float camX = 2 * x / (float)w - 1;
                //float camX = 1F;
                Vector2 rDir = new Vector2();
                rDir.X = cDir.X + cCam.X * camX;
                rDir.Y = cDir.Y + cCam.Y * camX;
                TraceRay(cPos, rDir, x);
            }

            string postext1 = "pos: " + Math.Round(cPos.X, 2).ToString() + "," + Math.Round(cPos.Y, 2).ToString();
            string postext2 = "cell: " + cell.X.ToString() + "," + cell.Y.ToString();

            spriteBatch.DrawString(font, postext1, new Vector2(40, gridsize * 1 + 8), Color.White);
            spriteBatch.DrawString(font, postext2, new Vector2(40, gridsize * 2 + 8), Color.White);

            string postext3 = "WASD/Arrows: move+turn     Tab: 2D/3D     Escape: Exit";
            postext3 += "     R: Tracers (2D)     T: Textures (3D)     F: Fisheye (3D)     B: Fullbright (3D)";
            spriteBatch.Draw(spacer, new Rectangle(0, h - 32, w, 32), new Color(Color.Black, 0.7F));
            spriteBatch.DrawString(font, postext3, new Vector2(20, gridsize * GetCell(new Vector2(0, h - 1)).Y - 1), Color.White);

            // end movable logic?

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private Vector2 GetCell(Vector2 pos)
        {
            float x = (float)Math.Floor(pos.X / gridsize);
            float y = (float)Math.Floor(pos.Y / gridsize);

            return new Vector2(x, y);
        }

        private void PlayerMove(string dir)
        {
            if (dir == "back")
                cPos = cPos - cDir * movespeed;
            else
                cPos = cPos + cDir * movespeed;
            cOffset = cPos - centerScreen;
        }

        private void PlayerTurn(string dir)
        {
            PlayerTurn(dir, turnspeed);
        }

        private void PlayerTurn(string dir, int amount)
        {
            if (dir == "left")
            {
                cDir = RotateVector(cDir, "left", amount);
                cCam = RotateVector(cCam, "left", amount);
            }
            else
            {
                cDir = RotateVector(cDir, "right", amount);
                cCam = RotateVector(cCam, "right", amount);
            }
        }

        private Vector2 RotateVector(Vector2 pos, string dir, int amount)
        {
            Vector2 newpos = new Vector2();

            double r = (2 * Math.PI) / amount;
            if (dir == "left")
            {
                r = 0 - r;
            }

            newpos.X = (float)Math.Cos(r) * pos.X - (float)Math.Sin(r) * pos.Y;
            newpos.Y = (float)Math.Cos(r) * pos.Y + (float)Math.Sin(r) * pos.X;

            return newpos;
        }


        private List<string> LoadMap(string mapname)
        {
            // Load the level and ensure all the lines are the same length
            int width;
            List<string> lines = new List<string>();
            string path = "Content\\maps\\" + mapname + ".txt";
            using (StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                string line = reader.ReadLine();
                width = line.Length;
                while (line != null)
                {
                    lines.Add(line);
                    if (line.Length != width)
                        throw new System.Exception(string.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine();
                }
            }

            for (int y = 0; y < lines.Count; y++)
            {
                for (int x = 0; x < lines[0].Length; x++)
                {
                    Vector2 cellCenter = new Vector2((x + 1) * gridsize - gridsize / 2, (y + 1) * gridsize - gridsize / 2);
                    if (lines[y][x] == 's')
                        startPos = cellCenter;
                    if (lines[y][x] == 'e')
                        endPos = cellCenter;
                }
            }

            return lines;
        }

        private bool isWall(char id)
        {
            switch (id)
            {
                case '1':
                case '2':
                    return true;
                case '0':
                case 's':
                case 'e':
                default:
                    return false;
            }
        }

        private Texture2D getTex(char id)
        {
            switch (id)
            {
                case '0':
                case 's':
                case 'e':
                    return grid1;
                case '1':
                    return wall;
                case '2':
                    return grid2;
                default:
                    return grid1;
            }
        }

        private void drawTile(Texture2D texture, Vector2 pos)
        {
            if (pos.X + 32 < 0 || pos.X >= GraphicsDevice.Viewport.Width ||
                pos.Y + 32 < 0 || pos.Y >= GraphicsDevice.Viewport.Height)
            {
                return;
            }

            Rectangle target = new Rectangle((int)pos.X + 1, (int)pos.Y + 1, 32, 32);
            Rectangle source = new Rectangle(0, 0, texture.Width, texture.Height);
            spriteBatch.Draw(texture, target, source, Color.White);
        }

        private Vector2 GetCoords(Vector2 dir, double dist)
        {
            double angle = GetAngle(dir);
            double y = Math.Sin(angle) * dist;
            double x = Math.Sqrt(dist * dist - y * y);

            if (dir.X < 0)
                x = -x;

            Vector2 coords = new Vector2((float)x, (float)y);
            return coords;
        }

        private double GetAngle(Vector2 dir)
        {
            double angle = Math.Atan2(dir.Y, dir.X);
            if (angle < 0)
            {
                angle = angle + Math.PI * 2;
            }

            return angle;
        }

        private double GetHypotenuse(double b, double angle)
        {
            return b / Math.Cos(angle);
        }

        private double GetSide(double hypo, double side)
        {
            return Math.Sqrt(hypo * hypo - side * side);
        }

        private double GetDistance(Vector2 dir)
        {
            return Math.Sqrt(dir.Y * dir.Y + dir.X * dir.X);
        }

        private Vector2 GetIntersect(Vector2 pos, Vector2 dir, char coord)
        { // should return a vector that can be added to the pos to locate the x intersect
            pos.X = pos.X % 32;
            pos.Y = pos.Y % 32;
            Vector2 ivect = new Vector2();

            double angle = GetAngle(dir);
            double dist;

            if (coord == 'x')
            {
                ivect.Y = 0;
                if (dir.X != 0)
                {
                    if (dir.X > 0)
                        ivect.X = gridsize - pos.X; // distance from pos to right side
                    else
                        if (pos.X == 0)
                        ivect.X -= 32;
                    else
                        ivect.X = -pos.X; // distance from pos to left side

                    dist = GetHypotenuse(ivect.X, angle);
                    ivect.Y = (float)GetSide(dist, ivect.X);
                    if (dir.Y < 0)
                        ivect.Y = -ivect.Y;
                }
            }
            else
            {
                angle = Math.PI / 2 - angle;
                ivect.X = 0;
                if (dir.Y != 0)
                {
                    if (dir.Y > 0)
                        ivect.Y = gridsize - pos.Y; // distance from pos to bottom side
                    else
                        if (pos.Y == 0)
                        ivect.Y -= 32;
                    else
                        ivect.Y = -pos.Y; // distance from pos to top side

                    dist = GetHypotenuse(ivect.Y, angle);
                    ivect.X = (float)GetSide(dist, ivect.Y);
                    if (dir.X < 0)
                        ivect.X = -ivect.X;
                }
            }

            return ivect;
        }

        private void TraceRay(Vector2 pos, Vector2 dir, int x)
        {
            Vector2 IntersectX = GetIntersect(pos, dir, 'x');
            Vector2 rayX = pos + IntersectX;
            Vector2 deltaX = GetIntersect(rayX, dir, 'x');
            double sideDistX = GetDistance(IntersectX);
            double deltaDistX = GetDistance(deltaX);

            Vector2 IntersectY = GetIntersect(pos, dir, 'y');
            Vector2 rayY = pos + GetIntersect(pos, dir, 'y');
            Vector2 deltaY = GetIntersect(rayY, dir, 'y');
            double sideDistY = GetDistance(IntersectY);
            double deltaDistY = GetDistance(deltaY);

            int hit = 0;
            int i = 0;
            int side = 0;

            while (hit == 0 && i < 1000)
            {
                Vector2 goodRay = new Vector2();

                if (sideDistX < sideDistY)
                {
                    if (!view3D && cone == true)
                        spriteBatch.Draw(spacer, rayX - cOffset, Color.LightGray);
                    goodRay = rayX;
                    if (dir.X < 0)
                    {
                        goodRay.X -= 1;
                    }
                    side = 0;
                    sideDistX += deltaDistX;
                    rayX += deltaX;
                }
                else
                {
                    if (!view3D && cone == true)
                        spriteBatch.Draw(spacer, rayY - cOffset, Color.LightGray);
                    goodRay = rayY;
                    if (dir.Y < 0)
                    {
                        goodRay.Y -= 1;
                    }
                    side = 1;
                    sideDistY += deltaDistY;
                    rayY += deltaY;
                }


                int rayCellY = (int)GetCell(goodRay).Y;
                int rayCellX = (int)GetCell(goodRay).X;

                int cellY = dir.Y < 0 ? rayCellY : rayCellY;
                int cellX = dir.X < 0 ? rayCellX : rayCellX;

                if (cellY < 0 || cellX < 0 || cellY >= map.Count || cellX >= map[0].Length || isWall(map[cellY][cellX]) == true)
                {
                    if (view3D == true)
                    {

                        double perpWallDist;

                        if (side == 0)
                            perpWallDist = Math.Abs((cellX * 32 - cPos.X + (dir.X < 0 ? 32 : 0)) / dir.X);
                        else
                            perpWallDist = Math.Abs((cellY * 32 - cPos.Y + (dir.Y < 0 ? 32 : 0)) / dir.Y);

                        double lineHeight;

                        if (!fisheye)
                            lineHeight = h / perpWallDist * wallheight;
                        else
                            lineHeight = h / GetDistance(cPos - goodRay) * 32;

                        int drawStart = (int)-lineHeight / 2 + h / 2;
                        int drawEnd = (int)lineHeight;

                        double cval;
                        float cflt;
                        if (!fullbright)
                        {
                            cval = (lineHeight + 200) / 2 * (100 / (double)h) / 100 + 0.1;
                            cval = (cval + (side == 1 ? 0.05 : 0));
                            cflt = (float)(cval > 1.0 ? 1.0 : cval);
                        }
                        else
                            cflt = 1.0F;

                        Color wallColor = new Color(cflt, cflt, cflt);

                        Rectangle trgRect = new Rectangle(x, drawStart, 1, drawEnd);
                        if (!textures)
                        {
                            spriteBatch.Draw(spacer, trgRect, wallColor);
                        }
                        else
                        {
                            double start;
                            if (side == 1)
                            {
                                start = goodRay.X % 32;
                                if (dir.Y >= 0)
                                    start = 32 - start;
                            }
                            else
                            {
                                start = goodRay.Y % 32;
                                if (dir.X < 0)
                                    start = 32 - start;
                            }

                            Texture2D wallTex;

                            if (cellY >= map.Count || cellY < 0 ||
                                cellX >= map[0].Length || cellX < 0)
                                wallTex = getTex('0');
                            else
                                wallTex = getTex(map[cellY][cellX]);

                            Rectangle srcRect = new Rectangle((int)(start * (wallTex.Width / 32.0)), 0, 1, wallTex.Height);
                            spriteBatch.Draw(wallTex, trgRect, srcRect, wallColor);
                        }
                    }
                    //else
                    //spriteBatch.Draw(spacer, goodRay - cOffset, Color.White);
                    hit = 1;
                }
                i++;
            }
        }
    }

}
