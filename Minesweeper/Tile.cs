// Copyright (c) William Humphreys.
// Licensed under the MIT license. See licence file in the project root for full license information.

namespace Minesweeper
{
    public class Tile
    {
        public bool IsFlag { get; set; }

        public bool IsMine { get; set; }

        public int Value { get; set; }

        public bool IsRevealed { get; set; }
    }
}
