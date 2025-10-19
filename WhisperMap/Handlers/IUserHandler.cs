using WhisperMap.Contracts;
using WhisperMap.Models;

namespace WhisperMap.Handlers;

public interface IUserHandler
{
    Task<Pedestrian> AddUserAsync(PostUserContract contract);
}