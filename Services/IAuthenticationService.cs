using Microsoft.Data.SqlClient;
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
            // Using stored procedure
            var parameters = new[]
        {
            new SqlParameter("@username", payrollNo),
            new SqlParameter("@pass_word", password)
        };

            var result = await _context.PasswordCheckResults
        .FromSqlRaw("EXEC Pro_password @username, @pass_word", parameters)
        .ToListAsync();

            // Check the `passer` column in the result set
            if (result.Count > 0)
            {
                var passer = result.First().Passer;
                if (passer == password)
                {
                    return true;
                }
            }

            return false;
        }

        /* Using MD5 hash
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
        */
    }


    /*
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

                // Log the values of passwordHash and storedHash
                Console.WriteLine("Computed password hash: " + BitConverter.ToString(passwordHash).Replace("-", ""));
                Console.WriteLine("Stored hash: " + BitConverter.ToString(storedHash).Replace("-", ""));

                bool matches = storedHash.SequenceEqual(passwordHash);
                Console.WriteLine("Password matches: " + matches);

                return matches;
                // Compare the computed hash with the stored hash
                return storedHash.SequenceEqual(passwordHash);
            }
        }
            */
    
}
