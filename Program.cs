using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace ado.net
{
     public class Program
     {
          private static string connectionString =
               ConfigurationManager.ConnectionStrings["sqlconnection"].ConnectionString;

          public static DataTable Cars;
          public static DataTable Owners;
          public static SqlDataAdapter CarsDataAdapter;
          public static SqlDataAdapter OwnersDataAdapter;

          static void Main(string[] args)
          {
               //CreateTables();
               Cars = CreateCarsDataTable();
               Owners = CreateOwnersDataTable();
               CarsDataAdapter = GetCarsAdapter();
               CarsDataAdapter.Fill(Cars);

               foreach (DataRow row in Cars.Rows)
               {
                    Console.WriteLine($"{row["id"]} {row["brand"]} {row["model"]}");
               }



               //  DataSet dataSet = new DataSet("muDb");
               // dataSet.Tables.Add("Cars");
               // dataSet.Tables.Add("Owners");
               // dataSet.Relations.Add("carOwner",
               //      dataSet.Tables["Cars"].Columns["id"],
               //      dataSet.Tables["Owners"].Columns["carId"]);


               //OwnersDataAdapter = new SqlDataAdapter("SELECT * FROM Owners", connectionString);

               UpdateRowInCarsTable(new Car() { Brand = "Mercedes", Model = "E300" }, 2);
               InsertRowInCarsTable(new Car
               {
                    Brand = "Toyota",
                    Model = "Auris"
               });

               foreach (DataRow row in Cars.Rows)
               {
                    Console.WriteLine($"{row["id"]} {row["brand"]} {row["model"]}");
               }

               DeleteRowInCarsTable(2);
               foreach (DataRow row in Cars.Rows)
               {
                    Console.WriteLine($"{row["id"]} {row["brand"]} {row["model"]}");
               }
               Console.ReadLine();
          }

          private static SqlDataAdapter GetCarsAdapter()
          {
               SqlDataAdapter tempAdapter = new SqlDataAdapter("SELECT * FROM Cars;", connectionString);

               //insert
               tempAdapter.InsertCommand = new SqlCommand("INSERT INTO Cars(brand,model) values(@Brand, @Model)");
               tempAdapter.InsertCommand.Parameters.Add("@Brand", SqlDbType.NVarChar, 255, "brand");
               tempAdapter.InsertCommand.Parameters.Add("@Model", SqlDbType.NVarChar, 255, "model");

               //update
               tempAdapter.UpdateCommand = new SqlCommand("UPDATE Cars SET brand=@Brand, model=@Model where id=@Id");
               tempAdapter.UpdateCommand.Parameters.Add("@Brand", SqlDbType.NVarChar, 255, "brand");
               tempAdapter.UpdateCommand.Parameters.Add("@Model", SqlDbType.NVarChar, 255, "model");
               SqlParameter updateParameter = tempAdapter.UpdateCommand.Parameters.Add("@Id", SqlDbType.Int);
               updateParameter.SourceColumn = "id";
               updateParameter.SourceVersion = DataRowVersion.Original;

               //delete
               tempAdapter.DeleteCommand = new SqlCommand("Delete from Cars where id=@Id");
               SqlParameter deleteParameter = tempAdapter.DeleteCommand.Parameters.Add("@Id", SqlDbType.Int);
               deleteParameter.SourceColumn = "id";
               deleteParameter.SourceVersion = DataRowVersion.Original;
               return tempAdapter;

          }
          private static void DeleteRowInCarsTable(int id)
          {
               foreach (DataRow row in Cars.Rows)
               {
                    if ((int)row["id"] == id)
                    {
                         row.Delete();
                    }
               }

               using (var sqlConnection = new SqlConnection(connectionString))
               {
                    CarsDataAdapter.DeleteCommand.Connection = sqlConnection;
                    CarsDataAdapter.Update(Program.Cars);
               }
          }

          private static void UpdateRowInCarsTable(Car updatedCar, int id)
          {
               foreach (DataRow row in Cars.Rows)
               {
                    if ((int)row["id"] == id)
                    {
                         row["brand"] = updatedCar.Brand;
                         row["model"] = updatedCar.Model;
                    }
               }

               using (var sqlConnection = new SqlConnection(connectionString))
               {
                    CarsDataAdapter.UpdateCommand.Connection = sqlConnection;
                    CarsDataAdapter.Update(Program.Cars);
               }
          }
          private static void InsertRowInCarsTable(Car insertCar)
          {
               DataRow temp = Cars.NewRow();
               temp["brand"] = insertCar.Brand;
               temp["model"] = insertCar.Model;
               Cars.Rows.Add(temp);
               using (var sqlConnection = new SqlConnection(connectionString))
               {
                    CarsDataAdapter.InsertCommand.Connection = sqlConnection;
                    CarsDataAdapter.Update(Program.Cars);
               }
          }

          static void CreateTables()
          {
               using (var connection = new SqlConnection(connectionString))
               {
                    connection.Open();
                    var sqlCommandText =
                         "CREATE TABLE Cars(id int not null identity(1,1) primary key, brand nvarchar(255), model nvarchar(255)) " +
                         "CREATE TABLE Owners(id int not null identity(1,1) primary key, FirstName nvarchar(255), LastName nvarchar(255), carId int not null)";
                    using (var sqlCommand = new SqlCommand(sqlCommandText, connection))
                    {
                         sqlCommand.ExecuteNonQuery();
                    }
               }
          }

          static DataTable CreateCarsDataTable()
          {
               DataTable Cars = new DataTable("Cars");

               DataColumn column = new DataColumn();
               column.DataType = System.Type.GetType("System.Int32");
               column.AutoIncrement = true;
               column.AutoIncrementSeed = 1;
               column.AutoIncrementStep = 1;
               column.ColumnName = "id";

               Cars.Columns.Add(column);
               Cars.Columns.Add("brand", typeof(string));
               Cars.Columns.Add("model", typeof(string));
               return Cars;
          }
          static DataTable CreateOwnersDataTable()
          {

               DataTable Owners = new DataTable("Owners");

               Owners.Columns.Add("id", typeof(int));
               Owners.Columns.Add("firstName", typeof(string));
               Owners.Columns.Add("lastName", typeof(string));
               Owners.Columns.Add("carId", typeof(int));
               return Owners;
          }
     }

     internal class Car
     {
          public string Brand { get; set; }
          public string Model { get; set; }
     }
}
