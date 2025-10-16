using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Grocery.Core.Data.Helpers;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        public GroceryListItemsRepository()
        {
            // Create table if it doesn't exist
            CreateTable(@"
                CREATE TABLE IF NOT EXISTS GroceryListItems (
                    Id INTEGER PRIMARY KEY,
                    GroceryListId INTEGER,
                    ProductId INTEGER,
                    Amount INTEGER,
                    FOREIGN KEY (GroceryListId) REFERENCES GroceryLists(Id),
                    FOREIGN KEY (ProductId) REFERENCES Products(Id)
                )");

            // Insert seed data (if not present)
            List<string> insertQueries = new List<string>
            {
                @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(1, 1, 3)",
                @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(1, 2, 1)",
                @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(1, 3, 4)",
                @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(2, 1, 2)",
                @"INSERT OR IGNORE INTO GroceryListItems(GroceryListId, ProductId, Amount) VALUES(2, 2, 5)"
            };

            InsertMultipleWithTransaction(insertQueries);
        }

        public List<GroceryListItem> GetAll()
        {
            var groceryListItems = new List<GroceryListItem>();
            const string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems";

            try
            {
                OpenConnection();
                using (var command = new SqliteCommand(selectQuery, Connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int groceryListId = reader.GetInt32(1);
                        int productId = reader.GetInt32(2);
                        int amount = reader.GetInt32(3);
                        groceryListItems.Add(new GroceryListItem(id, groceryListId, productId, amount));
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            var groceryListItems = new List<GroceryListItem>();
            const string query = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems WHERE GroceryListId = @GroceryListId";

            try
            {
                OpenConnection();
                using (var command = new SqliteCommand(query, Connection))
                {
                    command.Parameters.AddWithValue("@GroceryListId", id);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int itemId = reader.GetInt32(0);
                            int groceryListId = reader.GetInt32(1);
                            int productId = reader.GetInt32(2);
                            int amount = reader.GetInt32(3);
                            groceryListItems.Add(new GroceryListItem(itemId, groceryListId, productId, amount));
                        }
                    }
                }
            }
            finally
            {
                CloseConnection();
            }

            return groceryListItems;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            int newId;
            // Use last_insert_rowid() after insert to obtain the generated id reliably
            string insertQuery = @"
                INSERT INTO GroceryListItems(GroceryListId, ProductId, Amount)
                VALUES(@GroceryListId, @ProductId, @Amount);
                SELECT last_insert_rowid();";

            try
            {
                OpenConnection();
                using (var command = new SqliteCommand(insertQuery, Connection))
                {
                    command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                    command.Parameters.AddWithValue("@ProductId", item.ProductId);
                    command.Parameters.AddWithValue("@Amount", item.Amount);

                    var result = command.ExecuteScalar();
                    newId = Convert.ToInt32(result);
                }
            }
            finally
            {
                CloseConnection();
            }

            return new GroceryListItem(newId, item.GroceryListId, item.ProductId, item.Amount);
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            if (item == null) return null;

            const string deleteQuery = "DELETE FROM GroceryListItems WHERE Id = @Id";

            try
            {
                OpenConnection();
                using (var command = new SqliteCommand(deleteQuery, Connection))
                {
                    command.Parameters.AddWithValue("@Id", item.Id);
                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? item : null;
                }
            }
            finally
            {
                CloseConnection();
            }
        }

        public GroceryListItem? Get(int id)
        {
            const string query = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems WHERE Id = @Id";

            try
            {
                OpenConnection();
                using (var command = new SqliteCommand(query, Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int itemId = reader.GetInt32(0);
                            int groceryListId = reader.GetInt32(1);
                            int productId = reader.GetInt32(2);
                            int amount = reader.GetInt32(3);
                            return new GroceryListItem(itemId, groceryListId, productId, amount);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
            finally
            {
                CloseConnection();
            }

            return null;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            if (item == null) return null;

            const string updateQuery = "UPDATE GroceryListItems SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = @Id";

            try
            {
                OpenConnection();
                using (var command = new SqliteCommand(updateQuery, Connection))
                {
                    command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                    command.Parameters.AddWithValue("@ProductId", item.ProductId);
                    command.Parameters.AddWithValue("@Amount", item.Amount);
                    command.Parameters.AddWithValue("@Id", item.Id);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0 ? item : null;
                }
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}