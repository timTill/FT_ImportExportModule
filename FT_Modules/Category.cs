using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FT_Modules
{
	public class Category
	{
		[Key]
		public int Id { get; set; }

		[Display(Name = "Category Name")]
		[Required]
		public string Name { get; set; }
	}
}
