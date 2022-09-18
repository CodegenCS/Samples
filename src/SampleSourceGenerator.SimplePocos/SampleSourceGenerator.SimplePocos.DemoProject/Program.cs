using System;
using Dapper;
using MyProject.POCOs; // POCOs will be generated (on-the-fly!) under this namespace

namespace SampleSourceGenerator.Demo
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // This is the magic. The POCOs are not declared anywhere: they are generated on the fly!
            // If you debug and step-into the next line you'll see the generated POCOs.

            Product product = new Product() { Name = "Tesla Model S", ListPrice = 79990 };

            using (var cn = IDbConnectionFactory.CreateConnection())
            {
                var products = cn.Query<Product>("SELECT * FROM Production.Product");
                foreach (var p in products)
                    Console.WriteLine($"I'll buy a {p.Name} for {p.ListPrice:C}");
            }

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
