﻿using System;

namespace ColorQuery
{
    using GDIColor = System.Drawing.Color;
    using Color = System.Windows.Media.Color;

    public struct Cmyk
    {
        public const float MinValue = 0f, MaxValue = 1f;

        public Cmyk(float c, float m, float y, float k)
        {
            this.c = c;
            this.m = m;
            this.y = y;
            this.k = k;
        }

        public Cmyk(byte red, byte green, byte blue)
        {
            if (red == 0 && green == 0 && blue == 0)
            {
                // black
                c = m = y = k = 0;
            }

            // adust RGB range
            // [0-255] -> [0-1]
            float r = red / 255f;
            float g = green / 255f;
            float b = blue / 255f;

            // extract out K [0-1]
            k = 1 - Math.Max(r, Math.Max(g, b));

            c = (1 - r - k) / (1 - k);
            m = (1 - g - k) / (1 - k);
            y = (1 - b - k) / (1 - k);
        }

        public Cmyk(GDIColor color) : this(color.R, color.G, color.B) {}
        public Cmyk(Color color) : this(color.R, color.G, color.B) {}


        public float C
        {
            get => c;
            set => c = Clamp(value);
        }
        public float M
        {
            get => m;
            set => m = Clamp(value);
        }
        public float Y
        {
            get => y;
            set => y = Clamp(value);
        }
        public float K
        {
            get => k;
            set => k = Clamp(value);
        }

        public override string ToString()
        {
            return $"[C={C:F3}, M={M:F3}, Y={Y:F3}, K={K:F3}]";
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return Equals((Cmyk)obj);
        }

        public bool Equals(Cmyk cmyk)
        {
            return C == cmyk.C &&
                   M == cmyk.M &&
                   Y == cmyk.Y &&
                   K == cmyk.K;
        }

        public override int GetHashCode()
        {
            return (c, m, y, k).GetHashCode();
        }

        public static bool operator ==(Cmyk left, Cmyk right) => left.Equals(right);
        public static bool operator !=(Cmyk left, Cmyk right) => !left.Equals(right);

        public static implicit operator GDIColor(Cmyk cmyk)
        {
            var k = cmyk.K;
            return GDIColor.FromArgb(ftob(cmyk.C, k), ftob(cmyk.M, k), ftob(cmyk.Y, k));
        }
        public static implicit operator Color(Cmyk cmyk)
        {
            var k = cmyk.K;
            return Color.FromRgb(ftob(cmyk.C, k), ftob(cmyk.M, k), ftob(cmyk.Y, k));
        }

        // cmyk float to rgb byte
        static byte ftob(float val, float k) => (byte)(255 * (1 - val) * (1 - k));

        static float Clamp(float nValue) => Math.Min(MaxValue, Math.Max(MinValue, nValue));

        float c, m, y, k;
    }
}