// Copyright (c) William Humphreys.
// Licensed under the MIT license. See licence file in the project root for full license information.

using Avalonia;
using Avalonia.Logging.Serilog;
using Minesweeper.Views;

namespace Minesweeper
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<MainWindow>();
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToDebug();
    }
}
