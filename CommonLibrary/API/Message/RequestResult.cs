namespace CommonLibrary.API.Message
{
    public class RequestResult
    {

        private int returnCode;
        public int ReturnCode => returnCode;
        private string msg;
        public string Msg => msg;

        public bool IsSuccess => returnCode == 0 || returnCode == 1;
        /// <summary>
        /// 1:info 2:success 3:warning 4:error
        /// </summary>
        public RequestResult(int returnCode, string msg)
        {
            this.returnCode = returnCode;
            this.msg = msg;
        }
    }
}
