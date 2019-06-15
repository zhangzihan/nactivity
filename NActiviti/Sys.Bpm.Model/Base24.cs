namespace Sys
{
    public static class Base24
    {
        private static readonly string sel = "bcdfghjkmpqrtvwxy2346789";

        public static string Encode(string Text)
        {
            int Pos = 0;
            char[] Buf = new char[Text.Length << 1];

            int i;
            while ((i = Pos) < Text.Length)
            {
                Buf[i << 1] = sel[(Text[Pos]) >> 4];
                Buf[(i << 1) + 1] = sel[23 - (Text[Pos] & 0x0F)];
                Pos++;
            }

            return new string(Buf);
        }

        public static string Decode(string Text)
        {
            if (Text.Length % 2 != 0)
                return null;

            int[] NPos = new int[2];
            _ = new char[2];
            char[] Buf = new char[Text.Length >> 1];

            for (int i = 0; i < (Text.Length >> 1); i++)
            {
                NPos[0] = sel.IndexOf(Text[i << 1]);
                NPos[1] = 23 - sel.IndexOf(Text[(i << 1) + 1]);
                if (NPos[0] < 0 || NPos[1] < 0)
                {
                    return null;
                }

                Buf[i] = ((char)((NPos[0] << 4) | NPos[1]));
            }
            return new string(Buf);
        }
    }
}
