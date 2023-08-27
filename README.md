# Json.TimeInterval

Extends `System.Text.Json` with a custom new `TimeInterval` type. Be aware, it is implemented for deserialization or read only!

```json
{
    "TheDevice": {
        "Timeout": "3s",
        "PollingInterval": "500ms",
        "RefreshTokenEvery": "1h 15m"
    }
}
```

## Format

Use string quotes `""` to decorate values:

```json
"KeyName": "1500ms"
```

- Only unsigned integer values for time units.
- Possible delimeters: `- _:/'` (space included).
- `"3m 1h 2m"` is equivalent to `"1h 5m"`. Identical time units are sum up: `3m + 2m = 5m`.
- System TimeSpan is allowable, so `"00:01:02:000"` is equivalent to `"1h 2m"`.

### Examples

- "1h34m26s134ms"
- "1h 12m127s"
- "1d/12h 15m 7s"
- "3d/12h_15m:12s---347ms"

## How to use

```csharp
public class MyClass_ToDeserializeJson
{
    public Device TheDevice {get; set;}

    public class Device 
    {
        public TimeInterval Timeout {get; set;}
            = TimeSpan.FromSeconds( 3 );

        public TimeInterval PollingInterval {get; set;}
            = TimeSpan.FromMilliseconds( 500 );

        public TimeInterval RefreshTokenEvery {get; set;}
            = TimeSpan.FromHours( 1 )
            + TimeSpan.FromMinutes( 15 ) );
    }
}

...

public async Task Main()
{
    TimeSpan pollingInterval =
        // Use streams not strings! This is just a short example.
        JsonSerializer.Deserialize<MyClass_ToDeserializeJson>(
            File.ReadAllText( "./deviceconfig.json" ) )
        .TheDevice
        .PollingInterval;

    while (!Console.KeyAvailable) {
        Debug.Write(".")
        await Task.Delay( pollingInterval );
    }
}
```
