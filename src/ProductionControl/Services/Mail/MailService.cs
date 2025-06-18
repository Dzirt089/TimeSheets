using MailerVKT;

namespace ProductionControl.Services.Mail
{
	public class MailService(Sender sender)
	{
		public async Task SendMailAsync(MailParameters parameters)
		{
			await sender.SendAsync(parameters);
		}
	}
}
