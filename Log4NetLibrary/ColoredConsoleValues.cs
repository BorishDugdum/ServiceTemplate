using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Log4NetLibrary
{
    internal static class ColoredConsoleValues
    {
        public static List<ColoredConsoleAppender.LevelColors> Colors { get; } = new List<ColoredConsoleAppender.LevelColors>()
        {
            new ColoredConsoleAppender.LevelColors()
                {
                    ForeColor = ColoredConsoleAppender.Colors.HighIntensity,
                    Level = Level.Debug
                },
            new ColoredConsoleAppender.LevelColors()
                {
                    ForeColor = ColoredConsoleAppender.Colors.White,
                    Level = Level.Info
                },
            new ColoredConsoleAppender.LevelColors()
                {
                    BackColor = ColoredConsoleAppender.Colors.Yellow,
                    ForeColor = ColoredConsoleAppender.Colors.Red,
                    Level = Level.Warn
                },
            new ColoredConsoleAppender.LevelColors()
                {
                    BackColor = ColoredConsoleAppender.Colors.Red,
                    ForeColor = ColoredConsoleAppender.Colors.White,
                    Level = Level.Error
                },
            new ColoredConsoleAppender.LevelColors()
                {
                    BackColor = ColoredConsoleAppender.Colors.White,
                    ForeColor = ColoredConsoleAppender.Colors.Red,
                    Level = Level.Fatal
                },
            new ColoredConsoleAppender.LevelColors()
                {
                    BackColor = ColoredConsoleAppender.Colors.Blue,
                    ForeColor = ColoredConsoleAppender.Colors.White,
                    Level = Level.Verbose
                },
            new ColoredConsoleAppender.LevelColors()
                {
                    ForeColor = ColoredConsoleAppender.Colors.Green,
                    Level = Level.Notice
                },
            new ColoredConsoleAppender.LevelColors()
                {
                    BackColor = ColoredConsoleAppender.Colors.HighIntensity,
                    ForeColor = ColoredConsoleAppender.Colors.Blue,
                    Level = Level.Trace
                },
        };
    }
}
