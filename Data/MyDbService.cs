using System;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;

namespace DbsLook.Data
{
    public class MyDbService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate)
        {
            var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToArray());
        }


        public SqlConnection GetBaseDbConn()
        {
            string connetionString;
            SqlConnection cnn;
            connetionString = @"Data Source=LAPTOP-ROEKOCKI;Initial Catalog=VicIdentityDB;User ID=shiluan2;Password=6370635";
            cnn = new SqlConnection(connetionString);

            return cnn;
        }

        public SqlConnection GetCurrentDbConn() {

            var actDb = GetDBsActiveAsync().Result[0];

            var connetionString = $"Data Source={actDb.DataSource};Initial Catalog={actDb.InitialCatalog};User ID={actDb.UserId};Password={actDb.Password}";

            var cnn = new SqlConnection(connetionString);
            return cnn;
        }


        public Task<DbTable[]> GetTablesAsync()
        {
            var cnn = GetCurrentDbConn();
            cnn.Open();


            string sql = "select name, create_date, modify_date from sys.tables order by name";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                        select new DbTable { 
                            TableName = (string)r["name"],
                            CreateDate = (DateTime)r["create_date"],
                            ModifyDate = (DateTime)r["modify_date"]
                        }).ToArray());

            cnn.Close();

            return tarray;
        }


        public Task<DbProc[]> GetProceduresAsync()
        {
            var cnn = GetCurrentDbConn();

            cnn.Open();

            string sql = "select name, create_date, modify_date from sys.procedures order by name";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select new DbProc
                                          {
                                              Name = (string)r["name"],
                                              CreateDate = (DateTime)r["create_date"],
                                              ModifyDate = (DateTime)r["modify_date"]
                                          }).ToArray());

            cnn.Close();

            return tarray;
        }


        public Task<DbProcDetail[]> GetProceduresDetailAsync(string name)
        {
            var cnn = GetCurrentDbConn();
            cnn.Open();

            string sql = "SELECT [definition] FROM sys.sql_modules WHERE object_id = (OBJECT_ID(N'"+name+"')); ";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select new DbProcDetail
                                          {
                                              Definition = (string)r["definition"]
                                          }).ToArray());

            cnn.Close();

            return tarray;
        }


        public Task<Column[]> GetColumnsAsync(string tblname)
        {

            var cnn = GetCurrentDbConn();
            cnn.Open();


            string sql = @"
                SELECT COLUMN_NAME,*
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = '"+tblname+@"'
                ORDER BY ORDINAL_POSITION";
            
            
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select new Column
                                          {
                                              ColumnName = (string)r["column_name"],
                                              DataType = (string)r["data_type"]
                                          }).ToArray());

            cnn.Close();

            return tarray;
        }


        public Task<Function[]> GetFunctionsAsync()
        {

            var cnn = GetCurrentDbConn();
            cnn.Open();


            string sql = @"
                SELECT
                    ROUTINE_NAME, 
                    ROUTINE_DEFINITION , 
                    ROUTINE_SCHEMA, 
                    DATA_TYPE,
                    CREATED,
                    LAST_ALTERED
                FROM
                    INFORMATION_SCHEMA.ROUTINES 
                WHERE
                    ROUTINE_TYPE = 'FUNCTION'
                ORDER BY ROUTINE_NAME
                ";
            
            
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select new Function
                                          {
                                              RoutineName = (string)r["routine_name"],
                                              DataType = (string)r["data_type"],
                                              CreateDate = (DateTime)r["created"],
                                              ModifyDate = (DateTime)r["last_altered"]
                                          }).ToArray());

            cnn.Close();

            return tarray;
        }

        public Task<DbProcDetail[]> GetFunctionDefAsync(string name)
        {

            var cnn = GetCurrentDbConn();
            cnn.Open();


            string sql = $"select OBJECT_DEFINITION(object_id('{name}')) [definition]";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select new DbProcDetail
                                          {
                                              Definition = (string)r["definition"]
                                          }).ToArray());

            cnn.Close();

            return tarray;
        }



        public Task<DBManRoutineRegTableSpace[]> GetDBManRoutineRegTableSpacesAsync()
        {
            var cnn = GetCurrentDbConn();
            cnn.Open();


            string sql = @"
                SELECT  
                s.Name AS SchemaName,
                t.Name AS TableName,
                p.rows AS RowCounts,
                CAST(ROUND((SUM(a.used_pages) / 128.00), 2) AS NUMERIC(36, 2)) AS Used_MB,
                CAST(ROUND((SUM(a.total_pages) - SUM(a.used_pages)) / 128.00, 2) AS NUMERIC(36, 2)) AS Unused_MB,
                CAST(ROUND((SUM(a.total_pages) / 128.00), 2) AS NUMERIC(36, 2)) AS Total_MB 
                FROM sys.tables t 
                INNER JOIN sys.indexes i ON t.OBJECT_ID = i.object_id 
                INNER JOIN sys.partitions p ON i.object_id = p.OBJECT_ID AND i.index_id = p.index_id 
                INNER JOIN sys.allocation_units a ON p.partition_id = a.container_id 
                INNER JOIN sys.schemas s ON t.schema_id = s.schema_id 
                GROUP BY t.Name, s.Name, p.Rows 
                --ORDER BY s.Name, t.Name 
                --ORDER BY p.rows desc 
                ORDER BY Total_MB DESC 
                ";


            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select  DBManRoutineRegTableSpace.Map(r)
                                          ).ToArray());

            cnn.Close();

            return tarray;

        }



        public Task<RmaDataMappingValidation[]> GetRmaDataMappingValidationsAsync(string mapCode)
        {
            var cnn = GetCurrentDbConn();
            cnn.Open();


            string sql = @$"
                DECLARE
	                @mapping_code char(7)

                SET @mapping_code = '{mapCode}'

                ---------------------------
                --choose the staging table
                -- SELECT AssumingCompanyId,CedingCompanyId,MappingTableName
                -- FROM AssumingDataMapConfig
                ---------------------------
                DROP TABLE IF EXISTS #tmpV311
                DROP TABLE IF EXISTS #tmpPri

                SELECT top 0 * INTO #tmpV311 FROM Tmp_TAI_V311
                SELECT top 0 * INTO #tmpPri FROM Tmp_Pri

                DECLARE @stage_table varchar(255) = '#tmpV311'

                if SUBSTRING(@mapping_code, 5,3) = 'PFS'
	                set @stage_table = '#tmpPri'


                DECLARE
                    @idata VARCHAR(4000), @id int,
                    @rcode NVARCHAR(255),
                    @rdata VARCHAR(500),
                    @sql  VARCHAR(4000);


                DROP TABLE IF EXISTS  #tmpMap

                SELECT Id, ReferenceCode, IncomingData, RmaData
                INTO #tmpMap
                FROM AssumedDataMapping
                WHERE MappingCode = @mapping_code


                PRINT 'Selected # of mapping entries to validate'
                DECLARE @cnt0 int
                SELECT @cnt0 = count(1)
                        FROM #tmpMap
                PRINT @cnt0


                DECLARE @LOCAL_TABLEVARIABLE TABLE
                (
	                RowID int, 
	                ReferenceCode varchar(255),
	                IncomingData varchar(4000), 
	                RmaData varchar(500),
	                ErrMessage varchar(1024)
                )

                DECLARE cur CURSOR
                FOR
                    SELECT Id, ReferenceCode, IncomingData, RmaData
                    FROM #tmpMap

                OPEN cur
                FETCH NEXT FROM cur
                INTO @id, @rcode, @idata, @rdata;


                WHILE @@FETCH_STATUS = 0
                BEGIN

                    SET  @sql = N'select TOP 0 ' + @rcode + ', case When ' + @idata + ' Then ' + @rdata +' END from ' + @stage_table;
                    SET @sql = 'set noexec on; '+ @sql + '; set noexec off';

                    BEGIN TRY
                            EXEC (@sql);
                    END TRY
                    BEGIN CATCH
		                INSERT @LOCAL_TABLEVARIABLE 
		                VALUES (@id, @rcode, @idata, @rdata, ERROR_MESSAGE())
                    END CATCH

                    FETCH NEXT FROM cur
                        INTO @id, @rcode, @idata, @rdata;
                END

                CLOSE cur

                DEALLOCATE cur

                DROP TABLE IF EXISTS #tmpPri
                DROP TABLE IF EXISTS #tmpV311
                DROP TABLE IF EXISTS #tmpMap

                SELECT * FROM @LOCAL_TABLEVARIABLE
                ";


            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select RmaDataMappingValidation.Map(r)
                                          ).ToArray());

            cnn.Close();

            return tarray;

        }

        public Task<string[]> GetRmaDataMappingCodesAsync()
        {
            var cnn = GetCurrentDbConn();
            cnn.Open();

            string sql = $"select distinct MappingCode from AssumedDataMapping order by MappingCode";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select (string)r["MappingCode"]).ToArray());

            cnn.Close();

            return tarray;
        }


        /***************************************************/
        public Task<DbReg[]> GetDBsAsync()
        {

            var cnn = GetBaseDbConn();
            cnn.Open();


            string sql = @"select  * from DBMan_DbReg";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select DbReg.Map(r))
                                         .ToArray());

            cnn.Close();

            return tarray;
        }

        public Task<DbReg[]> GetDBsActiveAsync()
        {

            var cnn = GetBaseDbConn();
            cnn.Open();


            string sql = @"select  * from DBMan_DbReg where [Current]=1";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select DbReg.Map(r))
                                         .ToArray());

            cnn.Close();

            return tarray;
        }
        public void UpdateActiveDbAsync(string dbName)
        {

            var cnn = GetBaseDbConn();
            cnn.Open();


            string sql = $@"update DBMan_DbReg 
                        set [Current] = case when DBName ='{dbName}' then 1
                        else 0 end";

            var sqlcmd = new SqlCommand(sql, cnn);

            sqlcmd.ExecuteNonQuery();

            cnn.Close();
            
        }

        public Task<DBManRoutineReg[]> GetDBManRoutineRegsAsync()
        {

            var cnn = GetBaseDbConn();
            cnn.Open();


            string sql = @"SELECT [Name],[SQL],[Desc] FROM DBMan_RoutineReg";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select DBManRoutineReg.Map(r))
                                         .ToArray());

            cnn.Close();

            return tarray;
        }

        public Task<DBManQueryHead[]> GetDBManQueryHeadAsync(string dbName)
        {

            var cnn = GetBaseDbConn();
            cnn.Open();


            string sql = @$"SELECT [Name],[SQL] FROM DBMan_QueryHead WHERE DBName='*' OR (charindex('{dbName}',DBName)>0 AND '{dbName}' in (select DBName from  DBMan_DbReg))";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var tarray = Task.FromResult((from IDataRecord r in dr
                                          select DBManQueryHead.Map(r))
                                         .ToArray());

            cnn.Close();

            return tarray;
        }


        public Task<string> GetQueryRegdAsync(string dbName)
        {
            var cnn = GetBaseDbConn();
            cnn.Open();


            string sql = @$" select top 1 [sql] from dbman_queryhead where name='{dbName}'";
            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();
            dr.Read();
            var tarray = Task.FromResult(dr.GetString(0));

            cnn.Close();

            return tarray;
        }


        public Task<DataTable> GetQueryTableAsync(string sql)
        {
            var cnn = GetCurrentDbConn();
            cnn.Open();

            var sqlcmd = new SqlCommand(sql, cnn);

            var dr = sqlcmd.ExecuteReader();

            var dt = new DataTable();
            dt.Load(dr);
            
            var tarray = Task.FromResult(dt);

            cnn.Close();

            return tarray;
        }
    }
}
