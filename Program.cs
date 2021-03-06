﻿using System;
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
                    Console.WriteLine("7) Display all products");
                    Console.WriteLine("8) Display specific product information");
                    Console.WriteLine("9) Edit Category");
                    Console.WriteLine("10) Delete Product or Category");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"Option {choice} selected");
                    if (choice == "1")
                    {  
                        // create database object
                        // order categories by category ID
                        var db = new NorthwindConsole_32_MJMContext();
                        var query = db.Categories.OrderBy(p => p.CategoryName);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{query.Count()} records returned");
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        // loops  through categories and displays name and description
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName} - {item.Description}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else if (choice == "2")
                    {
                        // create new category object
                        Category category = new Category();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();
                        // creates validation object
                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();
                        // validates the category 
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
                                Console.ForegroundColor = ConsoleColor.Green;
                                logger.Info("Validation passed");
                                Console.ForegroundColor = ConsoleColor.White;
                                // saves category to database
                                db.AddCategory(db, category);
                                logger.Info($"{category} added successfully");
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
                        // displays category and related product
                        var db = new NorthwindConsole_32_MJMContext();
                        // orders by category id
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
                        // displays only products that are active
                        foreach (Product p in category.Products.Where(p => p.Discontinued == false))
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
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Product p in item.Products.Where(p => p.Discontinued == false))
                            {
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                        Console.ForegroundColor = ConsoleColor.Green;
                        logger.Info($"{db.Categories.Count()} categories returned");
                        Console.ForegroundColor = ConsoleColor.White;
                        {
                            
                        };
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
                                Console.ForegroundColor = ConsoleColor.Green;
                                logger.Info("Validation passed");
                                Console.ForegroundColor = ConsoleColor.White;
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
                    else if(choice == "7"){
                        
                        var db = new NorthwindConsole_32_MJMContext();
                        Console.WriteLine("1) Display all products");
                        Console.WriteLine("2) Display all active products");
                        Console.WriteLine("3) Display all discontinued products");
                        string option = Console.ReadLine();
                        if(option == "1"){
                            var query = db.Products.Where(p => p.Discontinued == false || p.Discontinued == true);
                        foreach(Product p in query){
                            if(p.Discontinued == false){
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else{
                                Console.ForegroundColor = ConsoleColor.Red;
                            }
                            Console.WriteLine($"{p.ProductName}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        logger.Info($"\nDisplaying all products\n");
                    } else if(option == "2"){
                        var query = db.Products.Where(p => p.Discontinued == false);
                        logger.Info($"Option {option} selected");
                        foreach(Product p in query){                      
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{p.ProductName}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        logger.Info($"\nDisplaying active products\n");
                    } else if(option == "3"){
                        var query = db.Products.Where(p => p.Discontinued == true);
                        logger.Info($"Option {option} selected");
                        foreach(Product p in query){
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{p.ProductName}");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        logger.Info($"\nDisplaying discontinued products\n");
                    }
                        
                    }
                    else if(choice == "8"){
                        // displays all information based on selected product id
                        Console.WriteLine("Enter products ID to view information");
                        var db = new NorthwindConsole_32_MJMContext();
                        var product = GetProduct(db);
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine($"\nProduct Id: {product.ProductId}");
                        Console.WriteLine($"Product name: {product.ProductName}");
                        Console.WriteLine($"Supplier Id: {product.SupplierId}");
                        Console.WriteLine($"Category Id: {product.CategoryId}");
                        Console.WriteLine($"Quantity per unit: {product.QuantityPerUnit}");
                        Console.WriteLine($"Unit price: {product.UnitPrice}");
                        Console.WriteLine($"Units in stock: {product.UnitsInStock}");
                        Console.WriteLine($"Units on order: {product.UnitsOnOrder}");
                        Console.WriteLine($"Reorder level: {product.ReorderLevel}");
                        Console.WriteLine($"Discontinued: {product.Discontinued}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    // edit category option
                    else if(choice == "9"){
                        Console.WriteLine("Choose a category to edit");
                        var db = new NorthwindConsole_32_MJMContext();
                        var category = GetCategory(db);
                        // if category is not null, replace with new category
                        if(category != null){
                            Category updatedCategory = EditCategory(db);
                            if(updatedCategory != null){
                                updatedCategory.CategoryId = category.CategoryId;
                                db.EditCategory(updatedCategory);
                                updatedCategory.Products = category.Products;
                                logger.Info($"Category {category.CategoryId} updated");
                            }
                        }
                    }
                    // allows for selection of product or category to delete
                    else if(choice == "10"){
                        var db = new NorthwindConsole_32_MJMContext();
                        Console.WriteLine("Select if you would like to delete a Category or a Product");
                        Console.WriteLine("1) Delete Product");
                        Console.WriteLine("2) Delete Category");
                        string option = Console.ReadLine();
                        if(option == "1"){
                            var product = GetProduct(db);
                            if(product != null){
                                db.DeleteProduct(product);
                                Console.ForegroundColor = ConsoleColor.Green;
                                logger.Info($"Product {product.ProductName} deleted");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else{
                                logger.Error($"Invalid product id entered");
                            }
                        } else if(option == "2"){
                            var category = GetCategory(db);
                            if(category != null){
                                db.DeleteCategory(category);
                                Console.ForegroundColor = ConsoleColor.Green;
                                logger.Info($"Category: {category.CategoryName} deleted");
                                Console.ForegroundColor = ConsoleColor.White;
                            }
                            else{
                                logger.Error($"Invalid category id entered");
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
        // returns a product based on product id
        public static Product GetProduct(NorthwindConsole_32_MJMContext db){
            var products = db.Products.OrderBy(p => p.ProductId);
            foreach(Product p in products){
                Console.WriteLine($"{p.ProductId}: {p.ProductName}");
            }
            if(int.TryParse(Console.ReadLine(), out int ProductId)){
                Product product = db.Products.FirstOrDefault(p => p.ProductId == ProductId);
                if(product != null){
                    logger.Info($"Product Id: {ProductId} selected");
                    return product;
                }
            }
            logger.Error("Invalid Product Id");
            return null;
        }
        // creates new product object to use as parameter for editing
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
            // validates the object
            ValidationContext context = new ValidationContext(product, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(product, context, results, true);
            if(!isValid){
                // checks for unique name
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
        // returns category based on category id
        public static Category GetCategory(NorthwindConsole_32_MJMContext db){
            var categories = db.Categories.OrderBy(p => p.CategoryId);
            foreach(Category c in categories){
                Console.WriteLine($"{c.CategoryId}: {c.CategoryName} - {c.Description}");
            }
            if(int.TryParse(Console.ReadLine(), out int CategoryId)){
                Category category = db.Categories.FirstOrDefault(c => c.CategoryId == CategoryId);
                if(category != null){
                    logger.Info($"Category Id: {CategoryId} selected");
                    return category;
                }
            }
            logger.Error("Invalid Category Id");
            return null;
        }
        // creates new category object to be used as parameter for editing category
        public static Category EditCategory(NorthwindConsole_32_MJMContext db){
            Category category = new Category();
            Console.WriteLine("Enter Category name");
            category.CategoryName = Console.ReadLine();
            Console.WriteLine("Enter Category description");
            category.Description = Console.ReadLine();
            // validates category
            ValidationContext context = new ValidationContext(category, null, null);
            List<ValidationResult> results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(category, context, results, true);
            if(!isValid){
                // checks for unique category name
                if(db.Categories.Any(c => c.CategoryName == category.CategoryName)){
                    isValid = false;
                    results.Add(new ValidationResult("Category name exists", new string[] { "Name" }));
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
            return category;
            
        }
    }
}
