using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeZone
{
        internal static class Writer
        {
            static int position = 0;

            internal static void WriteLine(object obj) => WriteLine(obj.ToString());

            internal static void WriteLine(string text)
            {
                BitmapFont uiFont = Profiles.EnvironmentProfile.font;
                Vec2 uiFontOldScale = new Vec2(uiFont.scale);

                uiFont.scale = new Vec2(1, 1);

                uiFont.DrawOutline(text, new Vec2(0, 8 * position), Color.White, Color.Black);

                uiFont.scale = uiFontOldScale;


                ++position;
            }

            internal static void Reset()
            {
                position = 0;
            }
            }
}
