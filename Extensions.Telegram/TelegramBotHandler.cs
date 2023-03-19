using Boa.TelegramBotService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Boa.Identity.Telegram
{
    public class TelegramBotHandler : ITelegramBotHandler
    {
        public int Order => 500;

        public async Task<bool> HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            return false;
        }
    }
}
