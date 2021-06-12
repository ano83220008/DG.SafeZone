using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeZone
{
    internal class SafeZoneManager
    {
        internal bool IsActive { get { return _progress >= 0 && _progress <= 1.0f; } }
        //ゲーム中のサークル進行度、すべての基準(0->1)
        float _progress = 0f;

        internal float Progress
        {
            set { SetProgress(value); }
            get { return _progress; }
        }

        //サークルの段階的進行の分割回数
        internal int circleLevelMax = 5;

        //サークルの段階的進行度
        internal int circleLevel = 0;

        //サークルの最大サイズ
        internal float radiusMax;

        //サークルの最大サイズに対する割合(0.0～1.0)
        internal float scale = 1f;

        //サークルの現在のサイズを計算
        internal float Radius { get { return radiusMax * scale; } }

        internal Color CircleColor { get { return playFade ? new Color(255, 255, 0, 80) : new Color(255, 0, 0, 80); } }

        //サークルの中心座標
        internal Vec2 position;

        //フェード処理用のタイマー
        internal float fadeTime = 0f;

        //
        internal bool playFade = false;

        internal void SetProgress(float v)
        {
            _progress = v;


            //サークルレベル算出
            circleLevel = (int)(_progress * circleLevelMax);

            //1レベルで変化するサイズ
            float step = 1 / (float)circleLevelMax;

            //サークルレベル単位の進行量
            var leveledProgress = circleLevel * step;

            //サークルレベル単位の進行量の余り
            var mod = (_progress - leveledProgress);
            if (mod * circleLevelMax < 0.5)
            {
                mod = 0;
            }
            else
            {
                mod = (mod * circleLevelMax * 2 / circleLevelMax) - step;
            }
            playFade = mod != 0;
            var targetScale = leveledProgress + mod;

            scale = 1 - (targetScale);
            //fadeTime = (Progress - scale)* circleLevel;

        }

        internal void Update(ref ZoneCircle zone)
        {
            zone.color = CircleColor;
            zone.radius = Radius;
            zone.position = position;
            zone.enabled = IsActive;
        }

    }
}
