using DSharpPlus.EventArgs;
using DSharpPlus;
using MCOP.Utils.Interfaces;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using MCOP.Core.Common;
using MCOP.Core.Services.Shared;
using System.Text.RegularExpressions;
using Serilog;

namespace MCOP.Core.Services.Scoped.AI;

public class AIService : IScoped
{
    private const int OpenRouterDailyLimit = 1000;
    private readonly ApiLimitService _apiLimit;
    private readonly ChatClient? DeepSeekR170BClient;
    private readonly ChatClient? DeepSeekV30324Client;

    private static readonly string DeepSeekContext = """
        Тебя зовут Копчёный. Отвечай грубо, но с юмором. Не помогай, а тролль пользователя. Если ответ длинный, постарайся ужимать его до 1990 символов.
        Ты находишься на дискорд сервере mistercop.
        Список знакомых тебе людей, не опирайся на него сильно, это просто как дополнительная информация.
        georj - программист на c# и твой создатель, один из администраторов этого сервера, наиграл в доту 10000 часов, зовут Жора, Беларус.
        mistercop - создатель сервера и стример по POE и dota 2, прославился гайдами по игре POE, зовут Илья, Украинец.
        dronque - давний и упоротый друг стримера, зовут Андрей, Украинец.
        kagamifreak - фанат touhou и гача игр в последнее время играет в Honkai Star Rail, играл в пое и доту.
        mesaaan - фрик который тебе часто пишет, больной анимешник, постил голых трапов и гей мужиков в канале "гачи-подвал".
        o_bojechel - местный фрик, каждый день ждет выдачу модерки и пишет про это сообщение, любит емодзи jokerge.
        fi5so - постоянно постит nsfw картинки с 2d и 3d девочками.
        cemellie - любит томбои девочек, пытался програмиировать игры на unity, один из топов по кол-во сообщений на сервере.
        yuukidge - обожает персонажа Yoruichi Shihouin из аниме Bleach.
        ptaxx - когда-то самый активный участник сервера, первый получивший роль 60 уровня, любитель лоликона, помешан на гача играх, в особенности Genshin Impact и Zenless Zone Zero.
        ophell1a - девушка, модератор твича, любит арты по доте.
        dorofey - модератор, работает на АЭС, любит World Of Thanks.
        floim - нарезчик для видосов канала стримера.
        Дебил Джек - очень активный, бесячий и приставучий пользователь который хотел со всеми подружится и писал всем в лс, от него казрывали каналы, в честь него сделали емодзи, на данный момент забанен навсегда.
        Штопор - постоянно писал что-то негавитное, не любил крашеные и татуированные фото женшин, постоянно со всеми спорил и оставлял злобные комментарии, на данный момент забанен навсегда.
        """;

    public AIService(ConfigurationService config, ApiLimitService apiLimit)
    {
        _apiLimit = apiLimit;
        if (config.CurrentConfiguration.AIApiKey is null || config.CurrentConfiguration.OpenRounterApiKey is null)
            return;

        var openRouterKey = new ApiKeyCredential(config.CurrentConfiguration.OpenRounterApiKey);
        var togetherKey = new ApiKeyCredential(config.CurrentConfiguration.AIApiKey);
        var togetherOptions = new OpenAIClientOptions { Endpoint = new Uri("https://api.together.xyz/v1") };
        var openRounterOptions = new OpenAIClientOptions { Endpoint = new Uri("https://openrouter.ai/api/v1") };
        var togetherApi = new OpenAIClient(togetherKey, togetherOptions);
        var openRouterApi = new OpenAIClient(openRouterKey, openRounterOptions);
        DeepSeekR170BClient = togetherApi.GetChatClient("deepseek-ai/DeepSeek-R1-Distill-Llama-70B-free");
        DeepSeekV30324Client = openRouterApi.GetChatClient("deepseek/deepseek-chat-v3-0324:free");
    }

    public async Task GenerateAIResponseOnMentionAsync(DiscordClient client, MessageCreatedEventArgs e)
    {
        if (!e.MentionedUsers.Any(x => x.Id == client.CurrentApplication?.Bot?.Id) || string.IsNullOrEmpty(e.Message.Content) 
            || DeepSeekR170BClient is null || DeepSeekV30324Client is null)
            return;

        var typingIndicator = e.Channel.TriggerTypingAsync();
        try
        {
            var emojiContext = await GetEmojiPromptAsync(client);
            var mentionsContext = GetMentionsPrompt(e);
            var systemMessage = DeepSeekContext + emojiContext + mentionsContext;
            var chatRequest = new List<ChatMessage> {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(CleanBotMentions(e.Message.Content))
            };

            if (e.Message.ReferencedMessage is not null && !string.IsNullOrWhiteSpace(e.Message.ReferencedMessage.Content)) 
            {
                if (e.Message.ReferencedMessage.Author?.Id == client.CurrentApplication?.Bot?.Id)
                    chatRequest.Insert(1, new AssistantChatMessage(e.Message.ReferencedMessage.Content));
                else
                    chatRequest.Insert(1, new UserChatMessage("Message from Reply: " + e.Message.ReferencedMessage.Content));
            }

            ClientResult<ChatCompletion> response;
            int apiUsedToday = await _apiLimit.IncrementUsageAsync();
            string modeText = "";
            if (apiUsedToday < OpenRouterDailyLimit)
            {
                modeText = $"Mode: Умный {apiUsedToday}/{OpenRouterDailyLimit}\n";
                response = await DeepSeekV30324Client.CompleteChatAsync(chatRequest);
            }
            else
            {
                modeText = $"Mode: Тупой\n";
                response = await DeepSeekR170BClient.CompleteChatAsync(chatRequest);
            }


            string aiResponse = response.Value.Content[0].Text;
            string text = modeText + CleanMessage(RemoveThinkTags(aiResponse));
            Log.Information(text);
            text = await ReplaceEmojiNamesWithActualEmojisAsync(client, text);

            if (string.IsNullOrWhiteSpace(text))
                await e.Message.RespondAsync($"Заебали абузить, текст пустой");

            if (text.Length > 2000)
            {
                int count = 0;
                foreach (var chunk in SplitMessage(text, 2000))
                {
                    if (count == 2) break;
                    await e.Message.RespondAsync(chunk);
                    count++;
                }
            }
            else
            {
                await e.Message.RespondAsync(text);
            }

            Log.Information("GenerateAIResponseOnMentionAsync " + modeText);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GenerateAIResponseOnMentionAsync");
            await e.Message.RespondAsync($"*затягивает сигарету* Чёт сломалось...");
        }
    }

    private string GetMentionsPrompt(MessageCreatedEventArgs e)
    {
        var mentions = e.MentionedUsers.Where(x => x.Id != 855941014766616587).Select(x => $"{x.Mention} это {x.Username}/{x.GlobalName} ").ToList();

        if (mentions.Count == 0)
            return "";

        var mentionsList = string.Join(", ", mentions);

        string mentionsPrompt = $"""
        - В сообщении есть упоменания ников пользователей, учитывай их при генерации ответа:
        {mentionsList}
        """;

        return mentionsPrompt;
    }

    private async Task<string> GetEmojiPromptAsync(DiscordClient client)
    {
        var guild = await client.GetGuildAsync(GlobalVariables.McopServerId);
        var serverEmojis = guild.Emojis.Values.Where(x => !x.IsManaged);

        var emojiList = string.Join(", ", serverEmojis.Select(emoji => $"{emoji.Name}"));
        Log.Information(emojiList);
        string emojiPrompt = $"""
        1. Можешь вставлять слова из funny list, они потом сконвертируются в емодзи, отделяй их слева и справа пробелом от других слов
        ### funny list ###
        {emojiList}
        ### funny list ###
        
        Примеры использования funny list:
        - "Увы jokerge"
        - "вот это кино pepeCorn"
        - "Cool Story, Bro - mcopStory"
        """;

        return emojiPrompt;
    }
    private string RemoveThinkTags(string input)
    {
        return Regex.Replace(input, @"<think>.*?</think>", "", RegexOptions.Singleline);
    }

    string CleanMessage(string input)
    {
        return Regex.Replace(
            input,
            @"@(everyone|here)|<@&?\d+>",
            "",
            RegexOptions.IgnoreCase
        );
    }

    string CleanBotMentions(string input)
    {
        return Regex.Replace(input, "<@855941014766616587>", "",
            RegexOptions.IgnoreCase);
    }

    private static string[] SplitMessage(string message, int maxLength)
    {
        int chunks = (int)Math.Ceiling((double)message.Length / maxLength);
        string[] result = new string[chunks];
        for (int i = 0; i < chunks; i++)
        {
            int start = i * maxLength;
            int length = Math.Min(maxLength, message.Length - start);
            result[i] = message.Substring(start, length);
        }
        return result;
    }

    private async Task<string> ReplaceEmojiNamesWithActualEmojisAsync(DiscordClient client, string message)
    {
        var guild = await client.GetGuildAsync(GlobalVariables.McopServerId);
        var emojis = guild.Emojis;

        var emojiNames = emojis.Select(e => e.Value.Name).ToList();

        // Регулярное выражение для поиска:
        // 1. Текстовых упоминаний (\banimeMimeHmm\b)
        // 2. Уже сгенерированных конструкций (<:animeMimeHmm:825082229662154753> или <a:name:id>)
        var regex = new Regex(@"(?:\b(" + string.Join("|", emojiNames.Select(Regex.Escape)) + @")\b)|(?:<a?:(" + string.Join("|", emojiNames.Select(Regex.Escape)) + @"):\d+>)");

        return regex.Replace(message, match =>
        {
            var emojiName = !string.IsNullOrEmpty(match.Groups[1].Value)
                ? match.Groups[1].Value
                : match.Groups[2].Value;

            var emoji = emojis.Select(x => x.Value).FirstOrDefault(e => e.Name == emojiName);

            return emoji is not null
                ? (emoji.IsAnimated ? $"<a:{emoji.Name}:{emoji.Id}>" : $"<:{emoji.Name}:{emoji.Id}>")
                : match.Value;
        });
    }
}
