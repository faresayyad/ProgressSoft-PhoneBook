using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PhoneBook.Data;
using PhoneBook.Models;
using ZXing;

namespace PhoneBook.Areas.Api.Controllers
{
    [Area("Api")]
    [Route("Api/[controller]/[action]/{id?}")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostingEnvironment { get; }
        private string filePath = null;
        private string ImgPath = null;


        public UploadController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            filePath = Path.Combine(_hostingEnvironment.WebRootPath, "files");
            ImgPath = Path.Combine(_hostingEnvironment.WebRootPath, "imgs");

        }
        public IActionResult UploadFile(IFormFile file)
        {
            try
            {
                // Get file extension
                var extension = Path.GetExtension(file.FileName);
                // copy uploaded file to path.
                IOExtension.CopyFileToPath(filePath, file);
                if (extension.Trim().ToLower() == ".csv")
                {
                    using (var reader = new StreamReader(file.OpenReadStream()))
                    using (var csvReader = new CsvReader(reader, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        var records = csvReader.GetRecords<CsvLine>().ToList();
                        var firstRow = records.FirstOrDefault();
                        IOExtension.ConvertBase64ToImageAndWriteToPath(firstRow.Photo, firstRow.Name + ".jpeg", ImgPath);

                        var entity = new PhoneBookEntity()
                        {
                            DOB = DateTime.ParseExact(firstRow.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            Address = !string.IsNullOrEmpty(firstRow.Address) ? firstRow.Address : string.Empty,
                            Email = !string.IsNullOrEmpty(firstRow.Email) ? firstRow.Email : string.Empty,
                            Gender = firstRow.Gender.Trim().ToLower() == "male" ? 1 : 2,
                            ImageName = firstRow.Name + ".jpeg",
                            Name = !string.IsNullOrEmpty(firstRow.Name) ? firstRow.Name : string.Empty,
                            PhoneNumber = !string.IsNullOrEmpty(firstRow.Phone) ? firstRow.Phone : string.Empty,
                        };
                        return Ok(new { msg = "File has been uploaded.", entity = entity });
                    }
                }
                else if (extension.Trim().ToLower() == ".xml")
                {

                    XmlDocument doc = new XmlDocument();
                    //var fullPath = filePath + "\\" + file.FileName;
                    doc.Load(file.OpenReadStream());

                    string name = doc.GetElementsByTagName("Name").Item(0).InnerText.Trim();
                    string Gender = doc.GetElementsByTagName("Gender").Item(0).InnerText.Trim();
                    string DOB = doc.GetElementsByTagName("DOB").Item(0).InnerText.Trim();
                    string Email = doc.GetElementsByTagName("Email").Item(0).InnerText.Trim();
                    string Phone = doc.GetElementsByTagName("Phone").Item(0).InnerText.Trim();
                    string Photo = doc.GetElementsByTagName("Photo").Item(0).InnerText.Trim();
                    string Address = doc.GetElementsByTagName("Address").Item(0).InnerText.Trim();

                    IOExtension.ConvertBase64ToImageAndWriteToPath(Photo, name + ".jpeg", ImgPath);
                    var entity = new PhoneBookEntity()
                    {
                        DOB = DateTime.ParseExact(DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                        Address = !string.IsNullOrEmpty(Address) ? Address : string.Empty,
                        Email = !string.IsNullOrEmpty(Email) ? Email : string.Empty,
                        Gender = Gender.Trim().ToLower() == "male" ? 1 : 2,
                        ImageName = name + ".jpeg",
                        Name = !string.IsNullOrEmpty(name) ? name : string.Empty,
                        PhoneNumber = !string.IsNullOrEmpty(Phone) ? Phone : string.Empty,
                    };

                    return Ok(new { msg = "File has been uploaded.", entity = entity });
                }
                else
                {
                    var bitMap = (Bitmap)Bitmap.FromStream(file.OpenReadStream());
                    var reader = new BarcodeReader();
                    var result = reader.Decode(bitMap);

                    if (!string.IsNullOrEmpty(result.Text))
                    {
                        var list = JsonConvert.DeserializeObject<List<PhoneBookExportViewModel>>(result.Text);
                        var model = list.FirstOrDefault();
                        var entity = new PhoneBookEntity()
                        {
                            DOB = DateTime.ParseExact(model.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                            Address = !string.IsNullOrEmpty(model.Address) ? model.Address : string.Empty,
                            Email = !string.IsNullOrEmpty(model.Email) ? model.Email : string.Empty,
                            Gender = model.Gender.Trim().ToLower() == "male" ? 1 : 2,
                            ImageName = "default.jpeg",
                            Name = !string.IsNullOrEmpty(model.Name) ? model.Name : string.Empty,
                            PhoneNumber = !string.IsNullOrEmpty(model.Phone) ? model.Phone : string.Empty,
                        };
                        return Ok(new { msg = "File has been uploaded.", entity = entity });
                    }
                }
                return BadRequest(new { msg = "Error: no records where added." });
            }
            catch (Exception e)
            {
                return BadRequest(new { msg = "Error:" + e.Message });
            }

        }


        public async Task<IActionResult> SubmitFile([FromBody] PhoneBookEntity phoneBookEntity)
        {
            await SaveToDB(phoneBookEntity);
            return Ok(new { msg = "Data has been saved." });
        }

        public async Task SaveToDB(PhoneBookEntity phoneBookEntity)
        {
            _context.PhoneBookEntity.Add(phoneBookEntity);
            await _context.SaveChangesAsync();
        }
    }
}
