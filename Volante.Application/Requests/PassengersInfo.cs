using System.Collections.Generic;

namespace Volante.Application.Requests
{
    public class PassengersInfo
    {
        public int Adults { get; set; }
        public int Seniors { get; set; }
        public List<int> ChildrenAges { get; set; }
    }
}