using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FT_Modules
{
	class Program
	{
		public const string XMLinPath = @"C:\Users\Szabolcs\Source\Repos\FoodTracker\FoodTracker\FoodTracker\Port\Data_in.xml";
		public const string XMLoutPath = @"C:\Users\Szabolcs\Source\Repos\FoodTracker\FoodTracker\FoodTracker\Port\Data_out.xml";

		public static PortDBViewModel ParseXML(string XMLPath)
		{
			PortDBViewModel DB_VM = new PortDBViewModel();

			XDocument doc = XDocument.Load(XMLPath);
			IEnumerable<Category> categories = from c in doc.Descendants("Category")
											   select new Category()
											   {
												   Id = (int)c.Element("CategoryID"),
												   Name = (string)c.Element("Name")
											   };

			IEnumerable<SubCategory> subCategories = from s in doc.Descendants("Subcategory")
													 select new SubCategory()
													 {
														 Id = (int)s.Element("SubcategoryID"),
														 Name = (string)s.Element("Name"),
														 CategoryId = (int)s.Element("CategoryID")
													 };

			IEnumerable<Food> Foods = from f in doc.Descendants("FoodItem")
									  select new Food()
									  {
										  ID = (int)f.Element("FoodID"),
										  Name = (string)f.Element("Name"),
										  Description = (string)f.Element("Description"),
										  CategoryId = (int)f.Element("CategoryID"),
										  SubCategoryId = (int)f.Element("SubcategoryId"),
										  BestBefore = string.IsNullOrEmpty((string)f.Element("BestBefore")) ? (DateTime?)null : (DateTime)f.Element("BestBefore"),
										  Unit = (int)f.Element("Unit"),
										  QuantityLeft = Enum.Parse<QuantityLeft>((string)f.Element("QuanityLeft")),
										  Measurement = Enum.Parse<MeasType>((string)f.Element("Measurement"))
									  };

			DB_VM.Categories = categories;
			DB_VM.Subcategories = subCategories;
			DB_VM.Foods = Foods;
			return DB_VM;
		}

		public static void stdOutXML(PortDBViewModel DB_VM)
		{
			Console.WriteLine("Kategóriák:\n");
			foreach (var c in DB_VM.Categories)
			{
				Console.WriteLine(c.Id + " " + c.Name);
			}

			Console.WriteLine("\nAlkategóriák:");
			foreach (var s in DB_VM.Subcategories)
			{
				Console.WriteLine(s.Id + " " + s.Name + " " + s.CategoryId);
			}

			Console.WriteLine("\nÉtelek:");
			foreach (var f in DB_VM.Foods)
			{
				Console.WriteLine(f.ID + " "
								+ f.Name + " "
								+ f.CategoryId + " "
								+ f.SubCategoryId + " "
								+ f.Description + " "
								+ f.BestBefore + " "
								+ f.Unit + " "
								+ f.QuantityLeft + " "
								+ f.Measurement);
			}
		}

		public static PortDBViewModel normalizeIDs(PortDBViewModel input)
		{
			//Initialize data counts
			int CategoryCount = input.Categories.Count();
			int SubCategoryCount = input.Subcategories.Count();
			int FoodItemsCount = input.Foods.Count();

			//data cloning for data modification
			List<Category> newCategories = input.Categories.ToList();
			List<SubCategory> newsubCategories = input.Subcategories.ToList();
			List<Food> newFoods = input.Foods.ToList();
			int newIndex = 1;


			//Distinct input values of Categ/Subcateg
			var OriginalCategoryIds = input.Categories.GroupBy(c => c.Id).ToList();
			var OriginalSubCategoryIds = input.Subcategories.GroupBy(s => s.Id).ToList();

			
			//Normalize Category.ID (1..2..)
			
			foreach (var categIDToChange in OriginalCategoryIds)
			{
				foreach (var category in newCategories)
				{
					if (category.Id == categIDToChange.Key)
					{
						category.Id = newIndex;
						newIndex++;
						break;
					}
				}
			}
			
			
			newIndex = 1;
			//Normalize Subcategory.ID (1..2..)
			foreach (var subcategIDToChange in OriginalSubCategoryIds)
			{
				foreach (var subcategory in newsubCategories)
				{
					if (subcategory.Id == subcategIDToChange.Key)
					{
						subcategory.Id = newIndex;
						newIndex++;
						break;
					}
				}
			}
			

			//Normalize subcategory.CategoryId
			for (int i = 0; i < CategoryCount; i++)
			{
				foreach (var subcategory in newsubCategories)
				{
					if (subcategory.CategoryId == OriginalCategoryIds[i].Key)
					{
						subcategory.CategoryId = newCategories[i].Id;						
					}
				}
			}

			//normalize the food.SubCategoryId
			for (int i = 0; i < SubCategoryCount; i++)
			{
				foreach (var food in newFoods)
				{
					if (food.SubCategoryId == OriginalSubCategoryIds[i].Key)
					{
						food.SubCategoryId = newsubCategories[i].Id;
					}
				}
			}

			//normalize the Food.Category
			for (int i = 0; i < CategoryCount; i++)
			{
				foreach (var food in newFoods)
				{
					if (food.CategoryId == OriginalCategoryIds[i].Key)
					{
						food.CategoryId = newCategories[i].Id;
						food.ID = 0;
					}
				}
			}

			//set all Category ID to 0 (EF can then insert it to DB)
			foreach (var item in newCategories)
			{
				item.Id = 0;
			}

			//set all Subcategory ID to 0 (EF can then insert it to DB)
			foreach (var item in newsubCategories)
			{
				item.Id = 0;
			}




			PortDBViewModel output = new PortDBViewModel
			{
				Categories = newCategories,
				Subcategories = newsubCategories,
				Foods = newFoods
			};
			return output;
		}

		public static void DumpDataToXML(PortDBViewModel DB_VM)
		{
			XDocument xmlDocument = new XDocument(
				new XDeclaration("1.0", "utf-8", "yes"),
				new XComment("Creating an XML Tree using LINQ to XML"),

				new XElement("Foods",
					new XElement("Categories",
						from category in DB_VM.Categories
						select new XElement("Category",
									new XElement("CategoryID", category.Id),
									new XElement("Name", category.Name)
									)),

					new XElement("Subcategories",
					from subcategory in DB_VM.Subcategories
					select new XElement("Subcategory",
								new XElement("SubcategoryID", subcategory.Id),
								new XElement("Name", subcategory.Name),
								new XElement("CategoryID", subcategory.CategoryId)
								)),

					new XElement("FoodItems",
					from foodItem in DB_VM.Foods
					select new XElement("FoodItem",
								new XElement("FoodID", foodItem.ID),
								new XElement("Name", foodItem.Name),
								new XElement("Description", foodItem.Description),
								new XElement("CategoryID", foodItem.CategoryId),
								new XElement("SubcategoryId", foodItem.SubCategoryId),
								new XElement("BestBefore", foodItem.BestBefore),
								new XElement("Unit", foodItem.Unit),
								new XElement("Measurement", foodItem.Measurement),
								new XElement("QuanityLeft", foodItem.QuantityLeft)
								))
							));
			xmlDocument.Save(XMLoutPath);
		}

		public static void ImportXML(string path)
		{

			PortDBViewModel ParsedXMLToDBWrite = ParseXML(path);
			List<Category> CategToAddToDB = new List<Category>();
			List<SubCategory> SubCategToAddToDB = new List<SubCategory>();
			List<Food> FoodToAddToDB = new List<Food>();

			/*foreach (var categitem in ParsedXMLToDBWrite.Categories.ToList())
			{
				Category catitem = new Category
				{
					Name = categitem.Name
				};
				CategToAddToDB.Add(catitem);
			}


			foreach (var subcategitem in ParsedXMLToDBWrite.Subcategories.ToList())
			{
				SubCategory subcatitem = new SubCategory
				{
					Name = subcategitem.Name,
					CategoryId = subcategitem.CategoryId
				};
				SubCategToAddToDB.Add(subcatitem);
			}


			foreach (var fooditem in ParsedXMLToDBWrite.Foods.ToList())
			{
				Food newFooditem = new Food
				{
					Name = fooditem.Name,
					Description = fooditem.Description,
					CategoryId = fooditem.CategoryId,
					SubCategoryId = fooditem.SubCategoryId,
					BestBefore = fooditem.BestBefore,
					Unit = fooditem.Unit,
					Measurement = fooditem.Measurement,
					QuantityLeft = fooditem.QuantityLeft
				};
				FoodToAddToDB.Add(newFooditem);
			}
			*/
		}
		public static void AddCategoryToDBTest()
		{
			using (var context = new ApplicationDbContext())
			{
				var test = new Category()
				{
					Name = "Kiskalap"
				};
				context.Category.Add(test);
				context.SaveChanges();
			}
		}

		public static void AddSubCategoryToDBTest()
		{
			using (var context = new ApplicationDbContext())
			{
				var test = new SubCategory()
				{
					Name = "Alkat",
					CategoryId = 1

				};
				context.SubCategory.Add(test);
				context.SaveChanges();
			}
		}

		public static void Add1FoodItemToDBTest()
		{
			using (var context = new ApplicationDbContext())
			{
				var test = new Food()
				{
					Name = "Alkat",
					CategoryId=1,
					SubCategoryId=1,
					
				
				};
				context.Foods.Add(test);
				context.SaveChanges();
			}
		}

		public static void AddManyFoodItemToDBTest()
		{
			using (var context = new ApplicationDbContext())
			{
				var test1 = new Food()
				{
					Name = "test1bol",
					CategoryId = 1,
					SubCategoryId = 1,
				};

				var test2 = new Food()
				{
					Name = "test2bol",
					CategoryId = 2,
					SubCategoryId = 1,
				};

				List<Food> fList = new List<Food>()
				{
					test1,test2
				};

				context.Foods.AddRange(fList);
				//context.Foods.Add(test1);
				context.SaveChanges();
			}
		}

		public static void AddManyCategoriesFromParamToDBTest(IEnumerable<Category> categList)
		{
			using (var context = new ApplicationDbContext())
			{
				context.Category.AddRange(categList);
				//context.Foods.Add(test1);
				context.SaveChanges();
			}
		}

		public static void AddManySubCategoriesFromParamToDBTest(IEnumerable<SubCategory> subcategList)
		{
			using (var context = new ApplicationDbContext())
			{
				context.SubCategory.AddRange(subcategList);
				//context.Foods.Add(test1);
				context.SaveChanges();
			}
		}


		public static void AddManyFoodItemFromParamToDBTest(IEnumerable<Food> foodList)
		{
			using (var context = new ApplicationDbContext())
			{
			

				context.Foods.AddRange(foodList);
				//context.Foods.Add(test1);
				context.SaveChanges();
			}
		}


		public static void Import()
		{

		}		

		static void Main(string[] args)
		{
			
			PortDBViewModel originalParsedXML = ParseXML(XMLinPath);
			stdOutXML(originalParsedXML);
			PortDBViewModel normalizedXml = normalizeIDs(originalParsedXML);
			stdOutXML(normalizedXml);
			//DumpDataToXML(normalizedXml);

			/*
			AddManyCategoriesFromParamToDBTest(normalizedXml.Categories);
			AddManySubCategoriesFromParamToDBTest(normalizedXml.Subcategories);
			AddManyFoodItemFromParamToDBTest(normalizedXml.Foods);
			*/

			Console.ReadKey();	

			//AddCategoryToDBTest();
			//AddSubCategoryToDBTest();
			//Add1FoodItemToDBTest();
			//AddManyFoodItemToDBTest();

		}
	}
}
