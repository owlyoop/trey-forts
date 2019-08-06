using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Text;

namespace KinematicCharacterController.Walkthrough.NetworkingExample
{
    public class SerializationBuffer
    {
        public byte[] InternalBuffer { get; set; }
        public int IndexPosition { get; set; }

        private float _growFactor;
        private int _initialSize;

        private static UIntFloat FloatConverter;
        private static Encoding Encoding;
        private static byte[] StringReadBuffer;
        private static byte[] StringWriteBuffer;
        private const int MaxStringLength = 1024 * 32;
        private const int InitialStringBufferSize = 1024;

        public SerializationBuffer(int initialSize, float growFactor)
        {
            InternalBuffer = new byte[initialSize];
            _growFactor = growFactor;
            _initialSize = initialSize;

            if (Encoding == null)
            {
                Encoding = new UTF8Encoding();
                StringWriteBuffer = new byte[MaxStringLength];
                StringReadBuffer = new byte[InitialStringBufferSize];
            }
        }

        public void Reset()
        {
            InternalBuffer = new byte[_initialSize];
        }

        public void SeekZero()
        {
            IndexPosition = 0;
        }

        private void CheckForSpace(ushort count)
        {
            if (IndexPosition + count < InternalBuffer.Length)
            {
                return;
            }

            int newSize = (int)Math.Ceiling(InternalBuffer.Length * _growFactor);
            while (IndexPosition + count >= newSize)
            {
                newSize = (int)Math.Ceiling(newSize * _growFactor);
            }

            byte[] tmp = new byte[newSize];
            InternalBuffer.CopyTo(tmp, 0);
            InternalBuffer = tmp;
        }

        #region Write
        public void WriteByte(byte value)
        {
            CheckForSpace(1);
            InternalBuffer[IndexPosition] = value;
            IndexPosition += 1;
        }

        public void WriteByte2(byte value0, byte value1)
        {
            CheckForSpace(2);
            InternalBuffer[IndexPosition] = value0;
            InternalBuffer[IndexPosition + 1] = value1;
            IndexPosition += 2;
        }

        public void WriteByte4(byte value0, byte value1, byte value2, byte value3)
        {
            CheckForSpace(4);
            InternalBuffer[IndexPosition] = value0;
            InternalBuffer[IndexPosition + 1] = value1;
            InternalBuffer[IndexPosition + 2] = value2;
            InternalBuffer[IndexPosition + 3] = value3;
            IndexPosition += 4;
        }

        public void WriteByte8(byte value0, byte value1, byte value2, byte value3, byte value4, byte value5, byte value6, byte value7)
        {
            CheckForSpace(8);
            InternalBuffer[IndexPosition] = value0;
            InternalBuffer[IndexPosition + 1] = value1;
            InternalBuffer[IndexPosition + 2] = value2;
            InternalBuffer[IndexPosition + 3] = value3;
            InternalBuffer[IndexPosition + 4] = value4;
            InternalBuffer[IndexPosition + 5] = value5;
            InternalBuffer[IndexPosition + 6] = value6;
            InternalBuffer[IndexPosition + 7] = value7;
            IndexPosition += 8;
        }

        public void WriteBytes(byte[] buffer, ushort count)
        {
            CheckForSpace(count);

            if (count == buffer.Length)
            {
                buffer.CopyTo(InternalBuffer, (int)IndexPosition);
            }
            else
            {
                //CopyTo doesnt take a count :(
                for (int i = 0; i < count; i++)
                {
                    InternalBuffer[IndexPosition + i] = buffer[i];
                }
            }
            IndexPosition += count;
        }

        public void WriteBytesAndSize(byte[] buffer, int count)
        {
            if (buffer == null || count == 0)
            {
                WriteUShort(0);
                return;
            }

            if (count > ushort.MaxValue)
            {
                Debug.LogError("NetworkWriter WriteBytesAndSize: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
            }

            WriteUShort((ushort)count);
            WriteBytes(buffer, (ushort)count);
        }

        public void WriteByte(bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7, bool b8)
        {
            byte bitmask = 0;

            if (b1)
            {
                bitmask |= 1;
            }
            if (b2)
            {
                bitmask |= 2;
            }
            if (b3)
            {
                bitmask |= 4;
            }
            if (b4)
            {
                bitmask |= 8;
            }
            if (b5)
            {
                bitmask |= 16;
            }
            if (b6)
            {
                bitmask |= 32;
            }
            if (b7)
            {
                bitmask |= 64;
            }
            if (b8)
            {
                bitmask |= 128;
            }

            WriteByte(bitmask);
        }

        public void WriteByte(bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7)
        {
            byte bitmask = 0;

            if (b1)
            {
                bitmask |= 1;
            }
            if (b2)
            {
                bitmask |= 2;
            }
            if (b3)
            {
                bitmask |= 4;
            }
            if (b4)
            {
                bitmask |= 8;
            }
            if (b5)
            {
                bitmask |= 16;
            }
            if (b6)
            {
                bitmask |= 32;
            }
            if (b7)
            {
                bitmask |= 64;
            }

            WriteByte(bitmask);
        }

        public void WriteByte(bool b1, bool b2, bool b3, bool b4, bool b5, bool b6)
        {
            byte bitmask = 0;

            if (b1)
            {
                bitmask |= 1;
            }
            if (b2)
            {
                bitmask |= 2;
            }
            if (b3)
            {
                bitmask |= 4;
            }
            if (b4)
            {
                bitmask |= 8;
            }
            if (b5)
            {
                bitmask |= 16;
            }
            if (b6)
            {
                bitmask |= 32;
            }

            WriteByte(bitmask);
        }

        public void WriteByte(bool b1, bool b2, bool b3, bool b4, bool b5)
        {
            byte bitmask = 0;

            if (b1)
            {
                bitmask |= 1;
            }
            if (b2)
            {
                bitmask |= 2;
            }
            if (b3)
            {
                bitmask |= 4;
            }
            if (b4)
            {
                bitmask |= 8;
            }
            if (b5)
            {
                bitmask |= 16;
            }

            WriteByte(bitmask);
        }

        public void WriteByte(bool b1, bool b2, bool b3, bool b4)
        {
            byte bitmask = 0;

            if (b1)
            {
                bitmask |= 1;
            }
            if (b2)
            {
                bitmask |= 2;
            }
            if (b3)
            {
                bitmask |= 4;
            }
            if (b4)
            {
                bitmask |= 8;
            }

            WriteByte(bitmask);
        }

        public void WriteByte(bool b1, bool b2, bool b3)
        {
            byte bitmask = 0;

            if (b1)
            {
                bitmask |= 1;
            }
            if (b2)
            {
                bitmask |= 2;
            }
            if (b3)
            {
                bitmask |= 4;
            }

            WriteByte(bitmask);
        }

        public void WriteByte(bool b1, bool b2)
        {
            byte bitmask = 0;

            if (b1)
            {
                bitmask |= 1;
            }
            if (b2)
            {
                bitmask |= 2;
            }

            WriteByte(bitmask);
        }

        public void WritePackedUInt(uint value)
        {
            if (value <= 240)
            {
                WriteByte((byte)value);
                return;
            }
            if (value <= 2287)
            {
                WriteByte((byte)((value - 240) / 256 + 241));
                WriteByte((byte)((value - 240) % 256));
                return;
            }
            if (value <= 67823)
            {
                WriteByte((byte)249);
                WriteByte((byte)((value - 2288) / 256));
                WriteByte((byte)((value - 2288) % 256));
                return;
            }
            if (value <= 16777215)
            {
                WriteByte((byte)250);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                return;
            }

            // all other values of uint
            WriteByte((byte)251);
            WriteByte((byte)(value & 0xFF));
            WriteByte((byte)((value >> 8) & 0xFF));
            WriteByte((byte)((value >> 16) & 0xFF));
            WriteByte((byte)((value >> 24) & 0xFF));
        }

        public void WritePackedULong(ulong value)
        {
            if (value <= 240)
            {
                WriteByte((byte)value);
                return;
            }
            if (value <= 2287)
            {
                WriteByte((byte)((value - 240) / 256 + 241));
                WriteByte((byte)((value - 240) % 256));
                return;
            }
            if (value <= 67823)
            {
                WriteByte((byte)249);
                WriteByte((byte)((value - 2288) / 256));
                WriteByte((byte)((value - 2288) % 256));
                return;
            }
            if (value <= 16777215)
            {
                WriteByte((byte)250);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                return;
            }
            if (value <= 4294967295)
            {
                WriteByte((byte)251);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                return;
            }
            if (value <= 1099511627775)
            {
                WriteByte((byte)252);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                WriteByte((byte)((value >> 32) & 0xFF));
                return;
            }
            if (value <= 281474976710655)
            {
                WriteByte((byte)253);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                WriteByte((byte)((value >> 32) & 0xFF));
                WriteByte((byte)((value >> 40) & 0xFF));
                return;
            }
            if (value <= 72057594037927935)
            {
                WriteByte((byte)254);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                WriteByte((byte)((value >> 32) & 0xFF));
                WriteByte((byte)((value >> 40) & 0xFF));
                WriteByte((byte)((value >> 48) & 0xFF));
                return;
            }

            // all others
            {
                WriteByte((byte)255);
                WriteByte((byte)(value & 0xFF));
                WriteByte((byte)((value >> 8) & 0xFF));
                WriteByte((byte)((value >> 16) & 0xFF));
                WriteByte((byte)((value >> 24) & 0xFF));
                WriteByte((byte)((value >> 32) & 0xFF));
                WriteByte((byte)((value >> 40) & 0xFF));
                WriteByte((byte)((value >> 48) & 0xFF));
                WriteByte((byte)((value >> 56) & 0xFF));
            }
        }

        public void WriteChar(char value)
        {
            WriteByte((byte)value);
        }

        public void WriteSByte(sbyte value)
        {
            WriteByte((byte)value);
        }

        public void WriteShort(short value)
        {
            WriteByte2((byte)(value & 0xff), (byte)((value >> 8) & 0xff));
        }

        public void WriteUShort(ushort value)
        {
            WriteByte2((byte)(value & 0xff), (byte)((value >> 8) & 0xff));
        }

        public void WriteInt(int value)
        {
            // little endian...
            WriteByte4(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff));
        }

        public void WriteUInt(uint value)
        {
            WriteByte4(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff));
        }

        public void WriteLong(long value)
        {
            WriteByte8(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 32) & 0xff),
                (byte)((value >> 40) & 0xff),
                (byte)((value >> 48) & 0xff),
                (byte)((value >> 56) & 0xff));
        }

        public void WriteULong(ulong value)
        {
            WriteByte8(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 32) & 0xff),
                (byte)((value >> 40) & 0xff),
                (byte)((value >> 48) & 0xff),
                (byte)((value >> 56) & 0xff));
        }

        public unsafe void WriteHalf(float value)
        {
            uint valueAsUInt = *((uint*)&value);
            WriteUShort((ushort)(baseTable[(valueAsUInt >> 23) & 0x1ff] + ((valueAsUInt & 0x007fffff) >> shiftTable[valueAsUInt >> 23])));
        }

        public void WriteFloat(float value)
        {
            FloatConverter.floatValue = value;
            WriteUInt(FloatConverter.intValue);
        }

        public void WriteDouble(double value)
        {
            FloatConverter.doubleValue = value;
            WriteULong(FloatConverter.longValue);
        }

        public void WriteDecimal(decimal value)
        {
            Int32[] bits = decimal.GetBits(value);
            WriteInt(bits[0]);
            WriteInt(bits[1]);
            WriteInt(bits[2]);
            WriteInt(bits[3]);
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteByte2(0, 0);
                return;
            }

            int len = Encoding.GetByteCount(value);

            if (len >= MaxStringLength)
            {
                throw new IndexOutOfRangeException("Serialize(string) too long: " + value.Length);
            }

            WriteUShort((ushort)(len));
            int numBytes = Encoding.GetBytes(value, 0, value.Length, StringWriteBuffer, 0);
            WriteBytes(StringWriteBuffer, (ushort)numBytes);
        }

        public void WriteBool(bool value)
        {
            if (value)
                WriteByte(1);
            else
                WriteByte(0);
        }

        public void WriteVector2(Vector2 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
        }

        public void WriteHalfVector2(Vector2 value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);
        }

        public void WriteVector3(Vector3 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
        }

        public void WriteHalfVector3(Vector3 value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);
            WriteHalf(value.z);
        }

        public void WriteVector4(Vector4 value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }

        public void WriteHalfVector4(Vector4 value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);
            WriteHalf(value.z);
            WriteHalf(value.w);
        }

        public void WriteColor(Color value)
        {
            WriteFloat(value.r);
            WriteFloat(value.g);
            WriteFloat(value.b);
            WriteFloat(value.a);
        }

        public void WriteHalfColor(Color value)
        {
            WriteHalf(value.r);
            WriteHalf(value.g);
            WriteHalf(value.b);
            WriteHalf(value.a);
        }

        public void WriteColor32(Color32 value)
        {
            WriteFloat(value.r);
            WriteFloat(value.g);
            WriteFloat(value.b);
            WriteFloat(value.a);
        }

        public void WriteQuaternion(Quaternion value)
        {
            WriteFloat(value.x);
            WriteFloat(value.y);
            WriteFloat(value.z);
            WriteFloat(value.w);
        }

        public void WriteHalfQuaternion(Quaternion value)
        {
            WriteHalf(value.x);
            WriteHalf(value.y);
            WriteHalf(value.z);
            WriteHalf(value.w);
        }

        public void WriteRect(Rect value)
        {
            WriteFloat(value.xMin);
            WriteFloat(value.yMin);
            WriteFloat(value.width);
            WriteFloat(value.height);
        }

        public void WritePlane(Plane value)
        {
            WriteVector3(value.normal);
            WriteFloat(value.distance);
        }

        public void WriteRay(Ray value)
        {
            WriteVector3(value.direction);
            WriteVector3(value.origin);
        }

        public void WriteMatrix4x4(Matrix4x4 value)
        {
            WriteFloat(value.m00);
            WriteFloat(value.m01);
            WriteFloat(value.m02);
            WriteFloat(value.m03);
            WriteFloat(value.m10);
            WriteFloat(value.m11);
            WriteFloat(value.m12);
            WriteFloat(value.m13);
            WriteFloat(value.m20);
            WriteFloat(value.m21);
            WriteFloat(value.m22);
            WriteFloat(value.m23);
            WriteFloat(value.m30);
            WriteFloat(value.m31);
            WriteFloat(value.m32);
            WriteFloat(value.m33);
        }

        public void WriteHalfMatrix4x4(Matrix4x4 value)
        {
            WriteHalf(value.m00);
            WriteHalf(value.m01);
            WriteHalf(value.m02);
            WriteHalf(value.m03);
            WriteHalf(value.m10);
            WriteHalf(value.m11);
            WriteHalf(value.m12);
            WriteHalf(value.m13);
            WriteHalf(value.m20);
            WriteHalf(value.m21);
            WriteHalf(value.m22);
            WriteHalf(value.m23);
            WriteHalf(value.m30);
            WriteHalf(value.m31);
            WriteHalf(value.m32);
            WriteHalf(value.m33);
        }
        #endregion

        #region Read
        public byte ReadByte()
        {
            return InternalBuffer[IndexPosition++];
        }

        public void ReadBytes(byte[] buffer, int count)
        {
            for (ushort i = 0; i < count; i++)
            {
                buffer[i] = InternalBuffer[IndexPosition + i];
            }
            IndexPosition += count;
        }

        public UInt32 ReadPackedUInt32()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }
            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return (UInt32)(240 + 256 * (a0 - 241) + a1);
            }
            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return (UInt32)(2288 + 256 * a1 + a2);
            }
            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16);
            }
            byte a4 = ReadByte();
            if (a0 >= 251)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16) + (((UInt32)a4) << 24);
            }
            throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + a0);
        }

        public UInt64 ReadPackedUInt64()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }
            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + 256 * (a0 - ((UInt64)241)) + a1;
            }
            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return 2288 + (((UInt64)256) * a1) + a2;
            }
            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16);
            }
            byte a4 = ReadByte();
            if (a0 == 251)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24);
            }
            byte a5 = ReadByte();
            if (a0 == 252)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32);
            }
            byte a6 = ReadByte();
            if (a0 == 253)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40);
            }
            byte a7 = ReadByte();
            if (a0 == 254)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48);
            }
            byte a8 = ReadByte();
            if (a0 == 255)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48) + (((UInt64)a8) << 56);
            }
            throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + a0);
        }

        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        public void ReadByte(out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7, out bool b8)
        {
            byte bitmask = ReadByte();

            if ((bitmask & 1) != 0)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }
            if ((bitmask & 2) != 0)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }
            if ((bitmask & 4) != 0)
            {
                b3 = true;
            }
            else
            {
                b3 = false;
            }
            if ((bitmask & 8) != 0)
            {
                b4 = true;
            }
            else
            {
                b4 = false;
            }
            if ((bitmask & 16) != 0)
            {
                b5 = true;
            }
            else
            {
                b5 = false;
            }
            if ((bitmask & 32) != 0)
            {
                b6 = true;
            }
            else
            {
                b6 = false;
            }
            if ((bitmask & 64) != 0)
            {
                b7 = true;
            }
            else
            {
                b7 = false;
            }
            if ((bitmask & 128) != 0)
            {
                b8 = true;
            }
            else
            {
                b8 = false;
            }
        }

        public void ReadByte(out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6, out bool b7)
        {
            byte bitmask = ReadByte();

            if ((bitmask & 1) != 0)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }
            if ((bitmask & 2) != 0)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }
            if ((bitmask & 4) != 0)
            {
                b3 = true;
            }
            else
            {
                b3 = false;
            }
            if ((bitmask & 8) != 0)
            {
                b4 = true;
            }
            else
            {
                b4 = false;
            }
            if ((bitmask & 16) != 0)
            {
                b5 = true;
            }
            else
            {
                b5 = false;
            }
            if ((bitmask & 32) != 0)
            {
                b6 = true;
            }
            else
            {
                b6 = false;
            }
            if ((bitmask & 64) != 0)
            {
                b7 = true;
            }
            else
            {
                b7 = false;
            }
        }

        public void ReadByte(out bool b1, out bool b2, out bool b3, out bool b4, out bool b5, out bool b6)
        {
            byte bitmask = ReadByte();

            if ((bitmask & 1) != 0)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }
            if ((bitmask & 2) != 0)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }
            if ((bitmask & 4) != 0)
            {
                b3 = true;
            }
            else
            {
                b3 = false;
            }
            if ((bitmask & 8) != 0)
            {
                b4 = true;
            }
            else
            {
                b4 = false;
            }
            if ((bitmask & 16) != 0)
            {
                b5 = true;
            }
            else
            {
                b5 = false;
            }
            if ((bitmask & 32) != 0)
            {
                b6 = true;
            }
            else
            {
                b6 = false;
            }
        }

        public void ReadByte(out bool b1, out bool b2, out bool b3, out bool b4, out bool b5)
        {
            byte bitmask = ReadByte();

            if ((bitmask & 1) != 0)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }
            if ((bitmask & 2) != 0)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }
            if ((bitmask & 4) != 0)
            {
                b3 = true;
            }
            else
            {
                b3 = false;
            }
            if ((bitmask & 8) != 0)
            {
                b4 = true;
            }
            else
            {
                b4 = false;
            }
            if ((bitmask & 16) != 0)
            {
                b5 = true;
            }
            else
            {
                b5 = false;
            }
        }

        public void ReadByte(out bool b1, out bool b2, out bool b3, out bool b4)
        {
            byte bitmask = ReadByte();

            if ((bitmask & 1) != 0)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }
            if ((bitmask & 2) != 0)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }
            if ((bitmask & 4) != 0)
            {
                b3 = true;
            }
            else
            {
                b3 = false;
            }
            if ((bitmask & 8) != 0)
            {
                b4 = true;
            }
            else
            {
                b4 = false;
            }
        }

        public void ReadByte(out bool b1, out bool b2, out bool b3)
        {
            byte bitmask = ReadByte();

            if ((bitmask & 1) != 0)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }
            if ((bitmask & 2) != 0)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }
            if ((bitmask & 4) != 0)
            {
                b3 = true;
            }
            else
            {
                b3 = false;
            }
        }

        public void ReadByte(out bool b1, out bool b2)
        {
            byte bitmask = ReadByte();

            if ((bitmask & 1) != 0)
            {
                b1 = true;
            }
            else
            {
                b1 = false;
            }
            if ((bitmask & 2) != 0)
            {
                b2 = true;
            }
            else
            {
                b2 = false;
            }
        }

        public short ReadShort()
        {
            ushort value = 0;
            value |= ReadByte();
            value |= (ushort)(ReadByte() << 8);
            return (short)value;
        }

        public ushort ReadUShort()
        {
            ushort value = 0;
            value |= ReadByte();
            value |= (ushort)(ReadByte() << 8);
            return value;
        }

        public int ReadInt()
        {
            uint value = 0;
            value |= ReadByte();
            value |= (uint)(ReadByte() << 8);
            value |= (uint)(ReadByte() << 16);
            value |= (uint)(ReadByte() << 24);
            return (int)value;
        }

        public uint ReadUInt()
        {
            uint value = 0;
            value |= ReadByte();
            value |= (uint)(ReadByte() << 8);
            value |= (uint)(ReadByte() << 16);
            value |= (uint)(ReadByte() << 24);
            return value;
        }

        public long ReadLong()
        {
            ulong value = 0;

            ulong other = ReadByte();
            value |= other;

            other = ((ulong)ReadByte()) << 8;
            value |= other;

            other = ((ulong)ReadByte()) << 16;
            value |= other;

            other = ((ulong)ReadByte()) << 24;
            value |= other;

            other = ((ulong)ReadByte()) << 32;
            value |= other;

            other = ((ulong)ReadByte()) << 40;
            value |= other;

            other = ((ulong)ReadByte()) << 48;
            value |= other;

            other = ((ulong)ReadByte()) << 56;
            value |= other;

            return (long)value;
        }

        public ulong ReadULong()
        {
            ulong value = 0;
            ulong other = ReadByte();
            value |= other;

            other = ((ulong)ReadByte()) << 8;
            value |= other;

            other = ((ulong)ReadByte()) << 16;
            value |= other;

            other = ((ulong)ReadByte()) << 24;
            value |= other;

            other = ((ulong)ReadByte()) << 32;
            value |= other;

            other = ((ulong)ReadByte()) << 40;
            value |= other;

            other = ((ulong)ReadByte()) << 48;
            value |= other;

            other = ((ulong)ReadByte()) << 56;
            value |= other;
            return value;
        }

        public decimal ReadDecimal()
        {
            Int32[] bits = new Int32[4];

            bits[0] = ReadInt();
            bits[1] = ReadInt();
            bits[2] = ReadInt();
            bits[3] = ReadInt();

            return new decimal(bits);
        }

        public unsafe float ReadHalf()
        {
            ushort valueAsUShort = ReadUShort();
            uint result = mantissaTable[offsetTable[valueAsUShort >> 10] + (valueAsUShort & 0x3ff)] + exponentTable[valueAsUShort >> 10];
            return *((float*)&result);
        }

        public float ReadFloat()
        {
            uint value = ReadUInt();
            return FloatConversion.ToSingle(value);
        }

        public double ReadDouble()
        {
            ulong value = ReadULong();
            return FloatConversion.ToDouble(value);
        }

        public string ReadString()
        {
            UInt16 numBytes = ReadUShort();
            if (numBytes == 0)
                return "";

            if (numBytes >= MaxStringLength)
            {
                throw new IndexOutOfRangeException("ReadString() too long: " + numBytes);
            }

            while (numBytes > StringReadBuffer.Length)
            {
                StringReadBuffer = new byte[StringReadBuffer.Length * 2];
            }

            ReadBytes(StringReadBuffer, numBytes);

            char[] chars = Encoding.GetChars(StringReadBuffer, 0, numBytes);
            return new string(chars);
        }

        public char ReadChar()
        {
            return (char)ReadByte();
        }

        public bool ReadBool()
        {
            int value = ReadByte();
            return value == 1;
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new IndexOutOfRangeException("NetworkReader ReadBytes " + count);
            }
            byte[] value = new byte[count];
            ReadBytes(value, count);
            return value;
        }

        public byte[] ReadBytesAndSize()
        {
            ushort sz = ReadUShort();
            if (sz == 0)
                return new byte[0];

            return ReadBytes(sz);
        }

        public Vector2 ReadVector2()
        {
            return new Vector2(ReadFloat(), ReadFloat());
        }

        public Vector2 ReadHalfVector2()
        {
            return new Vector2(ReadHalf(), ReadHalf());
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Vector3 ReadHalfVector3()
        {
            return new Vector3(ReadHalf(), ReadHalf(), ReadHalf());
        }

        public Vector4 ReadVector4()
        {
            return new Vector4(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Vector4 ReadHalfVector4()
        {
            return new Vector4(ReadHalf(), ReadHalf(), ReadHalf(), ReadHalf());
        }

        public Color ReadColor()
        {
            return new Color(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Color ReadHalfColor()
        {
            return new Color(ReadHalf(), ReadHalf(), ReadHalf(), ReadHalf());
        }

        public Color32 ReadColor32()
        {
            return new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());
        }

        public Quaternion ReadQuaternion()
        {
            return new Quaternion(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Quaternion ReadHalfQuaternion()
        {
            return new Quaternion(ReadHalf(), ReadHalf(), ReadHalf(), ReadHalf());
        }

        public Rect ReadRect()
        {
            return new Rect(ReadFloat(), ReadFloat(), ReadFloat(), ReadFloat());
        }

        public Plane ReadPlane()
        {
            return new Plane(ReadVector3(), ReadFloat());
        }

        public Ray ReadRay()
        {
            return new Ray(ReadVector3(), ReadVector3());
        }

        public Matrix4x4 ReadMatrix4x4()
        {
            Matrix4x4 m = new Matrix4x4();
            m.m00 = ReadFloat();
            m.m01 = ReadFloat();
            m.m02 = ReadFloat();
            m.m03 = ReadFloat();
            m.m10 = ReadFloat();
            m.m11 = ReadFloat();
            m.m12 = ReadFloat();
            m.m13 = ReadFloat();
            m.m20 = ReadFloat();
            m.m21 = ReadFloat();
            m.m22 = ReadFloat();
            m.m23 = ReadFloat();
            m.m30 = ReadFloat();
            m.m31 = ReadFloat();
            m.m32 = ReadFloat();
            m.m33 = ReadFloat();
            return m;
        }

        public Matrix4x4 ReadHalfMatrix4x4()
        {
            Matrix4x4 m = new Matrix4x4();
            m.m00 = ReadHalf();
            m.m01 = ReadHalf();
            m.m02 = ReadHalf();
            m.m03 = ReadHalf();
            m.m10 = ReadHalf();
            m.m11 = ReadHalf();
            m.m12 = ReadHalf();
            m.m13 = ReadHalf();
            m.m20 = ReadHalf();
            m.m21 = ReadHalf();
            m.m22 = ReadHalf();
            m.m23 = ReadHalf();
            m.m30 = ReadHalf();
            m.m31 = ReadHalf();
            m.m32 = ReadHalf();
            m.m33 = ReadHalf();
            return m;
        }
        #endregion

        #region Conversion Helpers
        [StructLayout(LayoutKind.Explicit)]
        internal struct UIntFloat
        {
            [FieldOffset(0)]
            public float floatValue;

            [FieldOffset(0)]
            public uint intValue;

            [FieldOffset(0)]
            public double doubleValue;

            [FieldOffset(0)]
            public ulong longValue;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct UIntDecimal
        {
            [FieldOffset(0)]
            public ulong longValue1;

            [FieldOffset(8)]
            public ulong longValue2;

            [FieldOffset(0)]
            public decimal decimalValue;
        }

        internal class FloatConversion
        {
            public static float ToSingle(uint value)
            {
                UIntFloat uf = new UIntFloat();
                uf.intValue = value;
                return uf.floatValue;
            }

            public static double ToDouble(ulong value)
            {
                UIntFloat uf = new UIntFloat();
                uf.longValue = value;
                return uf.doubleValue;
            }

            public static decimal ToDecimal(ulong value1, ulong value2)
            {
                UIntDecimal uf = new UIntDecimal();
                uf.longValue1 = value1;
                uf.longValue2 = value2;
                return uf.decimalValue;
            }
        }

        private static uint[] mantissaTable = GenerateMantissaTable();
        private static uint[] exponentTable = GenerateExponentTable();
        private static ushort[] offsetTable = GenerateOffsetTable();
        private static ushort[] baseTable = GenerateBaseTable();
        private static sbyte[] shiftTable = GenerateShiftTable(); // Transforms the subnormal representation to a normalized one. 

        private static uint ConvertMantissa(int i)
        {
            uint m = (uint)(i << 13); // Zero pad mantissa bits
            uint e = 0; // Zero exponent

            // While not normalized
            while ((m & 0x00800000) == 0)
            {
                e -= 0x00800000; // Decrement exponent (1<<23)
                m <<= 1; // Shift mantissa                
            }
            m &= unchecked((uint)~0x00800000); // Clear leading 1 bit
            e += 0x38800000; // Adjust bias ((127-14)<<23)
            return m | e; // Return combined number
        }

        private static uint[] GenerateMantissaTable()
        {
            uint[] mantissaTable = new uint[2048];
            mantissaTable[0] = 0;
            for (int i = 1; i < 1024; i++)
            {
                mantissaTable[i] = ConvertMantissa(i);
            }
            for (int i = 1024; i < 2048; i++)
            {
                mantissaTable[i] = (uint)(0x38000000 + ((i - 1024) << 13));
            }

            return mantissaTable;
        }

        private static uint[] GenerateExponentTable()
        {
            uint[] exponentTable = new uint[64];
            exponentTable[0] = 0;
            for (int i = 1; i < 31; i++)
            {
                exponentTable[i] = (uint)(i << 23);
            }
            exponentTable[31] = 0x47800000;
            exponentTable[32] = 0x80000000;
            for (int i = 33; i < 63; i++)
            {
                exponentTable[i] = (uint)(0x80000000 + ((i - 32) << 23));
            }
            exponentTable[63] = 0xc7800000;

            return exponentTable;
        }

        private static ushort[] GenerateOffsetTable()
        {
            ushort[] offsetTable = new ushort[64];
            offsetTable[0] = 0;
            for (int i = 1; i < 32; i++)
            {
                offsetTable[i] = 1024;
            }
            offsetTable[32] = 0;
            for (int i = 33; i < 64; i++)
            {
                offsetTable[i] = 1024;
            }

            return offsetTable;
        }

        private static ushort[] GenerateBaseTable()
        {
            ushort[] baseTable = new ushort[512];
            for (int i = 0; i < 256; ++i)
            {
                sbyte e = (sbyte)(127 - i);
                if (e > 24)
                { // Very small numbers map to zero
                    baseTable[i | 0x000] = 0x0000;
                    baseTable[i | 0x100] = 0x8000;
                }
                else if (e > 14)
                { // Small numbers map to denorms
                    baseTable[i | 0x000] = (ushort)(0x0400 >> (18 + e));
                    baseTable[i | 0x100] = (ushort)((0x0400 >> (18 + e)) | 0x8000);
                }
                else if (e >= -15)
                { // Normal numbers just lose precision
                    baseTable[i | 0x000] = (ushort)((15 - e) << 10);
                    baseTable[i | 0x100] = (ushort)(((15 - e) << 10) | 0x8000);
                }
                else if (e > -128)
                { // Large numbers map to Infinity
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                }
                else
                { // Infinity and NaN's stay Infinity and NaN's
                    baseTable[i | 0x000] = 0x7c00;
                    baseTable[i | 0x100] = 0xfc00;
                }
            }

            return baseTable;
        }

        private static sbyte[] GenerateShiftTable()
        {
            sbyte[] shiftTable = new sbyte[512];
            for (int i = 0; i < 256; ++i)
            {
                sbyte e = (sbyte)(127 - i);
                if (e > 24)
                { // Very small numbers map to zero
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                }
                else if (e > 14)
                { // Small numbers map to denorms
                    shiftTable[i | 0x000] = (sbyte)(e - 1);
                    shiftTable[i | 0x100] = (sbyte)(e - 1);
                }
                else if (e >= -15)
                { // Normal numbers just lose precision
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                }
                else if (e > -128)
                { // Large numbers map to Infinity
                    shiftTable[i | 0x000] = 24;
                    shiftTable[i | 0x100] = 24;
                }
                else
                { // Infinity and NaN's stay Infinity and NaN's
                    shiftTable[i | 0x000] = 13;
                    shiftTable[i | 0x100] = 13;
                }
            }

            return shiftTable;
        }
        #endregion
    }
}