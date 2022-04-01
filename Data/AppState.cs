using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DbsLook.Data
{
    public class AppState
    {
        public string DbName { get; private set; }

        public event Action OnChange;

        public void SetCurrentDB(string dbName)
        {
            DbName = dbName;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

}