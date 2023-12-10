# TPL DataFlow vs Channels
---

This repository was intended to show two different approaches to obtaining an asynchronous queue using for it [TPL DataFlow](https://www.nuget.org/packages/System.Threading.Tasks.Dataflow/) or [Channels](https://www.nuget.org/packages/System.Threading.Channels).

There are several pipelines for demo:
- **SimplePipeline** - shows how to create a simple operation using pipeline;
- **ThrowablePipeline** - shows how to deal with throwing exceptions in pipelines;
- **CancelablePipeline** - shows how to deal with cancellation  in pipelines;
- **ComplexPipeline** - shows how to build a complex pipeline with asynchronous data processing.

Besides that, you can also find benchmark projects.
It was added just to compare the two approaches.
