namespace KernelMemoryExamples;

public static class KM0002_SemanticKernelPlugin
{
    public static async Task RunAsync()
    {
        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        Console.WriteLine("Semantic Kernel ready.");

        string memoryServiceEndpoint = "http://127.0.0.1:9001/";

        string memoryServiceApiKey = "AAD";

        MemoryWebClient memoryConnector = new(memoryServiceEndpoint, memoryServiceApiKey);

        string pluginName = "memory";

        KernelPlugin plugin = kernel.ImportPluginFromObject(
            new MemoryPlugin(memoryConnector, waitForIngestionToComplete: true),
            pluginName);

        Console.WriteLine("Memory plugin imported.");

        await memoryConnector.ImportTextAsync(
            "Orion is a prominent set of stars visible during winter in " +
            "the northern celestial hemisphere. It is one of the 88 modern constellations; " +
            "it was among the 48 constellations listed by the 2nd-century astronomer Ptolemy. " +
            "It is named for a hunter in Greek mythology.",
            documentId: "OrionDefinition");

        KernelArguments arguments = new()
        {
            [MemoryPlugin.FilePathParam] = Path.Combine(TestConfiguration.DocumentRootPath, "NASA-news.pdf"),
            [MemoryPlugin.DocumentIdParam] = "NASA001",
        };

        await plugin["SaveFile"].InvokeAsync(kernel, arguments);

        Console.WriteLine("Memory updated.");

        string skPrompt = """
Question to Memory: {{$input}}

Answer from Memory: {{memory.ask $input}}

If the answer is empty say 'I don't know' otherwise reply with a preview of the answer,
truncated to 15 words. Prefix with one emoji relevant to the content.
""";

        KernelFunction function = kernel.CreateFunctionFromPrompt(skPrompt);

        Console.WriteLine("Semantic Function ready.");

        FunctionResult result = await function.InvokeAsync(kernel, "any news from NASA about Orion?");

        Console.WriteLine(result.GetValue<string>());

        result = await function.InvokeAsync(kernel, "define Orion");

        Console.WriteLine(result.GetValue<string>());
    }
}
