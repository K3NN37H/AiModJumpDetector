using System;
using System.Collections.Generic;
using System.Text;
using osu.GameModes.Edit.AiMod;
using osu.GameplayElements.HitObjects;
using osu.GameModes.Edit.AiMod.Reports;

namespace AiModJumpDetector
{
    /// <summary>
    /// Simple plugin that attempts to detect for jumps in a beatmap.
    /// </summary>
    public class AiModJumpDetector : AiModRuleset
    {
        public override AiModType Type
        {
            get { return AiModType.Compose; }
        }

        protected override void RunAllRules(List<HitObjectBase> hitObjects)
        {
            //Do your processing in here
            Reports.Add(new AiReport(Severity.Info, "Jump Detector loaded!"));
            HitObjectBase prev = null;
            double prevSpeed = -1;
            int count = 0;
            foreach (HitObjectBase h in hitObjects)
            {
                HitObjectType temp = h.Type;
                if ((temp & HitObjectType.Spinner) == HitObjectType.Spinner)
                {
                    if (h.Length < 1000)
                    {
                        Reports.Add(new AiReport(h.StartTime, Severity.Warning, "Spinner is less than 1000ms", 0, null));
                    }
                }

                if (prev != null)
                {
                    if ((temp & HitObjectType.Spinner) == HitObjectType.Spinner)
                    {
                        prev = null;
                        prevSpeed = -1;
                        continue;
                    }
                    int timeBet = h.StartTime - prev.EndTime;
                    if (timeBet > 1500) // reset on breaks of 1.5s or more
                    {
                        prev = h;
                        prevSpeed = -1;
                        continue;
                    }
                    double disX = (Math.Pow(h.PositionAtTime(h.StartTime).X - prev.EndPosition.X, 2.0));
                    double disY = (Math.Pow(h.PositionAtTime(h.StartTime).Y - prev.EndPosition.Y, 2.0));
                    double dis = (Math.Sqrt(disX + disY));
                    if (dis < 8) // prevents stacked notes from causing jump detection
                    {
                        prev = h;
                        continue;
                    }                  
                    double speed = dis / timeBet;
                    if (prevSpeed > -1)
                    {
                        if (speed > prevSpeed * Properties.Settings.Default.Sensitivity) // sensitivity for what counts as a jump here
                        {
                            Reports.Add(new AiReportTwoObjects(prev, h, null, Severity.Info, Decimal.Round((decimal)(speed/prevSpeed),2)+"x Jump detected", 0));
                            count += 1;
                        }                       
                    }
                    prevSpeed = speed;
                }
                prev = h;
            }
            Reports.Add(new AiReport(Severity.Info, count + " jumps detected"));
        }
    }
}
