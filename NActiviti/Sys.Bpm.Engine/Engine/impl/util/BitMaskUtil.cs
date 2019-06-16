/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Engine.Impl.Util
{
    /// <summary>
    /// Util class for manipulating bit-flag in ints.
    /// 
    /// Currently, only 8-bits are supported, but can be extended to use all 31 bits in the integer (1st of 32 bits is used for sign).
    /// 
    /// 
    /// </summary>
    public class BitMaskUtil
    {

        // First 8 masks as constant to prevent having to math.pow() every time a
        // bit needs flipping.

        private const int FLAG_BIT_1 = 1; // 000...00000001
        private const int FLAG_BIT_2 = 2; // 000...00000010
        private const int FLAG_BIT_3 = 4; // 000...00000100
        private const int FLAG_BIT_4 = 8; // 000...00001000
        private const int FLAG_BIT_5 = 16; // 000...00010000
        private const int FLAG_BIT_6 = 32; // 000...00100000
        private const int FLAG_BIT_7 = 64; // 000...01000000
        private const int FLAG_BIT_8 = 128; // 000...10000000

        private static readonly int[] MASKS = new int[] { FLAG_BIT_1, FLAG_BIT_2, FLAG_BIT_3, FLAG_BIT_4, FLAG_BIT_5, FLAG_BIT_6, FLAG_BIT_7, FLAG_BIT_8 };

        /// <summary>
        /// Set bit to '1' in the given int.
        /// </summary>
        /// <param name="current">
        ///          integer value </param>
        /// <param name="bitNumber">
        ///          number of the bit to set to '1' (right first bit starting at 1). </param>
        public static int SetBitOn(int value, int bitNumber)
        {
            if (bitNumber <= 0 || bitNumber > 8)
            {
                throw new System.ArgumentException("Only bits 1 through 8 are supported");
            }

            // To turn on, OR with the correct mask
            return value | MASKS[bitNumber - 1];
        }

        /// <summary>
        /// Set bit to '0' in the given int.
        /// </summary>
        /// <param name="current">
        ///          integer value </param>
        /// <param name="bitNumber">
        ///          number of the bit to set to '0' (right first bit starting at 1). </param>
        public static int SetBitOff(int value, int bitNumber)
        {
            if (bitNumber <= 0 || bitNumber > 8)
            {
                throw new System.ArgumentException("Only bits 1 through 8 are supported");
            }

            // To turn on, OR with the correct mask
            return value & ~MASKS[bitNumber - 1];
        }

        /// <summary>
        /// Check if the bit is set to '1'
        /// </summary>
        /// <param name="value">
        ///          integer to check bit </param>
        /// <param name="number">
        ///          of bit to check (right first bit starting at 1) </param>
        public static bool IsBitOn(int value, int bitNumber)
        {
            if (bitNumber <= 0 || bitNumber > 8)
            {
                throw new System.ArgumentException("Only bits 1 through 8 are supported");
            }

            return ((value & MASKS[bitNumber - 1]) == MASKS[bitNumber - 1]);
        }

        /// <summary>
        /// Set bit to '0' or '1' in the given int.
        /// </summary>
        /// <param name="current">
        ///          integer value </param>
        /// <param name="bitNumber">
        ///          number of the bit to set to '0' or '1' (right first bit starting at 1). </param>
        /// <param name="bitValue">
        ///          if true, bit set to '1'. If false, '0'. </param>
        public static int SetBit(int value, int bitNumber, bool bitValue)
        {
            if (bitValue)
            {
                return SetBitOn(value, bitNumber);
            }
            else
            {
                return SetBitOff(value, bitNumber);
            }
        }
    }

}