namespace gRPCNet.Modbus
{
    public interface IModbusSlaveDevice
    {
        #region Properties

        BooleanRegisters Coils { get; }
        BooleanRegisters DiscreteInputs { get; }
        TwoByteRegisters HoldingRegisters { get; }
        TwoByteRegisters InputRegisters { get; }

        #endregion

        #region Methods

        byte[] ProcessReceivedData(byte[] inputBuffer);
        void ClearCoils();
        void ClearDiscreteInputs();
        void ClearHoldingRegisters();
        void ClearInputRegisters();

        #endregion

        #region Events

        event ModbusSlaveDevice.CoilsChangedEventHandler CoilsChanged;
        event ModbusSlaveDevice.HoldingRegistersChangedEventHandler HoldingRegistersChanged;

        #endregion
    }
}
