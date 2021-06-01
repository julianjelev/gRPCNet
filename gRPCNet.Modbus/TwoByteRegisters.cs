using System;

namespace gRPCNet.Modbus
{
    public class TwoByteRegisters
    {
        private readonly Int16[] _localArray;

        public TwoByteRegisters() : this(65535) { }
        public TwoByteRegisters(UInt16 capacity)
        {
            _localArray = new Int16[capacity];
        }

        public Int16 this[UInt16 i]
        {
            get { return _localArray[i]; }
            set { _localArray[i] = value; }
        }

        public Int16[] LocalArray { get => _localArray; }
    }
}
