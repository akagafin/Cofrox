using Cofrox.Domain.Entities;

namespace Cofrox.Domain.Interfaces;

public interface ISystemProfileService
{
    SystemProfile GetCurrent();
}
