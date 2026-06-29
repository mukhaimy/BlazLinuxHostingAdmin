using Blaz9LinuxHostingAdmin.Models.Dbases;
using Microsoft.EntityFrameworkCore;

namespace Blaz9LinuxHostingAdmin.Data
{
    public class MainDbContext : DbContext
    {
        public MainDbContext()
        {
        }

        public MainDbContext(DbContextOptions<MainDbContext> options)
            : base(options)
        {
        }


        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    {
        //        // base.OnConfiguring(optionsBuilder);

        //        var builderConnStr = new ConfigurationBuilder()
        //           .SetBasePath(Directory.GetCurrentDirectory())
        //           .AddJsonFile("appsettings.json", optional: false);

        //        IConfiguration configBase = builderConnStr.Build();

        //        string dbConnstr = configBase.GetValue<string>("TheAppSetting:MainDbConnection") ?? throw new InvalidOperationException("TheAppSetting:MainDbConnection Value not found.");

        //        optionsBuilder.UseMySql(dbConnstr);
        //    }
        //}

        #region DbSet
        public DbSet<TheDemo> TheDemoSet { get; set; }


        #endregion
    }
}
