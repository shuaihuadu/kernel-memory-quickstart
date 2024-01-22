namespace KernelMemoryExamples;

public static class Example002_SemanticKernelPlugin
{
    public static async Task RunAsync()
    {
        string DocFileName = Path.Join(TestConfiguration.DocumentRootPath, "mydocs-NASA-news.pdf");

        const string Question1 = "Any news about Orion?";
        const string Question2 = "Any news about Hubble telescope?";
        const string Question3 = "What is a solar eclipse?";

        Kernel kernel = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(
                deploymentName: TestConfiguration.AzureOpenAI.DeploymentName,
                endpoint: TestConfiguration.AzureOpenAI.Endpoint,
                apiKey: TestConfiguration.AzureOpenAI.ApiKey)
            .Build();

        string skPrompt = """
                          Question to Kernel Memory: {{$input}}

                          Kernel Memory Answer: {{memory.ask $input}}

                          If the answer  is empty say 'I don't know' otherwise reply  with a preview of the answer, truncated to 15 words.
                          """;

        KernelFunction kernelFunction = kernel.CreateFunctionFromPrompt(skPrompt);

        IKernelMemory kernelMemory = GetMemoryConnector();

        KernelPlugin memoryPlugin = kernel.ImportPluginFromObject(new MemoryPlugin(kernelMemory, waitForIngestionToComplete: true), "memory");

        KernelArguments arguments = new()
        {
            [MemoryPlugin.FilePathParam] = DocFileName,
            [MemoryPlugin.DocumentIdParam] = "NASA001"
        };

        await memoryPlugin["SaveFile"].InvokeAsync(kernel, arguments);

        Console.WriteLine("---------");
        Console.WriteLine($"{Question1} (expected: some answer using the PDF provided)\n");

        FunctionResult answer = await kernelFunction.InvokeAsync(kernel, Question1);
        Console.WriteLine($"Answer: {answer.GetValue<string>()}");

        Console.WriteLine("---------");
        Console.WriteLine($"{Question2} (expected answer: \"I don't know\")");
        answer = await kernelFunction.InvokeAsync(kernel, Question2);
        Console.WriteLine($"Answer: {answer.GetValue<string>()}");

        Console.WriteLine("---------");
        Console.WriteLine($"{Question3} (expected answer: \"I don't know\")");
        answer = await kernelFunction.InvokeAsync(kernel, Question3);
        Console.WriteLine($"Answer: {answer.GetValue<string>()}");
    }

    private static IKernelMemory GetMemoryConnector(bool serverless = false)
    {
        if (!serverless)
        {
            return new MemoryWebClient("http://127.0.0.1:9001/");
        }

        Console.WriteLine("This code is intentionally disabled.");
        Console.WriteLine("To test the plugin with Serverless memory:");
        Console.WriteLine("* Add a project reference to CoreLib");
        Console.WriteLine($"* Uncomment/edit the code in {nameof(GetMemoryConnector)}");

        Environment.Exit(-1);

        return null;
    }
}