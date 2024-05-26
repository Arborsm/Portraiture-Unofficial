﻿using Microsoft.Xna.Framework;
using System.Collections.Generic;
namespace Portraiture
{
    public class ColorManipulation
    {
        public float light;
        public List<Color> palette;
        public float saturation;

        public ColorManipulation(List<Color> palette, float saturation = 100, float light = 100)
        {
            this.saturation = saturation;
            this.light = light;
            this.palette = palette;
        }

        public ColorManipulation(float saturation = 100, float light = 100)
        {
            this.saturation = saturation;
            this.light = light;
            palette = new List<Color>();
        }
    }
}
