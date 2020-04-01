using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PdaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<DataController> logger;

        public DataController(IConfiguration configuration, ILogger<DataController> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpPost("prescriptions")]
        public async Task<IActionResult> Prescription_Select(object data)
        {
            // return await RunProcedure("Prescription_Select", data);
            var str = data.ToString();

            var p1 = JsonConvert.DeserializeObject<PrescriptionParam>(str);
            var status = new DataTable();
            status.Columns.Add("IntNumber", typeof(int));
            foreach (var item in p1.Status)
            {
                var row = status.NewRow();
                row[0] = item;
                status.Rows.Add(row);
            }

            var param = new {
                fromDate = p1.FromDate,
                toDate = p1.ToDate,
                Status = status.AsTableValuedParameter("IntList")
            };

            string connectionString = configuration.GetConnectionString("Default");

            var command = new CommandDefinition("pda.Prescription_Select", param, commandType: CommandType.StoredProcedure);

            dynamic result = null;
            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                result = await connection.QueryAsync<dynamic>(command);
            }
            catch (Exception ex)
            {
                var error = ex.GetExceptionMessage();
                logger.LogError(error);
                throw new Exception(error);
            }

            return Ok(result);

        }

        [HttpPost("prescription")]
        public async Task<IActionResult> Prescription_SelectByID(object data)
        {
            return Ok(await RunProcedure("Prescription_SelectByID", data, false));
        }

        [HttpDelete("prescription")]
        public async Task<IActionResult> PrescriptionCondition_Delete(object data)
        {
            return Ok(await RunProcedure("PrescriptionCondition_Delete", data, false));
        }

        [HttpPut("prescriptionCondition")]
        public async Task<IActionResult> PrescriptionCondition_Insert(object data)
        {
            return Ok(await RunProcedure("PrescriptionCondition_Insert", data, false));
        }

        [HttpPost("login")]
        public async Task<IActionResult> User_Authentication(object data)
        {
            return Ok(await RunProcedure("User_Authentication", data, false));
        }

        [HttpPost("prescriptionDetail")]
        public async Task<IActionResult> PrescriptionDetail_Select(object data)
        {
            return Ok(await RunProcedure("PrescriptionDetail_Select", data, true));
        }

        private async Task<dynamic> RunProcedure(string procedureName, object data, bool arrayResult)
        {
            var str = data.ToString();

            var p1 = JsonConvert.DeserializeObject<Dictionary<string, object>>(str);

            string connectionString = configuration.GetConnectionString("Default");

            var command = new CommandDefinition("pda." + procedureName, p1, commandType: CommandType.StoredProcedure);

            dynamic result = null;
            try
            {
                using SqlConnection connection = new SqlConnection(connectionString);
                if (arrayResult)
                    result = await connection.QueryAsync<dynamic>(command);
                else
                    result = await connection.QueryFirstOrDefaultAsync<dynamic>(command);
            }
            catch (Exception ex)
            {
                var error = ex.GetExceptionMessage();
                logger.LogError(error);
                throw new Exception(error);
            }

            return result;
        }

    }

    public class PrescriptionParam
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public List<int> Status { get; set; }
    }

}