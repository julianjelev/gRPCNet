using System;

namespace gRPCNet.Modbus
{
    public class BooleanRegisters
    {
        private readonly bool[] _localArray;

        public BooleanRegisters() : this(65535) { }
        public BooleanRegisters(UInt16 capacity)
        {
            _localArray = new bool[capacity];
        }

        public bool this[UInt16 i]
        {
            get { return _localArray[i]; }
            set { _localArray[i] = value; }
        }

        public bool[] LocalArray { get => _localArray; }
    }
}
