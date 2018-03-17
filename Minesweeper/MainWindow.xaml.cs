﻿// Copyright (c) William Humphreys.
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

namespace Minesweeper
{
    public class MainWindow : Window
    {
        const int COLUMN_COUNT = 16;
        const int ROW_COUNT    = 16;
        const int MINE_COUNT   = 40;
        const int BUTTON_SIZE  = 25;

        Bitmap _happyFaceBitmap;
        Bitmap _unhappyFaceBitmap;

        Bitmap _flagBitmap;
        Bitmap _mineBitmap;

        private Button _restartButton;

        private TextBox _flagScore;

        private Grid _buttonGridPlaceholder;

        private Button[,] _displayGrid = new Button[ROW_COUNT, COLUMN_COUNT];

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

            _happyFaceBitmap   = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Resources") + Path.DirectorySeparatorChar.ToString() + "HappyFace.png");
            _unhappyFaceBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Resources") + Path.DirectorySeparatorChar.ToString() + "UnhappyFace.png");

            _flagBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Resources") + Path.DirectorySeparatorChar.ToString() + "Flag.png");
            _mineBitmap = new Bitmap(Path.Combine(Directory.GetCurrentDirectory(), "Resources") + Path.DirectorySeparatorChar.ToString() + "Mine.png");

            // Apply the color to the main window.
            this.Background = new SolidColorBrush { Color = Color.FromRgb(189, 189, 189) };

            // You need to get XAML controls in Avalonia manually. 
            _restartButton = this.FindControl<Button>("restart");
            _restartButton.Click += new EventHandler<RoutedEventArgs>(OnRestartButtonClick);

            _flagScore = this.FindControl<TextBox>("flagscore");

            _buttonGridPlaceholder = this.FindControl<Grid>("grid");

            CreateEmptyButtonGrid();
        }

        private void RestartGame()
        {
            _game.Start();

            ResetEmptyButtonGrid();

            PaintGameGrid();
        }

        private void CreateEmptyButtonGrid()
        {
            _buttonGridPlaceholder.Height = ROW_COUNT * BUTTON_SIZE;
            _buttonGridPlaceholder.Width  = COLUMN_COUNT * BUTTON_SIZE;
            
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COLUMN_COUNT; column++)
                {
                    _displayGrid[row, column] = new Button
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

                    _displayGrid[row, column].AddHandler(PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);

                    _buttonGridPlaceholder.Children.Add(_displayGrid[row, column]);
                }
            }
        }

        private void ResetEmptyButtonGrid()
        {
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COLUMN_COUNT; column++)
                {
                    _displayGrid[row, column].Background = new SolidColorBrush { Color = Color.FromRgb(189, 189, 189) };
                    _displayGrid[row, column].Content    = String.Empty;
                    _displayGrid[row, column].IsEnabled  = true;
                }
            }
        }

        private void OnRestartButtonClick(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }

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
                            Source = _flagBitmap,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        _game.FlagScore = 1;
                    }
                }

                PaintGameGrid();
            }
        }

        private void PaintGameGrid()
        {
            for (int row = 0; row < ROW_COUNT; row++)
            {
                for (int column = 0; column < COLUMN_COUNT; column++)
                {
                    if (_game.GameGrid[row, column].IsRevealed)
                    {
                        _displayGrid[row, column].Background = new SolidColorBrush { Color = Color.FromRgb(223, 221, 221) };
                        _displayGrid[row, column].IsEnabled = false;

                        if (_game.GameGrid[row, column].IsMine)
                        {
                            _displayGrid[row, column].Content = new Image
                            {
                                Source            = _mineBitmap,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                        }
                        else if (_game.GameGrid[row, column].Value > 0)
                        {
                            if (_game.GameGrid[row, column].Value == 1)
                            {
                                _displayGrid[row, column].Foreground = Brushes.Blue;
                            }
                            else if (_game.GameGrid[row, column].Value ==2)
                            {
                                _displayGrid[row, column].Foreground = Brushes.Green;
                            }
                            else if (_game.GameGrid[row, column].Value == 3)
                            {
                                _displayGrid[row, column].Foreground = Brushes.Red;
                            }
                            else if (_game.GameGrid[row, column].Value == 4)
                            {
                                _displayGrid[row, column].Foreground = Brushes.Purple;
                            }
                            else 
                            {
                                _displayGrid[row, column].Foreground = Brushes.Brown;
                            }

                            _displayGrid[row, column].Content    = _game.GameGrid[row, column].Value;
                        }

                    }

                    if (_game.IsGameActive)
                    {
                        _restartButton.Content = new Image
                        {
                            Source              = _happyFaceBitmap,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment   = VerticalAlignment.Center
                        };
                    }
                    else
                    {
                        _restartButton.Content = new Image
                        {
                            Source              = _unhappyFaceBitmap,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment   = VerticalAlignment.Center
                        };
                    }

                    _flagScore.Text = _game.FlagScore.ToString();
                }
            }
        }

    }
}