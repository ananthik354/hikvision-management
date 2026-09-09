namespace HikvisionBackend1.DTOs
{
    public class ResponceDTO
    {
      public bool success { get; set; }
        public string message{ get; set; }
        public object? data{ get; set; }

        public ResponceDTO(bool status_,string msg_) {
            success = status_;
            message = msg_;
        }
        public ResponceDTO(object data_)
        {
            success = true;
            message = "Success";
            data = data_;
        }
    }
}
