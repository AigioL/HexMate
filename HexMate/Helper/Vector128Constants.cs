#if INTRINSICS
using System;

namespace HexMate
{
    internal static class Vector128Constants
    {
        // Formatter

        internal static ReadOnlySpan<byte> s_upperHexLookupTable => new byte[]
        {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46
        };

        internal static ReadOnlySpan<byte> s_lowerHexLookupTable => new byte[]
        {
            0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x61, 0x62, 0x63, 0x64, 0x65, 0x66
        };

        // Parser

        internal static ReadOnlySpan<byte> s_upperLowerDigHexSelector => new byte[]
        {
            0x01, 0x01, 0x01, 0x00, 0x80, 0x01, 0x80, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01,
        };
        internal static ReadOnlySpan<byte> s_digits => new byte[]
        {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        };
        internal static ReadOnlySpan<byte> s_hexs => new byte[]
        {
            0xFF, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
        };
        internal static ReadOnlySpan<byte> s_evenBytes => new byte[]
        {
            0, 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30,
        };
        internal static ReadOnlySpan<byte> s_oddBytes => new byte[]
        {
            1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31,
        };
        internal static ReadOnlySpan<byte> s_validDig => new byte[]
        {
            0x80, 0x81, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
        internal static ReadOnlySpan<byte> s_validHex => new byte[]
        {
            0x00, 0x8A, 0x8B, 0x8C, 0x8D, 0x8E, 0x8F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };
    }
}
#endif