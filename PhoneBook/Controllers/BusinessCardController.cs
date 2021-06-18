using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CsvHelper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using PhoneBook.Data;
using PhoneBook.Models;

namespace PhoneBook.Controllers
{
    public class BusinessCardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IWebHostEnvironment _hostingEnvironment { get; }
        private string ImgPath = null;
        private string filePath = null;


        public BusinessCardController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            ImgPath = Path.Combine(_hostingEnvironment.WebRootPath, "imgs");
            filePath = Path.Combine(_hostingEnvironment.WebRootPath, "files");
        }

        public async Task<IActionResult> Index()
        {
            var allBC = await _context.PhoneBookEntity.ToListAsync();

            return View(allBC);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhoneBookViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imgName = null;

                if (model.ImageName != null)
                {
                    IOExtension.CopyFileToPath(ImgPath, model.ImageName);
                    imgName = model.ImageName.FileName;
                }


                var entity = new PhoneBookEntity()
                {
                    ImageName = imgName,
                    DOB = DateTime.ParseExact(model.DOB, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                    Address = model.Address,
                    Email = model.Email,
                    Gender = model.Gender,
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber
                };


                _context.PhoneBookEntity.Add(entity);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "BusinessCard");
            }
            return View(model);
        }

        public async Task<IActionResult> Remove(int id)
        {
            var entityInDb = await _context.PhoneBookEntity.FindAsync(id);
            if (entityInDb != null)
            {
                _context.PhoneBookEntity.Remove(entityInDb);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "BusinessCard");
        }

        public async Task<IActionResult> Export(int id, string type)
        {
            try
            {
                string exportedFilePath = string.Empty;
                if (id > 0)
                {
                    var entityInDb = await _context.PhoneBookEntity.FindAsync(id);
                    if (entityInDb != null)
                    {
                        var base64Image = IOExtension.ConvertToBase64(entityInDb.ImageName, ImgPath);
                        var entityToExport = new PhoneBookExportViewModel()
                        {
                            DOB = entityInDb.DOB.ToString(),
                            Address = entityInDb.Address,
                            Email = entityInDb.Email,
                            Gender = entityInDb.Gender == 1 ? "Male" : "Female",
                            Name = entityInDb.Name,
                            Phone = entityInDb.PhoneNumber,
                            Photo = base64Image
                        };

                        if (type.Trim().ToLower() == "csv")
                        {
                            var outputFilePath = filePath + "\\" + entityInDb.Name.Replace(" ", "-") + ".csv";
                            using (StreamWriter sw = new StreamWriter(outputFilePath, false, new UTF8Encoding(true)))
                            using (CsvWriter cw = new CsvWriter(sw, System.Globalization.CultureInfo.InvariantCulture))
                            {
                                cw.WriteHeader<PhoneBookExportViewModel>();
                                await cw.NextRecordAsync();
                                cw.WriteRecord<PhoneBookExportViewModel>(entityToExport);
                                await cw.NextRecordAsync();
                                exportedFilePath = outputFilePath;
                                return PhysicalFile(exportedFilePath, MimeTypes.GetMimeType(exportedFilePath), Path.GetFileName(exportedFilePath));
                            }
                        }
                        else if (type.Trim().ToLower() == "xml")
                        {
                            var outputFilePath = filePath + "\\" + entityInDb.Name.Replace(" ", "-") + ".xml";

                            XmlDocument xmlDoc = new XmlDocument();
                            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                            XmlElement root = xmlDoc.DocumentElement;
                            xmlDoc.InsertBefore(xmlDeclaration, root);



                            XmlElement body = xmlDoc.CreateElement(string.Empty, "BusinessCard", string.Empty);
                            xmlDoc.AppendChild(body);

                            XmlNode nameKeyNode = xmlDoc.CreateElement("Name");
                            nameKeyNode.InnerText = entityToExport.Name;
                            body.AppendChild(nameKeyNode);

                            XmlNode genderKeyNode = xmlDoc.CreateElement("Gender");
                            genderKeyNode.InnerText = entityToExport.Gender;
                            body.AppendChild(genderKeyNode);

                            XmlNode DOBKeyNode = xmlDoc.CreateElement("DOB");
                            DOBKeyNode.InnerText = entityToExport.DOB;
                            body.AppendChild(DOBKeyNode);

                            XmlNode EmailKeyNode = xmlDoc.CreateElement("Email");
                            EmailKeyNode.InnerText = entityToExport.Email;
                            body.AppendChild(EmailKeyNode);

                            XmlNode PhoneKeyNode = xmlDoc.CreateElement("Phone");
                            PhoneKeyNode.InnerText = entityToExport.Phone;
                            body.AppendChild(PhoneKeyNode);


                            XmlNode PhotoKeyNode = xmlDoc.CreateElement("Photo");
                            PhotoKeyNode.InnerText = base64Image;
                            body.AppendChild(PhotoKeyNode);
                            xmlDoc.Save(outputFilePath);
                            exportedFilePath = outputFilePath;
                            return PhysicalFile(exportedFilePath, MimeTypes.GetMimeType(exportedFilePath), Path.GetFileName(exportedFilePath));
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return View();
        }

    }
}
