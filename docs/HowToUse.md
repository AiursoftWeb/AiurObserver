# Learn AiurObserver

AiurObserver is a powerful C# development tool that allows you to construct an object that can be observed asynchronously. It comes with a set of operators that make it easy for you to manipulate and process data streams.

## Basic Usage

```csharp
var totalMessages = 0;
var asyncObservable = new AsyncObservable<int>();
var subscription = asyncObservable.Subscribe(_ =>
{
    totalMessages++;
    return Task.CompletedTask;
});
await asyncObservable.BroadcastAsync(2333);
Console.WriteLine(totalMessages); // 1
```

## Always Consume

You always need to consume the observable.

If you don't consume, you are actually defining something can be subscribed but no one is listening.

```csharp
var asyncObservable = new AsyncObservable<int>();
var query = asyncObservable.Filter(t => t > 100); // query is another IAsyncObservable, but no one is listening.

// Nothing happens!
await asyncObservable.BroadcastAsync(2333); 

var subscription = query.Subscribe(t => Console.WriteLine(t)); // Now we are listening.
await asyncObservable.BroadcastAsync(2333); // 2333
```

## Always unsubscribe when you no longer need to consume

If the observable is no longer needed, you should unsubscribe.

```csharp
ISubscription? subscription = null;
try
{
    subscription = _someService.Subscribe(t => Console.WriteLine(t));

    // Don't forget the `await` here!
    await asyncObservable.BroadcastAsync(2333);
}
finally
{
    subscription?.Unsubscribe();
}
```

## Basic consumers

Yes we provide some basic consumers for you.

* Counter
* StageFirst
* StageLast
* Adder
* Average

These are consumers already so you don't need to call `Subscribe` on them.

These must be added to the end of the observable chain.

```csharp
var asyncObservable = new AsyncObservable<int>();
var counter = asyncObservable
    .Counter();

// Then Access:
var count = counter.Count;
```

Don't forget, the query is appended on an observable not a collection. So it is not actually counting. It actually works as a consumer.

### Counter

`Counter()` will count how many times the observable has been broadcasted.

```csharp
var asyncObservable = new AsyncObservable<int>();
var counter = asyncObservable.Counter();

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(2333);

Console.WriteLine(counter.Count); // 3
```

### Stage First

`StageFirst()` will keep the first broadcasted message.

```csharp
var asyncObservable = new AsyncObservable<int>();
var first = asyncObservable.StageFirst();

Console.WriteLine(first.IsStaged); // False

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(33344);
await asyncObservable.BroadcastAsync(44455);

Console.WriteLine(first.Stage); // 2333
Console.WriteLine(first.IsStaged); // True
```

### Stage Last

`StageLast()` will keep the last broadcasted message.

```csharp
var asyncObservable = new AsyncObservable<int>();
var stage = asyncObservable.StageLast();
Console.WriteLine(stage.IsStaged); // False

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(33344);
await asyncObservable.BroadcastAsync(44455);

Console.WriteLine(stage.Stage); // 44455
Console.WriteLine(stage.IsStaged); // True
```

### Adder

`Adder()` will sum all broadcasted messages. It only works when the message is a number.

```csharp
var asyncObservable = new AsyncObservable<double>();
var summer = asyncObservable.Adder();

await asyncObservable.BroadcastAsync(2333);
await asyncObservable.BroadcastAsync(33344);
await asyncObservable.BroadcastAsync(44455);

Console.WriteLine(summer.Sum); // 80132
```

### Average

`Average()` will calculate the average of all broadcasted messages. It only works when the message is a number.

`Average()` only returns the total and the count. You need to calculate the average by yourself.

```csharp
var asyncObservable = new AsyncObservable<int>();
var average = asyncObservable.Average();

await asyncObservable.BroadcastAsync(1);

var (totalA, countA) = average.Average(); // 1 / 1
Console.WriteLine(totalA / countA); // 1

await asyncObservable.BroadcastAsync(2);
await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);

(totalA, countA) = average.Average(); // 12 / 5
Console.WriteLine(totalA / countA); // 2.4
```

### Average Recent

`AverageRecent()` will calculate the average of the last N broadcasted messages. It only works when the message is a number.

`AverageRecent()` only returns the total and the count. You need to calculate the average by yourself.

```csharp
var asyncObservable = new AsyncObservable<int>();
var averageRecent = asyncObservable.AverageRecent(3);

await asyncObservable.BroadcastAsync(1);

var (total, count) = averageRecent.Average(); // 1 / 1
Console.WriteLine(total / count); // 1

await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);
await asyncObservable.BroadcastAsync(3);

(total, count) = averageRecent.Average(); // 9 / 3
Console.WriteLine(total / count); // 3
```
