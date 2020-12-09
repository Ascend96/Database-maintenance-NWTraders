using System;
using NLog.Web;
using System.IO;
using System.Linq;
using NorthwindConsole.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NorthwindConsole
{
    class Program
    {
         // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Add New Records to Products");
                    Console.WriteLine("6) Edit a specific record from Products");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {
                        var db = new NorthwindConsole_32_MJMContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        Category category = new Category();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();
                        
                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                             var db = new NorthwindConsole_32_MJMContext();
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                // TODO: save category to db
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if (choice == "3")
                    {
                        var db = new NorthwindConsole_32_MJMContext();
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");
                        Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");
                                                foreach (Product p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    
                    else if (choice == "4")
                    {
                        var db = new NorthwindConsole_32_MJMContext();
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Product p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }
                    else if(choice == "5")
                    {
                        Product product = new Product();
                        Console.WriteLine("Enter Product Name");
                        product.ProductName = Console.ReadLine();
                        Console.WriteLine("Enter SupplierID");
                        product.SupplierId = Int32.Parse(Console.ReadLine());
                        Console.WriteLine("Enter CategoryID");
                        product.CategoryId = Int32.Parse(Console.ReadLine());
                        Console.WriteLine("Enter Quantity Per Unit");
                        product.QuantityPerUnit = Console.ReadLine();
                        Console.WriteLine("Enter Price Per Unit");
                        product.UnitPrice = decimal.Parse(Console.ReadLine());
                        Console.WriteLine("Enter how many units in stock");
                        product.UnitsInStock = Int16.Parse(Console.ReadLine());
                        Console.WriteLine("Enter how many units are on order");
                        product.UnitsOnOrder = Int16.Parse(Console.ReadLine());
                        Console.WriteLine("Enter Reorder Level");
                        product.ReorderLevel = Int16.Parse(Console.ReadLine());
                        string option;
                        Console.WriteLine("Enter if the product is discontinued or not (1 = True | 0 = False)");
                        option = Console.ReadLine();
                        if(option == "1"){
                            product.Discontinued = true;
                        } else if(option == "0"){
                            product.Discontinued = false;
                        }
                        logger.Info($"Product discontinued set to {product.Discontinued}");
                        
                        ValidationContext context = new ValidationContext(product, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(product, context, results, true);
                        if (isValid)
                        {
                             var db = new NorthwindConsole_32_MJMContext();
                            // check for unique product based on supplier and product name
                            if (db.Products.Any(p => p.ProductName == product.ProductName && p.SupplierId == product.SupplierId))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name exists from that supplier", new string[] { "Product Name, Supplier id" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                
                                db.AddProducts(db, product);

                                logger.Info($"Product: {product.ProductName} has been added");
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    else if(choice == "6"){
                        Console.WriteLine("Choose a product to edit");
                        var db = new NorthwindConsole_32_MJMContext();
                        var product = GetProduct(db);
                        if(product != null){
                            Product updatedProduct = EditProduct(db);
                            if(updatedProduct != null){
                                updatedProduct.ProductId = product.ProductId;
                                db.EditProduct(updatedProduct);
                                logger.Info($"Product {product.ProductId} updated");
                            } 
                        }

                        
                    }
                    Console.WriteLine();

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }
        public static Product GetProduct(NorthwindConsole_32_MJMContext db){
            var products = db.Products.OrderBy(p => p.ProductId);
            foreach(Product p in products){
                Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }
            if(int.TryParse(Console.ReadLine(), out int ProductId)){
                Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
                if(product != null){
                    return product;
                }
            }
            logger.Error("Invalid Product Id");
            return null;
        }
        public static Product EditProduct(NorthwindConsole_32_MJMContext db){
            Product product = new Product();
            Console.WriteLine("Enter Product name");
            product.ProductName = Console.ReadLine();
            Console.WriteLine("Enter SupplierID");
            product.SupplierId = Int32.Parse(Console.ReadLine());
            Console.WriteLine("Enter CategoryID");
            product.CategoryId = Int32.Parse(Console.ReadLine());
            Console.WriteLine("Enter Quantity Per Unit");
            product.QuantityPerUnit = Console.ReadLine();
            Console.WriteLine("Enter Price Per Unit");
            product.UnitPrice = decimal.Parse(Console.ReadLine());
            Console.WriteLine("Enter how many units in stock");
            product.UnitsInStock = Int16.Parse(Console.ReadLine());
            Console.WriteLine("Enter how many units are on order");
            product.UnitsOnOrder = Int16.Parse(Console.ReadLine());
            Console.WriteLine("Enter Reorder Level");
            product.ReorderLevel = Int16.Parse(Console.ReadLine());
            string option;
            Console.WriteLine("Enter if the product is discontinued or not (1 = True | 0 = False)");
            option = Console.ReadLine();
            if(option == "1"){
            product.Discontinued = true;
            } else if(option == "0"){
            product.Discontinued = false;
            }
            logger.Info($"Product discontinued set to {product.Discontinued}");
            
            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if(!isValid){

                if(db.Products.Any(p => p.ProductName == product.ProductName)){
                    isValid = false;
                    results.Add(new ValidationResult("Product name exists", new string[] { "Name" }));
                }
                else{
                    logger.Info("Validation passed");

                }
            }
            else if(!isValid){
                foreach(var result in results){
                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                }
                return null;
            }
            return product;

        }
    }
}
