using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using ePiggyWeb.DataManagement;
using ePiggyWeb.DataManagement.Entries;

namespace ePiggyWeb.Utilities
{
    public class DataJson
    {
		private DataManager DataManager { get; }

		public DataJson(DataManager dataManager)
		{
            DataManager = dataManager;
		}

		/*Writing/reading JSON files*/
		public void WriteIncomeToFile()
		{
            try
            {
                File.WriteAllText("userIncome.json", "");
                using var sw = File.AppendText("userIncome.json");
                foreach (var output in DataManager.Income.EntryList.Select(data => JsonSerializer.Serialize(data)))
                {
                    sw.WriteLine(output);
                }
            }
            catch (Exception e)
            {
				ExceptionHandler.Log(e.ToString());
            }
			
        }

		public void WriteExpensesToFile()
		{
            try
            {
                File.WriteAllText("userExpenses.json", "");
                using var sw = File.AppendText("userExpenses.json");
                foreach (var output in DataManager.Income.EntryList.Select(data => JsonSerializer.Serialize(data)))
                {
                    sw.WriteLine(output);
                }
            }
            catch (Exception e)
            {
				ExceptionHandler.Log(e.ToString());
            }
			
        }

		public void ReadIncomeFromFile()
		{
            try
			{
				var file = new StreamReader("userIncome.json");

                string line;
                while ((line = file.ReadLine()) != null)
				{
                    var entryList = new EntryList(EntryType.Income);
					try
					{
                        var entry = JsonSerializer.Deserialize<Entry>(line);//TRY CATCH
                        entryList.Add(entry);
					}
					catch (Exception e)
					{
                        ExceptionHandler.Log(e.ToString());
					}
                    DataManager.Expenses.AddRange(entryList);

				}

				file.Close();
			}
			catch (FileNotFoundException f)
			{
				ExceptionHandler.Log(f.ToString());
			}
		}

		public void ReadExpensesFromFile()
		{
            try
			{
				var file = new StreamReader("userExpenses.json");
                string line;
                while ((line = file.ReadLine()) != null)
				{
					var entryList = new EntryList(EntryType.Expense);
					try
					{
						var entry = JsonSerializer.Deserialize<Entry>(line);//TRY CATCH
                        entryList.Add(entry);
                    }
					catch (Exception e)
					{
						ExceptionHandler.Log(e.ToString());
					}
                    DataManager.Expenses.AddRange(entryList);
                }

				file.Close();
			}
			catch (FileNotFoundException f)
			{
				ExceptionHandler.Log(f.ToString());
			}
		}
	}
}
