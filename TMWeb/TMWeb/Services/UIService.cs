using CommonLibrary.API.Message;

namespace TMWeb.Services
{
    public class UIService
    {
        public Action<RequestResult>? ToastAction;
        public void ShowToast(RequestResult requestResult)
        {
            ToastAction?.Invoke(requestResult);
        }
    }
}
