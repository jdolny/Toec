using System.Web.Http;
using Toec_Common.Dto;

namespace Toec_UI.Controllers
{
    public class MessageController : ApiController
    {
        [HttpGet]
        public DtoBoolResponse DisplayMessage(string message,string title, int timeout)
        {
            MsgBox.Show(message, title, MsgBox.Buttons.OK,
                MsgBox.Icon.Info,timeout);
            return new DtoBoolResponse {Value = true};
        }
    }
}