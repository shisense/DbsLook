using System;
using System.Data;

namespace DbsLook.Data
{
    public class DbTable
    {
        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

        public string TableName { get; set; }
    }


    public class DbProc
    {
        public string Name { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime ModifyDate { get; set; }

    }

    public class DbProcDetail
    {
        public string Definition { get; set; }

    }

    public class Column
    {
        public string ColumnName { get; set; }
        public string DataType { get; set; }
    }

    public class Function
    {
        public string RoutineName { get; set; }
        public string DataType { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }

    }

    public class DbReg
    {
        public string DBName { get; set; }
        public string DataSource { get; set; }
        public string InitialCatalog { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Desc { get; set; }
        public bool Current { get; set; }


        public static DbReg Map(IDataRecord r)
        {

            return new DbReg
            {
                DBName = (string)r["dbname"],
                DataSource = (string)r["DataSource"],
                InitialCatalog = (string)r["InitialCatalog"],
                UserId = (string)r["UserId"],
                Password = (string)r["Password"],
                Current = (bool)r["Current"]
            };
        }
    }

    public class DBManRoutineRegTableSpace
    {
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public Int64 RowCounts { get; set; }
        public decimal Used_MB { get; set; }
        public decimal Unused_MB { get; set; }
        public decimal Total_MB { get; set; }

        public static DBManRoutineRegTableSpace Map(IDataRecord r)
        {

            return new DBManRoutineRegTableSpace
            {
                SchemaName = (string)r["SchemaName"],
                TableName = (string)r["TableName"],
                RowCounts = (Int64)r["RowCounts"],
                Used_MB = (decimal)r["Used_MB"],
                Unused_MB = (decimal)r["Unused_MB"],
                Total_MB = (decimal)r["Total_MB"]
            };
        }
    }


    public class DBManRoutineReg
    {
        public string Name { get; set; }
        public string SQL { get; set; }
        public string Desc { get; set; }

        public static DBManRoutineReg Map(IDataRecord r)
        {
            return new DBManRoutineReg
            {
                Name = (string)r["Name"],
                SQL = (string)r["SQL"],
                Desc = (string)r["Desc"]

            };
        }
    }
    public class RmaDataMappingValidation
    {
        public int RowID { get; set; }
        public string ReferenceCode { get; set; }
        public string IncomingData { get; set; }
        public string RmaData { get; set; }
        public string ErrMessage { get; set; }


        public static RmaDataMappingValidation Map(IDataRecord r)
        {
            return new RmaDataMappingValidation
            {
                RowID = (int)r["RowID"],
                ReferenceCode = (string)r["ReferenceCode"],
                IncomingData = (string)r["IncomingData"],
                RmaData = (string)r["RmaData"],
                ErrMessage = (string)r["ErrMessage"]
            };
        }
    }

    public class DBManQueryHead
    {
        public string Name { get; set; }
        public string SQL { get; set; }

        public static DBManQueryHead Map(IDataRecord r)
        {
            return new DBManQueryHead
            {
                Name = (string)r["Name"],
                SQL = (string)r["SQL"]

            };
        }
    }
}




