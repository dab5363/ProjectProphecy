﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ProjectProphecy.ns_Controls;
using ProjectProphecy.ns_Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectProphecy.ns_Graphics
{
    /// <summary>
    /// TODO: Unimplemented
    /// </summary>
    public class Bar : UIComponent
    {
        private Texture2D asset; // Portion image of the bar. Can be stretched.

        public override Texture2D Asset
        {
            get => asset;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            throw new NotImplementedException();
        }

        public override void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}