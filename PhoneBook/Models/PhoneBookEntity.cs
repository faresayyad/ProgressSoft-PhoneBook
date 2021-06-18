using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneBook.Models
{
    public class PhoneBookEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public DateTime DOB { get; set; }
        public string Email { get; set; }
        public string CountryKey { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageName { get; set; }
        public string Address { get; set; }
    }
}
