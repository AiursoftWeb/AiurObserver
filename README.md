# AiurObserver

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.cn/aiursoft/AiurObserver/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.cn/aiursoft/AiurObserver/badges/master/pipeline.svg)](https://gitlab.aiursoft.cn/aiursoft/AiurObserver/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.cn/aiursoft/AiurObserver/badges/master/coverage.svg)](https://gitlab.aiursoft.cn/aiursoft/AiurObserver/-/pipelines)
[![NuGet version (Aiursoft.AiurObserver)](https://img.shields.io/nuget/v/Aiursoft.AiurObserver.svg)](https://www.nuget.org/packages/Aiursoft.AiurObserver/)
[![ManHours](https://manhours.aiursoft.cn/r/gitlab.aiursoft.cn/aiursoft/AiurObserver.svg)](https://gitlab.aiursoft.cn/aiursoft/AiurObserver/-/commits/master?ref_type=heads)

AiurObserver is an async event driven framework.

## How to install

```bash
dotnet add package Aiursoft.AiurObserver
```

## How to use

It's very simple. You can create a class extends AsyncObservable<T> and then you can subscribe to it and broadcast messages to it.

`T` is the type of the message you want to broadcast.

If you no longer need to subscribe to the observable, you can call `UnRegister` method to unsubscribe.

Full example:

```csharp
var totalMessages = 0;
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable.Subscribe(_ =>
{
    totalMessages++;
    return Task.CompletedTask;
});
for (var i = 0; i < 10; i++)
{
    await asyncObservable.BroadcastAsync(2333);
}

Assert.AreEqual(10, totalMessages);

subscription.UnRegister();
        
for (var i = 0; i < 20; i++)
{
    await asyncObservable.BroadcastAsync(2333);
}
Assert.AreEqual(10, totalMessages);
```

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you with push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.