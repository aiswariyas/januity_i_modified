using MPPGv4.Dtos;

namespace MPPGv4.ServiceFactory
{
    public interface IProcessTokenClient
    {
        ProcessTokenResponseDto ProcessToken(ProcessTokenRequestDto processTokenRequestDto);
    }
}
