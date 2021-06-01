using gRPCNet.Modbus;
using ProtoBuf.Grpc.Client;
using System;
using System.Text;

namespace gRPCNet.Client.Services
{
    public interface IModbusMessageService
    {
        byte[] ProccessRequest(byte[] message);
        void StartProcessing();
        void StopProcessing();
    }

    public class ModbusMessageService : IModbusMessageService
    {
        private readonly GrpcChannelService _chanelService;
        private readonly IModbusSlaveDevice _modbusSlave;

        private Proto.ICardService _cardService;

        public ModbusMessageService(
            GrpcChannelService chanelService,
            IModbusSlaveDevice modbusSlave)
        {
            _chanelService = chanelService;
            _cardService = _chanelService.Channel != null ?
            _chanelService.Channel.CreateGrpcService<Proto.ICardService>() : null;
            // Да се създаде 1 инстанция на ModbusSlaveDevice.
            // Да се регистрират хендлъри на събитията CoilsChanged и HoldingRegistersChanged
            // отговорни за извличане и обработка на данните от заявките за писане в регистри Coils и HoldingRegisters.
            // Същите тези хендлъри трябва да запишат резултатите съответно в регистри DiscreteInputs и InputRegisters.
            // след което, трябва да нулират регистри Coils и HoldingRegisters.
            _modbusSlave = modbusSlave;
        }

        public void StartProcessing()
        {
            if (_modbusSlave != null)
            {
                _modbusSlave.CoilsChanged += OnCoilsChanged;
                _modbusSlave.HoldingRegistersChanged += OnHoldingRegistersChanged;
            }
        }

        // метода се извиква при всяко запитване на конектнатия клиент
        public byte[] ProccessRequest(byte[] message)
        {
            //да се извика метода ProcessReceivedData на ModbusSlaveDevice
            return _modbusSlave.ProcessReceivedData(message);

            //Thread.Sleep(1000);//work 
            //return message.Reverse().ToArray();
        }

        public void StopProcessing()
        {
            if (_modbusSlave != null)
            {
                _modbusSlave.CoilsChanged -= OnCoilsChanged;
                _modbusSlave.HoldingRegistersChanged -= OnHoldingRegistersChanged;
            }
        }

        private void OnHoldingRegistersChanged(UInt16 register, UInt16 numberOfHoldingRegisters)
        {
            Int16[] resultBuffer;

            int gRPCId = -1;        //число, идентификатор на извикваната процедура
            string concentratorId;  //4 UTF16 символа, ляво допълнени с '0'
            string controllerId;    //8 UTF16 символа, ляво допълнени с '0'
            string cardType;        //2 UTF16 символа, ляво допълнени с '0'
            string cardId;          //16 UTF16 символа, ляво допълнени с '0'
            int endpointRssi;       //число от 0 до 255
            int concentratorRssi;   //число от 0 до 255
            StringBuilder sb = new StringBuilder();
            /************************************************************************
            1. извличане на данните от HoldingRegisters
            *************************************************************************/
            #region 1

            // 1.0 идентификатор на извикваната процедура. определя се от ранга на адреса на 1-вия регистър (40001-49999; 0x9C41-0xC34F)

            //Can play
            if (register == 40001) gRPCId = 1;      //40001(0x9C41); 128 регистъра x16
            else if (register == 40127) gRPCId = 2; //40001(0x9C41) + 128(0x0080) - 2 = 40127(0x9CBF); 128 регистъра x16
            else if (register == 40255) gRPCId = 3; //40001(0x9C41) + 256(0x0100) - 2 = 40255(0x9D3F); 128 регистъра x16
            else if (register == 40383) gRPCId = 4; //40001(0x9C41) + 384(0x0180) - 2 = 40383(0x9DBF); 128 регистъра x16
            // 1.1 идентификатор на концентратора - 4 регистъра x16
            for (UInt16 i = 0; i <= 3; i++)
                sb.Append(Convert.ToChar(
                    _modbusSlave.HoldingRegisters[(UInt16)(register + i)]));
            #region OLD
            //sb.Append(
            //    Encoding.UTF8.GetString(
            //        BitConverter.GetBytes(
            //            _modbusSlave.HoldingRegisters[(UInt16)(register + i)])));
            #endregion
            concentratorId = sb.ToString();
            sb.Clear();
            // 1.2 идентификатор на контролера - 8 регистъра x16
            for (UInt16 i = 4; i <= 11; i++)
                sb.Append(Convert.ToChar(
                    _modbusSlave.HoldingRegisters[(UInt16)(register + i)]));
            controllerId = sb.ToString();
            sb.Clear();
            // 1.3 тип на картата - 1 регистър x16
            cardType = Convert.ToChar(
                _modbusSlave.HoldingRegisters[(UInt16)(register + 12)]).ToString();
            // 1.4 идентификатор на картата - 16 регистъра x16
            for (UInt16 i = 14; i <= 29; i++)
                sb.Append(Convert.ToChar(
                    _modbusSlave.HoldingRegisters[(UInt16)(register + i)]));
            cardId = sb.ToString();
            sb.Clear();
            // 1.5 endpointRssi и concentratorRssi - Hi byte и Lo byte в последния регистър x16
            byte[] b2 = BitConverter.GetBytes(
                _modbusSlave.HoldingRegisters[(UInt16)(register + numberOfHoldingRegisters - 1)]);
            endpointRssi = b2[0];//Hi byte
            concentratorRssi = b2[1];//Lo byte

            #endregion
            /************************************************************************
            2. анализ и обработка на данните, извикване на gRPC процедурите и получаване на резултата от тях
            *************************************************************************/
            #region 2

            //Can play
            if (gRPCId == 1)
            {
                resultBuffer = new Int16[numberOfHoldingRegisters + 20]; // !!! Да се увеличи с толкова колкото са необходими за CanPlayResponse 
                Proto.CanPlayResponse response;

                if (_cardService == null)
                {
                    response = new Proto.CanPlayResponse();
                    {
                        response.Time = DateTime.Now;
                        response.TransactionId = 1; // get as parameter
                        response.ConcentratorId = concentratorId;
                        response.ControllerId = controllerId;
                        response.CardType = cardType.Length > 1 ? int.Parse(cardType.TrimStart('0')) : int.Parse(cardType);
                        response.CardId = cardId;
                        response.CardNumber = string.Empty;
                        response.Permission = false;
                        response.DisplayLine1 = "ERROR";
                        response.DisplayLine2 = "Service unavailable";
                    };
                }
                else
                {
                    var request = new Proto.CanPlayRequest();
                    request.Time = DateTime.Now;
                    request.TransactionId = 1;// get as parameter
                    request.ConcentratorId = concentratorId;
                    request.ControllerId = controllerId;
                    request.CardType = cardType.Length > 1 ? int.Parse(cardType.TrimStart('0')) : int.Parse(cardType);
                    request.CardId = cardId;
                    request.ShouldPay = false;
                    request.EndpointRssi = endpointRssi;
                    request.ConcentratorRssi = concentratorRssi;
                    response = _cardService.CanPlayAsync(request).Result;
                }
                // Fill resultBuffer from CanPlayResponse
                int ix = 0;
                // 2.1 идентификатор на концентратора - 4 регистъра x16
                foreach (char c in response.ConcentratorId.PadLeft(4, '0'))
                {
                    resultBuffer[ix] = Convert.ToInt16(c);
                    ix++;
                }
                // 2.2 идентификатор на контролера - 8 регистъра x16
                foreach (char c in response.ControllerId.PadLeft(8, '0'))
                {
                    resultBuffer[ix] = Convert.ToInt16(c);
                    ix++;
                }
                // 2.3 тип на картата - 1 регистър x16
                if (response.CardType <= 9)
                    resultBuffer[ix] = Convert.ToInt16(Convert.ToChar(response.CardType));
                else
                    resultBuffer[ix] = (short)0;
                ix++;
                // 2.4 идентификатор на картата - 16 регистъра x16
                foreach (char c in response.CardId.PadLeft(16, '0'))
                {
                    resultBuffer[ix] = Convert.ToInt16(c);
                    ix++;
                }
                // 2.5 резултат - 1 регистър x16
                resultBuffer[ix] = response.Permission ? (short)1 : (short)0;
            }
            else
            {
                resultBuffer = new Int16[numberOfHoldingRegisters];
            }

            #endregion
            /************************************************************************
            3. прехвърляне на резултатните данни в InputRegisters
            *************************************************************************/
            #region 3
            UInt16 firstInputRegister = 30001;                  //30001-39999
            if (gRPCId == 1) firstInputRegister = 30001;        //30001(0x7531); 2048 регистъра x16
            else if (gRPCId == 2) firstInputRegister = 32047;   //30001(0x7531) + 2048(0x0800) - 2 = 32047(0x7D2F); 2048 регистъра x16
            else if (gRPCId == 3) firstInputRegister = 34095;   //30001(0x7531) + 4096(0x1000) - 2 = 34095(0X852F); 2048 регистъра x16
            else if (gRPCId == 4) firstInputRegister = 36143;   //30001(0x7531) + 6144(0x1800) - 2 = 36143(0x8D2F); 2048 регистъра x16
            for (var i = 0; i < resultBuffer.Length; i++)
                _modbusSlave.InputRegisters[unchecked((UInt16)(firstInputRegister + i))] = resultBuffer[i];

            #endregion
            /************************************************************************
            4. нулиране на HoldingRegisters
            *************************************************************************/
            #region 4

            _modbusSlave.ClearHoldingRegisters();

            #endregion
        }

        private void OnCoilsChanged(UInt16 coil, UInt16 numberOfCoils)
        {
            //_modbusSlave.Coils[coil];
        }



        //private void UInt16To
    }
}
