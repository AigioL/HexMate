#if INTRINSICS
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Sse2;
using static System.Runtime.Intrinsics.X86.Ssse3;
using static HexMate.Vector128Constants;
using static HexMate.VectorUtils;

namespace HexMate
{
    internal static partial class Utf8HexParser
    {
        internal static unsafe class Ssse3
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TryParse(ref byte* srcBytes, ref byte* destBytes, int destLength)
            {
                Debug.Assert(System.Runtime.Intrinsics.X86.Ssse3.IsSupported);

                var x0F = Vector128.Create((byte) 0x0F);
                var x30 = Vector128.Create((byte) 0x30);
                var x40 = Vector128.Create((byte) 0x40);
                var x60 = Vector128.Create((byte) 0x60);
                var xF0 = Vector128.Create((byte) 0xF0);
                var validDig = ReadVector<Vector128<byte>>(s_validDig);
                var validHex = ReadVector<Vector128<byte>>(s_validHex);
                var evenBytes = ReadVector<Vector128<byte>>(s_evenBytes);
                var oddBytes = ReadVector<Vector128<byte>>(s_oddBytes);
                var src = srcBytes;
                var dest = destBytes;

                var target = dest + FastMath.RoundDownTo16(destLength);
                while (dest != target)
                {
                    var inputLeft = LoadVector128(src);
                    src += 16;
                    var inputRight = LoadVector128(src);
                    src += 16;

                    var hiNibbleLeft = And(inputLeft, xF0);
                    var hiNibbleRight = And(inputRight, xF0);

                    var upperHexMaskLeft = CompareEqual(hiNibbleLeft, x40);
                    var lowerHexMaskLeft = CompareEqual(hiNibbleLeft, x60);
                    var digitMaskLeft = CompareEqual(hiNibbleLeft, x30);
                    var upperHexMaskRight = CompareEqual(hiNibbleRight, x40);
                    var lowerHexMaskRight = CompareEqual(hiNibbleRight, x60);
                    var digitMaskRight = CompareEqual(hiNibbleRight, x30);

                    var hexMaskLeft = Or(upperHexMaskLeft, lowerHexMaskLeft);
                    var hexMaskRight = Or(upperHexMaskRight, lowerHexMaskRight);

                    var hiNibbleValidLeft = Or(digitMaskLeft, hexMaskLeft);
                    var hiNibbleValidRight = Or(digitMaskRight, hexMaskRight);

                    var digitsAndFLeft = Or(inputLeft, hexMaskLeft);
                    var hexAndFLeft = Or(inputLeft, digitMaskLeft);
                    var digitsAndFRight = Or(inputRight, hexMaskRight);
                    var hexAndFRight = Or(inputRight, digitMaskRight);

                    var okDig1 = Shuffle(validDig, digitsAndFLeft);
                    var okHex1 = Shuffle(validHex, hexAndFLeft);
                    var okDig2 = Shuffle(validDig, digitsAndFRight);
                    var okHex2 = Shuffle(validHex, hexAndFRight);

                    var loNibbleValidLeft = Or(okDig1, okHex1);
                    var loNibbleValidRight = Or(okDig2, okHex2);

                    var validLeft = And(loNibbleValidLeft, hiNibbleValidLeft);
                    var validRight = And(loNibbleValidRight, hiNibbleValidRight);

                    loNibbleValidLeft = And(loNibbleValidLeft, x0F);
                    loNibbleValidRight = And(loNibbleValidRight, x0F);

                    var evenBytesLeft = Shuffle(loNibbleValidLeft, evenBytes);
                    var oddBytesLeft = Shuffle(loNibbleValidLeft, oddBytes);
                    var evenBytesRight = Shuffle(loNibbleValidRight, evenBytes);
                    var oddBytesRight = Shuffle(loNibbleValidRight, oddBytes);

                    evenBytesLeft = ShiftLeftLogical(evenBytesLeft.AsUInt16(), 4).AsByte();
                    evenBytesRight = ShiftLeftLogical(evenBytesRight.AsUInt16(), 4).AsByte();

                    evenBytesLeft = Or(evenBytesLeft, oddBytesLeft);
                    evenBytesRight = Or(evenBytesRight, oddBytesRight);

                    var result = UnpackLow(evenBytesLeft.AsInt64(), evenBytesRight.AsInt64()).AsByte();

                    var leftOk = MoveMask(validLeft);
                    var rightOk = MoveMask(validRight);

                    if (leftOk != 0xFFFF) goto LeftErr;
                    if (rightOk != 0xFFFF) goto RightErr;

                    Store(dest, result);
                    dest += 16;
                }

                srcBytes = src;
                destBytes = dest;
                return true;

            LeftErr:
                srcBytes = src - 32;
                destBytes = dest;
                return false;

            RightErr:
                srcBytes = src - 16;
                destBytes = dest;
                return false;
            }
        }
    }
}
#endif