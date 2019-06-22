#if INTRINSICS
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static System.Runtime.Intrinsics.X86.Avx;
using static System.Runtime.Intrinsics.X86.Avx2;
using static HexMate.Vector256Constants;
using static HexMate.VectorUtils;

namespace HexMate
{
    internal static partial class Utf8HexParser
    {
        internal static unsafe class Avx2
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal static bool TryParse(ref byte* srcBytes, ref byte* destBytes, int destLength)
            {
                Debug.Assert(System.Runtime.Intrinsics.X86.Avx2.IsSupported);

                var x0F = Vector256.Create((byte) 0x0F);
                var xF0 = Vector256.Create((byte) 0xF0);
                var digHexSelector = ReadVector<Vector256<byte>>(s_upperLowerDigHexSelector);
                var digits = ReadVector<Vector256<byte>>(s_digits);
                var hexs = ReadVector<Vector256<byte>>(s_hexs);
                var evenBytes = ReadVector<Vector256<byte>>(s_evenBytes);
                var oddBytes = ReadVector<Vector256<byte>>(s_oddBytes);
                var src = srcBytes;
                var dest = destBytes;

                var target = dest + FastMath.RoundDownTo32(destLength);
                while (dest != target)
                {
                    var inputLeft = LoadVector256(src);
                    src += 32;
                    var inputRight = LoadVector256(src);
                    src += 32;

                    var loNibbleLeft = And(inputLeft, x0F);
                    var loNibbleRight = And(inputRight, x0F);

                    var hiNibbleLeft = And(inputLeft, xF0);
                    var hiNibbleRight = And(inputRight, xF0);

                    var leftDigits = Shuffle(digits, loNibbleLeft);
                    var leftHex = Shuffle(hexs, loNibbleLeft);

                    var hiNibbleShLeft = ShiftRightLogical(hiNibbleLeft.AsInt16(), 4).AsByte();
                    var hiNibbleShRight = ShiftRightLogical(hiNibbleRight.AsInt16(), 4).AsByte();

                    var rightDigits = Shuffle(digits, loNibbleRight);
                    var rightHex = Shuffle(hexs, loNibbleRight);

                    var magicLeft = Shuffle(digHexSelector, hiNibbleShLeft);
                    var magicRight = Shuffle(digHexSelector, hiNibbleShRight);

                    var valueLeft = BlendVariable(leftDigits, leftHex, magicLeft);
                    var valueRight = BlendVariable(rightDigits, rightHex, magicRight);

                    var errLeft = ShiftLeftLogical(magicLeft.AsInt16(), 7).AsByte();
                    var errRight = ShiftLeftLogical(magicRight.AsInt16(), 7).AsByte();

                    var evenBytesLeft = Shuffle(valueLeft, evenBytes);
                    var oddBytesLeft = Shuffle(valueLeft, oddBytes);
                    var evenBytesRight = Shuffle(valueRight, evenBytes);
                    var oddBytesRight = Shuffle(valueRight, oddBytes);

                    evenBytesLeft = ShiftLeftLogical(evenBytesLeft.AsUInt16(), 4).AsByte();
                    evenBytesRight = ShiftLeftLogical(evenBytesRight.AsUInt16(), 4).AsByte();

                    evenBytesLeft = Or(evenBytesLeft, oddBytesLeft);
                    evenBytesRight = Or(evenBytesRight, oddBytesRight);

                    var result = Merge(evenBytesLeft, evenBytesRight);

                    var validationResultLeft = Or(errLeft, valueLeft);
                    var validationResultRight = Or(errRight, valueRight);

                    var leftOk = MoveMask(validationResultLeft);
                    var rightOk = MoveMask(validationResultRight);

                    if (leftOk != 0) goto LeftErr;
                    if (rightOk != 0) goto RightErr;

                    Store(dest, result);
                    dest += 32;
                }

                srcBytes = src;
                destBytes = dest;
                return true;

            LeftErr:
                srcBytes = src - 64;
                destBytes = dest;
                return false;

            RightErr:
                srcBytes = src - 32;
                destBytes = dest;
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static Vector256<byte> Merge(Vector256<byte> a, Vector256<byte> b)
            {
                var a1 = Permute4x64(a.AsUInt64(), 0b11_10_10_00);
                var b1 = Permute4x64(b.AsUInt64(), 0b11_00_01_00);
                return Blend(a1.AsUInt32(), b1.AsUInt32(), 0b1111_0000).AsByte();
            }
        }
    }
}
#endif