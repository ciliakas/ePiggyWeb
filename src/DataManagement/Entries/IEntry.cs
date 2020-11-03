﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ePiggyWeb.DataManagement.Goals;

namespace ePiggyWeb.DataManagement.Entries
{
    public interface IEntry : IGoal
    {
        public DateTime Date { get; set; }
        public bool Recurring { get; set; }
        public int Importance { get; set; }
        public void Edit(IEntry newEntry);
    }
}
