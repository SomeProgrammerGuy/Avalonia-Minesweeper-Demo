// Copyright (c) William Humphreys.
// Licensed under the MIT license. See licence file in the project root for full license information.

using System;
using System.IO;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Threading;
using Minesweeper.Models;

namespace Minesweeper.Views
{
    public class MainWindow : Window
    {
        const int COLUMN_COUNT = 16;
        const int ROW_COUNT    = 16;
        const int MINE_COUNT   = 40;
        const int BUTTON_SIZE  = 25;

        Bitmap _happyFaceBitmap;
        Bitmap _unhappyFaceBitmap;
        Bitmap _coolFaceBitmap;

        Bitmap _flagBitmap;
        Bitmap _mineBitmap;
        Bitmap _explodedMineBitmap;

        private TextBox _timer;

        private Button _restartButton;

        private TextBox _flagScore;

        private Grid _buttonGridPlaceholder;

        private Button[,] _buttonGrid = new Button[ROW_COUNT, COLUMN_COUNT];

        DispatcherTimer _dispatcherTimer;

        Game _game;

        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            this.AttachDevTools();
#endif
            _game = new Game(ROW_COUNT, COLUMN_COUNT, MINE_COUNT);

            RestartGame();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            // Path.Combine with the file name doesnt create the right cross platform folder
            // slash so Path.DirectorySeparatorChar has been used before the filename.

            _happyFaceBitmap   = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Assets") + Path.DirectorySeparatorChar.ToString() + "HappyFace.png");
            _unhappyFaceBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Assets") + Path.DirectorySeparatorChar.ToString() + "UnhappyFace.png");
            _coolFaceBitmap    = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Assets") + Path.DirectorySeparatorChar.ToString() + "CoolFace.png");

            _flagBitmap         = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Assets") + Path.DirectorySeparatorChar.ToString() + "Flag.png");
            _mineBitmap         = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Assets") + Path.DirectorySeparatorChar.ToString() + "Mine.png");
            _explodedMineBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Assets") + Path.DirectorySeparatorChar.ToString() + "ExplodedMine.png");

            // You need to get a handle to the XAML controls in Avalonia manually.
            _timer = this.FindControl<TextBox>("timer");
                         
            _restartButton = this.FindControl<Button>("restart");
            _restartButton.Click += new EventHandler<RoutedEventArgs>(OnRestartButtonClick);

            _flagScore = this.FindControl<TextBox>("flagscore");

            _buttonGridPlaceholder = this.FindControl<Grid>("grid");

            // Uses Avalonia.Threading to create a background timer.
            _dispatcherTimer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background, UpdateTimer);

            CreateEmptyButtonGrid();
        }

        /// <summary>
        /// Reset and restart the game.
        /// </summary>
        private void RestartGame()
        {
            _game.Start();

            ResetEmptyButtonGrid();

            _timer.Text = "0";

            PaintGame();
        }

        /// <summary>
        /// The event handler for the DispatchTimer. 
        /// 
        /// This should fire every second once the game has started
        /// and increment the game counter from 0 to a maximum of 999.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateTimer(object sender, EventArgs e)
        {
            _game.Timer++;

            if (_game.Timer <= 999)
            {
                // Display timer.
                _timer.Text = _game.Timer.ToString();
            }
            else
            {
                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();
                }
            }
        }

        /// <summary>
        /// Dynamically create and configure to a set of default properties
        /// a grid of ROW_COUNT * COLUMN_COUNT buttons.
        /// </summary>
        private void CreateEmptyButtonGrid()
        {
            _buttonGridPlaceholder.Height = ROW_COUNT * BUTTON_SIZE;
            _buttonGridPlaceholder.Width  = COLUMN_COUNT * BUTTON_SIZE;
            
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COLUMN_COUNT; column++)
                {
                    _buttonGrid[row, column] = new Button
                    {
                        Name                = "Button_" + row.ToString() + "_" + column.ToString(),
                        Height              = BUTTON_SIZE,
                        Width               = BUTTON_SIZE,
                        HorizontalAlignment = HorizontalAlignment.Left,   // HorizontalAlignment.Left Enum needs using Avalonia.Layout;
                        VerticalAlignment   = VerticalAlignment.Top,      // VerticalAlignment.Top Enum needs using Avalonia.Layout;
                        Margin              = new Thickness((row * BUTTON_SIZE), (column * BUTTON_SIZE), 0.0, 0.0),
                        Padding             = new Thickness(0,2,0,0),
                        Background          = new SolidColorBrush { Color = Color.FromRgb(189, 189, 189) },
                        BorderThickness     = 1.0,
                        BorderBrush         = new SolidColorBrush { Color = Color.FromRgb(123, 123, 123) },
                        FontSize            = 15,
                        FontWeight          = FontWeight.ExtraBold
                    };

                    _buttonGrid[row, column].AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);

                    _buttonGridPlaceholder.Children.Add(_buttonGrid[row, column]);
                }
            }
        }

        /// <summary>
        /// Reset all the buttons in the button grid to the 
        /// default color, content and enable them.
        /// </summary>
        private void ResetEmptyButtonGrid()
        {
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COLUMN_COUNT; column++)
                {
                    _buttonGrid[row, column].Background = new SolidColorBrush { Color = Color.FromRgb(189, 189, 189) };
                    _buttonGrid[row, column].Content    = String.Empty;
                    _buttonGrid[row, column].IsEnabled  = true;
                }
            }
        }

        /// <summary>
        /// The event handler for the restart button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRestartButtonClick(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }

        /// <summary>
        /// The event handler to detect when the left or right mouse button
        /// has been pressed over one of the buttons in the dynamic button grid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPointerPressed(object sender, PointerPressedEventArgs e)
        {
            Button button = sender as Button;

            string[] buttonGridPosition = button.Name.Split("_");

            int row    = Convert.ToInt32(buttonGridPosition[1]);
            int column = Convert.ToInt32(buttonGridPosition[2]);

            if (!_game.GameGrid[row, column].IsRevealed)
            {
                if (e.MouseButton == MouseButton.Left)
                {
                    _game.RevealTile(row, column);

                    // Start the timer after the first tile is opened.
                    if (_game.State == GameState.Start)
                    {
                        _game.State = GameState.Running;

                        if (_dispatcherTimer != null)
                        {
                            _dispatcherTimer.Start();
                        }
                    }
                }
                else if (e.MouseButton == MouseButton.Right)
                {
                    if (_game.GameGrid[row, column].IsFlag)
                    {
                        // Remove flag.

                        _game.GameGrid[row, column].IsFlag = false;

                        button.Content = String.Empty;

                        _game.FlagScore = -1;
                    }
                    else
                    {
                        // Place flag.

                        _game.GameGrid[row, column].IsFlag = true;

                        button.Content = new Image
                        {
                            Source            = _flagBitmap,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        _game.FlagScore = 1;
                    }
                }

                PaintGame();
            }
        }

        private void PaintGame()
        {
            PaintDashboard();
            PaintGrid();
        }

        private void PaintDashboard()
        {
            // Display the restart button image.
            Bitmap restartButtonBitmapSource;

            if (_game.State == GameState.Lost)
            {
                restartButtonBitmapSource = _unhappyFaceBitmap;

                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();
                }
            }
            else if (_game.State == GameState.Won)
            {
                restartButtonBitmapSource = _coolFaceBitmap;

                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();
                }
            }
            else
            {
                restartButtonBitmapSource = _happyFaceBitmap;
            }

            _restartButton.Content = new Image
            {
                Source = restartButtonBitmapSource
            };

            // Display the flag count.
            _flagScore.Text = _game.FlagScore.ToString();
        }

        private void PaintGrid()
        {
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COLUMN_COUNT; column++)
                {
                    if (_game.GameGrid[row, column].IsRevealed)
                    {
                        _buttonGrid[row, column].Background = new SolidColorBrush { Color = Color.FromRgb(223, 221, 221) };
                        _buttonGrid[row, column].IsEnabled = false;

                        if (_game.GameGrid[row, column].IsMine)
                        {
                            Bitmap tileBitmapSource;

                            if (_game.State == GameState.Won)
                            {
                                tileBitmapSource = _flagBitmap;
                            }
                            else
                            {
                                if (_game.GameGrid[row, column].IsExplodedMine)
                                {
                                    tileBitmapSource = _explodedMineBitmap;
                                }
                                else
                                {
                                    tileBitmapSource = _mineBitmap;
                                }
                            }

                            _buttonGrid[row, column].Content = new Image
                            {
                                Source = tileBitmapSource,
                            };
                        }
                        else if (_game.GameGrid[row, column].Value > 0)
                        {
                            if (_game.GameGrid[row, column].Value == 1)
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Blue;
                            }
                            else if (_game.GameGrid[row, column].Value ==2)
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Green;
                            }
                            else if (_game.GameGrid[row, column].Value == 3)
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Red;
                            }
                            else if (_game.GameGrid[row, column].Value == 4)
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Purple;
                            }
                            else if (_game.GameGrid[row, column].Value == 5)
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Orange;
                            }
                            else if (_game.GameGrid[row, column].Value == 6)
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Pink;
                            }
                            else if (_game.GameGrid[row, column].Value == 7)
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Yellow;
                            }
                            else 
                            {
                                _buttonGrid[row, column].Foreground = Brushes.Brown;
                            }

                            _buttonGrid[row, column].Content = _game.GameGrid[row, column].Value;
                        }
                    }
                }
            }
        }
    }
}
