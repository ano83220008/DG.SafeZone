using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeZone
{
    internal class ZoneCircle
    {
        //サークルの現在のサイズを計算
        internal float radius;

        //サークルの中心座標
        internal Vec2 position;

        //サークルの色
        internal Color color = new Color(255, 0, 0, 80);

        //サークルが有効か否か（falseなら判定も描画処理も無効）
        internal bool enabled = false;

        internal bool Contains(Vec2 pos)
        {
            if (!enabled) return false;

            return Vec2.Distance(position, pos) < radius;
        }

        internal void DrawZone()
        {
            if (!enabled) return;

            Graphics.DrawCircle(position, radius, color);
        }

    }
}
