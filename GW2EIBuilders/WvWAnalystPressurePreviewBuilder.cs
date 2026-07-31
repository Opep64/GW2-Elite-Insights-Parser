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
    private const double PlotBottom = 306;
    private const double SquadEventY = 348;
    private const double EnemyEventY = 374;
    private const string SquadColor = "#56d6c9";
    private const string EnemyColor = "#ff7272";

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
        double plotWidth = PlotRight - PlotLeft;
        double plotHeight = PlotBottom - PlotTop;
        double barWidth = Math.Clamp(plotWidth / sampleCount * 0.34, 1.1, 4.0);
        var svg = new StringBuilder(64_000);

        svg.AppendLine("""<?xml version="1.0" encoding="UTF-8"?>""");
        svg.AppendLine($"""<svg xmlns="http://www.w3.org/2000/svg" width="{Width}" height="{Height}" viewBox="0 0 {Width} {Height}" role="img" aria-labelledby="title desc">""");
        svg.AppendLine("""<title id="title">Team Pressure Comparison — Strips</title>""");
        svg.AppendLine($"""<desc id="desc">Three-second rolling squad and enemy damage with boon strips, downs, and kills for {WebUtility.HtmlEncode(encounterLabel)}.</desc>""");
        svg.AppendLine("""<rect width="960" height="420" rx="18" fill="#101923"/>""");
        svg.AppendLine("""<rect x="1" y="1" width="958" height="418" rx="17" fill="none" stroke="#2b3a4b"/>""");
        svg.AppendLine("""<text x="28" y="34" fill="#f1f5f9" font-family="Segoe UI,Arial,sans-serif" font-size="18" font-weight="700">Team Pressure Comparison</text>""");
        svg.AppendLine("""<text x="28" y="57" fill="#90a4b8" font-family="Segoe UI,Arial,sans-serif" font-size="12">Burst · Comparison · Strips</text>""");
        AppendLegend(svg);

        for (int gridIndex = 0; gridIndex <= 4; gridIndex++)
        {
            double ratio = gridIndex / 4.0;
            double y = PlotBottom - ratio * plotHeight;
            svg.AppendLine($"""<line x1="{F(PlotLeft)}" y1="{F(y)}" x2="{F(PlotRight)}" y2="{F(y)}" stroke="#8fa3b8" stroke-opacity=".16"/>""");
            svg.AppendLine($"""<text x="{F(PlotLeft - 10)}" y="{F(y + 4)}" text-anchor="end" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">{FormatCompact(damageCeiling * ratio)}</text>""");
            svg.AppendLine($"""<text x="{F(PlotRight + 10)}" y="{F(y + 4)}" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">{FormatCompact(stripCeiling * ratio)}</text>""");
        }

        for (int gridIndex = 0; gridIndex <= 5; gridIndex++)
        {
            double ratio = gridIndex / 5.0;
            double x = PlotLeft + ratio * plotWidth;
            long time = (long)Math.Round(endTime * ratio);
            svg.AppendLine($"""<line x1="{F(x)}" y1="{F(PlotTop)}" x2="{F(x)}" y2="{F(PlotBottom)}" stroke="#8fa3b8" stroke-opacity=".13"/>""");
            svg.AppendLine($"""<text x="{F(x)}" y="326" text-anchor="middle" fill="#8fa3b8" font-family="Segoe UI,Arial,sans-serif" font-size="11">{FormatSeconds(time)}</text>""");
        }

        AppendBars(svg, analysis.Times, analysis.Squad.Strips, sampleCount, endTime, stripCeiling, barWidth, SquadColor, -barWidth * 0.55);
        AppendBars(svg, analysis.Times, analysis.Enemy.Strips, sampleCount, endTime, stripCeiling, barWidth, EnemyColor, barWidth * 0.55);
        svg.AppendLine($"""<path d="{BuildDamagePath(analysis.Times, analysis.Squad.Damage, sampleCount, endTime, damageCeiling)}" fill="none" stroke="{SquadColor}" stroke-width="2.6" stroke-linejoin="round" stroke-linecap="round"/>""");
        svg.AppendLine($"""<path d="{BuildDamagePath(analysis.Times, analysis.Enemy.Damage, sampleCount, endTime, damageCeiling)}" fill="none" stroke="{EnemyColor}" stroke-width="2.6" stroke-linejoin="round" stroke-linecap="round"/>""");

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

    private static void AppendLegend(StringBuilder svg)
    {
        svg.AppendLine($"""<rect x="424" y="26" width="10" height="10" rx="2" fill="{SquadColor}" fill-opacity=".38"/><text x="440" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Squad strips</text>""");
        svg.AppendLine($"""<rect x="514" y="26" width="10" height="10" rx="2" fill="{EnemyColor}" fill-opacity=".38"/><text x="530" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Enemy strips</text>""");
        svg.AppendLine($"""<line x1="615" y1="31" x2="637" y2="31" stroke="{SquadColor}" stroke-width="3"/><text x="643" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Squad damage</text>""");
        svg.AppendLine($"""<line x1="738" y1="31" x2="760" y2="31" stroke="{EnemyColor}" stroke-width="3"/><text x="766" y="35" fill="#b9c6d3" font-family="Segoe UI,Arial,sans-serif" font-size="11">Enemy damage</text>""");
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
        double offset)
    {
        double plotWidth = PlotRight - PlotLeft;
        double plotHeight = PlotBottom - PlotTop;
        for (int index = 0; index < count; index++)
        {
            int value = values[index];
            if (value <= 0)
            {
                continue;
            }

            double x = PlotLeft + times[index] / (double)endTime * plotWidth + offset - barWidth / 2;
            double height = Math.Max(1, value / ceiling * plotHeight);
            double y = PlotBottom - height;
            svg.AppendLine($"""<rect x="{F(x)}" y="{F(y)}" width="{F(barWidth)}" height="{F(height)}" fill="{color}" fill-opacity=".34"/>""");
        }
    }

    private static string BuildDamagePath(
        IReadOnlyList<long> times,
        IReadOnlyList<long> values,
        int count,
        long endTime,
        double ceiling)
    {
        double plotWidth = PlotRight - PlotLeft;
        double plotHeight = PlotBottom - PlotTop;
        var path = new StringBuilder(count * 14);
        for (int index = 0; index < count; index++)
        {
            double x = PlotLeft + times[index] / (double)endTime * plotWidth;
            double y = PlotBottom - Math.Max(0, values[index]) / ceiling * plotHeight;
            path.Append(index == 0 ? 'M' : 'L')
                .Append(F(x))
                .Append(' ')
                .Append(F(y))
                .Append(' ');
        }
        return path.ToString().TrimEnd();
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
