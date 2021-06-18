using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PhoneBook.Models
{
    public class PhoneBookViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "Gender")]
        public int Gender { get; set; }
        [Display(Name = "Date of birth")]
        [Required]
        public string DOB { get; set; }
        [Display(Name = "Email")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        public string CountryKey { get; set; }
        [Display(Name = "phone number")]
        [Phone]
        [Required]
        public string PhoneNumber { get; set; }
        [Display(Name = "Image")]
        public IFormFile ImageName { get; set; }
        [Display(Name = "Address")]
        [Required]
        public string Address { get; set; }
    }

    public class QRArray
    {
        public IEnumerable<PhoneBookExportViewModel> PhoneBookExportViewModel { get; set; }
    }
    public class PhoneBookExportViewModel
    {
        public string Name { get; set; }
        
        public string Gender { get; set; }
        
        public string DOB { get; set; }

        public string Email { get; set; }
       
        public string Phone { get; set; }
        
        public string Photo { get; set; }
        
        public string Address { get; set; }
    }
}
