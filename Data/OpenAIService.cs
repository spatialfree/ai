using OpenAI;
using OpenAI.Edits;
namespace ai.Data;

public class OpenAIService {
  public OpenAIClient api;
  public OpenAIService(string? apiKey) {
    this.api = new OpenAIClient(new OpenAIAuthentication(apiKey));

    CompletionRequest.DefaultCompletionRequestArgs = new CompletionRequest(
      OpenAI.Models.Model.Davinci,
      "",
      max_tokens: 256, 
      temperature: 0.7,
      frequencyPenalty: 0.5,
      presencePenalty: 0.5,
      stopSequences: new string [] {".\n"} //\n\n
    ); 
  }

  public async Task<string[]> EditAsync(
    string? input, 
    string? instruction,
    int? editCount = 1,
    double? temperature = 1 
    ) {
    EditRequest request = new EditRequest(
      input, instruction, 
      editCount: editCount, 
      temperature: temperature
    );

    var endpoint = api.EditsEndpoint;
    var result = await endpoint.CreateEditAsync(request);
    return result.Choices.Select(x => x.Text).ToArray<string>();
    
    // Console.WriteLine($"result: {result.ToString().TrimStart()}");
    // return result.ToString();
  }

  public async IAsyncEnumerable<string> CompletionStream(string? prompt) {
    CompletionRequest request = new CompletionRequest();
    request.Prompt = prompt;

    var endpoint = api.CompletionsEndpoint;
    await foreach (var token in endpoint.StreamCompletionEnumerableAsync(request)) {
      yield return token.Completions[0].Text;
    }
  }
}