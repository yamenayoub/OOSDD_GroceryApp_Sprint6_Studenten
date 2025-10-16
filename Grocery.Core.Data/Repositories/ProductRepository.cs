using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;
using Grocery.Core.Data.Helpers;
using System.Data;
using System.Diagnostics;


namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection ,IProductRepository
    {
        public ProductRepository()
        {
            CreateTable("CREATE TABLE IF NOT EXISTS Products (Id INTEGER PRIMARY KEY, Name TEXT, Stock INTEGER, ShelfLife DATE, Price REAL)");

            List<string> listInsertQuries = new List<string>
            {
                @"INSERT OR IGNORE INTO Products (Id, Name, Stock, ShelfLife, Price) VALUES (1, 'Melk', 300, '2025-09-25', 0.95);",
                @"INSERT OR IGNORE INTO Products (Id, Name, Stock, ShelfLife, Price) VALUES (2, 'Kaas', 100, '2025-09-30', 7.98);",
                @"INSERT OR IGNORE INTO Products (Id, Name, Stock, ShelfLife, Price) VALUES (3, 'Brood', 400, '2025-09-12', 2.19);",
                @"INSERT OR IGNORE INTO Products (Id, Name, Stock, ShelfLife, Price) VALUES (4, 'Cornflakes', 0, '2025-12-31', 1.48);"
            };

            InsertMultipleWithTransaction(listInsertQuries);
        }
        
        public List<Product> GetAll()
        {
            var products = new List<Product>();
            
            try
            {
                OpenConnection();
                using (SqliteCommand command = new ("SELECT Id, Name, Stock, ShelfLife, Price FROM Products;", Connection))
                {
                    var results = command.ExecuteReader();

                    while (results.Read())
                    {
                        int id = results.GetInt32(0);
                        string name = results.GetString(1);
                        int stock = results.GetInt32(2);
                        DateOnly shelfLife = DateOnly.FromDateTime(results.GetDateTime(3));
                        decimal price = results.GetDecimal(4);
                        products.Add(new Product(id, name, stock, shelfLife, price));
                        Console.WriteLine(results);
                    }
                }
            }
            finally
            {
                CloseConnection();
            }
            
            return products;
        }

        public Product? Get(int id)
        {
            try
            {
                OpenConnection();
                using (SqliteCommand command = new ("SELECT Id, Name, Stock, ShelfLife, Price FROM Products WHERE Id = @Id;", Connection))
                {
                    command.Parameters.AddWithValue("Id", id);
                    var reader = command.ExecuteReader();
                    if (!reader.Read())
                    {
                        return null;
                    }

                    int Id = reader.GetInt32(0);
                    string Name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly shelfLife = DateOnly.Parse(reader.GetString(3));
                    decimal price = reader.GetDecimal(4);
                    return new Product(Id, Name, stock, shelfLife, price);
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public Product Add(Product item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item), "Product item cannot be null.");
            }

            int newId;
            string insertQuery = @"
                INSERT INTO Products (Name, Stock, ShelfLife, Price) 
                VALUES (@Name, @Stock, @ShelfLife, @Price);
                SELECT last_insert_rowid();";

            try
            {
                OpenConnection();
                using (SqliteCommand command = new(insertQuery, Connection))
                {
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Stock", item.Stock);
                    command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Price", item.Price);

                    var result = command.ExecuteScalar();
                    newId = Convert.ToInt32(result);
                }
            }
            finally
            {
                CloseConnection();
            }

            return new Product(newId, item.Name, item.Stock, item.ShelfLife, item.Price);
        }

        public Product? Delete(Product item)
        {
            throw new NotImplementedException();
        }

        public Product? Update(Product item)
        {
            try
            {
                OpenConnection();
                using (SqliteCommand command = new ("UPDATE Products SET Name = @Name, Stock = @Stock, ShelfLife = @ShelfLife, Price = @Price WHERE Id = @Id", Connection))
                {
                    command.Parameters.AddWithValue("@Id", item.Id);
                    command.Parameters.AddWithValue("@Name", item.Name);
                    command.Parameters.AddWithValue("@Stock", item.Stock);
                    command.Parameters.AddWithValue("@ShelfLife", item.ShelfLife.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Price", item.Price);
                    if (command.ExecuteNonQuery() == 0) return null;
                    return item;
                }
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}
