using System.Globalization;
using System.Net;
using System.Text;
using GW2EIBuilders.HtmlModels;

namespace GW2EIBuilders;

internal static class WvWAnalystPressurePreviewBuilder
{
    private const int Width = 960;
    private const int Height = 420;
    private const double PlotLeft = 76;
    private const double PlotTop = 92;
    private const double PlotRight = 902;
    private const double DefaultPlotBottom = 306;
    private const double PositioningPlotBottom = 232;
    private const double PositioningLaneTop = 258;
    private const double PositioningLaneBottom = 306;
    private const double SquadEventY = 348;
    private const double EnemyEventY = 374;
    private const string SquadColor = "#56d6c9";
    private const string EnemyColor = "#ff7272";
    private const string PositioningColor = "#e5bd56";

    public static string? Build(CombatReplayAnalysisDto? analysis, string encounterLabel)
    {
        if (analysis is null || analysis.Times.Length < 2)
        {
            return null;
        }

        int sampleCount = new[]
        {
            analysis.Times.Length,
            analysis.Squad.Damage.Length,
            analysis.Enemy.Damage.Length,
            analysis.Squad.Strips.Length,
            analysis.Enemy.Strips.Length,
        }.Min();
        if (sampleCount < 2)
        {
            return null;
        }

        CombatReplayPositioningAnalysisDto positioning = analysis.Positioning;
        int positioningSampleCount = new[]
        {
            sampleCount,
            positioning.EligiblePlayerCount.Length,
            positioning.InPositionCount.Length,
            positioning.InPositionRate.Length,
        }.Min();
        bool hasPositioningLane = positioning.HasCommander &&
            positioningSampleCount >= 2 &&
            positioning.EligiblePlayerCount.Take(positioningSampleCount).Any(count => count > 0);

        long endTime = Math.Max(1, analysis.Times.Take(sampleCount).Max());
        long maxDamage = Math.Max(
            1,
            Math.Max(
                analysis.Squad.Damage.Take(sampleCount).DefaultIfEmpty().Max(),
                analysis.Enemy.Damage.Take(sampleCount).DefaultIfEmpty().Max()));
        int maxStrips = Math.Max(
            1,
            Math.Max(
                analysis.Squad.Strips.Take(sampleCount).DefaultIfEmpty().Max(),
                analysis.Enemy.Strips.Take(sampleCount).DefaultIfEmpty().Max()));
        double damageCeiling = NiceCeiling(maxDamage);
        double stripCeiling = NiceCeiling(maxStrips);
        double plotBottom = hasPositioningLane ? PositioningPlotBottom : DefaultPlotBottom;
        double plotWidth = PlotRight - PlotLeft;
        double plotHeight = plotBottom - PlotTop;
        double barWidth = Math.Clamp(plotWidth / sampleCount * 0.34, 1.1, 4.0);
        var svg = new StringBuilder(64_000);

        svg.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        svg.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{Width}" height="{Height}" viewBox="0 0 {Width} {Height}" role="img" aria-labelledby="title desc">""");
        svg.AppendLine("""<title id="title">Team Pressure Comparison — Strips</title>""");
        string positioningDescription = hasPositioningLane ? ", squad positioning" : "";
        svg.AppendLine($"""<desc id="desc">Three-second rolling squad and enemy damage with boon strips{positioningDescription}, downs, and kills for {WebUtility.HtmlEncode(encounterLabel)}.</desc>""");
        svg.AppendLine("""<rect width="960" height="420" rx="18" fill="#101923"/>""");
        svg.AppendLine("""<rect x="1" y="1" width="958" height="418" rx="17" fill="none" stroke="#2b3a4b"/>""");
        svg.AppendLine("""<text x="28" y="34" fill="#f1f5f9" font-family="Segoe UI,Arial,sans-serif" font-size="18" font-weight="700">Team Pressure Comparison</text>""");
        svg.AppendLine("""<text x="28" y="57" fill="#90a4b8" font-family="Segoe UI,Arial,sans-serif" font-size="12">Burst · Comparison · Strips</text>""");
        AppendLegend(svg, hasPositioningLane);

        for (int gridIndex = 0; gridIndex <= 4; gridIndex++)
        {
            double ratio = gridIndex / 4.0;
            double y = plotBottom - ratio * plotHeight;
            svg.AppendLine($"""<line x1="{F(PlotLeft)}" y1="{F(y)}" x2="{F(PlotRight)}" y2="{F(y)}" stroke="#8fa3b8" stroke-opacity=".16"/>""");
            svg.AppendLine($"""<text x="{F(PlotLeft - 10)}" y="{F(y + 4)}" text-anchor="end" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">{FormatCompact(damageCeiling * ratio)}</text>""");
            svg.AppendLine($"""<text x="{F(PlotRight + 10)}" y="{F(y + 4)}" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">{FormatCompact(stripCeiling * ratio)}</text>""");
        }

        for (int gridIndex = 0; gridIndex <= 5; gridIndex++)
        {
            double ratio = gridIndex / 5.0;
            double x = PlotLeft + ratio * plotWidth;
            long time = (long)Math.Round(endTime * ratio);
            double gridBottom = hasPositioningLane ? PositioningLaneBottom : plotBottom;
            svg.AppendLine($"""<line x1="{F(x)}" y1="{F(PlotTop)}" x2="{F(x)}" y2="{F(gridBottom)}" stroke="#8fa3b8" stroke-opacity=".13"/>""");
            svg.AppendLine($"""<text x="{F(x)}" y="326" text-anchor="middle" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">{FormatSeconds(time)}</text>""");
        }

        AppendBars(svg, analysis.Times, analysis.Squad.Strips, sampleCount, endTime, stripCeiling, barWidth, SquadColor, -barWidth * 0.55, plotBottom);
        AppendBars(svg, analysis.Times, analysis.Enemy.Strips, sampleCount, endTime, stripCeiling, barWidth, EnemyColor, barWidth * 0.55, plotBottom);
        svg.AppendLine($"""<path d="{BuildDamagePath(analysis.Times, analysis.Squad.Damage, sampleCount, endTime, damageCeiling, plotBottom)}" fill="none" stroke="{SquadColor}" stroke-width="2.6" stroke-linejoin="round" stroke-linecap="round"/>""");
        svg.AppendLine($"""<path d="{BuildDamagePath(analysis.Times, analysis.Enemy.Damage, sampleCount, endTime, damageCeiling, plotBottom)}" fill="none" stroke="{EnemyColor}" stroke-width="2.6" stroke-linejoin="round" stroke-linecap="round"/>""");

        if (hasPositioningLane)
        {
            AppendPositioningLane(svg, analysis.Times, positioning, positioningSampleCount, endTime);
        }

        svg.AppendLine("""<text x="68" y="352" text-anchor="end" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">Squad</text>""");
        svg.AppendLine("""<text x="68" y="378" text-anchor="end" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">Enemy</text>""");
        svg.AppendLine($"""<line x1="{F(PlotLeft)}" y1="{F(SquadEventY)}" x2="{F(PlotRight)}" y2="{F(SquadEventY)}" stroke="#8fa3b8" stroke-opacity=".12"/>""");
        svg.AppendLine($"""<line x1="{F(PlotLeft)}" y1="{F(EnemyEventY)}" x2="{F(PlotRight)}" y2="{F(EnemyEventY)}" stroke="#8fa3b8" stroke-opacity=".12"/>""");
        AppendEvents(svg, analysis.Events.Downs.Events, isKill: false, endTime);
        AppendEvents(svg, analysis.Events.Kills.Events, isKill: true, endTime);
        svg.AppendLine("""<text x="918" y="404" text-anchor="end" fill="#60758a" font-family="Segoe UI,Arial,sans-serif" font-size="10">Elite Insights · WvW Analyst preview</text>""");
        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    private static void AppendLegend(StringBuilder svg, bool hasPositioningLane)
    {
        svg.AppendLine($"""<rect x="424" y="26" width="10" height="10" rx="2" fill="{SquadColor}" fill-opacity=".38"/><text x="440" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Squad strips</text>""");
        svg.AppendLine($"""<rect x="514" y="26" width="10" height="10" rx="2" fill="{EnemyColor}" fill-opacity=".38"/><text x="530" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Enemy strips</text>""");
        svg.AppendLine($"""<line x1="615" y1="31" x2="637" y2="31" stroke="{SquadColor}" stroke-width="3"/><text x="643" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Squad damage</text>""");
        svg.AppendLine($"""<line x1="738" y1="31" x2="760" y2="31" stroke="{EnemyColor}" stroke-width="3"/><text x="766" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Enemy damage</text>""");
        if (hasPositioningLane)
        {
            svg.AppendLine($"""<line x1="738" y1="55" x2="760" y2="55" stroke="{PositioningColor}" stroke-width="3"/><text x="766" y="59" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Squad in position</text>""");
        }
    }

    private static void AppendBars(
        StringBuilder svg,
        IReadOnlyList<long> times,
        IReadOnlyList<int> values,
        int count,
        long endTime,
        double ceiling,
        double barWidth,
        string color,
        double offset,
        double plotBottom)
    {
        double plotWidth = PlotRight - PlotLeft;
        double plotHeight = plotBottom - PlotTop;
        for (int index = 0; index < count; index++)
        {
            int value = values[index];
            if (value <= 0)
            {
                continue;
            }

            double x = PlotLeft + times[index] / (double)endTime * plotWidth + offset - barWidth / 2;
            double height = Math.Max(1, value / ceiling * plotHeight);
            double y = plotBottom - height;
            svg.AppendLine($"""<rect x="{F(x)}" y="{F(y)}" width="{F(barWidth)}" height="{F(height)}" fill="{color}" fill-opacity=".34"/>""");
        }
    }

    private static string BuildDamagePath(
        IReadOnlyList<long> times,
        IReadOnlyList<long> values,
        int count,
        long endTime,
        double ceiling,
        double plotBottom)
    {
        double plotWidth = PlotRight - PlotLeft;
        double plotHeight = plotBottom - PlotTop;
        var path = new StringBuilder(count * 14);
        for (int index = 0; index < count; index++)
        {
            double x = PlotLeft + times[index] / (double)endTime * plotWidth;
            double y = plotBottom - Math.Max(0, values[index]) / ceiling * plotHeight;
            path.Append(index == 0 ? 'M' : 'L')
                .Append(F(x))
                .Append(' ')
                .Append(F(y))
                .Append(' ');
        }
        return path.ToString().TrimEnd();
    }

    private static void AppendPositioningLane(
        StringBuilder svg,
        IReadOnlyList<long> times,
        CombatReplayPositioningAnalysisDto positioning,
        int count,
        long endTime)
    {
        double plotWidth = PlotRight - PlotLeft;
        double laneHeight = PositioningLaneBottom - PositioningLaneTop;
        svg.AppendLine("""<text x="68" y="286" text-anchor="end" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">Position</text>""");
        for (int gridIndex = 0; gridIndex <= 2; gridIndex++)
        {
            double rate = gridIndex * 50.0;
            double y = PositioningLaneBottom - rate / 100.0 * laneHeight;
            svg.AppendLine($"""<line x1="{F(PlotLeft)}" y1="{F(y)}" x2="{F(PlotRight)}" y2="{F(y)}" stroke="#8fa3b8" stroke-opacity=".16"/>""");
            svg.AppendLine($"""<text x="{F(PlotRight + 10)}" y="{F(y + 4)}" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">{rate:0}%</text>""");
        }

        int segmentStart = -1;
        for (int index = 0; index <= count; index++)
        {
            bool eligible = index < count && positioning.EligiblePlayerCount[index] > 0;
            if (eligible && segmentStart < 0)
            {
                segmentStart = index;
            }
            if (eligible || segmentStart < 0)
            {
                continue;
            }

            int segmentEnd = index - 1;
            var linePath = new StringBuilder((segmentEnd - segmentStart + 1) * 16);
            var areaPath = new StringBuilder((segmentEnd - segmentStart + 1) * 16 + 40);
            double startX = PositionX(times[segmentStart], endTime, plotWidth);
            double endX = PositionX(times[segmentEnd], endTime, plotWidth);
            areaPath.Append('M').Append(F(startX)).Append(' ').Append(F(PositioningLaneBottom)).Append(' ');
            for (int pointIndex = segmentStart; pointIndex <= segmentEnd; pointIndex++)
            {
                double x = PositionX(times[pointIndex], endTime, plotWidth);
                double y = PositionY(positioning.InPositionRate[pointIndex], laneHeight);
                linePath.Append(pointIndex == segmentStart ? 'M' : 'L').Append(F(x)).Append(' ').Append(F(y)).Append(' ');
                areaPath.Append('L').Append(F(x)).Append(' ').Append(F(y)).Append(' ');
            }
            areaPath.Append('L').Append(F(endX)).Append(' ').Append(F(PositioningLaneBottom)).Append(" Z");
            svg.AppendLine($"""<path d="{areaPath}" fill="{PositioningColor}" fill-opacity=".12"/>""");
            svg.AppendLine($"""<path d="{linePath.ToString().TrimEnd()}" fill="none" stroke="{PositioningColor}" stroke-width="2.6" stroke-linejoin="round" stroke-linecap="round"/>""");
            if (segmentStart == segmentEnd)
            {
                double y = PositionY(positioning.InPositionRate[segmentStart], laneHeight);
                svg.AppendLine($"""<circle cx="{F(startX)}" cy="{F(y)}" r="2.2" fill="{PositioningColor}"/>""");
            }
            segmentStart = -1;
        }
    }

    private static double PositionX(long time, long endTime, double plotWidth)
    {
        return PlotLeft + time / (double)endTime * plotWidth;
    }

    private static double PositionY(double rate, double laneHeight)
    {
        return PositioningLaneBottom - Math.Clamp(rate, 0, 100) / 100.0 * laneHeight;
    }

    private static void AppendEvents<TEvent>(
        StringBuilder svg,
        IReadOnlyList<TEvent> events,
        bool isKill,
        long endTime)
        where TEvent : CombatReplayDownEventDto
    {
        double plotWidth = PlotRight - PlotLeft;
        foreach (TEvent eventEntry in events)
        {
            double x = PlotLeft + Math.Clamp(eventEntry.Time / (double)endTime, 0, 1) * plotWidth;
            double y = eventEntry.IsEnemy ? SquadEventY : EnemyEventY;
            string color = eventEntry.IsEnemy ? SquadColor : EnemyColor;
            if (isKill)
            {
                svg.AppendLine($"""<path d="M {F(x)} {F(y - 6)} L {F(x + 6)} {F(y)} L {F(x)} {F(y + 6)} L {F(x - 6)} {F(y)} Z" fill="{color}"/>""");
            }
            else
            {
                svg.AppendLine($"""<circle cx="{F(x)}" cy="{F(y)}" r="5" fill="#101923" stroke="{color}" stroke-width="2.2"/>""");
            }
        }
    }

    private static double NiceCeiling(double value)
    {
        if (value <= 0)
        {
            return 1;
        }

        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(value)));
        double normalized = value / magnitude;
        double nice = normalized <= 1 ? 1 : normalized <= 2 ? 2 : normalized <= 5 ? 5 : 10;
        return nice * magnitude;
    }

    private static string FormatCompact(double value)
    {
        if (value >= 1_000_000)
        {
            return $"{value / 1_000_000:0.#}m";
        }
        if (value >= 1_000)
        {
            return $"{value / 1_000:0.#}k";
        }
        return value.ToString("0", CultureInfo.InvariantCulture);
    }

    private static string FormatSeconds(long milliseconds)
    {
        return $"{milliseconds / 1000.0:0}s";
    }

    private static string F(double value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
