namespace gRPCNet.ServerAPI.CommonServices.Utils
{
    public interface ILogger
    {
        void InfoLog(string log, string ip = "");
        void WarningLog(string log, string ip = "");
        void ErrorLog(string log, string ip = "");
        void LogException(string exceptionMessage);
    }
}
