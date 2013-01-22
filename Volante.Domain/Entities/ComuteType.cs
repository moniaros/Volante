using System;

namespace Volante.Domain.Entities
{
    [Flags]
    public enum ComuteType
    {
        Bike = 1,
        Bus = 2,
        Car = 4,
        Foot = 8,
        Plane = 16,
        Ship = 32,
        Train = 64,
        Tram = 128,
        Underground = 256
    }
}
