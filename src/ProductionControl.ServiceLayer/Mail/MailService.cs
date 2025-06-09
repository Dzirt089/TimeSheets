using MailerVKT;

namespace ProductionControl.ServiceLayer.Mail
{
	public class MailService(Sender sender)
	{
		public async Task SendMailAsync(MailParameters parameters)
		{
			await sender.SendAsync(parameters);
		}
	}
}
