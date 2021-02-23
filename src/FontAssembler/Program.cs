﻿using FontAssembler.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FontAssembler
{
    public static class Program
    {
        public static void Main()
        {
            var lines = File.ReadAllLines(@"font/dec_5_bit.txt");

            var glyphs = lines.Select(x => ParseLine(x)).OrderBy(x => (int)x.Character).ToArray();

            var asm = glyphs.Select(Pack).SelectMany(x => x).ToArray();

            var lookup = CreateLookup(glyphs);

            File.WriteAllLines(@"../../../asm/glyphs.asm", lookup.Concat(asm));
        }

        private static IEnumerable<string> CreateLookup(Glyph[] glyphs)
        {
            for (var c = 32; c < 96; c++)
            {
                var glyph = glyphs.Single(x => x.Character == (char)c);

                if (c == 32)
                {
                    yield return $"LOOKUP,\t{glyph.Label}";
                }
                else
                {
                    yield return $"      \t{glyph.Label}";
                }
            }

            yield return string.Empty;
        }

        private static Glyph ParseLine(string line, char delimiter = '|')
        {
            var parts = line.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
            {
                throw new ArgumentOutOfRangeException(nameof(line));
            }

            return new Glyph(
                parts[0].Single(),
                parts[1].Trim(),
                parts[2].Split(',').Select(x => x.ToDecimal()).ToArray()
                );
        }

        private static IEnumerable<string> Pack(Glyph glyph)
        {
            var word = 0;

            var lines = new List<string>();

            for (var i = 0; i < glyph.Definition.Length; i++)
            {
                if (i % 2 == 0)
                {
                    word |= (glyph.Definition[i] & 0b_1111_1100) << 4;
                }
                else
                {
                    word |= (glyph.Definition[i] & 0b_1111_1100) >> 2;

                    var line = lines.Count == 0
                        ? $"{glyph.Label}{new string(' ', 6 - glyph.Label.Length)}\t{word.ToOctalString()}"
                        : $"      \t{word.ToOctalString()}";

                    lines.Add(line);

                    word = 0;
                }
            }

            return lines;
        }

    }
}
