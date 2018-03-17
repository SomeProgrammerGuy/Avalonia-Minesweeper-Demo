using Avalonia;
using Avalonia.Markup.Xaml;

namespace Minesweeper
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
