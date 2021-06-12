using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeZone
{
    class SafeZoneEvent
    {
        enum Role { Offline, Host, Client }

        static ZoneCircle zone = new ZoneCircle();
        static SafeZoneManager zmanager;

        static int timeMax;
        static int timeStartTick;
        static int timePrevTick;

        static int zoneStartTime;

        static int LevelTime { get { return Environment.TickCount - timeStartTick; } }
        static int ZoneTime { get { return LevelTime - zoneStartTime; } }
        static int ZoneTimeMax { get { return timeMax - zoneStartTime; } }

        static float ZoneProgress { get { return ZoneTime / (float)ZoneTimeMax; } }

        /// Net Data ID
        static readonly string did_ZoneKey = "szmod";
        static readonly string did_TimeLimit = did_ZoneKey + "_timelimit";
        static readonly string did_ZonePosition = did_ZoneKey + "_zoneposition";
        static readonly string did_ZoneSize = did_ZoneKey + "_zonesize";
        static readonly string did_ZoneColor = did_ZoneKey + "_zonecolor";
        static readonly string did_ZoneEnabled = did_ZoneKey + "_zoneenabled";

        static Profile Host
        {
            get
            {
                if (Network.isActive)
                {
                    return Network.host.profile;
                }
                return Profiles.active.Find(p => p.isHost);
            }
        }

        static string KillTimerDID(Profile p)
        {
            return "szmod_kt_" + p.steamID;
        }

        static Role GetRoll()
        {
            if (Network.isActive)
            {
                if (Network.isServer)
                {
                    return Role.Host;
                }
                if (Network.isClient)
                {
                    return Role.Client;
                }
            }
            return Role.Offline;
        }

        internal static bool TryGetMatchSetting(string id, out MatchSetting result)
        {
            result = TeamSelect2.matchSettings.Find(m => m.id == id);
            return result != null;
        }

        internal static void OnLevelStart_Host(GameLevel __instance)
        {
            if (!TryGetMatchSetting("zonesize", out var settingZoneSize))
            {
                return;
            }
            if (!TryGetMatchSetting("zonestart", out var settingZoneStart))
            {
                return;

            }
            if (!TryGetMatchSetting("timelimit", out var settingTimeLimit))
            {
                return;

            }

            zmanager = new SafeZoneManager();
            zmanager.radiusMax = (int)settingZoneSize.value;
            //ゾーンの位置設定
            Vec2 center = new Vec2(Profiles.active.Average(p => p.duck.position.x), Profiles.active.Average(p => p.duck.position.y));
            zmanager.position = center;


            //タイマー設定
            timeStartTick = Environment.TickCount;
            timePrevTick = Environment.TickCount;
            zoneStartTime = zoneStartTime = (int)settingZoneStart.value * 1000;
            //設定からタイムリミットを読込

            //制限時間をﾐﾘ秒に変換してセット
            timeMax = (int)settingTimeLimit.value * 1000;

            if (Network.isActive)
            {
                Profiles.EnvironmentProfile.netData.Set(did_TimeLimit, timeMax);
            }

            

            ResetKillTimer();
        }

        internal static void OnLevelStart(GameLevel level)
        {
            if (GetRoll() != Role.Client)
            {
                OnLevelStart_Host(level);
            }
        }

        static void ResetKillTimer()
        {
            Profiles.active.ForEach(p => {
                Host.netData.Set(KillTimerDID(p), -1);
            });
        }

        static void UpdateKillTimer()
        {
            var gap = Environment.TickCount - timePrevTick;
           
            foreach (var p in Profiles.active)
            {
                if (!zone.Contains(p.duck.position))
                {
                    //アヒルが範囲外なのでタイマー進行
                    var timer = Host.netData.Get<int>(KillTimerDID(p));
                    if (timer == -1)
                    {
                        //タイマー初期化
                        Host.netData.Set(KillTimerDID(p), 0);
                    }
                    else
                    {
                        //前回からの経過時間
                        
                        timer += gap;

                        if (timer >= 5000)
                        {
                            p.duck.Kill(new DTFall());
                        }

                        Host.netData.Set(KillTimerDID(p), timer);
                    }
                }
                else
                {
                    Host.netData.Set(KillTimerDID(p), -1);
                }
            }
            timePrevTick = Environment.TickCount;
        }

        static void SetNetDataZone(Profile p, ZoneCircle zone)
        {
            p.netData.Set(did_ZonePosition, zone.position);
            p.netData.Set(did_ZoneSize, zone.radius);
            p.netData.Set(did_ZoneColor, string.Format("{0},{1},{2},{3}", zone.color.r, zone.color.g,zone.color.b,zone.color.a));
            p.netData.Set(did_ZoneEnabled, zone.enabled);
        }

        static ZoneCircle GetNetDataZone(Profile p)
        {
            var d = p.netData.Get<string>(did_ZoneColor);
            if (d == null)
            {
                return new ZoneCircle() { enabled = false };
            }
            var s = d.Split(',');
            var c = new Color(byte.Parse(s[0]), byte.Parse(s[1]), byte.Parse(s[2]), byte.Parse(s[3]));

            return new ZoneCircle()
            {
                position = p.netData.Get<Vec2>(did_ZonePosition),
                radius = p.netData.Get<float>(did_ZoneSize),
                color = c,
                enabled = p.netData.Get<bool>(did_ZoneEnabled)
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zone"></param>
        /// <returns>有効なゾーンが存在するかどうかを返す。</returns>
        static bool UpdateZone(Role roll, ref ZoneCircle zone)
        {
            if (roll != Role.Client)
            {
                //ゾーンの進捗を更新する。(-?～0f～1f)
                //zone.Progress = GameTime / (float)timeMax;
                if (zmanager == null)
                {
                    return false;
                }

                zmanager.Progress = Math.Min(ZoneProgress, 1f);
                zmanager.Update(ref zone);
                //Profiles.active[hostIndex].netData.Set(did_Zone, zone);
                SetNetDataZone(Host, zone);
            }
            else
            {
                zone = GetNetDataZone(Host);
            }

            if (!zone.enabled) return false;
            return true;

        }

        static void DrawKillTimer()
        {
            var uiFont = Profiles.EnvironmentProfile.font;
            foreach (var p in Profiles.active)
            {
                var killTimer = Host.netData.Get<int>(KillTimerDID(p));
                if (killTimer == -1) continue;

                uiFont.DrawOutline((killTimer / 1000 + 1).ToString(), p.duck.position + new Vec2(0, -16), Color.Red, Color.Yellow);
            }
        }

        internal static void OnLevelPostDrawLayer(GameLevel __instance, Layer layer, bool isWaiting)
        {
            if ((Network.isActive && __instance.networkStatus != NetLevelStatus.Ready) || !__instance.initialized)
                return;

            if (isWaiting) return;

           

            if (layer == Layer.Foreground)
            {
                var role = GetRoll();
                if (!UpdateZone(role, ref zone))
                {
                    //有効なゾーンが存在しない。
                    return;
                }

                if (role!=Role.Client)
                {
                    UpdateKillTimer();
                }

                zone.DrawZone();

                DrawKillTimer();
            }
            if (layer == Layer.HUD)
            {

#if SHOW_TEST_LOG
                Writer.Reset();
                Writer.WriteLine(GetRoll());
                if(GetRoll()==Role.Host && zmanager == null)
                {
                    Writer.WriteLine("Host Not Found.");
                }
                /*
                Writer.WriteLine("Zone Size=" + zone.radius);
                Writer.WriteLine("Zone Position=" + zone.position);
                Writer.WriteLine("Zone Color=" + zone.color);
                Writer.WriteLine("Zone Enabled=" + zone.enabled);

                if (GetRoll() != Role.Client)
                {
                    Writer.WriteLine("HostIndex:" + hostIndex);
                    Writer.WriteLine("GTime:" + GameTime + "/" + timeMax);
                    Writer.WriteLine("ZTime:" + ZoneTime + "/" + ZoneTimeMax);
                    Writer.WriteLine("Ratio:" + GameTime / (float)timeMax);
                }
                */

                int index = 0;
                Profiles.active.ForEach(p =>
                {
                    Writer.WriteLine(p.steamID);
                    Writer.WriteLine(" kt = " + (Host.netData.Get<int>(KillTimerDID(p))));

                });

#endif
            }
        }
    }
}
