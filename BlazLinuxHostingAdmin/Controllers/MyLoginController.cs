using BlazLinuxHostingAdmin.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace BlazLinuxHostingAdmin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyLoginController : ControllerBase
    {
        private UserManager<ApplicationUser> userManager;
        private RoleManager<IdentityRole> roleManager;
        private IWebHostEnvironment tEnvironment;

        // private readonly MainDbContext _context;

        public MyLoginController(UserManager<ApplicationUser> userMgr, RoleManager<IdentityRole> roleMgr, IWebHostEnvironment environment)//, MainDbContext context)
        {
            userManager = userMgr;
            roleManager = roleMgr;
            tEnvironment = environment;

            //_context = context;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            string hsl = "?APA";
            hsl = await Phase1();
            //hsl += "--!!---!!---!!---!!---!!--";
            //hsl += await Phase2();
            //hsl = await Phase3();

            return hsl;
        }

        private async Task<string> Phase1()
        {
            string kode = "";

            var roleSuperAdmin = new IdentityRole { Name = "SuperAdmin" };

            IdentityResult exc;
            exc = await roleManager.CreateAsync(roleSuperAdmin);
            kode += exc.Succeeded ? "rSA1" : "0";


            #region Super Admin
            ApplicationUser userSuperAdmin = new ApplicationUser
            {
                PhoneNumberConfirmed = true,
                UserName = "the.admin",
                Email = "admin@albaik.id",
                RealName = "Super Admin",
                EmailConfirmed = true,
                RegisterDate = DateTime.Now,
            };

            exc = await userManager.CreateAsync(userSuperAdmin, "13WindRoun)-P@ss");
            kode += exc.Succeeded ? "uAL" : "0";

            exc = await userManager.AddToRoleAsync(userSuperAdmin, roleSuperAdmin.Name);
            kode += exc.Succeeded ? "urAL" : "0";

            #endregion


            return kode;
        }


    }
}
