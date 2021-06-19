<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace WyMerge2 {
    class Pixel {
        public Color color;
        public Vector2 position;

        public Pixel(Color color, int x, int y) {
            this.color = color;
            this.position = new Vector2(x, y);
        }

        public Pixel(Color color, Vector2 position) {
            this.color = color;
            this.position = position;
        }

        public int Composite(Program.InterpolationTypes comparator) {
            switch (comparator) {
                case Program.InterpolationTypes.Hue:
                    return (int)(color.GetHue() * (255 / 360f));
                case Program.InterpolationTypes.Brightness:
                    return (int)(color.GetBrightness() * 255);
                case Program.InterpolationTypes.Random:
                    return Program.rng.Next(0, 255);
                default:
                    throw new NotImplementedException("Unimplemented comparator in composite calculation");
            }
        }

        public static Pixel Interpolate(Pixel a, Pixel b, float t) {
            Color col = Color.FromArgb(
                Lerp(a.color.R, b.color.R, t),
                Lerp(a.color.G, b.color.G, t),
                Lerp(a.color.B, b.color.B, t)
            );
            Vector2 pos = new Vector2(
                Lerp((int)a.position.X, (int)b.position.X, t),
                Lerp((int)a.position.Y, (int)b.position.Y, t)
            );
            return new Pixel(col, pos);
        }

        private static int Lerp(int a, int b, float t) {
            return (int)(a + (b - a) * t);
        }
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace WyMerge2 {
    class Pixel {
        public Color color;
        public Vector2 position;

        public Pixel(Color color, int x, int y) {
            this.color = color;
            this.position = new Vector2(x, y);
        }

        public Pixel(Color color, Vector2 position) {
            this.color = color;
            this.position = position;
        }

        public int Composite(Program.InterpolationTypes comparator) {
            switch (comparator) {
                case Program.InterpolationTypes.Hue:
                    return (int)(color.GetHue() * (255 / 360f));
                case Program.InterpolationTypes.Brightness:
                    return (int)(color.GetBrightness() * 255);
                case Program.InterpolationTypes.Random:
                    return Program.rng.Next(0, 255);
                default:
                    throw new NotImplementedException("Unimplemented comparator in composite calculation");
            }
        }

        public static Pixel Interpolate(Pixel a, Pixel b, float t) {
            Color col = Color.FromArgb(
                Lerp(a.color.R, b.color.R, t),
                Lerp(a.color.G, b.color.G, t),
                Lerp(a.color.B, b.color.B, t)
            );
            Vector2 pos = new Vector2(
                Lerp((int)a.position.X, (int)b.position.X, t),
                Lerp((int)a.position.Y, (int)b.position.Y, t)
            );
            return new Pixel(col, pos);
        }

        private static int Lerp(int a, int b, float t) {
            return (int)(a + (b - a) * t);
        }
    }
}
>>>>>>> 59b946e... Fix minor bug in frame export
