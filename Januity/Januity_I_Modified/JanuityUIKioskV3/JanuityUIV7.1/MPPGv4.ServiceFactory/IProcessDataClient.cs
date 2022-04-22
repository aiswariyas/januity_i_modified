using MPPGv4.Dtos;

namespace MPPGv4.ServiceFactory
{
    public interface IProcessDataClient
    {
        ProcessDataResponseDto ProcessData(ProcessDataRequestDto processDataRequestDto);
    }
}
