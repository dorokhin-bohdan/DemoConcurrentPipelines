using ConcurrentPipelines.Channels;

#region SimplePipeline

//Console.WriteLine("Simple Channels Pipeline");
//await new SimplePipeline().RunAsync();

#endregion

#region ThrowablePipeline

//Console.WriteLine("Throwable Channels Pipeline");
//await new ThrowablePipeline().RunAsync();

#endregion

#region 

Console.WriteLine("Cancelable Channels Pipeline");
await new CancelablePipeline().RunAsync();

#endregion

#region ComplexPipeline

//Console.WriteLine("Complex Channels Pipeline");
//await new ComplexPipeline().RunAsync();

#endregion

