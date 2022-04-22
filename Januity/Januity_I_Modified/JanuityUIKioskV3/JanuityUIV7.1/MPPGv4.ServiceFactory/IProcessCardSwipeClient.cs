using MPPGv4.Dtos;

namespace MPPGv4.ServiceFactory
{
    public interface IProcessCardSwipeClient
    {
        ProcessCardSwipeResponseDto ProcessCardSwipe(ProcessCardSwipeRequestDto processCardSwipeRequestDto);
    }
}
