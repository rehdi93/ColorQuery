using System;

namespace ColorQuery
{
    using Color = System.Windows.Media.Color;

    public struct Cmyk
    {
        public const float MinValue = 0f, MaxValue = 1f;

        public Cmyk(float c, float m, float y, float k)
        {
            this.c = Clamp(c);
            this.m = Clamp(m);
            this.y = Clamp(y);
            this.k = Clamp(k);
        }

        public Cmyk(byte red, byte green, byte blue)
        {
            if (red == 0 && green == 0 && blue == 0)
            {
                // black
                c = m = y = k = 0;
            }
            else
            {
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
        }

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
            return $"{C:F3} {M:F3} {Y:F3} {K:F3}";
        }

        public override bool Equals(object obj)
        {
            if (obj is Cmyk cmyk)
            {
                return Equals(cmyk);
            }

            return false;
        }

        public bool Equals(Cmyk cmyk)
        {
            return C == cmyk.C &&
                   M == cmyk.M &&
                   Y == cmyk.Y &&
                   K == cmyk.K;
        }

        public override int GetHashCode() => (c, m, y, k).GetHashCode();

        public static bool operator ==(Cmyk left, Cmyk right) => left.Equals(right);
        public static bool operator !=(Cmyk left, Cmyk right) => !(left == right);

        public static implicit operator Color(Cmyk cmyk)
        {
            (byte r, byte g, byte b) = (
                (byte)(255 * (1 - cmyk.c) * (1 - cmyk.k)),
                (byte)(255 * (1 - cmyk.m) * (1 - cmyk.k)),
                (byte)(255 * (1 - cmyk.y) * (1 - cmyk.k))
            );
            return Color.FromRgb(r, g, b);
        }

        static float Clamp(float nValue) => Math.Min(MaxValue, Math.Max(MinValue, nValue));

#pragma warning disable IDE0051 // unused private members
        private void Deconstruct(out float c, out float m, out float y, out float k)
        {
            c = this.c; m = this.m;
            y = this.y; k = this.k;
        }

        float c, m, y, k;
    }
}
