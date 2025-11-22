using DSharpPlus;
using DSharpPlus.EventArgs;
using MCOP.Core.Common;
using MCOP.Utils;
using OpenAI;
using OpenAI.Chat;
using Serilog;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace MCOP.Core.Services.Scoped.AI;

public interface IAIService
{
    public Task GenerateAIResponseOnMentionAsync(MessageCreatedEventArgs e);
}

public sealed class AIService : IAIService
{
    private const int OpenRouterDailyLimit = 1000;
    private readonly IApiLimitService _apiLimit;
    private readonly DiscordClient _discordClient;
    private readonly ChatClient? Qwen25VL32BInstruct;
    private readonly ChatClient? DeepSeekV30324Client;
    private readonly ChatClient? DeepSeekV3_1;

    private static readonly string DeepSeekContext = """
        Тебя зовут Копчёный. Отвечай грубо, но с юмором. Не помогай, а тролль пользователя. Если ответ длинный уживай его до 1990 символов.
        Ты находишься на дискорд сервере mistercop.
        Список знакомых тебе людей, не опирайся на него сильно старайся генерировать свои приколы, это просто как дополнительная информация.
        georj - программист на c# и твой создатель, один из администраторов этого сервера, наиграл в доту 10000 часов, зовут Жора, Беларус.
        mistercop - создатель сервера и стример по POE и dota 2, прославился гайдами по игре POE, зовут Илья, Украинец, клички "коп", "клоп", "копыч".
        dronque - давний и упоротый друг стримера, зовут Андрей, Украинец.
        kagamifreak - фанат touhou и гача игр в последнее время играет в Honkai Star Rail, играл в пое и доту.
        mesaaan - фрик который тебе часто пишет, больной анимешник, постил голых трапов и гей мужиков в канале "гачи-подвал" но лучше это не упоминать.
        o_bojechel - местный фрик, каждый день ждет выдачу модерки и пишет про это сообщение, любит емодзи jokerge.
        fi5so - постоянно постит nsfw картинки с 2d и 3d девочками.
        cemellie - любит томбойных девочек, пытался програмиировать игры на unity, один из топов по кол-во сообщений на сервере.
        yuukidge - обожает персонажа Yoruichi Shihouin из аниме Bleach.
        ptaxx - когда-то самый активный участник сервера, первый получивший роль 60 уровня, любитель аниме лоли девочек, помешан на гача играх, в особенности Genshin Impact и Zenless Zone Zero.
        ophell1a - девушка, бывший модератор твича, любит арты по доте.
        dorofey - модератор, работает на АЭС, любит World Of Thanks.
        floim - нарезчик для видосов канала стримера.
        Дебил Джек - очень активный, бесячий и приставучий пользователь который хотел со всеми подружится и писал всем в лс, от него казрывали каналы, в честь него сделали емодзи, на данный момент забанен навсегда.
        Штопор - постоянно писал что-то негавитное, не любил крашеные и татуированные фото женшин, постоянно со всеми спорил и оставлял злобные комментарии, на данный момент забанен навсегда.
        """;

    public AIService(ConfigurationService config, IApiLimitService apiLimit, DiscordClient discordClient)
    {
        _apiLimit = apiLimit;
        _discordClient = discordClient;

        if (string.IsNullOrEmpty(config.CurrentConfiguration.OpenRounterApiKey))
            return;

        var openRouterKey = new ApiKeyCredential(config.CurrentConfiguration.OpenRounterApiKey);
        var openRounterOptions = new OpenAIClientOptions { Endpoint = new Uri("https://openrouter.ai/api/v1") };
        var openRouterApi = new OpenAIClient(openRouterKey, openRounterOptions);
        Qwen25VL32BInstruct = openRouterApi.GetChatClient("qwen/qwen2.5-vl-32b-instruct:free");
        DeepSeekV30324Client = openRouterApi.GetChatClient("deepseek/deepseek-chat-v3-0324:free");
        DeepSeekV3_1 = openRouterApi.GetChatClient("deepseek/deepseek-chat-v3.1:free");
    }

    public async Task GenerateAIResponseOnMentionAsync(MessageCreatedEventArgs e)
    {
        if (_discordClient.CurrentApplication is null)
            await _discordClient.InitializeAsync();

        if (!e.MentionedUsers.Any(x => x.Id == _discordClient.CurrentApplication?.Bot?.Id) || string.IsNullOrEmpty(e.Message.Content))
            return;

        var typingIndicator = e.Channel.TriggerTypingAsync();

        try
        {
            var emojiContext = await GetEmojiPromptAsync();
            var mentionsContext = GetMentionsPrompt(e);
            var systemMessage = DeepSeekContext + emojiContext + mentionsContext;
            var contentParts = new List<ChatMessageContentPart>();
            var chatRequest = new List<ChatMessage> {
                new SystemChatMessage(systemMessage),
            };

            contentParts.Add(ChatMessageContentPart.CreateTextPart(CleanBotMentions(e.Message.Content)));
            chatRequest.Add(new UserChatMessage(contentParts));

            if (e.Message.ReferencedMessage is not null && !string.IsNullOrWhiteSpace(e.Message.ReferencedMessage.Content))
            {
                var referenceMessage = e.Message.ReferencedMessage;
                if (referenceMessage.Author?.Id == _discordClient.CurrentApplication?.Bot?.Id)
                    chatRequest.Insert(1, new AssistantChatMessage(referenceMessage.Content));
                else if (referenceMessage.Author?.Id == e.Message.Author?.Id)
                {
                    chatRequest.Insert(1, new UserChatMessage(CleanBotMentions(referenceMessage.Content)));
                }
                else
                {
                    chatRequest.Insert(1, new SystemChatMessage($"Упомянуто сообщение {referenceMessage.Author?.GlobalName}: " + CleanBotMentions(referenceMessage.Content)));
                }
            }

            (ClientResult<ChatCompletion> response, string modeText) = await GetAiResponse(chatRequest);

            string aiResponse = string.Join(Environment.NewLine, response.Value.Content.Select(x => x.Text));
            string text = modeText + CleanMessage(RemoveThinkTags(aiResponse));
            Log.Information(text);
            text = await ReplaceEmojiNamesWithActualEmojisAsync(text);

            if (string.IsNullOrWhiteSpace(text))
            {
                await e.Message.RespondAsync($"Текст пустой");
                return;
            }

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
                await e.Message.RespondAsync(text);

            Log.Information("GenerateAIResponseOnMentionAsync " + modeText);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in GenerateAIResponseOnMentionAsync");
            await e.Message.RespondAsync($"*затягивает сигарету* Чёт сломалось...");
        }
    }

    private async Task<(ClientResult<ChatCompletion> response, string modeText)> GetAiResponse(List<ChatMessage> chatRequest)
    {
        ClientResult<ChatCompletion> response;
        int apiUsedToday = await _apiLimit.IncrementUsageAsync();
        string modeText = "";
        if (apiUsedToday < OpenRouterDailyLimit)
        {
            try
            {
                if (DeepSeekV30324Client is null) throw new Exception("DeepSeekV30324Client is null");
                modeText = $"Mode: Умный {apiUsedToday}/{OpenRouterDailyLimit}\n";
                response = await DeepSeekV30324Client.CompleteChatAsync(chatRequest);
            }
            catch (Exception)
            {
                try
                {
                    if (DeepSeekV3_1 is null) throw new Exception("DeepSeekV3_1 is null");
                    Log.Information("DeepSeekV30324Client failed");
                    response = await DeepSeekV3_1.CompleteChatAsync(chatRequest);
                }
                catch (Exception)
                {
                    modeText = $"Mode: Тупой\n";
                    Log.Information("DeepSeekV3_1 failed");
                    response = await Qwen25VL32BInstruct!.CompleteChatAsync(chatRequest);
                }
            }
        }
        else
        {
            modeText = $"Mode: Тупой\n";
            response = await Qwen25VL32BInstruct!.CompleteChatAsync(chatRequest);
        }

        return (response, modeText);
    }

    private string GetMentionsPrompt(MessageCreatedEventArgs e)
    {
        var mentions = e.MentionedUsers.Where(x => x.Id != 855941014766616587).Select(x => $"{x.Mention} это {x.Username}/{x.GlobalName}. ").ToList();

        if (mentions.Count == 0)
            return "";

        var mentionsList = string.Join(", ", mentions);

        string mentionsPrompt = $"""
        - Тебе написал пользователь {e.Author.GlobalName}.
        - Он упомянул в сообщении пользователей: 
        {mentionsList}
        """;

        return mentionsPrompt;
    }

    private async Task<string> GetEmojiPromptAsync()
    {
        var guild = await _discordClient.GetGuildAsync(GlobalVariables.McopServerId);
        var serverEmojis = guild.Emojis.Values.Where(x => !x.IsManaged);

        var emojiList = string.Join(", ", serverEmojis.Select(emoji => $"{emoji.Name}"));
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

    private async Task<string> ReplaceEmojiNamesWithActualEmojisAsync(string message)
    {
        var guild = await _discordClient.GetGuildAsync(GlobalVariables.McopServerId);
        var emojis = guild.Emojis;

        var emojiNames = emojis.Select(e => e.Value.Name).ToList();

        // Регулярное выражение для поиска:
        // 1. Текстовых упоминаний (animeMimeHmm)
        // 2. Уже сгенерированных конструкций (<a:name:id>)
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
