﻿using System;
using System.Linq;
using Biggy.Core;

namespace Demo
{
    public class jsonChinookDb : ChinookDbBase
    {
        private jsonDbCore _db;

        public jsonDbCore Database { get { return _db; } }

        public jsonChinookDb(bool dropCreateTables = false)
        {
            _db = new jsonDbCore();
            if (dropCreateTables)
            {
                this.DropCreateAll();
            }
            this.LoadData();
        }

        public jsonChinookDb(string dbName, bool dropCreateTables = false)
        {
            _db = new jsonDbCore(dbName);
            if (dropCreateTables)
            {
                this.DropCreateAll();
            }
            this.LoadData();
        }

        public override IDataStore<T> CreateRelationalStoreFor<T>()
        {
            return _db.CreateStoreFor<T>();
        }

        public override IDataStore<T> CreateDocumentStoreFor<T>()
        {
            return _db.CreateStoreFor<T>();
        }

        public override void DropCreateAll()
        {
            _db.TryDropTable("Artist");
            _db.TryDropTable("Album");
            _db.TryDropTable("Track");
            _db.TryDropTable("artistdocuments");
        }
    }
}