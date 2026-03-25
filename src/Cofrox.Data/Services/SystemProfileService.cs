using System.Runtime.InteropServices;
using Cofrox.Domain.Entities;
using Cofrox.Domain.Interfaces;

namespace Cofrox.Data.Services;

public sealed class SystemProfileService : ISystemProfileService
{
    public SystemProfile GetCurrent()
    {
        var memoryStatus = new MemoryStatusEx();
        if (GlobalMemoryStatusEx(memoryStatus))
        {
            return new SystemProfile
            {
                TotalPhysicalMemoryBytes = memoryStatus.TotalPhys,
            };
        }

        return new SystemProfile
        {
            TotalPhysicalMemoryBytes = 0,
        };
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx buffer);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private sealed class MemoryStatusEx
    {
        public MemoryStatusEx()
        {
            Length = (uint)Marshal.SizeOf<MemoryStatusEx>();
        }

        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }
}
