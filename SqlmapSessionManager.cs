using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlmapHelper
{
    public class SqlmapSessionManager : IDisposable
    {
        private SqlmapSession _session = null;
        public SqlmapSessionManager(SqlmapSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }
            _session = session;
        }
        public async Task<string> CreateNewTask()
        {
            return JObject.Parse(await _session.VulnUrlGet("/task/new")).SelectToken("taskid").ToString();
        }
        public async Task<bool> DelTask(string taskid)
        {
            return (bool)JObject.Parse(await _session.VulnUrlGet("/task/" + taskid + "/delete")).SelectToken("success");
        }
        public async Task<Dictionary<string, object>> GetAllTaskIdList()
        {
            Dictionary<string, object> pairs = new Dictionary<string, object>();
            JObject @object = JObject.Parse(await _session.VulnUrlGet("/admin/list"));
            @object = @object["tasks"] as JObject;
            foreach (var item in @object)
            {
                pairs.Add(item.Key, item.Value);
            }
            return pairs;
        }
        public async Task<List<SqlmapScanDataModel>> GetScanData(string taskid)
        {

            JObject obj = Newtonsoft.Json.Linq.JObject.Parse(await _session.VulnUrlGet("/scan/" + taskid + "/data"));
            string url = obj["data"][0]["value"]["url"].ToString() + obj["data"][0]["value"]["query"].ToString();
            string dbms = obj["data"][1]["value"][0]["dbms"].ToString() + obj["data"][1]["value"][0]["dbms_version"][0].ToString();

            List<SqlmapScanDataModel> scanLogModels = new List<SqlmapScanDataModel>();
            scanLogModels.Add(new SqlmapScanDataModel
            {
                Url = url,
                Dbms = dbms,

            });


            return scanLogModels;
        }

        public async Task<Dictionary<string, object>> GetOptions(string taskid)
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            JObject jObject = JObject.Parse(await _session.VulnUrlGet("/option/" + taskid + "/list"));
            jObject = jObject["options"] as JObject;

            foreach (var item in jObject)
            {
                options.Add(item.Key, item.Value);
            }
            options.Remove("sqlShell");
            options.Remove("wizard");
            return options;
        }
        public async Task<SqlmapStatusModel> GetSqlmapStatus(string taskid)
        {
            JObject @object = JObject.Parse(await _session.VulnUrlGet("/scan/" + taskid + "/status"));
            SqlmapStatusModel sqlmapStatus = new SqlmapStatusModel();
            sqlmapStatus.Status = @object["status"].ToString();
            if (@object["returncode"].Type != JTokenType.Null)
            {
                sqlmapStatus.ReturnCode = int.Parse(@object["returncode"].ToString());
            }
            return sqlmapStatus;
        }
        public async Task<List<SqlmapScanLogModel>> GetLog(string taskid)
        {
            JObject @object = JObject.Parse(await _session.VulnUrlGet("/scan/" + taskid + "/log"));
            JArray items = @object["log"] as JArray;
            List<SqlmapScanLogModel> scanLogModels = new List<SqlmapScanLogModel>();
            foreach (var item in items)
            {
                //SqlmapScanLogModel scanLogModel = new SqlmapScanLogModel();
                //scanLogModel.Message = item["message"].ToString();
                //scanLogModel.Level = item["level"].ToString();
                //scanLogModel.Time = item["time"].ToString();
                scanLogModels.Add(new SqlmapScanLogModel
                {
                    Message = item["message"].ToString(),
                    Level = item["level"].ToString(),
                    Time = item["time"].ToString()
                });

            }

       
            return scanLogModels;
        }
        public async Task<bool> TaskStart(string taskid, Dictionary<string, object> pairs)
        {
            string json = JsonConvert.SerializeObject(pairs);
            JToken token = JObject.Parse(await _session.VulnUrlPost("/scan/" + taskid + "/start", json));
            if (token.SelectToken("success").ToString() == "False")
            {
                return false;
            }
            return true;
        }
        public async Task<bool> TaskStop(string taskid)
        {
            JToken token = JObject.Parse(await _session.VulnUrlGet("/scan/" + taskid + "/stop"));
            return (bool)token.SelectToken("success");
        }
        public async Task<bool> DelAllTask()
        {
            await _session.VulnUrlGet("/admin/flush");
            int aa = int.Parse(JObject.Parse(await _session.VulnUrlGet("/admin/list")).SelectToken("tasks_num").ToString());
            return aa == 0;

        }
        public void Dispose()
        {
            _session.Dispose();
            _session = null;
        }
    }
}
