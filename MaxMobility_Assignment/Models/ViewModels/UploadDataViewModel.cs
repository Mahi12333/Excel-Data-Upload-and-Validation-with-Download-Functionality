using Microsoft.AspNetCore.Http;

namespace MaxMobility_Assignment.Models.ViewModels
{
    public class UploadDataViewModel
    {
        public IFormFile File { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }
}
