using MPPGv4.Dtos;

namespace MPPGv4.ServiceFactory
{
    public interface IProcessKeyPadEntryClient
    {
        ProcessKeyPadEntryResponseDto ProcessKeyPadEntry(ProcessKeyPadEntryRequestDto processKeyPadEntryRequestDto);
    }
}
