﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

namespace PrincessMonoSmasher
{
    class BoxEntity : Entity
    {
        private bool isDying;
        private DeathType typeOfDeath;
        private int deathTimer;
        public bool isLightBox;
        public bool isLightOn;

        public bool IsDead
        {
            get { return isDying; }
        }

        public BoxEntity(Point position, bool isLightBox)
            : base(new Point(0, 0), position, false, true)
        {
            this.isDying = false;
            this.typeOfDeath = DeathType.Generic;
            this.isLightBox = isLightBox;
            if (isLightBox)
            {
                texture = new Point(1, 0);
                isLightOn = false;
            }
        }

        public override void Update()
        {
            if (!isDying)
            {
                base.Update();
            }
            else
            {
                #region Death Handling
                deathTimer++;
                if (typeOfDeath == DeathType.Fall)
                {
                    if (deathTimer > 24)
                    {
                        alive = false;
                    }
                }
                else if (typeOfDeath == DeathType.Burn)
                {
                    if (deathTimer > 24)
                    {
                        alive = false;
                    }
                }
                //else if (typeOfDeath == DeathType.Drown)
                //{
                    //Drowning doesn't happen with a box
                //}
                else if (typeOfDeath == DeathType.Generic)
                {
                    if (deathTimer > 24)
                    {
                        alive = false;
                    }
                }
                #endregion
            }
        }

        public override void CheckRestingPos()
        {
            base.CheckRestingPos();
            isLightOn = false;

            Entity e = GameClient.GetEntityAt(Position.X, Position.Y);
            if (e != null)
            {
                if (e is TeleporterEntity)
                {
                    Point telePos = ((TeleporterEntity)e).CanTeleport();
                    if (telePos != new Point(-1, -1))
                    {
                        PositionLast = new Point(telePos.X - (Position.X - PositionLast.X), telePos.Y - (Position.Y - PositionLast.Y));
                        Position = telePos;
                        GameClient.PlaySoundEffect(GameClient.sndTeleport);
                    }
                    else
                    {
                        //GameClient.PlaySoundEffect(GameClient.sndCantTeleport);
                    }
                }
            }

            Tile t = GameClient.grid[Position.X, Position.Y];
            if (t.type == new Point(0, 1)) //Pusher Right
            {
                if (TryMove(1, 0, 10))
                    GameClient.PlaySoundEffect(GameClient.sndPusher);
            }
            else if (t.type == new Point(1, 1)) //Pusher Down
            {
                if (TryMove(0, 1, 10))
                    GameClient.PlaySoundEffect(GameClient.sndPusher);
            }
            else if (t.type == new Point(2, 1)) //Pusher Left
            {
                if (TryMove(-1, 0, 10))
                    GameClient.PlaySoundEffect(GameClient.sndPusher);
            }
            else if (t.type == new Point(3, 1)) //Pusher Up
            {
                if (TryMove(0, -1, 10))
                    GameClient.PlaySoundEffect(GameClient.sndPusher);
            }
            else if (t.type == new Point(2, 0)) //Hole
            {
                Kill(DeathType.Fall);
            }
            else if (t.type == new Point(4, 0)) //Lava
            {
                Kill(DeathType.Burn);
            }
            else if (t.type == new Point(5, 0)) //Water
            {
                GameClient.grid[Position.X, Position.Y] = new Tile(new Point(6, 0));
                GameClient.PlaySoundEffect(GameClient.sndBlockWater);
                alive = false;
            }
            else if (t.type == new Point(3, 0)) //Ice
            {
                TryMove(Position.X - PositionLast.X, Position.Y - PositionLast.Y, 1);//<--Force might change here depending on preference
            }
        }

        public void Kill(DeathType type)
        {
            isDying = true;
            deathTimer = 0;
            typeOfDeath = type;
            isStatic = true;
            isSolid = false;
            if (type == DeathType.Burn)
                GameClient.PlaySoundEffect(GameClient.sndBurn);
            if (type == DeathType.Fall)
                GameClient.PlaySoundEffect(GameClient.sndFall);
        }

        public override void Draw()
        {
            if (isLightBox)
            {
                texture = new Point((isLightOn) ? 2 : 1, 0);
            }
            if (!isDying)
                base.Draw();
            else
            {
                #region Death Drawing
                if (typeOfDeath == DeathType.Fall)
                {
                    float scale = 1 - ((deathTimer / 6) / 4f);
                    Gl.sB.Draw(sheet, DrawPosition + new Vector2(8), new Rectangle(texture.X * 16, texture.Y * 16, 16, 16), Color.White, 0f, new Vector2(8,8), scale, SpriteEffects.None, 0f);
                }
                //TODO: Impliment other types of death for boxes
                #endregion
            }
        }
    }
}
