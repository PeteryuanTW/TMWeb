using CommonLibrary.API.Message;
using CommonLibrary.MachinePKG.EFModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.MachineData
{
    public class RFIDMsg
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public string Data { get; set; }
    }

    public class Sensor
    {
        public int section { get; set; }
        public int dataType { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string ChineseName { get; set; }
        public string CurrentValue { get; set; }
    }

    public class TagRes
    {
        public DateTime FIRST_TIME { get; set; }
        public DateTime? LAST_TIME { get; set; }
        public int READ_COUNT { get; set; }
        public string SERIAL_NUMBER { get; set; }
        public string AUFNR { get; set; }
        public int AUFNR_QTY { get; set; }
        public string SITE_ID { get; set; }
        public int BATCH_NO { get; set; }
        public int BATCH_ITEM_NO { get; set; }
        public string SCAN_OUTCOME { get; set; }
        public string EPC_CODE { get; set; }
        public int EPC_REPLICATE { get; set; }
        public string UPC_CODE { get; set; }
        public string FELLI_ITEM { get; set; }
        public int UPC_OK { get; set; }
        public int UPC_NOT_OK { get; set; }
        public int NOT_SCAN { get; set; }
        public double RSSI_DBM { get; set; }
        public string USER_ID { get; set; }
        public Nullable<DateTime> CREATE_TIME { get; set; }
    }

    public class RegalscanRFIDLoginModel
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }

    public class RegalscanRFIDLoginResponse
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public string Data { get; set; }
        public RegalscanRFIDToken TnToken { get; set; }
    }

    public class RegalscanRFIDToken
    {
        public string TokenStr { get; set; }
        public DateTime Expires { get; set; }
    }

    public class RegalscanRFIDWorkModeModel
    {
        [Required]
        public string AUFNR { get; set; }
        [Required]
        public int AUFNR_QTY { get; set; }
        [Required]
        public int PACKAGE_QTY { get; set; }
        [Required]
        public int SITE_ID { get; set; }
        [Required]
        public string PRODUCT_CODE { get; set; }
    }

    public class RegalscanRFIDWorkModeResponse
    {
        public int Code { get; set; }
        public string Msg { get; set; }
        public object Data { get; set; }
        public object TnToken { get; set; }
    }

    public class Site
    {
        public int SITE_ID { get; set; }
        public string SITE_NAME { get; set; }
        public string LOCATION { get; set; }
        public string REMARK { get; set; }
        public string IP_ADDRESS { get; set; }
        public string USER_ID { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public DateTime? LAST_TIME { get; set; }
    }

    public class UPC
    {
        public string SERIAL_NUMBER { get; set; }
        public string FELLI_ITEM { get; set; }
        public string UPC_CODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string EPC_CODE { get; set; }
        public string NATIONAL_CODE { get; set; }
        public string MANUFACTURER_CODE { get; set; }
        public string PRODUCT_CODE { get; set; }
        public string SERIAL_CODE { get; set; }
        public string USER_ID { get; set; }
        public DateTime CREATE_TIME { get; set; }
        public DateTime? LAST_TIME { get; set; }
    }


    public class RegalscanRFIDMachine : WebAPIMachine
    {
        private string token = string.Empty;
        public string Token => token;
        private DateTime tokenExpiredTime;
        public DateTime TokenExpiredTime => tokenExpiredTime;

        private bool frontSensorOn = false;
        public bool FrontSensorOn => frontSensorOn;
        private bool beeperSensorOn = false;
        public bool BeeperSensorOn => beeperSensorOn;

        private List<TagRes> rfidTags = new();

        public List<TagRes> RFIDTags => rfidTags;

        private List<Site> sites = new();

        public List<Site> Sites => sites;

        private List<UPC> upcs = new();

        public List<UPC> Upcs => upcs;

        public RegalscanRFIDWorkModeModel regalscanRFIDWorkModeModel = new();

        public RegalscanRFIDMachine(Machine machine) : base(machine)
        {

        }
        public override async Task ConnectAsync()
        {
            var loginDTO = new RegalscanRFIDLoginModel()
            {
                Account = "999",
                Password = "999",
            };
            string loginStr = JsonConvert.SerializeObject(loginDTO);
            try
            {
                var loginResponse = await httpClient.PostAsync($"http://{Ip}:{Port}/api/Token/Post", new StringContent(loginStr, Encoding.UTF8, "application/json"));
                if (loginResponse.IsSuccessStatusCode)
                {
                    string loginResponseStr = await loginResponse.Content.ReadAsStringAsync();
                    var loginResponseObj = JsonConvert.DeserializeObject<RegalscanRFIDLoginResponse>(loginResponseStr);
                    token = loginResponseObj.TnToken.TokenStr;
                    tokenExpiredTime = loginResponseObj.TnToken.Expires;
                    await base.ConnectAsync();
                }
                else
                {
                    token = string.Empty;
                    tokenExpiredTime = DateTime.Now;
                    Error("Login rfid fail");
                }
            }
            catch (Exception e)
            {
                token = string.Empty;
                tokenExpiredTime = DateTime.Now;
                Error(e.Message);
            }
            try
            {
                var siteRes = await httpClient.GetAsync($"http://{Ip}:{Port}/api/Site/Get?Authorization={token}");
                if (siteRes.IsSuccessStatusCode)
                {
                    var siteResponse = await siteRes.Content.ReadAsStringAsync();
                    RFIDMsg rfidMsg = JsonConvert.DeserializeObject<RFIDMsg>(siteResponse);
                    sites = JsonConvert.DeserializeObject<List<Site>>(rfidMsg.Data);
                }
                else
                {
                    sites = new();
                    Error("Get rfid sites fail");
                }

            }
            catch (Exception e)
            {

                sites = new();
                Error(e.Message);
            }
            try
            {
                var upsRes = await httpClient.GetAsync($"http://{Ip}:{Port}/api/UpcReference/Get?Authorization={token}");
                if (upsRes.IsSuccessStatusCode)
                {
                    var siteResponse = await upsRes.Content.ReadAsStringAsync();
                    RFIDMsg rfidMsg = JsonConvert.DeserializeObject<RFIDMsg>(siteResponse);
                    upcs = JsonConvert.DeserializeObject<List<UPC>>(rfidMsg.Data);
                }
                else
                {
                    upcs = new();
                    Error("Get rfid upc fail");
                }
            }
            catch (Exception e)
            {
                upcs = new();
                Error(e.Message);
            }
        }

        protected override async Task UpdateStatus()
        {
            await UpdateSensor();
            await UpdateTags();
            UIUPdate();
        }

        private async Task UpdateSensor()
        {
            var sensorsRes = await httpClient.GetAsync($"http://{Ip}:{Port}/api/IOStatus/Get");
            if (sensorsRes.IsSuccessStatusCode)
            {
                bool frontRes = false;
                bool beeperRes = false;
                var sensorsResponse = await sensorsRes.Content.ReadAsStringAsync();
                //WriteLog(apiResponse);
                RFIDMsg rfidMsg = JsonConvert.DeserializeObject<RFIDMsg>(sensorsResponse);
                //WriteLog(rfidMsg.Data);
                List<Sensor> sensors = JsonConvert.DeserializeObject<List<Sensor>>(rfidMsg.Data);
                Sensor front = sensors.FirstOrDefault(x => x.Name == "PHOTO_SENSOR_START");
                Sensor beeper = sensors.FirstOrDefault(x => x.Name == "BEEPER");
                if (front is not null && beeper is not null)
                {
                    frontSensorOn = front.CurrentValue == "1";
                    beeperSensorOn = beeper.CurrentValue == "1";
                }
                else
                {
                    ErrorReset();
                    Error("I/O aip missing");
                }
            }
            else
            {
                Error(sensorsRes.ReasonPhrase);
            }
        }

        private async Task UpdateTags()
        {
            var tagsRes = await httpClient.GetAsync($"http://{Ip}:{Port}/api/Tag/Get");
            if (tagsRes.IsSuccessStatusCode)
            {
                var tagResponse = await tagsRes.Content.ReadAsStringAsync();
                RFIDMsg rfidMsg = JsonConvert.DeserializeObject<RFIDMsg>(tagResponse);
                var tagRes = JsonConvert.DeserializeObject<List<TagRes>>(rfidMsg.Data);
                rfidTags = tagRes;
            }
            else
            {
                ErrorReset();
                Error($"tags api error");
            }
        }

        private void ErrorReset()
        {
            frontSensorOn = false;
            beeperSensorOn = false;
            rfidTags = new();
        }

        public async Task DeployWorkMode()
        {
            var workModeStr = JsonConvert.SerializeObject(regalscanRFIDWorkModeModel);
            var changeWorkmodeResponse = await httpClient.PostAsync($"http://{Ip}:{Port}//api/WorkMode/Post?Authorization={token}", new StringContent(workModeStr, Encoding.UTF8, "application/json"));
            if (changeWorkmodeResponse.IsSuccessStatusCode)
            {
                string changeWorkmodeResponseStr = await changeWorkmodeResponse.Content.ReadAsStringAsync();
                var changeWorkmodeResponseObj = JsonConvert.DeserializeObject<RegalscanRFIDWorkModeResponse>(changeWorkmodeResponseStr);
            }
            else
            {
            }
        }


        public override async Task<RequestResult> SetTag(Tag tag, object val)
        {
            try
            {
                if (MachineStatus == Status.Running)
                {
                    var apiRes = await httpClient.PostAsync($"http://{Ip}:{Port}{tag.String2}?Authorization={token}", new StringContent(val.ToString(), Encoding.UTF8, "application/json"));
                    if (apiRes.IsSuccessStatusCode)
                    {
                        var apiResponse = await apiRes.Content.ReadAsStringAsync();
                        tag.SetValue(apiResponse);
                        return new(2, $"Send api to machine {Name} success");
                    }
                    else
                    {
                        return new(4, $"Send api to machine {Name} fail");
                    }
                }
                else
                {
                    return new(4, $"Machine {Name} is not running");
                }

            }
            catch (Exception e)
            {
                return new(4, $"Send api to machine {Name} fail({e.Message})");
            }
        }
    }
}
