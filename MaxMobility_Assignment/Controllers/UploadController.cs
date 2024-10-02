using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using MaxMobility_Assignment.Data;
using MaxMobility_Assignment.Models;
using MaxMobility_Assignment.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MaxMobility_Assignment.Controllers
{
    public class UploadController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UploadController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(UploadDataViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                TempData["StatusMessage"] = "Please select a file.";
                return View();
            }

            if (!model.File.FileName.EndsWith(".xlsx") && !model.File.FileName.EndsWith(".xls"))
            {
                TempData["StatusMessage"] = "Please upload a valid Excel file.";
                return View();
            }

            var uploadedDataList = new List<UploadDataViewModel>();

            using (var stream = new MemoryStream())
            {
                await model.File.CopyToAsync(stream);
                stream.Position = 0; // Reset the stream position to the beginning
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1); // Skip header row
                                                             // Check if there are any data rows (if the file is empty)
                    if (!rows.Any())
                    {
                        TempData["StatusMessage"] = "The file is empty. Please upload a file with data.";
                        return View();
                    }

                    foreach (var row in rows)
                    {
                        var name = row.Cell(2).GetString();
                        var email = row.Cell(3).GetString();
                        var phone = row.Cell(4).GetString();
                        var address = row.Cell(5).GetString();
                        var status = "Success";

                        //// Print the values before inserting them into the database
                        //Console.WriteLine($"Name: {name}, Email: {email}, Phone: {phone}, Address: {address}, Status: {status}");


                        // Validate email and fields
                        if (string.IsNullOrWhiteSpace(name) || !IsValidEmail(email) ||
                             !IsValidPhoneNumber(phone) || string.IsNullOrWhiteSpace(address))
                        {

                            // If validation fails, mark as Failed and skip the submission
                            status = "Failed";
                            // Print the values before inserting them into the database
                            //Console.WriteLine($"Name: {name}, Email: {email}, Phone: {phone}, Address: {address}, Status: {status}");
                        }
                        else
                        {
                            // If validation passes, proceed to insert into the database
                            await _context.InsertUploadedDataAsync(name, email, phone, address, status);
                        }

                        // Add the data to the list for display
                        uploadedDataList.Add(new UploadDataViewModel
                        {
                            Name = name,
                            Email = email,
                            PhoneNo = phone,
                            Address = address,
                            Status = status
                        });
                    }
                }
            }
            // Fetch all uploaded data from the database
           // var allUploadedData = await _context.UploadedDatas.ToListAsync();
            TempData["StatusMessage"] = "File uploaded and data processed successfully!";
            //TempData["UploadedData"] = uploadedDataList; // Save list for displaying
            TempData["DownloadLink"] = true; // Set link availability for download

            // Pass the uploaded data to the view
            return View(uploadedDataList); // Return the list to be dis


        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Check if it is exactly 10 digits and consists only of numbers
            return Regex.IsMatch(phoneNumber, @"^\d{10}$");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                // First, check the basic format using MailAddress
                var mail = new System.Net.Mail.MailAddress(email);

                // Second, validate using a regular expression for stricter email validation
                var emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(email, emailRegex))
                {
                    return false;
                }

                // Optional: Check if the domain looks reasonable (e.g., avoid unlikely domains)
                var domain = mail.Host.ToLower();
                if (domain.EndsWith(".com") || domain.EndsWith(".org") || domain.EndsWith(".net"))
                {
                    return mail.Address == email;
                }
                else
                {
                    return false; // If domain doesn't match typical TLDs, reject it
                }
            }
            catch
            {
                return false;
            }
        }

        public async Task<IActionResult> DownloadData()
        {
            try
            {
                // Retrieve uploaded data from the database
                var uploadedDataList = await _context.UploadedDatas.ToListAsync();

                if (!uploadedDataList.Any())
                {
                    TempData["StatusMessage"] = "No data available for download.";
                    return RedirectToAction("UploadFile"); // Redirect to your desired action
                }

                // Create a new Excel workbook
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Uploaded Data");

                    // Add headers with styling
                    var headerRow = worksheet.Row(1);
                    headerRow.Cell(1).Value = "Sr No";
                    headerRow.Cell(2).Value = "Name";
                    headerRow.Cell(3).Value = "Email";
                    headerRow.Cell(4).Value = "Phone No";
                    headerRow.Cell(5).Value = "Address";
                    headerRow.Cell(6).Value = "Status";

                    // Apply borders to the header cells
                    for (int col = 1; col <= 6; col++) // Only apply borders to the actual columns with data
                    {
                        headerRow.Cell(col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        headerRow.Cell(col).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }


                    // Add data with serial number
                    for (int i = 0; i < uploadedDataList.Count; i++)
                    {
                        var currentRow = worksheet.Row(i + 2);
                        currentRow.Cell(1).Value = i + 1; // Serial Number
                        currentRow.Cell(2).Value = uploadedDataList[i].Name;
                        currentRow.Cell(3).Value = uploadedDataList[i].Email;
                        currentRow.Cell(4).Value = uploadedDataList[i].Phone;
                        currentRow.Cell(5).Value = uploadedDataList[i].Address;
                        currentRow.Cell(6).Value = uploadedDataList[i].Status;

                        // Apply borders only to the cells containing data
                        for (int col = 1; col <= 6; col++) // Columns from Sr No to Status
                        {
                            currentRow.Cell(col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            currentRow.Cell(col).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        }
                    }

                    // Adjust column widths
                    worksheet.Columns().AdjustToContents();

                    // Save to a MemoryStream
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        stream.Position = 0; // Reset position for reading

                        // Return the file for download
                        var fileName = "UploadedData_Mahitosh.xlsx";
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error downloading data: {ex.Message}");
                TempData["StatusMessage"] = "An error occurred while downloading the data. Please try again.";
                return RedirectToAction("UploadFile"); // Redirect to your desired action
            }
        }
    }
}
