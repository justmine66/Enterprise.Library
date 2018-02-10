using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Enterprise.Library.Common.Encoders
{
    /// <summary>Contains utility APIs to assist with common encoding and decoding operations.
    /// </summary>
    public static class Base64UrlTextEncoder
    {
        /// <summary>Encodes supplied data into Base64 and replaces any URL encodable characters into non-URL encodable characters.
        /// </summary>
        /// <param name="data">The binary data to be encoded.</param>
        /// <returns>Base64 encoded string modified with non-URL encodable characters.</returns>
        public static string Encode(byte[] data)
        {
            return Base64UrlTextEncoderInternal.Base64UrlEncode(data);
        }
        /// <summary>Encodes supplied data into Base64 and replaces any URL encodable characters into non-URL encodable characters.
        /// </summary>
        /// <param name="plainText">The plain text to be encoded.</param>
        /// <param name="encoding">The encoding to be encoded.</param>
        /// <returns>Base64 encoded string modified with non-URL encodable characters.</returns>
        public static string Encode(string plainText, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return Base64UrlTextEncoderInternal.Base64UrlEncode(encoding.GetBytes(plainText));
        }
        /// <summary>Decodes supplied string by replacing non-URL encodable characters with URL encodable characters and then decodes the base64 string.
        /// </summary>
        /// <param name="base64UrlEncodedText">The string to be decoded.</param>
        /// <returns>The decoded binary data.</returns>
        public static byte[] Decode(string base64UrlEncodedText)
        {
            return Base64UrlTextEncoderInternal.Base64UrlDecode(base64UrlEncodedText);
        }
        /// <summary>Decodes supplied string by replacing non-URL encodable characters with URL encodable characters and then decodes the base64 string.
        /// </summary>
        /// <param name="base64UrlEncodedText">The string to be decoded.</param>
        /// <param name="encoding">The encoding to be decoded.</param>
        /// <returns>The decoded plain string.</returns>
        public static string Decode(string base64UrlEncodedText, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }

            return encoding.GetString(Base64UrlTextEncoderInternal.Base64UrlDecode(base64UrlEncodedText));
        }

        static class Base64UrlTextEncoderInternal
        {
            static readonly byte[] EmptyBytes = new byte[0];

            /// <summary>Encodes <paramref name="input"/> using base64url encoding.
            /// </summary>
            /// <param name="input">The binary input to encode.</param>
            /// <returns>The base64url-encoded form of <paramref name="input"/>.</returns>
            public static string Base64UrlEncode(byte[] input)
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input));
                }

                return Base64UrlEncode(input, offset: 0, count: input.Length);
            }
            /// <summary>Encodes <paramref name="input"/> using base64url encoding.
            /// </summary>
            /// <param name="input">The binary input to encode.</param>
            /// <param name="offset">The offset into <paramref name="input"/> at which to begin encoding.</param>
            /// <param name="count">The number of bytes from <paramref name="input"/> to encode.</param>
            /// <returns>The base64url-encoded form of <paramref name="input"/>.</returns>
            public static string Base64UrlEncode(byte[] input, int offset, int count)
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input));
                }

                ValidateParameters(input.Length, nameof(input), offset, count);

                // Special-case empty input
                if (count == 0)
                {
                    return string.Empty;
                }

                var buffer = new char[GetArraySizeRequiredToEncode(count)];
                int numBase64Chars = Base64UrlEncode(input, offset, buffer, outputOffset: 0, count: count);

                return new string(buffer, startIndex: 0, length: numBase64Chars);
            }
            /// <summary>Encodes <paramref name="input"/> using base64url encoding.
            /// </summary>
            /// <param name="input">The binary input to encode.</param>
            /// <param name="offset">The offset into <paramref name="input"/> at which to begin encoding.</param>
            /// <param name="output">Buffer to receive the base64url-encoded form of <paramref name="input"/>.Array must be large enough to hold <paramref name="outputOffset"/> charachters and the full base64-encoded <paramref name="input"/>, including padding characters.</param>
            /// <param name="outputOffset">The offset into <paramref name="output"/> at which to begin writing the base64url-encoded form of <paramref name="input"/>.</param>
            /// <param name="count">The number of bytes from <paramref name="input"/> to encode.</param>
            /// <returns>The number of characters written to <paramref name="output"/>, less any padding characters.</returns>
            public static int Base64UrlEncode(byte[] input, int offset, char[] output, int outputOffset, int count)
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input));
                }
                if (output == null)
                {
                    throw new ArgumentNullException(nameof(output));
                }

                ValidateParameters(input.Length, nameof(input), offset, count);
                if (outputOffset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(outputOffset));
                }

                int arraySizeRequired = GetArraySizeRequiredToEncode(count);
                if (output.Length - outputOffset < arraySizeRequired)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.Base64UrlTextEncoder_InvalidCountOffsetOrLength,
                            nameof(count),
                            nameof(outputOffset),
                            nameof(output)),
                        nameof(count));
                }

                //special-case empty input.
                if (count == 0)
                {
                    return 0;
                }

                // Use base64url encoding with no padding characters. See RFC 4648, Sec. 5.
                // Start with default Base64 encoding.
                int numBase64Chars = Convert.ToBase64CharArray(input, offset, count, output, outputOffset);
                //Fix up '+'=>'-' and '/'=>'_' and drop padding characters(eg: =).
                for (int i = outputOffset; i - outputOffset < numBase64Chars; i++)
                {
                    char ch = output[i];
                    switch (ch)
                    {
                        case '+':
                            output[i] = '-';
                            break;
                        case '/':
                            output[i] = '_';
                            break;
                        case '=':
                            return i - outputOffset;
                    }
                }

                return numBase64Chars;
            }
            /// <summary>Decodes a base64url-encoded string.
            /// </summary>
            /// <param name="input">The base64-encoded input to decode.</param>
            /// <returns>The base64-encoded form of input.</returns>
            /// <remarks>
            /// The input must not contains any whitespace or padding characters.
            /// Throw <see cref="FormatException"/> if input is malformed.
            /// </remarks>
            public static byte[] Base64UrlDecode(string input)
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input));
                }

                return Base64UrlDecode(input, offset: 0, count: input.Length);
            }
            /// <summary>
            /// Decodes a base64url-encoded substring of a given string.
            /// </summary>
            /// <param name="input">A string containing the base64url-encoded input to decode.</param>
            /// <param name="offset">The position in <paramref name="input"/> at which decoding should begin.</param>
            /// <param name="count">The number of characters in <paramref name="input"/> to decode.</param>
            /// <returns>The base64url-decoded form of the input.</returns>
            /// <remarks>
            /// The input must not contain any whitespace or padding characters.
            /// Throws <see cref="FormatException"/> if the input is malformed.
            /// </remarks>
            public static byte[] Base64UrlDecode(string input, int offset, int count)
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input));
                }

                ValidateParameters(input.Length, nameof(input), offset, count);

                // Special-case empty input
                if (count == 0)
                {
                    return EmptyBytes;
                }

                // Create array large enough for the Base64 characters, not just shorter Base64-URL-encoded form.
                var buffer = new char[GetArraySizeRequiredToDecode(count)];

                return Base64UrlDecode(input, offset, buffer, bufferOffset: 0, count: count);
            }
            /// <summary>
            /// Decodes a base64url-encoded <paramref name="input"/> into a <c>byte[]</c>.
            /// </summary>
            /// <param name="input">A string containing the base64url-encoded input to decode.</param>
            /// <param name="offset">The position in <paramref name="input"/> at which decoding should begin.</param>
            /// <param name="buffer">
            /// Scratch buffer to hold the <see cref="char"/>s to decode. Array must be large enough to hold
            /// <paramref name="bufferOffset"/> and <paramref name="count"/> characters as well as Base64 padding
            /// characters. Content is not preserved.
            /// </param>
            /// <param name="bufferOffset">
            /// The offset into <paramref name="buffer"/> at which to begin writing the <see cref="char"/>s to decode.
            /// </param>
            /// <param name="count">The number of characters in <paramref name="input"/> to decode.</param>
            /// <returns>The base64url-decoded form of the <paramref name="input"/>.</returns>
            /// <remarks>
            /// The input must not contain any whitespace or padding characters.
            /// Throws <see cref="FormatException"/> if the input is malformed.
            /// </remarks>
            public static byte[] Base64UrlDecode(string input, int offset, char[] buffer, int bufferOffset, int count)
            {
                if (input == null)
                {
                    throw new ArgumentNullException(nameof(input));
                }
                if (buffer == null)
                {
                    throw new ArgumentNullException(nameof(buffer));
                }

                ValidateParameters(input.Length, nameof(input), offset, count);
                if (bufferOffset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(bufferOffset));
                }

                if (count == 0)
                {
                    return EmptyBytes;
                }

                // Assumption: input is base64url encoded without padding and contains no whitespace.
                int numPaddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(count);
                int arraySizeRequired = checked(count + numPaddingCharsToAdd);
                Debug.Assert(arraySizeRequired % 4 == 0, "Invariant: Array length must be a multiple of 4.");

                if (buffer.Length - bufferOffset < arraySizeRequired)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.Base64UrlTextEncoder_InvalidCountOffsetOrLength,
                            nameof(count),
                            nameof(bufferOffset),
                            nameof(input)),
                        nameof(count));
                }

                // Copy input into buffer, fixing up '-' -> '+' and '_' -> '/'.
                var i = bufferOffset;
                for (var j = offset; i - bufferOffset < count; i++, j++)
                {
                    var ch = input[j];
                    if (ch == '-')
                    {
                        buffer[i] = '+';
                    }
                    else if (ch == '_')
                    {
                        buffer[i] = '/';
                    }
                    else
                    {
                        buffer[i] = ch;
                    }
                }

                // Add the padding characters back.
                for (; numPaddingCharsToAdd > 0; i++, numPaddingCharsToAdd--)
                {
                    buffer[i] = '=';
                }

                // Decode.
                // If the caller provided invalid base64 chars, they'll be caught here.
                return Convert.FromBase64CharArray(buffer, bufferOffset, arraySizeRequired);
            }
            /// <summary>Get the minimum of output <c>char[]</c> size requied for encoding <paramref name="count"/> <see cref="byte"/>s with the <see cref="Base64UrlEncode(byte[], int, char[], int, int)"/> method.
            /// </summary>
            /// <param name="count">The number of characters to encode.</param>
            /// <returns>The minimum of output <c>char[]</c> size requiring for encoding <paramref name="count"/> <see cref="byte"/>s.</returns>
            public static int GetArraySizeRequiredToEncode(int count)
            {
                var numWholeOrPartialInputBlocks = checked(count + 2) / 3;
                return checked(numWholeOrPartialInputBlocks * 4);
            }
            /// <summary>
            /// Gets the minimum <c>char[]</c> size required for decoding of <paramref name="count"/> characters
            /// with the <see cref="Base64UrlDecode(string, int, char[], int, int)"/> method.
            /// </summary>
            /// <param name="count">The number of characters to decode.</param>
            /// <returns>
            /// The minimum <c>char[]</c> size required for decoding  of <paramref name="count"/> characters.
            /// </returns>
            public static int GetArraySizeRequiredToDecode(int count)
            {
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                if (count == 0)
                {
                    return 0;
                }

                var numPaddingCharsToAdd = GetNumBase64PaddingCharsToAddForDecode(count);

                return checked(count + numPaddingCharsToAdd);
            }
            static void ValidateParameters(int bufferLength, string inputName, int offset, int count)
            {
                if (offset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset));
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                if (bufferLength - offset < count)
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.Base64UrlTextEncoder_InvalidCountOffsetOrLength,
                            nameof(count),
                            nameof(offset),
                            inputName), nameof(count));
                }
            }
            static int GetNumBase64PaddingCharsToAddForDecode(int inputLength)
            {
                switch (inputLength % 4)
                {
                    case 0:
                        return 0;
                    case 2:
                        return 2;
                    case 3:
                        return 1;
                    default:
                        throw new FormatException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Base64UrlTextEncoder_MalformedInput,
                                inputLength));
                }
            }
        }
    }
}
