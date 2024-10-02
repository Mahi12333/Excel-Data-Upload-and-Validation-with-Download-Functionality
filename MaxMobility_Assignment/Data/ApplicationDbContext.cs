using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using MaxMobility_Assignment.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MaxMobility_Assignment.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<UploadedData> UploadedDatas { get; set; }

        public async Task<int> InsertUploadedDataAsync(string name, string email, string phone, string address, string status)
        {
            var nameParam = new SqlParameter("@Name", name);
            var emailParam = new SqlParameter("@Email", email);
            var phoneParam = new SqlParameter("@Phone", phone);
            var addressParam = new SqlParameter("@Address", address);
            var statusParam = new SqlParameter("@Status", status);

            return await Database.ExecuteSqlRawAsync("EXEC dbo.InsertUploadedData @Name, @Email, @Phone, @Address, @Status",
    nameParam, emailParam, phoneParam, addressParam, statusParam);

        }
    }
}
