using System;

namespace gRPCNet.Modbus
{
    public class ModbusSlaveDevice : IModbusSlaveDevice
    {
        private readonly byte _unitIdentifier = 1;
        private readonly object _lockCoils = new object();
        private readonly object _lockDiscreteInputs = new object();
        private readonly object _lockHoldingRegisters = new object();
        private readonly object _lockInputRegisters = new object();

        #region Public properties

        public BooleanRegisters Coils { get; }
        public BooleanRegisters DiscreteInputs { get; }
        public TwoByteRegisters HoldingRegisters { get; }
        public TwoByteRegisters InputRegisters { get; }

        #endregion

        #region Public ctor's

        public ModbusSlaveDevice() : this(65535, 65535, 65535, 65535) { }

        public ModbusSlaveDevice(UInt16 coils, UInt16 discreteInputs, UInt16 holdingRegisters, UInt16 inputRegisters)
        {
            Coils = new BooleanRegisters(coils);
            DiscreteInputs = new BooleanRegisters(discreteInputs);
            HoldingRegisters = new TwoByteRegisters(holdingRegisters);
            InputRegisters = new TwoByteRegisters(inputRegisters);
        }

        #endregion

        #region Public events

        public delegate void CoilsChangedEventHandler(UInt16 coil, UInt16 numberOfCoils);
        public event CoilsChangedEventHandler CoilsChanged = delegate { };

        public delegate void HoldingRegistersChangedEventHandler(UInt16 register, UInt16 numberOfRegisters);
        public event HoldingRegistersChangedEventHandler HoldingRegistersChanged = delegate { };

        #endregion

        #region Public methods

        public byte[] ProcessReceivedData(byte[] inputBuffer)
        {
            ModbusProtocol receiveData = new ModbusProtocol();
            ModbusProtocol sendData = new ModbusProtocol();

            try
            {
                UInt16[] word = new UInt16[1];
                byte[] b2 = new byte[2];//byteData
                receiveData.TimeStamp = DateTime.Now;
                receiveData.IsRequest = true;
                // Lese Transaction identifier
                b2[1] = inputBuffer[0];
                b2[0] = inputBuffer[1];
                Buffer.BlockCopy(b2, 0, word, 0, 2);
                receiveData.TransactionIdentifier = word[0];
                // Lese Protocol identifier
                b2[1] = inputBuffer[2];
                b2[0] = inputBuffer[3];
                Buffer.BlockCopy(b2, 0, word, 0, 2);
                receiveData.ProtocolIdentifier = word[0];
                // Lese length
                b2[1] = inputBuffer[4];
                b2[0] = inputBuffer[5];
                Buffer.BlockCopy(b2, 0, word, 0, 2);
                receiveData.Length = word[0];
                // Lese unit identifier
                receiveData.UnitIdentifier = inputBuffer[6];
                // Check UnitIdentifier
                if ((receiveData.UnitIdentifier != this._unitIdentifier) & (receiveData.UnitIdentifier != 0))
                    return new byte[0];//!!!
                // Lese function code
                receiveData.FunctionCode = inputBuffer[7];
                // Lese starting address 
                b2[1] = inputBuffer[8];
                b2[0] = inputBuffer[9];
                Buffer.BlockCopy(b2, 0, word, 0, 2);
                receiveData.StartingAddress = word[0];

                if (receiveData.FunctionCode <= 4)
                {
                    // Lese quantity
                    b2[1] = inputBuffer[10];
                    b2[0] = inputBuffer[11];
                    Buffer.BlockCopy(b2, 0, word, 0, 2);
                    receiveData.Quantity = word[0];
                }
                else if (receiveData.FunctionCode == 5)
                {
                    receiveData.ReceiveCoilValues = new UInt16[1];
                    // Lese Value
                    b2[1] = inputBuffer[10];
                    b2[0] = inputBuffer[11];
                    Buffer.BlockCopy(b2, 0, receiveData.ReceiveCoilValues, 0, 2);
                }
                else if (receiveData.FunctionCode == 6)
                {
                    receiveData.ReceiveRegisterValues = new UInt16[1];
                    // Lese Value
                    b2[1] = inputBuffer[10];
                    b2[0] = inputBuffer[11];
                    Buffer.BlockCopy(b2, 0, receiveData.ReceiveRegisterValues, 0, 2);
                }
                else if (receiveData.FunctionCode == 15)
                {
                    // Lese quantity
                    b2[1] = inputBuffer[10];
                    b2[0] = inputBuffer[11];
                    Buffer.BlockCopy(b2, 0, word, 0, 2);
                    receiveData.Quantity = word[0];
                    receiveData.ByteCount = inputBuffer[12];
                    if ((receiveData.ByteCount % 2) != 0)
                        receiveData.ReceiveCoilValues = new UInt16[receiveData.ByteCount / 2 + 1];
                    else
                        receiveData.ReceiveCoilValues = new UInt16[receiveData.ByteCount / 2];
                    // Lese Value
                    Buffer.BlockCopy(inputBuffer, 13, receiveData.ReceiveCoilValues, 0, receiveData.ByteCount);
                }
                else if (receiveData.FunctionCode == 16)
                {
                    // Lese quantity
                    b2[1] = inputBuffer[10];
                    b2[0] = inputBuffer[11];
                    Buffer.BlockCopy(b2, 0, word, 0, 2);
                    receiveData.Quantity = word[0];
                    receiveData.ByteCount = inputBuffer[12];
                    receiveData.ReceiveRegisterValues = new UInt16[receiveData.Quantity];
                    for (int i = 0; i < receiveData.Quantity; i++)
                    {
                        // Lese Value
                        b2[1] = inputBuffer[13 + i * 2];
                        b2[0] = inputBuffer[14 + i * 2];
                        Buffer.BlockCopy(b2, 0, receiveData.ReceiveRegisterValues, i * 2, 2);
                    }
                }
                else if (receiveData.FunctionCode == 23)
                {
                    // Lese starting Address Read
                    b2[1] = inputBuffer[8];
                    b2[0] = inputBuffer[9];
                    Buffer.BlockCopy(b2, 0, word, 0, 2);
                    receiveData.StartingAddressRead = word[0];
                    // Lese quantity Read
                    b2[1] = inputBuffer[10];
                    b2[0] = inputBuffer[11];
                    Buffer.BlockCopy(b2, 0, word, 0, 2);
                    receiveData.QuantityRead = word[0];
                    // Lese starting Address Write
                    b2[1] = inputBuffer[12];
                    b2[0] = inputBuffer[13];
                    Buffer.BlockCopy(b2, 0, word, 0, 2);
                    receiveData.StartingAddressWrite = word[0];
                    // Lese quantity Write
                    b2[1] = inputBuffer[14];
                    b2[0] = inputBuffer[15];
                    Buffer.BlockCopy(b2, 0, word, 0, 2);
                    receiveData.QuantityWrite = word[0];
                    receiveData.ByteCount = inputBuffer[16];
                    receiveData.ReceiveRegisterValues = new UInt16[receiveData.QuantityWrite];
                    for (int i = 0; i < receiveData.QuantityWrite; i++)
                    {
                        // Lese Value
                        b2[1] = inputBuffer[17 + i * 2];
                        b2[0] = inputBuffer[18 + i * 2];
                        Buffer.BlockCopy(b2, 0, receiveData.ReceiveRegisterValues, i * 2, 2);
                    }
                }
            }
            catch { }

            return CreateAnswer(receiveData, sendData);
        }
        public void ClearCoils()
        {
            lock (_lockCoils)
            {
                for (int i = 0; i < Coils.LocalArray.Length; i++)
                    Coils[(UInt16)i] = false;
            }
        }
        public void ClearDiscreteInputs()
        {
            lock (_lockDiscreteInputs)
            {
                for (int i = 0; i < DiscreteInputs.LocalArray.Length; i++)
                    DiscreteInputs[(UInt16)i] = false;
            }
        }
        public void ClearHoldingRegisters()
        {
            lock (_lockHoldingRegisters)
            {
                for (int i = 0; i < HoldingRegisters.LocalArray.Length; i++)
                    HoldingRegisters[(UInt16)i] = 0;
            }
        }
        public void ClearInputRegisters()
        {
            lock (_lockInputRegisters)
            {
                for (int i = 0; i < InputRegisters.LocalArray.Length; i++)
                    InputRegisters[(UInt16)i] = 0;
            }
        }

        #endregion

        private byte[] CreateAnswer(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            byte[] answer;
            switch (receiveData.FunctionCode)
            {
                // Read Coils
                case 1:
                    answer = ReadCoils(receiveData, sendData);
                    break;
                // Read Input Registers
                case 2:
                    answer = ReadDiscreteInputs(receiveData, sendData);
                    break;
                // Read Holding Registers
                case 3:
                    answer = ReadHoldingRegisters(receiveData, sendData);
                    break;
                // Read Input Registers
                case 4:
                    answer = ReadInputRegisters(receiveData, sendData);
                    break;
                // Write single coil
                case 5:
                    answer = WriteSingleCoil(receiveData, sendData);
                    break;
                // Write single register
                case 6:
                    answer = WriteSingleRegister(receiveData, sendData);
                    break;
                // Write Multiple coils
                case 15:
                    answer = WriteMultipleCoils(receiveData, sendData);
                    break;
                // Write Multiple registers
                case 16:
                    answer = WriteMultipleRegisters(receiveData, sendData);
                    break;
                // ReadWrite Multiple registers
                case 23:
                    answer = ReadWriteMultipleRegisters(receiveData, sendData);
                    break;
                // Error: Function Code not supported
                default:
                    sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                    sendData.ExceptionCode = 1;
                    answer = ComposeException(sendData.ErrorCode, sendData.ExceptionCode, receiveData, sendData);
                    break;
            }
            sendData.TimeStamp = DateTime.Now;
            return answer;
        }

        #region modbus functions

        private byte[] ReadCoils(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = receiveData.UnitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;

            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x07D0))
            {
                //Invalid quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }

            if (((receiveData.StartingAddress + 1 + receiveData.Quantity) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting address or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }

            if (sendData.ExceptionCode == 0)
            {
                if ((receiveData.Quantity % 8) == 0)
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8);
                else
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8 + 1);

                sendData.SendCoilValues = new bool[receiveData.Quantity];
                lock (_lockCoils)
                {
                    Array.Copy(Coils.LocalArray, receiveData.StartingAddress + 1, sendData.SendCoilValues, 0, receiveData.Quantity);
                }
            }

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[9];

            byte[] b2;// ???  = new byte[2]
            sendData.Length = (byte)(outputBuffer.Length - 6);
            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;
            //Set ByteCount
            outputBuffer[8] = sendData.ByteCount;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendCoilValues = null;
            }

            if (sendData.SendCoilValues != null)
                for (int i = 0; i < sendData.ByteCount; i++)
                {
                    b2 = new byte[2];
                    for (int j = 0; j < 8; j++)
                    {
                        byte boolValue;
                        if (sendData.SendCoilValues[i * 8 + j] == true)
                            boolValue = 1;
                        else
                            boolValue = 0;
                        b2[1] = (byte)((b2[1]) | (boolValue << j));
                        if ((i * 8 + j + 1) >= sendData.SendCoilValues.Length)
                            break;
                    }
                    outputBuffer[9 + i] = b2[1];
                }

            return outputBuffer;
        }

        private byte[] ReadDiscreteInputs(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;

            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x07D0))
            {
                //Invalid quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if (((receiveData.StartingAddress + 1 + receiveData.Quantity) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                if ((receiveData.Quantity % 8) == 0)
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8);
                else
                    sendData.ByteCount = (byte)(receiveData.Quantity / 8 + 1);

                sendData.SendCoilValues = new bool[receiveData.Quantity];
                Array.Copy(DiscreteInputs.LocalArray, receiveData.StartingAddress + 1, sendData.SendCoilValues, 0, receiveData.Quantity);
            }
            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[9 + sendData.ByteCount];
            byte[] b2;// = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);

            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set Unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;
            //Set ByteCount
            outputBuffer[8] = sendData.ByteCount;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendCoilValues = null;
            }
            if (sendData.SendCoilValues != null)
                for (int i = 0; i < sendData.ByteCount; i++)
                {
                    b2 = new byte[2];
                    for (int j = 0; j < 8; j++)
                    {

                        byte boolValue;
                        if (sendData.SendCoilValues[i * 8 + j] == true)
                            boolValue = 1;
                        else
                            boolValue = 0;
                        b2[1] = (byte)((b2[1]) | (boolValue << j));
                        if ((i * 8 + j + 1) >= sendData.SendCoilValues.Length)
                            break;
                    }
                    outputBuffer[9 + i] = b2[1];
                }

            return outputBuffer;
        }

        private byte[] ReadHoldingRegisters(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x007D))
            {
                //Invalid quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if (((receiveData.StartingAddress + 1 + receiveData.Quantity) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                sendData.ByteCount = (byte)(2 * receiveData.Quantity);
                sendData.SendRegisterValues = new Int16[receiveData.Quantity];
                lock (_lockHoldingRegisters)
                {
                    Buffer.BlockCopy(HoldingRegisters.LocalArray, receiveData.StartingAddress * 2 + 2, sendData.SendRegisterValues, 0, receiveData.Quantity * 2);
                }
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = (UInt16)(0x03 + sendData.ByteCount);

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[9 + sendData.ByteCount];
            byte[] b2;// = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);

            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set Unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;
            //Set ByteCount
            outputBuffer[8] = sendData.ByteCount;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendRegisterValues = null;
            }
            if (sendData.SendRegisterValues != null)
                for (int i = 0; i < (sendData.ByteCount / 2); i++)
                {
                    b2 = BitConverter.GetBytes((Int16)sendData.SendRegisterValues[i]);
                    outputBuffer[9 + i * 2] = b2[1];
                    outputBuffer[10 + i * 2] = b2[0];
                }


            return outputBuffer;
        }

        private byte[] ReadInputRegisters(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            if ((receiveData.Quantity < 1) || (receiveData.Quantity > 0x007D))
            {
                //Invalid quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if (((receiveData.StartingAddress + 1 + receiveData.Quantity) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                sendData.ByteCount = (byte)(2 * receiveData.Quantity);
                sendData.SendRegisterValues = new Int16[receiveData.Quantity];
                Buffer.BlockCopy(InputRegisters.LocalArray, receiveData.StartingAddress * 2 + 2, sendData.SendRegisterValues, 0, receiveData.Quantity * 2);
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = (UInt16)(0x03 + sendData.ByteCount);

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[9 + sendData.ByteCount];
            byte[] b2;// = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);
            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set Unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;
            //Set ByteCount
            outputBuffer[8] = sendData.ByteCount;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendRegisterValues = null;
            }
            if (sendData.SendRegisterValues != null)
                for (int i = 0; i < (sendData.ByteCount / 2); i++)
                {
                    b2 = BitConverter.GetBytes((Int16)sendData.SendRegisterValues[i]);
                    outputBuffer[9 + i * 2] = b2[1];
                    outputBuffer[10 + i * 2] = b2[0];
                }

            return outputBuffer;
        }

        private byte[] WriteSingleCoil(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAddress = receiveData.StartingAddress;
            sendData.ReceiveCoilValues = receiveData.ReceiveCoilValues;
            if ((receiveData.ReceiveCoilValues[0] != 0x0000) && (receiveData.ReceiveCoilValues[0] != 0xFF00))
            {
                //Invalid Value
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if (((receiveData.StartingAddress + 1) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                if (receiveData.ReceiveCoilValues[0] == 0xFF00)
                {
                    lock (_lockCoils)
                        Coils[(UInt16)(receiveData.StartingAddress + 1)] = true;
                }
                if (receiveData.ReceiveCoilValues[0] == 0x0000)
                {
                    lock (_lockCoils)
                        Coils[(UInt16)(receiveData.StartingAddress + 1)] = false;
                }
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = 0x06;

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[12];

            byte[] b2;// = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);
            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendRegisterValues = null;
            }
            else
            {
                b2 = BitConverter.GetBytes((int)receiveData.StartingAddress);
                outputBuffer[8] = b2[1];
                outputBuffer[9] = b2[0];
                b2 = BitConverter.GetBytes((int)receiveData.ReceiveCoilValues[0]);
                outputBuffer[10] = b2[1];
                outputBuffer[11] = b2[0];

                // fire event
                CoilsChanged((UInt16)(receiveData.StartingAddress + 1), 1);
            }

            return outputBuffer;
        }

        private byte[] WriteSingleRegister(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAddress = receiveData.StartingAddress;
            sendData.ReceiveRegisterValues = receiveData.ReceiveRegisterValues;
            if ((receiveData.ReceiveRegisterValues[0] < 0x0000) || (receiveData.ReceiveRegisterValues[0] > 0xFFFF))
            {
                //Invalid Value
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if (((receiveData.StartingAddress + 1) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                lock (_lockHoldingRegisters)
                {
                    HoldingRegisters[(UInt16)(receiveData.StartingAddress + 1)] = unchecked((Int16)receiveData.ReceiveRegisterValues[0]);
                }
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = 0x06;

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[12];

            byte[] b2;// = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);
            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set Unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendRegisterValues = null;
            }
            else
            {
                b2 = BitConverter.GetBytes((int)receiveData.StartingAddress);
                outputBuffer[8] = b2[1];
                outputBuffer[9] = b2[0];
                b2 = BitConverter.GetBytes((int)receiveData.ReceiveRegisterValues[0]);
                outputBuffer[10] = b2[1];
                outputBuffer[11] = b2[0];

                // fire event
                HoldingRegistersChanged((UInt16)(receiveData.StartingAddress + 1), 1);
            }

            return outputBuffer;
        }

        private byte[] WriteMultipleCoils(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAddress = receiveData.StartingAddress;
            sendData.Quantity = receiveData.Quantity;
            if ((receiveData.Quantity == 0x0000) || (receiveData.Quantity > 0x07B0))
            {
                //Invalid Quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((((int)receiveData.StartingAddress + 1 + (int)receiveData.Quantity) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                lock (_lockCoils)
                {
                    for (int i = 0; i < receiveData.Quantity; i++)
                    {
                        int shift = i % 16;
                        int mask = 0x1;
                        mask <<= shift;
                        if ((receiveData.ReceiveCoilValues[i / 16] & (UInt16)mask) == 0)
                            Coils[(UInt16)(receiveData.StartingAddress + i + 1)] = false;
                        else
                            Coils[(UInt16)(receiveData.StartingAddress + i + 1)] = true;
                    }
                }
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = 0x06;

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[12];

            byte[] b2;// = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);
            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set Unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendRegisterValues = null;
            }
            else
            {
                b2 = BitConverter.GetBytes((int)receiveData.StartingAddress);
                outputBuffer[8] = b2[1];
                outputBuffer[9] = b2[0];
                b2 = BitConverter.GetBytes((int)receiveData.Quantity);
                outputBuffer[10] = b2[1];
                outputBuffer[11] = b2[0];

                // fire event
                CoilsChanged((UInt16)(receiveData.StartingAddress + 1), receiveData.Quantity);
            }

            return outputBuffer;
        }

        private byte[] WriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            sendData.StartingAddress = receiveData.StartingAddress;
            sendData.Quantity = receiveData.Quantity;

            if ((receiveData.Quantity == 0x0000) || (receiveData.Quantity > 0x07B0))
            {
                //Invalid Quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if ((((int)receiveData.StartingAddress + 1 + (int)receiveData.Quantity) > 65535) || (receiveData.StartingAddress < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                lock (_lockHoldingRegisters)
                {
                    for (int i = 0; i < receiveData.Quantity; i++)
                    {
                        HoldingRegisters[(UInt16)(receiveData.StartingAddress + i + 1)] = unchecked((Int16)receiveData.ReceiveRegisterValues[i]);
                    }
                }
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = 0x06;

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[12];

            byte[] b2;// = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);
            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set Unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendRegisterValues = null;
            }
            else
            {
                b2 = BitConverter.GetBytes((int)receiveData.StartingAddress);
                outputBuffer[8] = b2[1];
                outputBuffer[9] = b2[0];
                b2 = BitConverter.GetBytes((int)receiveData.Quantity);
                outputBuffer[10] = b2[1];
                outputBuffer[11] = b2[0];

                // fire event
                HoldingRegistersChanged((UInt16)(receiveData.StartingAddress + 1), receiveData.Quantity);
            }

            return outputBuffer;
        }

        private byte[] ReadWriteMultipleRegisters(ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = this._unitIdentifier;
            sendData.FunctionCode = receiveData.FunctionCode;
            if ((receiveData.QuantityRead < 0x0001) ||
                (receiveData.QuantityRead > 0x007D) ||
                (receiveData.QuantityWrite < 0x0001) ||
                (receiveData.QuantityWrite > 0x0079) ||
                (receiveData.ByteCount != (receiveData.QuantityWrite * 2)))
            {
                //Invalid Quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 3;
            }
            if (((receiveData.StartingAddressRead + 1 + receiveData.QuantityRead) > 65535) ||
                ((receiveData.StartingAddressWrite + 1 + receiveData.QuantityWrite) > 65535) ||
                (receiveData.QuantityWrite < 0) ||
                (receiveData.QuantityRead < 0))
            {
                //Invalid Starting adress or Starting address + quantity
                sendData.ErrorCode = (byte)(receiveData.FunctionCode + 0x80);
                sendData.ExceptionCode = 2;
            }
            if (sendData.ExceptionCode == 0)
            {
                sendData.SendRegisterValues = new Int16[receiveData.QuantityRead];
                lock (_lockHoldingRegisters)
                {
                    Buffer.BlockCopy(
                        HoldingRegisters.LocalArray,
                        receiveData.StartingAddressRead * 2 + 2,
                        sendData.SendRegisterValues,
                        0,
                        receiveData.QuantityRead * 2);
                    for (int i = 0; i < receiveData.QuantityWrite; i++)
                    {
                        HoldingRegisters[(UInt16)(receiveData.StartingAddressWrite + i + 1)] = unchecked((Int16)receiveData.ReceiveRegisterValues[i]);
                    }
                }
                sendData.ByteCount = (byte)(2 * receiveData.QuantityRead);
            }
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = Convert.ToUInt16(3 + 2 * receiveData.QuantityRead);

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[9 + sendData.ByteCount];

            byte[] b2;// = new byte[2];
            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set Length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set Unit Identifier
            outputBuffer[6] = sendData.UnitIdentifier;
            //Set Function Code
            outputBuffer[7] = sendData.FunctionCode;
            //Set ByteCount
            outputBuffer[8] = sendData.ByteCount;

            if (sendData.ExceptionCode > 0)
            {
                outputBuffer[7] = sendData.ErrorCode;
                outputBuffer[8] = sendData.ExceptionCode;
                sendData.SendRegisterValues = null;
            }
            else
            {
                if (sendData.SendRegisterValues != null)
                    for (int i = 0; i < (sendData.ByteCount / 2); i++)
                    {
                        b2 = BitConverter.GetBytes((Int16)sendData.SendRegisterValues[i]);
                        outputBuffer[9 + i * 2] = b2[1];
                        outputBuffer[10 + i * 2] = b2[0];
                    }
                // fire event
                HoldingRegistersChanged((UInt16)(receiveData.StartingAddressWrite + 1), receiveData.QuantityWrite);
            }

            return outputBuffer;
        }

        private byte[] ComposeException(int errorCode, int exceptionCode, ModbusProtocol receiveData, ModbusProtocol sendData)
        {
            sendData.IsResponse = true;
            sendData.TransactionIdentifier = receiveData.TransactionIdentifier;
            sendData.ProtocolIdentifier = receiveData.ProtocolIdentifier;
            sendData.UnitIdentifier = receiveData.UnitIdentifier;
            sendData.ErrorCode = (byte)errorCode;
            sendData.ExceptionCode = (byte)exceptionCode;
            if (sendData.ExceptionCode > 0)
                sendData.Length = 0x03;
            else
                sendData.Length = (ushort)(0x03 + sendData.ByteCount);

            byte[] outputBuffer;
            if (sendData.ExceptionCode > 0)
                outputBuffer = new byte[9];
            else
                outputBuffer = new byte[9 + sendData.ByteCount];

            byte[] b2;// ???  = new byte[2];
            sendData.Length = (byte)(outputBuffer.Length - 6);

            //Set Transaction identifier
            b2 = BitConverter.GetBytes((int)sendData.TransactionIdentifier);
            outputBuffer[0] = b2[1];
            outputBuffer[1] = b2[0];
            //Set Protocol identifier
            b2 = BitConverter.GetBytes((int)sendData.ProtocolIdentifier);
            outputBuffer[2] = b2[1];
            outputBuffer[3] = b2[0];
            //Set length
            b2 = BitConverter.GetBytes((int)sendData.Length);
            outputBuffer[4] = b2[1];
            outputBuffer[5] = b2[0];
            //Set init Identifier
            outputBuffer[6] = sendData.UnitIdentifier;

            outputBuffer[7] = sendData.ErrorCode;
            outputBuffer[8] = sendData.ExceptionCode;

            return outputBuffer;
        }

        #endregion
    }
}
