using DSharpPlus.EventArgs;
using DSharpPlus;
using MCOP.Utils.Interfaces;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using MCOP.Core.Exceptions;
using MCOP.Core.Common;
using MCOP.Core.Services.Shared;
using System.Text.RegularExpressions;

namespace MCOP.Core.Services.Scoped.AI;

public class AIService : IScoped
{
    private const int OpenRouterDailyLimit = 1000;
    private readonly ApiLimitService _apiLimit;
    private readonly ChatClient? DeepSeekR170BClient;
    private readonly ChatClient? DeepSeekV30324Client;

    private static readonly string DeepSeekContext = "Тебя зовут Копчёный. Отвечай грубо, но с юмором. Не помогай, а тролль пользователя. Если ответ длинный, постарайся ужимать его до 1990 символов.";
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
            var chatRequest = new ChatMessage[] {
                new SystemChatMessage(systemMessage),
                new UserChatMessage(CleanBotMentions(e.Message.Content))
            };

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

            if (string.IsNullOrWhiteSpace(text))
                await e.Message.RespondAsync($"Заебали абузить, текст пустой");

            if (text.Length > 2000)
            {
                foreach (var chunk in SplitMessage(text, 2000))
                    await e.Message.RespondAsync(chunk);
            }
            else
            {
                await e.Message.RespondAsync(text);
            }
        }
        catch (Exception ex)
        {
            await e.Message.RespondAsync($"*затягивает сигарету* Чёт сломалось...");
            throw new McopException(ex, "AI Error");
        }
    }

    private string GetMentionsPrompt(MessageCreatedEventArgs e)
    {
        var mentions = e.MentionedUsers.Where(x => x.Id != 855941014766616587).Select(x => $"{x.Mention} это {x.Username} ").ToList();

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

        var emojiList = string.Join(", ", serverEmojis.Select(emoji => $"{emoji}"));
        string emojiPrompt = $"""
        1. You will be penalized & fined $1000 if you use words from the ban list. If you use one word from the ban list, I will stop the generation right away
        ### ban list ###
        everyone
        ### ban list ###
        2. Можешь использовать эмодзи из emoji list, где уместно, они отделены запятой
        ### emoji list ###
        {emojiList}
        ### emoji list ###
        
        Примеры использования emoji list:
        - "Увы <:jokerge:1363419830702706758>"
        - "вот это кино <a:pepeCorn:908257282066903060>"
        - "Cool Story, Bro - <:mcopStory:473225146124206081>"
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
}
