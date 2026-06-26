using Microsoft.AspNetCore.Identity;

namespace BlazLinuxHostingAdmin.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public DateTime RegisterDate { get; set; }

        public string RealName { get; set; } = string.Empty;

        public string? MainRole { get; set; }
    }

}
