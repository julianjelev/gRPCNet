using System;

namespace gRPCNet.Modbus
{
    public class ModbusProtocol
    {
        public enum ProtocolType { ModbusTCP = 0, ModbusUDP = 1, ModbusRTU = 2 };

        public DateTime TimeStamp { get; set; }
        public bool IsRequest { get; set; }
        public bool IsResponse { get; set; }
        public UInt16 TransactionIdentifier { get; set; }
        public UInt16 ProtocolIdentifier { get; set; }
        public UInt16 Length { get; set; }
        public byte UnitIdentifier { get; set; }
        public byte FunctionCode { get; set; }
        public UInt16 StartingAddress { get; set; }
        public UInt16 StartingAddressRead { get; set; }
        public UInt16 StartingAddressWrite { get; set; }
        public UInt16 Quantity { get; set; }
        public UInt16 QuantityRead { get; set; }
        public UInt16 QuantityWrite { get; set; }
        public byte ByteCount { get; set; }
        public byte ExceptionCode { get; set; }
        public byte ErrorCode { get; set; }
        public UInt16[] ReceiveCoilValues { get; set; }
        public UInt16[] ReceiveRegisterValues { get; set; }
        public bool[] SendCoilValues { get; set; }
        public Int16[] SendRegisterValues { get; set; }
        public UInt16 CRC { get; set; }
    }
}
