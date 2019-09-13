using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace FT_Modules
{
	public class ApplicationDbContext : DbContext
	{
		/*public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)*/
		public ApplicationDbContext()
		{			
			
		}
		public DbSet<SubCategory> SubCategory { get; set; }
		public DbSet<Category> Category { get; set; }
		public DbSet<Food> Foods { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Data Source=localhost\\SZABSQL;Initial Catalog=FT_Modules_Test;Persist Security Info=True;User ID=szabi;Password=sqlpassword");
		}



	}
}
