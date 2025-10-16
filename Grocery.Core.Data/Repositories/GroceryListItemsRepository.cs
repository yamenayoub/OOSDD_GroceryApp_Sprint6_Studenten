using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Grocery.Core.Data.Helpers;

using Microsoft.Data.Sqlite;
namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {

        public GroceryListItemsRepository()
        {
            // Create
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItems (
                Id INTEGER PRIMARY KEY,
                GroceryListId INTEGER,
                ProductId INTEGER,
                Amount INTEGER,
                FOREIGN KEY (GroceryListId) REFERENCES GroceryList(Id),
                FOREIGN KEY (ProductId) REFERENCES Products(Id)
            )");
            // Insert
            List<string> insertQueries = [
                @"INSERT OR IGNORE INTO GroceryListItems(Id, GroceryListId, ProductId, Amount) VALUES(1, 1, 1, 3)",
                @"INSERT OR IGNORE INTO GroceryListItems(Id, GroceryListId, ProductId, Amount) VALUES(2, 1, 2, 1)",
                @"INSERT OR IGNORE INTO GroceryListItems(Id, GroceryListId, ProductId, Amount) VALUES(3, 1, 3, 4)",
                @"INSERT OR IGNORE INTO GroceryListItems(Id, GroceryListId, ProductId, Amount) VALUES(4, 2, 1, 2)",
                @"INSERT OR IGNORE INTO GroceryListItems(Id, GroceryListId, ProductId, Amount) VALUES(5, 2, 2, 5)"
            ];

            InsertMultipleWithTransaction(insertQueries);

        }

        public List<GroceryListItem> GetAll()
        {
            List<GroceryListItem> groceryListItems = [];
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems";
            
            try
            {
                OpenConnection();
                using (SqliteCommand command = new(selectQuery, Connection))
                {
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int groceryListId = reader.GetInt32(1);
                        int productId = reader.GetInt32(2);
                        int amount = reader.GetInt32(3);
                        groceryListItems.Add(new(id, groceryListId, productId, amount));
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
            var query = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems WHERE GroceryListId = @GroceryListId";
            
            try
            {
                OpenConnection();
                using (SqliteCommand command = new(query, Connection))
                {
                    command.Parameters.AddWithValue("@GroceryListId", id);
                    SqliteDataReader reader = command.ExecuteReader();
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
            finally
            {
                CloseConnection();
            }
            
            return groceryListItems;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            int newId;
            string insertQuery = @"
                INSERT INTO GroceryListItems(GroceryListId, ProductId, Amount) 
                VALUES(@GroceryListId, @ProductId, @Amount);
                SELECT last_insert_rowid();";
            
            try
            {
                OpenConnection();
                using (SqliteCommand command = new(insertQuery, Connection))
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
            throw new NotImplementedException();
        }

        public GroceryListItem? Get(int id)
        {
            try
            {
                OpenConnection();
                using (SqliteCommand command = new("SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItems WHERE Id = @Id", Connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    SqliteDataReader reader = command.ExecuteReader();
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
            catch (Exception e)
            {
                Console.WriteLine($"Is er een fout opgetreden: {e.Message}");
            }
            finally
            {
                CloseConnection();
            }
            
            return null;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            string updateQuery = "UPDATE GroceryListItems SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = @Id";
            
            if (item != null)
            {
                try
                {
                    OpenConnection();
                    using (SqliteCommand command = new(updateQuery, Connection))
                    {
                        command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                        command.Parameters.AddWithValue("@ProductId", item.ProductId);
                        command.Parameters.AddWithValue("@Amount", item.Amount);
                        command.Parameters.AddWithValue("@Id", item.Id);
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return item;
                        }
                    }
                }
                finally
                {
                    CloseConnection();
                }
            }
            return null;
        }
    }
}
