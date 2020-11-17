using System.Collections.Generic;

namespace Foundation.Features.Blocks.AboutVisitorBlock
{
    public class ProfileViewModel
    {
        public ProfileViewModel()
        {

        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string State { get; set; }

        public string GetAddress()
        {
            if (string.IsNullOrEmpty(City) && string.IsNullOrEmpty(State)) return "N/A";
            var address = new List<string>();
            if (!string.IsNullOrEmpty(City)) address.Add(City);
            if (!string.IsNullOrEmpty(State)) address.Add(State);
            return string.Join(", ", address);
        }
    }
}