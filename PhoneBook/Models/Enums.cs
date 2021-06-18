using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneBook.Models
{
    public class Enums
    {
        public enum Gender
        {
            [Display(Name = "Male")]
            Male = 1,
            [Display(Name = "Female")]
            Female = 2,
        }
    }
}
