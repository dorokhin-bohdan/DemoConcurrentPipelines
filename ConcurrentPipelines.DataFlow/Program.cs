using ConcurrentPipelines.DataFlow;

#region SimplePipeline

Console.WriteLine("Simple DataFlow Pipeline");
await new SimplePipeline().RunAsync();

#endregion

#region ThrowablePipeline

//Console.WriteLine("Throwable DataFlow Pipeline");
//await new ThrowablePipeline().RunAsync();

#endregion

#region CancelablePipeline

//Console.WriteLine("Cancelable DataFlow Pipeline");
//await new CancelablePipeline().RunAsync();

#endregion

#region ComplexPipeline

//Console.WriteLine("Complex DataFlow Pipeline");
//await new ComplexPipeline().RunAsync();

#endregion
