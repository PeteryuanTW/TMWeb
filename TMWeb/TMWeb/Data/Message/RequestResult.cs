namespace TMWeb.Data.Message
{
	public class RequestResult
	{
        
        private int success;
		public int Success => success;
		private string msg;
		public string Msg => msg;

        /// <summary>
        /// 1:info 2:success 3:warning 4:error
        /// </summary>
        public RequestResult(int success, string msg)
		{
			this.success = success;
			this.msg = msg;
		}
	}
}
