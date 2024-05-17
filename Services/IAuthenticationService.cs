using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using Workflows.Models;

namespace Workflows.Services
{
    public interface IAuthenticationService
    {
        Task<bool> AuthenticateAsync(string payrollNo, string password);
    }
    // Authentication service implementation
    public class AuthenticationService : IAuthenticationService
    {
        private readonly KtdaleaveContext _context;

        public AuthenticationService(KtdaleaveContext context)
        {
            _context = context;
        }

        public async Task<bool> AuthenticateAsync(string payrollNo, string password)
        {
            // Log the values of payrollNo and password
            Console.WriteLine($"AuthenticationService received payrollNo: {payrollNo}, password: {password}");
            var employee = await _context.EmployeeBkps.SingleOrDefaultAsync(e => e.PayrollNo == payrollNo);
            if (employee == null)
            {
                return false; // User not found
            }

            // Validate password
            if (!VerifyPasswordHash(password, employee.Pass))
            {
                return false; // Incorrect password
            }

            return true; // Authentication successful
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash)
        {
            // Implement your password hash verification logic here
            // Example: use a library like BCrypt.Net to compare hashes
            // Example: return BCrypt.Net.BCrypt.Verify(password, storedHash);

            // For demonstration purposes, assuming password comparison logic here
            // This is NOT secure, use a proper hashing algorithm in production
            using (var md5 = MD5.Create())
            {
                byte[] passwordHash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Compare the computed hash with the stored hash
                return storedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
