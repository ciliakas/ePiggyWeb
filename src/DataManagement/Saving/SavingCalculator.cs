using System;
using System.Collections.Generic;
using ePiggyWeb.DataManagement.Entries;
using ePiggyWeb.DataManagement.Goals;
using ePiggyWeb.Utilities;

namespace ePiggyWeb.DataManagement.Saving
{
    public class SavingCalculator
    {
        public DataManager DataManager { get; set; }
        public SavingType SavingType { get; set; }

        private static decimal SavingRatio { get; } = 1M;
        private static decimal RegularSavingValue { get; } = 0.25M;
        private static decimal MaximalSavingValue { get; } = 0.5M;
        private static decimal MinimalSavingValue { get; } = 0.1M;

        private List<SavingSuggestion> IncomeOffers { get; } = new List<SavingSuggestion>();
        private List<SavingSuggestion> ExpensesOffers { get; } = new List<SavingSuggestion>();
        public SavingCalculator(DataManager dataManager, SavingType savingType = SavingType.Regular)
        {
            DataManager = dataManager;
            SavingType = savingType;
        }


        public static int CalculateProgress(decimal saved, decimal target)
        {
            if (saved < 0)
            {
                return 0;
            }
            if (target <= 0)
            {
                return 100;
            }
            var progress = (int)(saved * 100 / target);
            return progress > 100 ? 100 : progress;
        }


        public bool GetSuggestedExpensesOffers(IEntryList entryList, IGoal goal, SavingType savingType, IList<ISavingSuggestion> entrySuggestions)
        {
            SavingType = savingType;
            var savedAmount = DataManager.Income.EntryList.GetUntilToday().GetSum() - DataManager.Expenses.EntryList.GetUntilToday().GetSum(); // current balance
            var enumCount = Enum.GetValues(typeof(Importance)).Length;

            for (var i = enumCount; i > (int)Importance.Necessary; i--)
            {
                var expenses = DataManager.Expenses.EntryList.GetBy((Importance)i - 1);
                //var expenses = groupedByImportance[i - 1].Entries;
                var ratio = enumCount - i;
                foreach (var entry in expenses)
                {
                    decimal amountAfterSaving;

                    switch (SavingType)
                    {
                        case SavingType.Minimal:
                            amountAfterSaving = entry.Amount * SavingRatio * ratio * MinimalSavingValue;
                            break;
                        case SavingType.Regular:
                            amountAfterSaving = entry.Amount * SavingRatio * ratio * RegularSavingValue;
                            break;
                        case SavingType.Maximal:
                            var maximalSaving = MaximalSavingValue * SavingRatio * ratio >= 1;
                            var temp = maximalSaving ? 1M : MaximalSavingValue;
                            amountAfterSaving = entry.Amount * temp;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    AddToExpensesOfferList(entry, amountAfterSaving, entrySuggestions);

                    savedAmount += entry.Amount - amountAfterSaving;
                    if (goal.Amount <= savedAmount)
                    {
                        return true; //Saved enough
                    }
                }
            }
            return false; //Didn't save enough
        }

        public bool CheckGoal(IGoal goal, SavingType savingType)
        {
            SavingType = savingType;
            var balance = DataManager.Income.EntryList.GetSum() - DataManager.Expenses.EntryList.GetSum();
            if (balance >= 0)
            {
                //Goal goal = new Goal();
                return balance - goal.Amount >= 0 || GenerallySavingMoney();
            }

            return false;   //user already in debt(negative balance) without adding goal expenses
        }

        private bool GenerallySavingMoney()
        {
            foreach (var data in DataManager.Income.EntryList)
            {
                ChoosingImportance(data, EntryType.Income);
            }
            foreach (var data in DataManager.Expenses.EntryList)
            {
                ChoosingImportance(data, EntryType.Expense);
            }
            return true;
        }

        private bool ChoosingImportance(IEntry dataEntry, EntryType entryType)
        {
            return dataEntry.Importance == (int)Importance.Necessary
                   || ImportanceBasedCalculation(dataEntry, SavingRatio * (dataEntry.Importance - 1), entryType);
        }

        // ImportanceBasedCalculation always returns true, doesn't really make sense for a method to always return true
        private bool ImportanceBasedCalculation(IEntry dataEntry, decimal importanceBasedSavingRatio, EntryType entryType)
        {
            switch (SavingType)
            {
                case SavingType.Minimal:
                    switch (entryType)
                    {
                        case EntryType.Income:
                            AddToIncomeOfferList(dataEntry, dataEntry.Amount * MinimalSavingValue * importanceBasedSavingRatio);
                            break;
                        case EntryType.Expense:
                            AddToExpensesOfferList(dataEntry, dataEntry.Amount * MinimalSavingValue * importanceBasedSavingRatio);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(entryType), entryType, null);
                    }
                    return true;
                case SavingType.Maximal:
                    {
                        var maximalSaving = MaximalSavingValue * importanceBasedSavingRatio >= 1;

                        var temp = maximalSaving ? 1M : MaximalSavingValue;

                        switch (entryType)
                        {
                            case EntryType.Income:
                                AddToIncomeOfferList(dataEntry, dataEntry.Amount * temp);
                                break;
                            case EntryType.Expense:
                                AddToExpensesOfferList(dataEntry, dataEntry.Amount * temp);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(entryType), entryType, null);
                        }
                        return true;
                    }
                case SavingType.Regular:
                    switch (entryType)
                    {
                        case EntryType.Income:
                            AddToIncomeOfferList(dataEntry, dataEntry.Amount * RegularSavingValue * importanceBasedSavingRatio);
                            break;
                        case EntryType.Expense:
                            AddToExpensesOfferList(dataEntry, dataEntry.Amount * RegularSavingValue * importanceBasedSavingRatio);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(entryType), entryType, null);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }


        private void AddToIncomeOfferList(IEntry entry, decimal amountAfterSaving)
        {
            var newIncomeOffer = new SavingSuggestion(entry, amountAfterSaving);
            IncomeOffers.Add(newIncomeOffer);
        }

        private void AddToExpensesOfferList(IEntry entry, decimal amountAfterSaving)
        {
            var newExpensesOffer = new SavingSuggestion(entry, amountAfterSaving);
            ExpensesOffers.Add(newExpensesOffer);
        }

        private static void AddToExpensesOfferList(IEntry entry, decimal amountAfterSaving, ICollection<ISavingSuggestion> entrySuggestions)
        {
            var newExpensesOffer = new SavingSuggestion(entry, amountAfterSaving);
            entrySuggestions.Add(newExpensesOffer);
        }
    }
}
