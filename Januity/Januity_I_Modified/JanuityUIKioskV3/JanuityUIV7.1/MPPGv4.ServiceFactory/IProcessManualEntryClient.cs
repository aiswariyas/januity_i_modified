using MPPGv4.Dtos;

namespace MPPGv4.ServiceFactory
{
    public interface IProcessManualEntryClient
    {
        ProcessManualEntryResponseDto ProcessManualEntry(ProcessManualEntryRequestDto processManualEntryRequestDto);
    }
}
