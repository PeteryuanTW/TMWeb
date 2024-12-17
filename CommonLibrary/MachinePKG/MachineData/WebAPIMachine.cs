using CommonLibrary.API.Message;
using CommonLibrary.MachinePKG.EFModel;
using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CommonLibrary.MachinePKG.MachineData
{
    public class WebAPIMachine : Machine
    {
        protected HttpClient httpClient;
        public WebAPIMachine(Machine machine) : base(machine)
        {
            httpClient = new();
        }

        public override Task ConnectAsync()
        {
            Running();
            return Task.CompletedTask;
        }

        public override async Task<RequestResult> UpdateTag(Tag tag)
        {
            try
            {
                if (MachineStatus != Status.Disconnect || MachineStatus != Status.TryConnecting)
                {
                    var targetType = MachineTypeEnumHelper.GetTypeByCode(tag.DataType);
                    var apiRes = await httpClient.GetAsync($"http://{Ip}:{Port}{tag.String1}");
                    if (apiRes.IsSuccessStatusCode)
                    {
                        var apiResponse = await apiRes.Content.ReadAsStringAsync();
                        tag.SetValue(apiResponse);
                    }
                    else
                    {
                        return new(1, $"Machine status {Status.Disconnect} or {Status.TryConnecting} is not allow to update tag");
                    }
                }
            }
            catch (IOException e)
            {
                Disconnect(e.Message);
                return new(4, $"Update tags fail({e.Message})");
            }
            catch (SocketException e)
            {
                Disconnect(e.Message);
                return new(4, $"Update tags fail({e.Message})");
            }
            catch (InvalidOperationException e)
            {
                Disconnect(e.Message);
                return new(4, $"Update tags fail({e.Message})");
            }
            catch (Exception e)
            {
                var a = e.GetType();
                Error(e.Message);
                return new(4, $"Update tags fail({e.Message})");
            }
            return new(0, "testing");
        }
    }
}
